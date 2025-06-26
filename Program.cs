using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using SUPPLY_API;
using SUPPLY_API.Models;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.Configure<RuTokenSettings>(builder.Configuration.GetSection("RuTokenSettings"));
builder.Services.AddHttpClient<DaDataService>(); // HttpClient через DI


// === Фоновые службы ===
builder.Services.AddHostedService<EmailCleanupHostedService>();
builder.Services.AddHostedService<DuplicateCleanupService>();

// === Email настройки ===
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailSender>();

// === Строка подключения (одна на все контексты) ===
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

// === JWT-аутентификация ===
var secretKey = "YourSecureKeyHereMustBeLongEnough"; // Вынести в конфигурацию!
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

app.UseAuthentication(); // <-- Обязательно до Authorization
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

