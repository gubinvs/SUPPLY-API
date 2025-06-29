using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using SUPPLY_API;
using SUPPLY_API.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка токена RuTokenSettings:Token с переопределением из переменной окружения ---
var ruTokenFromConfig = builder.Configuration["RuTokenSettings:Token"];
var ruTokenFromEnv = Environment.GetEnvironmentVariable("RU_SERVICE_TOKEN");

if (!string.IsNullOrEmpty(ruTokenFromEnv))
{
    builder.Configuration["RuTokenSettings:Token"] = ruTokenFromEnv;
}
else if (string.IsNullOrEmpty(ruTokenFromConfig))
{
    throw new Exception("RU_SERVICE_TOKEN не найден ни в конфиге, ни в переменных окружения!");
}

// === Настройка защиты данных ===
var keysDirectory = new DirectoryInfo("/app/keys");
var dataProtectionBuilder = builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDirectory);

if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapiNG();
}

// === Установка порта ===
var port = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// === Сервисы ===
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Конфигурация RuTokenSettings
builder.Services.Configure<RuTokenSettings>(builder.Configuration.GetSection("RuTokenSettings"));

// Регистрируем сервисы
builder.Services.AddHttpClient<DaDataService>();
builder.Services.AddScoped<SomeServiceUsingToken>();
builder.Services.AddScoped<CollaboratorSystemContext>();
builder.Services.AddScoped<TokenService>();

// Email настройки
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailSender>();

// Прочее
builder.Services.Configure<CurrentServer>(builder.Configuration.GetSection("ServerAddresses"));

// === Фоновые службы ===
builder.Services.AddHostedService<EmailCleanupHostedService>();
builder.Services.AddHostedService<DuplicateCleanupService>();
builder.Services.AddHostedService<DataCopyService>(); // Копирование базы данных HANDY


// === Строка подключения ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));

// === DbContext-ы ===
builder.Services.AddDbContext<UnitMeasurementComponentContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<SupplyUnitMeasurementContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<SupplyProviderContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<SupplyPriceComponentContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<SupplyManufacturerContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<SupplyComponentContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<SupplyCompanyContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<CollaboratorSystemContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<CompanyCollaboratorContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<DeliveryAddressContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<ManufacturerComponentContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContext<HandyDbContext>(options => options.UseMySQL("ConnectionStringsHandy:DefaultConnection", serverVersion));

// === JWT-аутентификация ===
var secretKey = "YourSecureKeyHereMustBeLongEnough"; // Лучше хранить в конфигурации
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// === CORS ===
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

// === Middleware ===
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
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
