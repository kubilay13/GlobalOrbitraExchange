using Binance.Net.Clients;
using BlockChainAI;
using GlobalOrbitra.Db;
using GlobalOrbitra.Services;
using GlobalOrbitra.Services.WalletService;
using GlobalOrbitra.Services.WalletService.WalletBackgroundService;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/LoginSignUp/Login";
        options.AccessDeniedPath = "/LoginSignUp/AccessDenied";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Binance Services
builder.Services.AddScoped<BinanceServices>();
builder.Services.AddSingleton<BinanceSocketClient>();
builder.Services.AddSingleton<BinanceRestClient>();

// Wallet Services
builder.Services.AddScoped<WalletService>();

// ETH Wallet Services
builder.Services.AddScoped<EthWalletListenerService>();
builder.Services.AddHostedService<EthWalletBackgroundService>();

// TRON Wallet Services
builder.Services.AddScoped<TronWalletListenerService>();
builder.Services.AddHostedService<TronWalletBackgroundService>();

// Mail Service
builder.Services.AddSingleton(new GlobalOrbitra.Services.MailService.GmailMailService(
    "akdogankubilay431@gmail.com", // Mail Address
    "aqrj wiuc hkzv jdha" // Mail App Password
));

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Binance Service baþlatma
using (var scope = app.Services.CreateScope())
{
    try
    {
        var binanceService = scope.ServiceProvider.GetRequiredService<BinanceServices>();
        await binanceService.SubscribeToPriceUpdatesAsync();
        Console.WriteLine("Binance service baþarýyla baþlatýldý.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Binance service baþlatma hatasý: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

// SignalR Hubs
app.MapHub<PriceUpdateHub>("/priceUpdateHub");
app.MapHub<PriceUpdateHub>("/priceUpdateFuturesAmount");

app.Run();