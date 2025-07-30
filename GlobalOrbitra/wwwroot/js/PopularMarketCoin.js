
    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".card-title").forEach((el) => {
            el.addEventListener("click", function (e) {
                e.preventDefault();

                const span = this.querySelector(".span");
                if (!span) return;

                const symbolText = span.textContent.trim(); 
                const baseSymbol = symbolText.split("/")[0]; 
                const fullSymbol = baseSymbol + "USDT"; 

                window.location.href = `/CoinMarket/CoinMarket?symbol=${fullSymbol}`;
            });
        });
  });

