using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("localConnection"))).AddDefaultIdentity<IdentityUser>(options =>
   {
       options.SignIn.RequireConfirmedAccount = false;
       options.Password.RequireDigit = true;
       options.Password.RequireLowercase = true;
       options.Password.RequireNonAlphanumeric = false;
       options.Password.RequireUppercase = false;
       options.Password.RequiredLength = 6;
       options.Password.RequiredUniqueChars = 0;
   }).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
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
