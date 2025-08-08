const priceUpdateConnection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7016/priceUpdateHub")
    .build();

const lastPrices = {};

priceUpdateConnection.on("ReceivePriceUpdate", function (symbol, price, color, priceChangePercentage) {
    const priceElement = document.getElementById(symbol);
    const percentageElement = document.getElementById(`${symbol}-percentage`);

    const newPrice = parseFloat(price);
    const oldPrice = lastPrices[symbol] ?? newPrice;
    lastPrices[symbol] = newPrice;

    // 🔼 Karttaki fiyat güncelleme
    if (priceElement) {
        priceElement.innerText = newPrice.toLocaleString('en-US', { minimumFractionDigits: 2 });

        if (newPrice > oldPrice) {
            priceElement.style.color = "#6fcf97"; // yeşil
        } else if (newPrice < oldPrice) {
            priceElement.style.color = "#eb5757"; // kırmızı
        }

        // font-size küçült (important ile)
        priceElement.style.setProperty('font-size', '14px', 'important');
    }

    // 🔼 Karttaki badge güncelleme
    if (percentageElement) {
        const sign = priceChangePercentage > 0 ? "+" : "";
        const icon =
            priceChangePercentage > 0
                ? '<i class="fas fa-caret-up"></i>'
                : priceChangePercentage < 0
                    ? '<i class="fas fa-caret-down"></i>'
                    : '';

        percentageElement.innerHTML = `${sign}${priceChangePercentage.toFixed(2)}% ${icon}`;
        percentageElement.className = `badge ${priceChangePercentage >= 0 ? "green" : "red"}`;

        // font-size küçült (important ile)
        percentageElement.style.setProperty('font-size', '12px', 'important');
    }

    // 🔼 Tablodaki yüzde hücresi güncelleme (data-symbol ile)
    const tableCell = document.querySelector(`.last-update[data-symbol="${symbol}"]`);
    if (tableCell) {
        const sign = priceChangePercentage > 0 ? "+" : "";
        tableCell.innerText = `${sign}${priceChangePercentage.toFixed(2)}%`;

        tableCell.classList.remove("green", "red");
        tableCell.classList.add(priceChangePercentage >= 0 ? "green" : "red");

        // font-size küçült (important ile)
        tableCell.style.setProperty('font-size', '12px', 'important');
    }
});


priceUpdateConnection.start().catch(err => console.error(err.toString()));
