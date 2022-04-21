var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpsRedirection(o => o.HttpsPort = 3001);

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
