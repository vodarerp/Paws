# 🐾 Platforma za Ljubimce — Strategija Implementacije

> **Verzija:** 1.0  
> **Datum:** Mart 2026  
> **Tim:** Solo developer + Claude ekosistem  
> **Stack:** ASP.NET Core · React · React Native · SQL Server · Azure

---

## 1. Ključni Principi

### Vertikalni slojevi, ne horizontalni

Ne raditi: ceo API → ceo UI. Raditi: feature po feature, od DB-a do ekrana.

**Zašto:** Horizontalni pristup znači meseci bez ijedne funkcionalne stvari za testiranje. Vertikalni pristup daje prvu živu feature za 1–2 nedelje — i odmah je jasno gde nešto ne štima.

### API uvek jedan korak ispred UI-a

Konkretno: 2 dana na API endpointima, pa paralelno React Web + React Native.  
Ne pisati sve endpointe unapred. Pisati tačno ono što UI treba za taj slice.

### React Native paralelno sa React Web-om

Deljeni API layer i TypeScript interfejsi čine ovo realističnim. Ako se mobile ostavi za kraj, kasno se otkriva da neke API odluke ne rade za mobilni UX.

---

## 2. Redosled Implementacije

### Faza 0 — Infrastruktura (dan 1–2)

Jedina faza gde se ne radi feature. Postavljaju se šine za sve posle.

**Redosled:**

1. **Clean Architecture solution struktura** — odmah, ne naknadno
   ```
   src/
   ├── Domain/
   ├── Application/
   ├── Infrastructure/
   └── API/
   ```

2. **SQL Server lokalno + EF Core setup** — prva migracija kreira samo `Users` tabelu. Ništa više. Verifikovati da migracija radi.

3. **ASP.NET Identity + JWT skeleton** — `POST /auth/register` i `POST /auth/login` koji vraćaju token. Potrebno za svaki sledeći endpoint.

4. **GitHub Actions → Azure App Service** — CI/CD od prvog dana. Eliminiše "radi lokalno, ne radi na serveru" situacije.

5. **Swagger/OpenAPI** — automatski od prvog kontrolera, ostaje tokom celog razvoja.

---

### Faza 1 — Vertikalni Slice: Oglasi + Auth (nedelja 1–2)

Srce Faze 0 produkta.

| Dan | Šta se radi |
|-----|-------------|
| 1–2 | DB migracije (`Posts`, `Pets` osnovno) · Domain entiteti · EF konfiguracija |
| 2–3 | `PostsController` CRUD: `GET /posts` (feed), `POST /posts`, `GET /posts/{id}` |
| 3–4 | React Web — Feed stranica + Create Post forma |
| 3–4 | React Native — isti Feed (paralelno) |
| 5   | Integracija · Kategorije (Udomljavanje/Izgubljen/Nađen) · Image upload |

**Image upload — od dana 1 ide na Azure Blob Storage, nikad Base64 u bazi.**  
Detalji u sekciji 4.

---

### Faza 2 — Vertikalni Slice: Amber Alert + Push (nedelja 3)

Ključni diferenciator platforme — prioritet odmah posle bazičnih oglasa.

| Redosled | Šta se radi | Zašto taj redosled |
|----------|------------|-------------------|
| 1. | Geo query logika (SQL Server `geography` tip, spatial index) | Testirati kroz Swagger pre UI-a |
| 2. | FCM integracija + background job za push | Push za offline korisnike važniji od real-time za online |
| 3. | SignalR `AlertHub` | Dolazi posle FCM-a |
| 4. | UI — "Izgubljen" forma, vizuelno istaknute kartice, "Vidim ovog psa" dugme | |

---

### Faza 3 — Vertikalni Slice: In-app Chat (nedelja 4)

| Šta | Napomena |
|-----|----------|
| SignalR `ChatHub` | Typing indicators uključeni |
| `ChatConversations` + `ChatMessages` tabele | Standardni messaging model |
| Chat UI (Web + Mobile) | |
| Phone number share flow | Eksplicitno dugme "Podeli broj" — bezbednost i UX, ne ostavljati za naknadno |

---

### Faza 4 — Vertikalni Slice: Trust Score + Moderacija (nedelja 5)

U Fazi 0, Trust Score je "Lite" — akumulira se ali ne gate-uje ništa. Implementacija je relativno jednostavna.

- `TrustScoreHistory` tabela + `CurrentScore` na `Users`
- Hangfire background job za procesiranje akcija i dodeljivanje poena
- Keyword filter za oglase (Regex + lista: "prodajem", "cena", "dinara"...)
- Community report flow (3 prijave → auto-skrivanje oglasa)
- Minimalni admin dashboard — flagovane objave + akcije

---

### Faza 5 — Onboarding + UX Polish (dan 3–4)

- Onboarding kviz (3 pitanja → personalizovan feed)
- Profili ljubimaca (osnovna kartica, dropdown za rasu — ne AI u Fazi 0)
- Notification settings (2–3 toggle-a)
- UX review svih flow-ova

---

### Zatvorena Beta — Novi Sad (4–8 nedelja)

- 3–5 udruženja + najaktivniji članovi FB grupa
- Popuniti feed realnim oglasima pre javnog lansiranja
- Bug fixing na osnovu stvarne upotrebe
- Pratiti KPI metrike (videti ProjectPlan v3, sekcija 12)

---

## 3. Clean Architecture — Gde Šta Ide

```
src/
├── Domain/                          # Entiteti, enumi, business rules
│   ├── Entities/                    # Post, User, Pet, AmberAlert...
│   └── Enums/                       # PostCategory, AlertStatus, UserRole...
│   ⚠️  Nema zavisnosti ka spolja
│   ⚠️  Nema interfejsa ka infrastrukturi ovde
│
├── Application/                     # Use cases, CQRS handlers
│   ├── Interfaces/                  # ← interfejsi za Infrastructure servise
│   │   ├── IBlobStorageService.cs
│   │   ├── INotificationService.cs
│   │   └── IImageProcessingService.cs
│   ├── Posts/Commands/              # CreatePostCommand, handler...
│   ├── Posts/Queries/               # GetFeedQuery, handler...
│   ├── DTOs/
│   └── Validators/                  # FluentValidation
│
├── Infrastructure/                  # Implementacije interfejsa
│   ├── Persistence/                 # EF Core DbContext, Migrations
│   ├── Services/
│   │   ├── BlobStorageService.cs    # implementira IBlobStorageService
│   │   ├── NotificationService.cs
│   │   └── ImageProcessingService.cs
│   └── Identity/                    # ASP.NET Identity, JWT
│
└── API/
    ├── Controllers/
    ├── Hubs/                        # ChatHub, AlertHub
    ├── Middleware/
    └── Program.cs                   # DI registracija
```

### Pravilo: Interfejs u Application, Implementacija u Infrastructure

```csharp
// Application/Interfaces/IBlobStorageService.cs
public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream stream, string fileName, string contentType);
    Task DeleteImageAsync(string imageUrl);
}

// Infrastructure/Services/BlobStorageService.cs
public class BlobStorageService : IBlobStorageService
{
    // Azure SDK pozivi
}

// API/Program.cs — jedino mesto gde se spaja interfejs sa konkretnom klasom
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
```

**Prednost:** Handler-i u Application sloju se unit testuju bez Azure connection string-a ili mrežnih poziva — mock-uje se interfejs.

---

## 4. Image Upload — Blob Storage od Dana 1

### Zašto ne Base64 u bazi

| Problem | Posledica |
|---------|-----------|
| Base64 povećava veličinu fajla za ~33% | 2MB slika = 2.7MB u SQL-u |
| `SELECT * FROM Posts` vuče sve slike | Feed query eksponencijalno spor |
| SQL Server nije napravljen za binarne podatke | Fragmentovani pages, problemi sa EF Core |
| Migracija naknadno je bolna | Script + deploy sa zero-downtime na produkcijskim podacima |

### Flow

```
Klijent (multipart/form-data)
    → .NET API
        1. Validacija tipa fajla (samo JPG/PNG/WebP, max 10MB)
        2. Strip EXIF metapodataka (GPS lokacija u svakoj telefon fotografiji!)
        3. Resize ako > 1920px širina
        4. Konvertuj u WebP (manji fajlovi, isti kvalitet)
    → Azure Blob Storage (upload stream)
        ← vraća URL
    → SQL Server (čuva samo URL string)
    → Klijent ({ imageUrl: "https://..." })
```

### MediaUrls u bazi — JSON kolona

```csharp
// Post entitet
public List<string> MediaUrls { get; set; } = new();

// EF Core konfiguracija
builder.Property(p => p.MediaUrls)
    .HasColumnType("nvarchar(max)")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new()
    );
```

### Azure Blob Setup (15 minuta)

```bash
az storage account create \
  --name petplatformastorage \
  --resource-group pet-platform-rg \
  --location westeurope \
  --sku Standard_LRS

az storage container create \
  --name pet-images \
  --account-name petplatformastorage \
  --public-access blob
```

Za lokalni razvoj: **Azurite** — emulator koji ne zahteva Azure nalog.

---

## 5. Šta Se NE Radi u Fazi 0

| Feature | Razlog |
|---------|--------|
| Python AI mikroservis | Dropdown za rasu je dovoljan, štedi 2–3 nedelje |
| Trust Score gate-ovi | Score se akumulira, ali ne blokira ništa do Faze 1 |
| Foster lista (strukturirana) | Faza 1 |
| QR verifikacija primopredaje | Faza 1 |
| Follow-up sistem | Faza 1 |
| Veterinarski karton | Faza 1 |
| Prijava zlostavljanja sa PDF | Faza 1 |
| Donacije/kampanje | Faza 1 |
| Progresivni Amber Alert radijus | Faza 1 |
| Social feed "Zajednica" tab | Faza 1.5, uslov: 500+ aktivnih korisnika |
| Monetizacija | Nije prioritet u Fazi 0 |

---

## 6. Generalni Principi Kroz Sve Faze

**Cursor-based pagination na feed-u odmah** — offset pagination je teško promeniti naknadno sa stvarnim podacima.

**Shared TypeScript tipovi** između React Web i React Native od prvog dana. Definisati API response interfejse jednom, uvesti u oba projekta.

**Swagger od dana 1** — svaki endpoint dokumentovan odmah. Pomaže za testiranje i za vlastiti sanity check.

**Konzistentna error struktura od prvog endpointa:**
```json
{
  "error": {
    "code": "ALERT_LIMIT_EXCEEDED",
    "message": "Već imate aktivan Amber Alert u poslednjih 24h.",
    "details": {}
  }
}
```

**Verzionisanje API-a:** `/api/v1/` — od prvog dana.

**Nikad binary u SQL-u** — slike, videi, PDF-ovi idu na Blob Storage. U bazi samo URL-ovi.

---

## 7. Vremenski Plan (Aproksimacija)

| Period | Šta |
|--------|-----|
| Dan 1–2 | Faza 0 — Infrastruktura |
| Nedelja 1–2 | Vertikalni Slice 1 — Oglasi + Auth + Image Upload |
| Nedelja 3 | Vertikalni Slice 2 — Amber Alert + Push |
| Nedelja 4 | Vertikalni Slice 3 — In-app Chat |
| Nedelja 5 | Vertikalni Slice 4 — Trust Score + Moderacija |
| Dan 3–4 (nedelja 6) | Vertikalni Slice 5 — Onboarding + UX Polish |
| Nedelja 7–14 | Zatvorena beta — Novi Sad |

**Ukupno do zatvorene bete: ~6 nedelja** solidnog rada.

---

## 8. Sledeći Koraci

1. Kreirati GitHub repo i solution strukturu
2. Podesiti Azure resurse (App Service, SQL Database, Blob Storage)
3. Implementirati Fazu 0 (infrastruktura + CI/CD)
4. Početi Vertikalni Slice 1 — `Users` + `Posts` DB migracije
