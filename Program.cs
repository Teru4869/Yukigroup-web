using Yukigroup_WEB.Services;

Console.WriteLine("=== Program.cs äJén ===");
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AccountingSheetService>();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();
Console.WriteLine("=== builder.Build() äÆóπ ===");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

Console.WriteLine("=== app.Run() é¿çs ===");
app.Run();

builder.Logging.AddConsole();