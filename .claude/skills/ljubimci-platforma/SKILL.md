---
name: ljubimci-platforma
description: >
  Projektni skill za Platformu za Ljubimce (Srbija) — centralizovana platforma za udomljavanje,
  pronalaženje izgubljenih ljubimaca i zaštitu životinja. Koristi ovaj skill na početku SVAKE
  sesije pre generisanja bilo kakvog koda, arhitekturnih odluka, ili dizajna feature-a.
  Sadrži domenski model, business rules, CQRS konvencije, DB šemu i fazni scope.
  Obavezno pročitaj pre rada na: entitetima, API endpointima, Trust Score logici,
  Amber Alert sistemu, moderaciji, notifikacijama, ili bilo kojoj komponenti platforme.
---

# Platforma za Ljubimce — Projektni Skill

## VAŽNO: Kako koristiti ovaj skill

Na početku svake sesije:
1. Pročitaj ovaj fajl u celosti
2. Učitaj relevantne reference fajlove na osnovu oblasti rada:
   - `references/domain-model.md` — entiteti, enumi, value objects, domain events
   - `references/business-rules.md` — Trust Score, Amber Alert, moderacija pravila
   - `references/cqrs-conventions.md` — CQRS pattern, FluentValidation, kod konvencije
   - `references/db-schema.md` — tabele, relacije, geo upiti, indexing
3. Potvrdi korisniku: "Skill učitan, radim na [oblast]. Faza 0 scope je aktivan."

---

## 1. Projekat Overview

**Šta:** Centralizovana digitalna platforma (Web + Mobile) za udomljavanje ljubimaca,
pronalaženje izgubljenih ljubimaca i zaštitu životinja u Srbiji.

**Ključni diferenciator:** Amber Alert sistem — push notifikacije bazirane na lokaciji
kada se prijavi nestanak ljubimca. Nešto što FB/Viber grupe ne mogu da ponude.

**Tim:** Solo developer + Claude ekosistem
**Grad lansiranja:** Novi Sad (zatvorena beta → meki launch → širenje)
**Status:** Implementacija Faze 0

---

## 2. Tech Stack

| Sloj | Tehnologija |
|---|---|
| Backend API | ASP.NET Core Web API (C#, .NET 8) |
| Frontend Web | React + Vite + TypeScript |
| Frontend Mobile | React Native (iOS + Android) |
| Baza podataka | SQL Server (Azure SQL) |
| AI/ML Servis | Python + FastAPI (od Faze 1.5) |
| Cloud | Microsoft Azure |
| Real-time | SignalR (Azure SignalR Service) |
| Push notifikacije | Firebase Cloud Messaging (FCM) |
| Background jobs | Hangfire |
| State management | Zustand + TanStack Query |

**Faza 0 stack (bez Python servisa):**
`.NET API + React + React Native + SQL Server + Azure + FCM + SignalR`

**Od Faze 1.5 se dodaje:**
`Python FastAPI + Docker + Azure Container Instance`

---

## 3. Arhitektura — Clean Architecture

```
src/
├── Domain/
│   ├── Common/          # BaseEntity, IDomainEvent, ValueObject base
│   ├── Entities/        # Svi domenski entiteti
│   ├── Enums/           # Svi enumi
│   ├── ValueObjects/    # Record structs
│   ├── Events/          # MediatR INotification eventi
│   ├── Exceptions/      # DomainException hijerarhija
│   └── Interfaces/      # IRepository<T>, servis interfejsi
│
├── Application/
│   ├── Common/          # BaseCommand, BaseQuery, IPipelineBehavior
│   ├── Commands/        # Po feature-u: CreatePost/, ActivateAlert/...
│   ├── Queries/         # Po feature-u: GetFeed/, GetFosterMatches/...
│   ├── DTOs/            # Response/Request DTO-ovi
│   ├── Validators/      # FluentValidation validator klase
│   └── Behaviors/       # ValidationBehavior, LoggingBehavior
│
├── Infrastructure/
│   ├── Persistence/     # EF Core DbContext, Migrations, Repositories
│   ├── Services/        # NotificationService, ImageHashingService...
│   ├── Identity/        # ASP.NET Identity, JWT
│   ├── BackgroundJobs/  # Hangfire job definicije
│   └── ExternalApis/    # FCM klijent, Azure Blob, AI servis klijent
│
└── API/
    ├── Controllers/     # REST kontroleri
    ├── Hubs/            # SignalR: ChatHub, AlertHub, FeedHub
    ├── Middleware/       # RateLimiting, ErrorHandling, RequestLogging
    └── Filters/         # AuthorizationFilters
```

> **Detalji domenskog modela:** `references/domain-model.md`
> **Detalji CQRS konvencija:** `references/cqrs-conventions.md`

---

## 4. Fazni Scope — Brza Referenca

### Faza 0 — IMPLEMENTIRATI

| Feature | Napomena |
|---|---|
| Oglasna tabla (3 kategorije) | Udomljavanje, Izgubljen, Nađen |
| Integrisani Amber Alert | "Izgubljen" objava automatski push 10km radijus |
| Dugme "Vidim ovog psa" | Mini-formular sa lokacijom, šalje notifikaciju vlasniku |
| Profili korisnika | Dvoslojni sistem lokacije (opština + opcioni GPS) |
| Profili ljubimaca | Osnovna kartica, dropdown za rasu (30-40 rasa) |
| In-app chat | Bez otkrivanja broja telefona |
| Onboarding kviz | 3 pitanja, personalizuje feed |
| Trust Score Lite | Vidljiv, akumulira se, NE gate-uje ništa |
| Push notifikacije | FCM: Amber Alert + chat + status promene |
| Automatska moderacija | Obavezna polja, rate limit, keyword filter, image hash |
| Community reporting | Prijavi objavu, auto-skrivanje na 3 prijave |
| Admin dashboard | Minimalni: flagovi, prijave, osnovne brojke |
| Notifikacione kontrole | 2-3 toggle-a u settings |

### Faza 0 — NE IMPLEMENTIRATI

- AI prepoznavanje rasa (dropdown je dovoljan)
- Python AI mikroservis
- Trust Score gate-ovi (score se akumulira ali ne blokira)
- Foster lista strukturirana, QR verifikacija primopredaje
- Follow-up sistem (1/3/6 meseci), Veterinarski karton
- Prijava zlostavljanja sa PDF, Donacije/kampanje
- Progresivni Amber Alert radijus, Admin panel za udruženja
- Monetizacija, Social feed "Zajednica" tab

### Faza 1 — Sledeće

Trust Score gate-ovi, Foster lista, QR verifikacija primopredaje, Follow-up sistem,
Veterinarski karton, PDF prijave zlostavljanja, Donacije sa transparentnošću,
Progresivni Amber Alert radijus (5→10→20km), Admin panel za udruženja,
Rule-based Nađen↔Izgubljen matching, Pametni push za "Nađen".

### Faza 1.5 (uslov: 500+ MAU)

AI prepoznavanje rasa (Python FastAPI + ResNet/EfficientNet),
AI-bazirani matching za "Nađen↔Izgubljen", Social "Zajednica" feed tab.

### Faza 3

AI Detektiv (facial recognition za pse), AI Matchmaker (ML scoring),
Integracija sa državnim službama, Affiliate program, Crna lista deljenja.

> **Detalji business rules:** `references/business-rules.md`
> **Detalji DB šeme:** `references/db-schema.md`

---

## 5. Naming Conventions

| Kontekst | Konvencija | Primer |
|---|---|---|
| C# klase/metode | PascalCase | `TrustScoreService`, `ActivateAmberAlert` |
| C# privatna polja | _camelCase | `_repository`, `_notificationService` |
| C# lokalne var | camelCase | `userId`, `alertRadius` |
| DB tabele | PascalCase plural | `AmberAlerts`, `TrustScoreHistory` |
| DB kolone | PascalCase | `LocationZone`, `LastKnownLatLng` |
| API endpointi | kebab-case | `/api/v1/amber-alerts` |
| API verzija | Uvek v1 prefix | `/api/v1/...` |
| UI tekstovi | Srpski | "Izgubljen pas", "Pouzdan korisnik" |
| Kod identifikatori | Engleski | `PostCategory.Lost`, `UserRole.Foster` |
| Enum vrednosti | PascalCase | `AlertStatus.Active`, `PostCategory.Lost` |

---

## 6. Kod Stil — Pravila za Generisanje Koda

### Komentari

Pri generisanju bilo kog C# koda za ovaj projekat:

**Nikad ne dodavati:**
- XML `/// <summary>` tagove — ni na klasama, ni na metodama, ni na propertyima
- Inline komentare koji opisuju šta kod radi (`// Kreira korisnika`, `// Vraća rezultat`)
- Komentare koji ponavljaju ime metode ili parametra

**Dodati samo kada:**
- Komentar objašnjava **zašto**, ne šta — neočigledno biznis pravilo
- `// NIKAD` — bezbednosno ili privacy upozorenje
- `// Faza X` — kad nešto namerno ostavljamo za kasniju fazu
- `// TODO:` — konkretna stavka za budući rad

**Primer ispravnog outputa:**
```csharp
public void ApplyTrustScoreChange(TrustScoreAction action)
{
    var before = TrustScore;
    var points = TrustScoreCalculator.GetPoints(action);
    // Score ne može biti negativan
    TrustScore = points > 0 ? TrustScore.Add(points) : TrustScore.Subtract(Math.Abs(points));

    if (before.Category != TrustScore.Category)
        AddDomainEvent(new TrustScoreCategoryChangedEvent(Id, before.Category, TrustScore.Category));
}

public void MarkAsLost() { Status = PetStatus.Lost; SetUpdated(); }
public void MarkAsFound() { Status = PetStatus.WithOwner; SetUpdated(); }
```

---

## 7. Sedam Kritičnih Pravila

1. **Amber Alert = Integrisani model** — nema zasebnog alert sistema. "Izgubljen" objava automatski aktivira push notifikaciju. Nikada ne implementirati kao poseban flow.

2. **Lokacija = nikada tačna adresa** — samo `LocationZone` (opština/naselje) u profilu korisnika. GPS koordinate (`LastKnownLatLng`) se čuvaju ali nikad ne prikazuju drugim korisnicima.

3. **Telefon = skriven do eksplicitnog odobrenja** — in-app chat ne otkriva broj dok korisnik ne klikne "Podeli broj". `PhoneShared` boolean na konverzaciji.

4. **Trust Score u Fazi 0 = dekorativni** — vidljiv, akumulira se, ali ne blokira nijednu akciju. Gate-ovi dolaze u Fazi 1.

5. **Moderacija = troslojna, nikada pre-moderacija** — auto filteri → community report (3 prijave = auto-hide) → admin pregled. Nikad ne čekati manuelni odobrenje pre objavljivanja.

6. **Registracija za "Izgubljen" = minimalna** — korisnik je u panici. Obavezna polja: email + ime. Sve ostalo popunjava naknadno.

7. **Feed prioriteti su apsolutni** — "Izgubljen" objave uvek na vrhu feed-a, bez izuzetka. Hitni oglasi nikad ne tonu ispod regularnih.
