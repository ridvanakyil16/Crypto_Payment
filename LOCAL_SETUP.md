# Crypto Payment - Local Development Setup

Bu kılavuz, Crypto Payment projesini local bilgisayarınızda çalıştırmak için gerekli adımları içerir.

---

## 📋 Gereksinimler

### Yazılımlar
- **.NET 8.0 SDK** - [İndir](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** veya **Rider** veya **VS Code**
- **Git** - [İndir](https://git-scm.com/)

### Opsiyonel
- **SQL Server Management Studio** (MSSQL kullanıyorsanız)
- **DB Browser for SQLite** (SQLite kullanıyorsanız)

---

## 🚀 Kurulum Adımları

### 1. Repoyu Klonlayın

```bash
git clone https://github.com/ridvanakyil16/Crypto_Payment.git
cd Crypto_Payment
```

### 2. Local Development Branch'ine Geçin

```bash
git checkout local-development
```

> ⚠️ **ÖNEMLİ:** `local-development` branch'i local çalışma için optimize edilmiştir. `main` branch Heroku production içindir.

---

### 3. Database Seçimi

Proje otomatik olarak environment'a göre database seçer:

#### A. SQLite (Önerilen - Kolay)

**Hiçbir şey yapmanıza gerek yok!** Proje otomatik olarak SQLite kullanacak.

```bash
# DATABASE_URL environment variable yoksa SQLite kullanılır
# Database dosyası: Crypto_Payment/invoice.db
```

**Avantajlar:**
- ✅ Kurulum gerektirmez
- ✅ Dosya tabanlı
- ✅ Hafif ve hızlı

**Dezavantajlar:**
- ❌ Production'da kullanılmaz (Heroku PostgreSQL kullanır)

---

#### B. SQL Server (Opsiyonel)

Eğer SQL Server kullanmak isterseniz:

**1. appsettings.Development.json'u düzenleyin:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InvoiceDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**2. Program.cs'i düzenleyin:**

`UseSqlite` yerine `UseSqlServer` kullanın:

```csharp
else
{
    // Local SQL Server
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseSqlServer(connectionString);
    Console.WriteLine("[DATABASE] Using SQL Server (Local Development)");
}
```

---

### 4. NuGet Paketlerini Yükleyin

```bash
cd Crypto_Payment
dotnet restore
```

---

### 5. Database Migration

#### SQLite İçin:

```bash
dotnet ef database update
```

#### SQL Server İçin:

```bash
# Migration oluştur (ilk kez)
dotnet ef migrations add InitialCreate

# Database'i oluştur
dotnet ef database update
```

> 💡 **Not:** Eğer migration hatası alırsanız, önce `dotnet tool install --global dotnet-ef` komutunu çalıştırın.

---

### 6. Projeyi Çalıştırın

#### Visual Studio:
- `Crypto_Payment.sln` dosyasını açın
- F5 veya "Start Debugging" butonuna basın

#### VS Code / Terminal:
```bash
cd Crypto_Payment
dotnet run
```

#### Rider:
- Projeyi açın
- Run butonuna basın

---

### 7. Tarayıcıda Açın

Proje çalıştığında otomatik olarak tarayıcı açılacak:

```
https://localhost:5001
```

veya

```
http://localhost:5000
```

---

## 🔧 Yapılandırma

### appsettings.Development.json

Local development için tüm ayarlar burada:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=invoice.db"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "ridvanakyil16@gmail.com",
    "Pass": "ghhngdawlscbrkxt",
    "From": "ridvanakyil16@gmail.com",
    "DisplayName": "Crypto Payment - Local Dev"
  },
  "Plisio": {
    "ApiKey": "8cTh9ysB5sGvUd6UfX0heH2VGRNpmKrsScW_L54Qk8T5xrlhvBKuGyI4cmacMu6q",
    "BaseUrl": "https://api.plisio.net/api/v1"
  },
  "AllowedHosts": "*"
}
```

---

## 🧪 Test Etme

### 1. Fatura Oluşturma

1. Ana sayfaya git: `https://localhost:5001`
2. "Fatura Oluştur" butonuna tıkla
3. Formu doldur:
   - Müşteri seçin
   - Para birimi: USDT_TRX veya EURO
   - Tutar girin
   - Email girin
   - **Callback URL:** `https://localhost:5001/api/callback` (otomatik dolu)

4. "Fatura Oluştur" butonuna tıkla

---

### 2. Ödeme Testi

#### A. Test Endpoint ile (Önerilen)

```bash
# Fatura ID'sini al (örnek: 1)
curl "https://localhost:5001/api/callback/test?invoiceId=1&status=completed"
```

**Response:**
```json
{
  "success": true,
  "invoiceId": 1,
  "oldStatus": "pending",
  "newStatus": "completed",
  "message": "Invoice status updated from 'pending' to 'completed'"
}
```

#### B. Gerçek Ödeme ile

1. Fatura oluştur
2. Ödeme sayfasını aç: `https://localhost:5001/pay/1`
3. QR kodu tara veya wallet adresine gönder
4. 3 saniye içinde status güncellenecek

---

## 📁 Proje Yapısı

```
Crypto_Payment/
├── Crypto_Payment/              # Ana proje
│   ├── Controllers/             # API ve MVC controllers
│   ├── Data/                    # DbContext
│   ├── DTOS/                    # Data Transfer Objects
│   ├── Helpers/                 # Helper sınıfları
│   ├── Manager/                 # Business logic
│   ├── Models/                  # Entity models
│   ├── Services/                # Service interfaces
│   ├── Views/                   # Razor views
│   ├── wwwroot/                 # Static files
│   ├── Program.cs               # Entry point
│   ├── appsettings.json         # Production config
│   ├── appsettings.Development.json  # Local config
│   └── invoice.db               # SQLite database (gitignore'da)
├── .gitignore                   # Git ignore rules
├── LOCAL_SETUP.md               # Bu dosya
└── README.md                    # Proje açıklaması
```

---

## 🐛 Sorun Giderme

### 1. "Database update failed" Hatası

**Çözüm:**
```bash
# Migration'ları sıfırla
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### 2. "Port already in use" Hatası

**Çözüm:**
```bash
# Farklı port kullan
dotnet run --urls "https://localhost:5002;http://localhost:5003"
```

veya `launchSettings.json` dosyasını düzenleyin.

---

### 3. "Plisio API Error" Hatası

**Çözüm:**
- API key'i kontrol edin: `appsettings.Development.json`
- Internet bağlantınızı kontrol edin
- Plisio dashboard'da API key'in aktif olduğundan emin olun

---

### 4. Callback Çalışmıyor

**Neden:** Localhost'tan Plisio callback alamazsınız (public URL gerekir).

**Çözüm:**
Test endpoint'ini kullanın:
```bash
curl "https://localhost:5001/api/callback/test?invoiceId=1&status=completed"
```

---

## 🔄 Branch'ler Arası Geçiş

### Local Development'a Geç
```bash
git checkout local-development
```

### Production (Main) Branch'e Geç
```bash
git checkout main
```

> ⚠️ **UYARI:** `main` branch'i Heroku için optimize edilmiştir. Local'de çalışmayabilir.

---

## 📊 Database Yönetimi

### SQLite Database'i Görüntüleme

**DB Browser for SQLite:**
1. [İndir](https://sqlitebrowser.org/)
2. `Crypto_Payment/invoice.db` dosyasını aç
3. Tabloları görüntüle ve düzenle

**VS Code Extension:**
- SQLite Viewer extension'ı yükle
- `invoice.db` dosyasına sağ tıklayıp "Open Database" seçin

---

### Database'i Sıfırlama

```bash
# SQLite
rm Crypto_Payment/invoice.db
dotnet ef database update

# SQL Server
dotnet ef database drop -f
dotnet ef database update
```

---

## 🚀 Production'a Deploy

### Heroku'ya Deploy

**1. Main branch'e geç:**
```bash
git checkout main
```

**2. Değişiklikleri merge et:**
```bash
git merge local-development
```

**3. Push et:**
```bash
git push origin main
```

Heroku otomatik olarak deploy edecek.

---

## 📝 Geliştirme İpuçları

### 1. Hot Reload Kullanın

```bash
dotnet watch run
```

Kod değişikliklerinde otomatik yeniden başlatır.

---

### 2. Logging Seviyesini Artırın

`appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

---

### 3. Browser DevTools

- F12 ile açın
- Console'da `[POLLING]` loglarını kontrol edin
- Network tab'ında API çağrılarını izleyin

---

## 🔐 Güvenlik

### Hassas Bilgiler

**appsettings.Development.json** dosyasında hassas bilgiler var:
- SMTP şifresi
- Plisio API key

> ⚠️ Bu dosyayı **asla** public repo'ya push etmeyin!

`.gitignore` dosyası bunu engelliyor:
```
appsettings.*.json
!appsettings.Development.json
!appsettings.json
```

---

## 📞 Destek

Sorun yaşarsanız:
1. Bu kılavuzu tekrar okuyun
2. GitHub Issues'da arayın
3. Yeni issue açın

---

## ✅ Checklist

Başlamadan önce kontrol edin:

- [ ] .NET 8.0 SDK yüklü
- [ ] Git yüklü
- [ ] Repo klonlandı
- [ ] `local-development` branch'ine geçildi
- [ ] `dotnet restore` çalıştırıldı
- [ ] `dotnet ef database update` çalıştırıldı
- [ ] `dotnet run` çalıştırıldı
- [ ] `https://localhost:5001` açıldı
- [ ] Fatura oluşturuldu
- [ ] Test endpoint çalıştı

---

## 🎉 Başarılı!

Artık Crypto Payment projesini local'de çalıştırabilirsiniz!

**Sonraki Adımlar:**
1. Kodu inceleyin
2. Yeni özellikler ekleyin
3. Test edin
4. Commit edin
5. Pull request açın

Happy coding! 🚀
