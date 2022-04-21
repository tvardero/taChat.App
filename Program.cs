global using taChat.App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using taChat.App.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpsRedirection(o => o.HttpsPort = 3001);

builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(
    builder.Configuration.GetConnectionString("data")
));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMvc();

var app = builder.Build();

app.UseStaticFiles();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
    app.UseDeveloperExceptionPage();
}

app.MapControllerRoute(
    name: null!,
    pattern: "{controller=Home}/{action=Index}"
);

app.Run();
