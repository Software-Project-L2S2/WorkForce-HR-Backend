using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;
using WorkforceAPI.Services;
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<EmployeeContext>()
    .AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<EmployeeService>();

// CORS
builder.Services.AddCors(options => options.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins("http://localhost:3000")
          .AllowAnyHeader()
          .AllowAnyMethod()));

builder.Services.AddControllers();
var app = builder.Build();

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "HR", "Employee" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run(); app.Run();