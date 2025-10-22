# GlobalOrbitraExchange

**GlobalOrbitraExchange**, **.NET ile geliştirilen ve çoklu blokzincir desteği sunan bir kripto para borsası altyapısıdır**.  
Bitcoin, Ethereum, Binance Smart Chain, Solana ve Tron ağlarında işlem yapmayı hedeflemektedir. Proje hâlen yapım aşamasındadır ve güvenli, hızlı bir kullanıcı deneyimi sunmayı amaçlamaktadır.

<img width="1918" height="859" alt="Screenshot_2" src="https://github.com/user-attachments/assets/bc5e89de-8756-491e-ba27-9f373701663d" />
<img width="1599" height="683" alt="Screenshot_9" src="https://github.com/user-attachments/assets/63982db2-4f90-4dee-b5d7-a1cf5f338c85" />
<img width="1919" height="837" alt="Screenshot_3" src="https://github.com/user-attachments/assets/3d229676-bb93-46d6-80fa-4c8b3a5b9beb" />
<img width="1919" height="801" alt="Screenshot_4" src="https://github.com/user-attachments/assets/4b824af9-f955-42dc-b052-5c3816de5490" />
<img width="1918" height="835" alt="Screenshot_5" src="https://github.com/user-attachments/assets/014fbc48-0b15-4eba-a0cc-fdaa93236f46" />


## Özellikler
- Çoklu blokzincir desteği 
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


