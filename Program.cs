using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.DataProtection;
using SUPPLY_API;
using SUPPLY_API.Models;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка RuTokenSettings:Token из переменных окружения ---
var ruToken = Environment.GetEnvironmentVariable("RU_SERVICE_TOKEN");
if (!string.IsNullOrEmpty(ruToken))
{
    builder.Configuration["RuTokenSettings:Token"] = ruToken;
}
else if (string.IsNullOrEmpty(builder.Configuration["RuTokenSettings:Token"]))
{
    throw new Exception("RU_SERVICE_TOKEN не найден ни в конфиге, ни в переменных окружения!");
}

// --- Регистрация и валидация JwtSettings ---
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(jwt =>
        !string.IsNullOrWhiteSpace(jwt.SecretKey) &&
        Encoding.UTF8.GetByteCount(jwt.SecretKey) >= 32,
        "JwtSettings:SecretKey должен быть задан и иметь длину не менее 32 байт");

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
{
    throw new Exception("JwtSettings:SecretKey не найден или слишком короткий");
}
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

// --- DataProtection ---
var keysPath = Path.Combine(Path.GetTempPath(), "keys");
var keysDir = new DirectoryInfo(keysPath);
if (!keysDir.Exists) keysDir.Create();

var dataProtectionBuilder = builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDir);
if (OperatingSystem.IsWindows()) dataProtectionBuilder.ProtectKeysWithDpapiNG();

// --- Порт ---
var port = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");



// --- Сервисы ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RuTokenSettings>(builder.Configuration.GetSection("RuTokenSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CurrentServer>(builder.Configuration.GetSection("ServerAddresses"));

builder.Services.AddHttpClient<DaDataService>();
builder.Services.AddScoped<SomeServiceUsingToken>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailSender>();

builder.Services.AddHostedService<EmailCleanupHostedService>();
builder.Services.AddHostedService<DuplicateCleanupComponentService>();// удаление дублей номенклатуры
builder.Services.AddHostedService<RemoveDuplicatesManufacturer>();// удаление дублей производителей
// builder.Services.AddHostedService<DataCopyService>();



// --- Проверка строк подключения ---
var defaultConn = builder.Configuration.GetConnectionString("AppDatabase");
var handyConn = builder.Configuration.GetConnectionString("HandyDatabase");

// --- DbContexts ---
builder.Services.AddDbContext<UnitMeasurementComponentContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<SupplyUnitMeasurementContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<SupplyProviderContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<SupplyPriceComponentContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<SupplyManufacturerContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<SupplyComponentContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<SupplyCompanyContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<CollaboratorSystemContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<CompanyCollaboratorContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<DeliveryAddressContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<ManufacturerComponentContext>(opt => opt.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
builder.Services.AddDbContext<HandyDbContext>(opt => opt.UseMySql(handyConn, ServerVersion.AutoDetect(handyConn)));


// --- JWT ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true));
});

// --- CORS ---
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowOnlySupply", policy =>
//         policy.WithOrigins("https://supply.encomponent.ru") // Только этот источник
//               .AllowAnyHeader()
//               .AllowAnyMethod());
// });

var app = builder.Build();

// --- Middleware ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll"); // Если все кому не лень запрашивают
// app.UseCors("AllowOnlySupply"); // только конкретному сайту

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
