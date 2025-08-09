using Binance.Net.Clients;
using BlockChainAI;
using GlobalOrbitra.Services;
using GlobalOrbitra.Services.WalletService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BinanceServices>();
builder.Services.AddSingleton<BinanceSocketClient>();
builder.Services.AddSingleton<BinanceRestClient>();

builder.Services.AddScoped<TronWalletService>();
builder.Services.AddScoped<EthWalletService>();
builder.Services.AddScoped<BscWalletService>();
builder.Services.AddScoped<SolWalletService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<PriceUpdateHub>("/priceUpdateHub");
app.MapHub<PriceUpdateHub>("/priceUpdateFuturesAmount");

app.Run();
