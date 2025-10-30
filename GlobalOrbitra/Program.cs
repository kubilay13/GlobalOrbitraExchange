using Binance.Net.Clients;
using BlockChainAI;
using GlobalOrbitra.Db;
using GlobalOrbitra.Services;
using GlobalOrbitra.Services.WalletService;
using GlobalOrbitra.Services.WalletService.WalletBackgroundService;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/LoginSignUp/Login";
        options.AccessDeniedPath = "/LoginSignUp/AccessDenied";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BinanceServices>();
builder.Services.AddSingleton<BinanceSocketClient>();
builder.Services.AddSingleton<BinanceRestClient>();

builder.Services.AddScoped<WalletService>();

builder.Services.AddScoped<EthWalletListenerService>();
builder.Services.AddHostedService<EthWalletBackgroundService>();

builder.Services.AddScoped<TronWalletListenerService>();
builder.Services.AddHostedService<TronWalletBackgroundService>();


builder.Services.AddSingleton(new GlobalOrbitra.Services.MailService.GmailMailService(
    "akdogankubilay431@gmail.com", // Mail Address
    "" // Mail App Password
));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var binanceService = scope.ServiceProvider.GetRequiredService<BinanceServices>();
    await binanceService.SubscribeToPriceUpdatesAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<PriceUpdateHub>("/priceUpdateHub");
app.MapHub<PriceUpdateHub>("/priceUpdateFuturesAmount");

app.Run();
