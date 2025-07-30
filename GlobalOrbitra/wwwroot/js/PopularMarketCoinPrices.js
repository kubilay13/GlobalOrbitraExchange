const coins = {
    BCHUSDT: { name: "BCH/USDT", logo: "/images/bitcoin-cash-bch-logo.svg" },
    BTCUSDT: { name: "BTC/USDT", logo: "/images/BTC.svg" },
    ETHUSDT: { name: "ETH/USDT", logo: "/images/ETH.svg" },
    BNBUSDT: { name: "BNB/USDT", logo: "/images/BNB.svg" },
    TRXUSDT: { name: "TRX/USDT", logo: "/images/tron-trx-logo.svg" },
    SOLUSDT: { name: "SOL/USDT", logo: "/images/SOLANA.svg" },
    LTCUSDT: { name: "LTC/USDT", logo: "/images/litecoin-ltc-logo.svg" },
    ARBUSDT: { name: "ARB/USDT", logo: "/images/arbitrum-arb-logo.svg" },
    SUIUSDT: { name: "SUI/USDT", logo: "/images/sui-sui-logo.svg" },
    XRPUSDT: { name: "XRP/USDT", logo: "/images/XRP.svg" },
    ADAUSDT: { name: "ADA/USDT", logo: "/images/CARDANO.svg" },
    AVAXUSDT: { name: "AVAX/USDT", logo: "/images/AVALANCE.svg" },
    USDCUSDT: { name: "USDC/USDT", logo: "/images/usd-coin-usdc-logo.svg" },
};

const params = new URLSearchParams(window.location.search);
const symbol = params.get("symbol")?.toUpperCase() || "BTCUSDT";

const coin = coins[symbol];
if (coin) {
    document.getElementById("coin-logo").src = coin.logo;
    document.getElementById("coin-name").innerText = coin.name;
} else {
    document.getElementById("coin-name").innerText = "Bilinmeyen Coin";
}

const priceElement = document.getElementById("price");
const percentageElement = document.getElementById("price-percentage");
let lastPrice = null;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7016/priceUpdateHub")
    .build();

connection.on("ReceivePriceUpdate", (receivedSymbol, price, color, changePercent) => {
    if (receivedSymbol !== symbol) return;

    const newPrice = parseFloat(price);
    const oldPrice = lastPrice ?? newPrice;
    lastPrice = newPrice;

    priceElement.innerText = newPrice.toLocaleString("en-US", { minimumFractionDigits: 2 });

    if (newPrice > oldPrice) {
        priceElement.style.color = "#6fcf97";
    } else if (newPrice < oldPrice) {
        priceElement.style.color = "#eb5757";
    } else {
        priceElement.style.color = "white";
    }

    // Badge görünümü ve renk ayarı
    const sign = changePercent > 0 ? "+" : "";
    const icon = changePercent > 0 ? "▲" : changePercent < 0 ? "▼" : "";

    percentageElement.innerHTML = `${sign}${changePercent.toFixed(2)}% ${icon}`;
    percentageElement.className = "badge"; // önce sınıfları temizle
    percentageElement.classList.add(changePercent >= 0 ? "green" : "red");
});

connection.start().catch(err => console.error(err.toString()));