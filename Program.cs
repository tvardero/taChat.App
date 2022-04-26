global using taChat.App.Models;
global using taChat.App.Models.ViewModels;
global using taChat.App.Repository;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpsRedirection(o => o.HttpsPort = 3001);

builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(
    builder.Configuration.GetConnectionString("data")
));

builder.Services.AddIdentity<User, IdentityRole>(o =>
    {
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase = false;
        o.Password.RequireDigit = false;
        o.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMvc();

var app = builder.Build();

app.UseStaticFiles();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
    app.UseStatusCodePages();
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: null!,
    pattern: "{action=ViewRoom}",
    defaults: new { controller = "Chat" }
);

app.MapControllerRoute
(
    name: null!,
    pattern: "{controller}/{action}"
);

app.Run();
