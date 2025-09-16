# GlobalOrbitraExchange

**GlobalOrbitraExchange**, **.NET ile geliştirilen ve çoklu blokzincir desteği sunan bir kripto para borsası altyapısıdır**.  
Bitcoin, Ethereum, Binance Smart Chain, Solana ve Tron ağlarında işlem yapmayı hedeflemektedir. Proje hâlen yapım aşamasındadır ve güvenli, hızlı bir kullanıcı deneyimi sunmayı amaçlamaktadır.

![Screenshot_5](https://github.com/user-attachments/assets/4bec5cb5-c34b-410b-90a7-ece67e848350)
<img width="1599" height="683" alt="Screenshot_9" src="https://github.com/user-attachments/assets/63982db2-4f90-4dee-b5d7-a1cf5f338c85" />
<img width="1599" height="540" alt="Screenshot_8" src="https://github.com/user-attachments/assets/54cbeff7-d6f9-48f1-b3e9-fc14bd3301aa" />
<img width="1599" height="622" alt="Screenshot_7" src="https://github.com/user-attachments/assets/153a5a47-0f67-4029-b1c5-d3e9b05d4a72" />
<img width="1599" height="610" alt="Screenshot_6" src="https://github.com/user-attachments/assets/ab7e5486-6e93-4e64-bac0-84223c8d87ef" />
![Screenshot_3](https://github.com/user-attachments/assets/c7f56a30-d415-40cb-a004-846296902012)

## Özellikler
- Çoklu blokzincir desteği (BTC, ETH, BSC, SOL, TRX)
- Kullanıcı ve işlem yönetimi
- .NET + Entity Framework tabanlı veri tabanı yönetimi
- Güvenli ve hızlı API altyapısı

## Kurulum
1. Depoyu klonlayın:
   ```bash
   git clone https://github.com/kubilay13/GlobalOrbitraExchange.git
Gerekli paketleri yükleyin:

# Gerekli paketleri yükleyin
dotnet restore

# Veri tabanını hazırlayın
dotnet ef migrations add InitialCreate
dotnet ef database update

# Sunucuyu başlatın
dotnet run


