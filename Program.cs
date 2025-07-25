using CuentasCorrientes.Data;
using CuentasCorrientes.Services;
using CuentasCorrientes.Services.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<LoggerService>();
builder.Services.AddScoped<IGetClientTypeService, GetClientTypeService>();
builder.Services.AddScoped<ICreateClientTypeService, CreateClientTypeService>();
builder.Services.AddScoped<IUpdateClientTypeService, UpdateClientTypeService>();
builder.Services.AddScoped<IClientTypeRepository, ClientTypeRepository>();
builder.Services.AddScoped<IDeleteClientTypeService, DeleteClientTypeService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
