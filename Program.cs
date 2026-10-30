using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
AddDotEnvFile(builder.Configuration, ResolveDotEnvPath(builder.Environment));
builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:LocalConnection"]
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__LocalConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No database connection string configured. Set ConnectionStrings__DefaultConnection in the environment or .env file.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<MercadoPagoOptions>(builder.Configuration.GetSection("MercadoPago"));
builder.Services.AddHttpClient<MercadoPagoSubscriptionService>(client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com/");
});

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<LoggerService>();
builder.Services.AddTransient<IGetClientTypeService, GetClientTypeService>();
builder.Services.AddScoped<ICreateClientTypeService, CreateClientTypeService>();
builder.Services.AddScoped<IUpdateClientTypeService, UpdateClientTypeService>();
builder.Services.AddScoped<IDeleteClientTypeService, DeleteClientTypeService>();
builder.Services.AddTransient<IGetClientService, GetClientService>();
builder.Services.AddScoped<ICreateClientService, CreateClientService>();
builder.Services.AddScoped<IUpdateClientService, UpdateClientService>();
builder.Services.AddScoped<IDeleteClientService, DeleteClientService>();
builder.Services.AddTransient<IGetCurrentAccountService, GetCurrentAccountService>();
builder.Services.AddScoped<ICreateCurrentAccountService, CreateCurrentAccountService>();
builder.Services.AddScoped<IUpdateCurrentAccountService, UpdateCurrentAccountService>();
builder.Services.AddScoped<IDeleteCurrentAccountService, DeleteCurrentAccountService>();
builder.Services.AddTransient<IGetMovementService, GetMovementService>();
builder.Services.AddScoped<ICreateMovementService, CreateMovementService>();
builder.Services.AddScoped<IUpdateMovementService, UpdateMovementService>();
builder.Services.AddScoped<IDeleteMovementService, DeleteMovementService>();

builder.Services.AddScoped<IClientTypeRepository, ClientTypeRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ICurrentAccountRepository, CurrentAccountRepository>();
builder.Services.AddScoped<IMovementsRepository, MovementsRepository>();

builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();

#region Claims
foreach (var claim in ClaimsStore.RoleCLaims)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(claim.Type.Replace(" ", ""), policy =>
            policy.RequireClaim(claim.Type, claim.Value));
}

foreach (var claim in ClaimsStore.UserClaims)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(claim.Type.Replace(" ", ""), policy =>
            policy.RequireClaim(claim.Type, claim.Value));
}

foreach (var claim in ClaimsStore.MovementClaims)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(claim.Type.Replace(" ", ""), policy =>
            policy.RequireClaim(claim.Type, claim.Value));
}

foreach (var claim in ClaimsStore.ClientClaims)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(claim.Type.Replace(" ", ""), policy =>
            policy.RequireClaim(claim.Type, claim.Value));
}

foreach (var claim in ClaimsStore.AccountClaims)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(claim.Type.Replace(" ", ""), policy =>
            policy.RequireClaim(claim.Type, claim.Value));
}
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

    app.Run();

static void AddDotEnvFile(IConfigurationBuilder configurationBuilder, string filePath)
{
    if (!File.Exists(filePath))
    {
        return;
    }

    var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    foreach (var rawLine in File.ReadAllLines(filePath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        values[key.Replace("__", ":")] = line[(separatorIndex + 1)..].Trim();
    }

    configurationBuilder.AddInMemoryCollection(values);
}

static string ResolveDotEnvPath(IHostEnvironment environment)
{
    var configuredPath = Environment.GetEnvironmentVariable("CUENTASCORRIENTES_DOTENV_PATH");
    if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
    {
        return configuredPath;
    }

    var candidatePaths = new[]
    {
        Path.Combine(environment.ContentRootPath, ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), ".env")
    };

    foreach (var candidatePath in candidatePaths)
    {
        if (File.Exists(candidatePath))
        {
            return candidatePath;
        }
    }

    return candidatePaths[0];
}
