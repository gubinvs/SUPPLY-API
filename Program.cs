using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SUPPLY_API;
using SUPPLY_API.Models;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка токена RuTokenSettings:Token из переменных окружения ---
var ruTokenFromEnv = Environment.GetEnvironmentVariable("RU_SERVICE_TOKEN");
if (!string.IsNullOrEmpty(ruTokenFromEnv))
{
    builder.Configuration["RuTokenSettings:Token"] = ruTokenFromEnv;
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

// Получаем JwtSettings для конфигурации аутентификации
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
{
    throw new Exception("JwtSettings:SecretKey не найден или слишком короткий");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

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

// --- Настройка порта ---
var port = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// --- Добавляем сервисы ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Конфигурации для других секций
builder.Services.Configure<RuTokenSettings>(builder.Configuration.GetSection("RuTokenSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CurrentServer>(builder.Configuration.GetSection("ServerAddresses"));

// Регистрируем сервисы с зависимостями
builder.Services.AddHttpClient<DaDataService>();
builder.Services.AddScoped<SomeServiceUsingToken>();
builder.Services.AddScoped<TokenService>();       // TokenService с внедрением IOptions<JwtSettings>
builder.Services.AddScoped<EmailSender>();

// Фоновые службы
builder.Services.AddHostedService<EmailCleanupHostedService>();
builder.Services.AddHostedService<DuplicateCleanupService>();
builder.Services.AddHostedService<DataCopyService>();


// --- Проверяем и регистрируем строки подключения к БД ---
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(defaultConnectionString))
{
    throw new InvalidOperationException("DefaultConnection string is not configured.");
}

var handyConnectionString = builder.Configuration.GetConnectionString("HandyConnection");
if (string.IsNullOrEmpty(handyConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Handy string is not configured.");
}

// Регистрируем DbContext'ы с MySQL
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

