using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using SUPPLY_API;
using SUPPLY_API.Models;
using MySql.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка токена RuTokenSettings:Token ---
var ruTokenFromEnv = Environment.GetEnvironmentVariable("RU_SERVICE_TOKEN");
if (!string.IsNullOrEmpty(ruTokenFromEnv))
{
    builder.Configuration["RuTokenSettings:Token"] = ruTokenFromEnv;
}
else if (string.IsNullOrEmpty(builder.Configuration["RuTokenSettings:Token"]))
{
    throw new Exception("RU_SERVICE_TOKEN не найден ни в конфиге, ни в переменных окружения!");
}

// --- Настройка DataProtection ---
var keysPath = Path.Combine(Path.GetTempPath(), "keys");
var keysDirectory = new DirectoryInfo(keysPath);
if (!keysDirectory.Exists)
    keysDirectory.Create();

var dataProtectionBuilder = builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDirectory);

if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapiNG();
}

// --- Порт ---
var port = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// --- Сервисы ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Конфигурации
builder.Services.Configure<RuTokenSettings>(builder.Configuration.GetSection("RuTokenSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CurrentServer>(builder.Configuration.GetSection("ServerAddresses"));

// HTTP Клиенты и сервисы
builder.Services.AddHttpClient<DaDataService>();
builder.Services.AddScoped<SomeServiceUsingToken>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailSender>();

// Фоновые службы
builder.Services.AddHostedService<EmailCleanupHostedService>();
builder.Services.AddHostedService<DuplicateCleanupService>();
builder.Services.AddHostedService<DataCopyService>();

// --- Строки подключения ---
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(defaultConnectionString))
{
    throw new InvalidOperationException("DefaultConnection string is not configured.");
}

var handyConnectionString = builder.Configuration.GetConnectionString("HandyConnection");
if (string.IsNullOrEmpty(handyConnectionString))
{
    throw new InvalidOperationException("ConnectionStringsHandy string is not configured.");
}

builder.Services.AddDbContext<UnitMeasurementComponentContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<HandyDbContext>(opt => opt.UseMySQL(handyConnectionString));


// --- Регистрация DbContext-ов с MySQL ---
builder.Services.AddDbContext<UnitMeasurementComponentContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<SupplyUnitMeasurementContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<SupplyProviderContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<SupplyPriceComponentContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<SupplyManufacturerContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<SupplyComponentContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<SupplyCompanyContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<CollaboratorSystemContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<CompanyCollaboratorContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<DeliveryAddressContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<ManufacturerComponentContext>(opt => opt.UseMySQL(defaultConnectionString));
builder.Services.AddDbContext<HandyDbContext>(opt => opt.UseMySQL(handyConnectionString));

// --- JWT аутентификация ---
var secretKey = builder.Configuration["JwtSettings:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
    throw new Exception("JwtSettings:SecretKey не найден в конфигурации");

var key = Encoding.UTF8.GetBytes(secretKey);

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
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true);
    });
});

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

app.UseCors("AllowAll");

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
