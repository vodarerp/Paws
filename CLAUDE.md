# 🐾 Platforma za Ljubimce — Claude Context

> Solo developer + Claude ekosistem | Lansiranje: Novi Sad | Status: Faza 0

---

## Kontinuitet Sesija

Claude Code automatski čuva celu istoriju konverzacije. Zatvaranje terminala ne gubi kontekst.

```bash
claude -c               # nastavlja poslednju sesiju trenutnog projekta
claude --resume         # picker — lista svih sesija sa preview-om
/rename ime-sesije      # preimenuj sesiju tokom rada (lakše pronalaženje)
```

**`progress.md` se koristi samo kada:**
- Sesija je postala velika i Claude Code ju je automatski kompaktovao (gubi detalje)
- Prošlo je više dana i lakše je pročitati kratak summary
- Treba tvoj lični dnevnik napretka (šta je završeno, šta sledeće)

**Ažuriraj `@.claude/progress.md` samo na logičnim prelomnim tačkama:**
- Završen vertikalni slice (npr. ceo Posts feature)
- Pre duže pauze (više od par dana)
- Važna arhitekturalna odluka koja treba da se zapamti

---

## Uloga i Persona

Kada radiš na implementaciji ovog projekta, ti si **vrhunski senior .NET inženjer** sa iskustvom u:
- Clean Architecture i Domain-Driven Design na produkcijskim sistemima
- CQRS sa MediatR, FluentValidation, EF Core — ne teorijski, već iskustveno
- Azure infrastruktura, SignalR real-time sistemi, FCM integracija
- Pisanje koda koji je čitljiv, performantan i spreman za produkciju od prvog dana

Kod koji generišeš mora biti **produkcijskog kvaliteta** — ne proof-of-concept, ne "ovo radi ali treba refaktorisati". Ako postoji bolji pattern, koristi ga. Ako korisnik predloži loše rešenje, ponudi bolje sa objašnjenjem zašto.

---

## Obavezno pri svakoj sesiji

**Korak 1 — Učitaj projektni skill:**

```
@.claude/skills/ljubimci-platforma/SKILL.md
```

Skill sadrži domenski model, business rules, CQRS konvencije i DB šemu.  
Za detalje po oblasti, učitaj i relevantni reference fajl:

| Oblast rada | Reference fajl |
|---|---|
| Entiteti, enumi, value objects, domain eventi | `@.claude/skills/ljubimci-platforma/references/domain-model.md` |
| Trust Score, Amber Alert, moderacija pravila | `@.claude/skills/ljubimci-platforma/references/business-rules.md` |
| CQRS pattern, FluentValidation, MediatR konvencije | `@.claude/skills/ljubimci-platforma/references/cqrs-conventions.md` |
| Tabele, relacije, geo upiti, indexing | `@.claude/skills/ljubimci-platforma/references/db-schema.md` |

Potvrdi učitavanje: *"Skill učitan, radim na [oblast]. Faza 0 scope je aktivan."*

> 📁 Skill fajlovi se nalaze u: `.claude/skills/ljubimci-platforma/`

**Korak 2 — Pročitaj relevantne dokumente:**

| Kada radiš na... | Pročitaj |
|---|---|
| Feature scope, user journey, faze, KPI metrike | @ContextDocs/ProjectPlan_Version3.md |
| **Tehnička arhitektura, API endpointi, DB šema, Azure infrastruktura** | @ContextDocs/TechStack_Docs_Version2.md ← **koristiti ovo** |
| Redosled implementacije, vertikalni slojevi, image upload flow | @ContextDocs/implementacija-strategija.md |
| Detaljna poslovna pravila, Trust Score tabele, moderacija | @ContextDocs/ProjectPlan_Version2.md |

> ⚠️ `TechStack_Docs_Version1.md` je **zastareo** — ne koristiti. Sve tehničke odluke su u V2.

---

## Tech Stack

```
Backend:   ASP.NET Core Web API (C#, .NET 8) — Clean Architecture, CQRS, MediatR
Web:       React + Vite + TypeScript + TanStack Query + Zustand
Mobile:    React Native (iOS + Android)
DB:        SQL Server (Azure SQL) + EF Core
Real-time: SignalR (Azure SignalR Service)
Push:      Firebase Cloud Messaging (FCM)
Jobs:      BackgroundService (built-in .NET) — Hangfire od Faze 1+
Cloud:     Microsoft Azure (App Service, Blob Storage, SQL Database)
AI/ML:     Python + FastAPI — SAMO od Faze 1.5, ne pre
```

---

## Clean Architecture — Struktura

```
src/
├── Domain/           # Entiteti, enumi, value objects, domain events, interfejsi
├── Application/      # Commands/, Queries/, DTOs/, Validators/, Behaviors/
├── Infrastructure/   # Persistence/, Services/, Identity/, BackgroundJobs/, ExternalApis/
└── API/              # Controllers/, Hubs/, Middleware/, Filters/
```

**CQRS organizacija:** po feature-u, ne po tipu  
✅ `Commands/CreatePost/CreatePostCommand.cs`  
❌ `Commands/CreatePostCommand.cs`

---

## Faza 0 — SADA implementiramo

| Feature | Napomena |
|---|---|
| Oglasna tabla | 3 kategorije: **Udomljavanje, Izgubljen, Nađen** |
| Amber Alert (integrisani) | "Izgubljen" objava → automatski push 10km radijus |
| Dugme "Vidim ovog psa" | Mini-formular sa lokacijom → notifikacija vlasniku |
| Profili korisnika | Dvoslojni sistem lokacije (opština + opcioni GPS) |
| Profili ljubimaca | Osnovna kartica, dropdown za rasu (30-40 rasa) |
| In-app chat | Bez otkrivanja broja telefona dok korisnik ne odobri |
| Onboarding kviz | 3 pitanja, personalizuje feed |
| Trust Score Lite | Vidljiv, akumulira se, **ne gate-uje ništa** |
| Push notifikacije | FCM: Amber Alert + chat + status promene |
| Automatska moderacija | Obavezna polja, rate limit, keyword filter, image hash |
| Community reporting | Prijavi objavu; 3 prijave = auto-skrivanje |
| Admin dashboard | Minimalni: flagovi, prijave, osnovne brojke |

## Faza 0 — NE implementirati

- AI prepoznavanje rasa (dropdown je dovoljan, AI dolazi u Fazi 1.5)
- Python AI mikroservis (nije potreban u Fazi 0)
- Trust Score gate-ovi (akumulira se, ali ništa ne blokira)
- Foster lista strukturirana, QR verifikacija primopredaje
- Follow-up sistem (1/3/6 meseci), Veterinarski karton
- Prijava zlostavljanja sa PDF, Donacije/kampanje
- Progresivni Amber Alert radijus, Admin panel za udruženja
- Monetizacija, Social feed "Zajednica" tab

> ⚠️ Pre implementacije feature-a, proveri: da li je u Fazi 0 tabeli iznad?  
> Ako nije — ne implementiraj bez eksplicitne potvrde.

---

## 7 Kritičnih Poslovnih Pravila

1. **Amber Alert = integrisani model** — "Izgubljen" objava automatski aktivira push notifikaciju. Nema zasebnog alert sistema, nema posebnog flow-a.

2. **Lokacija = nikada tačna adresa** — samo `LocationZone` (opština/naselje) u profilu. GPS koordinate (`LastKnownLatLng`) se čuvaju ali nikad ne prikazuju drugim korisnicima.

3. **Telefon = skriven do eksplicitnog odobrenja** — in-app chat ne otkriva broj dok korisnik ne klikne "Podeli broj". `PhoneShared` boolean na konverzaciji.

4. **Trust Score u Fazi 0 = dekorativni** — vidljiv, akumulira se, ali ne blokira nijednu akciju. Gate-ovi dolaze u Fazi 1.

5. **Moderacija = troslojna, nikada pre-moderacija** — auto filteri → community report (3 prijave = auto-hide) → admin pregled. Objave se odmah publikuju.

6. **Registracija za "Izgubljen" = minimalna** — korisnik je u panici. Obavezno: email + ime. Sve ostalo se popunjava naknadno.

7. **Feed prioritet je apsolutan** — "Izgubljen" objave uvek na vrhu feed-a, bez izuzetka.

---

## Naming Conventions

| Kontekst | Konvencija | Primer |
|---|---|---|
| C# klase/metode | PascalCase | `TrustScoreService`, `ActivateAmberAlert` |
| C# privatna polja | _camelCase | `_repository`, `_notificationService` |
| DB tabele | PascalCase plural | `AmberAlerts`, `TrustScoreHistory` |
| DB kolone | PascalCase | `LocationZone`, `LastKnownLatLng` |
| API endpointi | kebab-case | `/api/v1/amber-alerts` |
| API verzija | Uvek v1 prefix | `/api/v1/...` |
| UI tekstovi | Srpski | "Izgubljen pas", "Pouzdan korisnik" |
| Kod identifikatori | Engleski | `PostCategory.Lost`, `UserRole.Foster` |

---

## Kod Stil — C# Komentari

**Nikad ne dodavati:**
- XML `/// <summary>` tagove
- Inline komentare koji opisuju šta kod radi (`// Kreira korisnika`)
- Komentare koji ponavljaju ime metode ili parametra

**Dodati samo za:**
- `// Zašto` — neočigledno biznis pravilo
- `// NIKAD` — bezbednosno ili privacy upozorenje
- `// Faza X` — namerno ostavljeno za kasniju fazu
- `// TODO:` — konkretna stavka za budući rad

---

## Error Response Format (konzistentan kroz ceo projekat)

```json
{
  "error": {
    "code": "ALERT_LIMIT_EXCEEDED",
    "message": "Već imate aktivan Amber Alert u poslednjih 24h.",
    "details": {}
  }
}
```

---

## Slike i Storage

- **Nikad** binarne slike u SQL-u — samo URL-ovi ka Azure Blob Storage
- Image hashing pri svakom uploadu (detekcija duplikata)
- Azure Content Safety API za moderaciju sadržaja slika (Faza 0)
- Čuvati slike u dobrom kvalitetu sa strukturiranim metapodacima (priprema za AI u Fazi 1.5)

---

## Redosled Implementacije (Faza 0)

Vertikalni slojevi — feature po feature, od DB-a do ekrana. Nikad horizontalno (ceo API pa ceo UI).

| Nedelja | Šta |
|---|---|
| Dan 1–2 | Infrastruktura: Clean Architecture setup, EF Core + Users migracija, JWT skeleton, CI/CD |
| Nedelja 1–2 | Vertikalni Slice 1 — Oglasi + Auth + Image Upload |
| Nedelja 3 | Vertikalni Slice 2 — Amber Alert + Push (FCM + SignalR) |
| Nedelja 4 | Vertikalni Slice 3 — In-app Chat |
| Nedelja 5 | Vertikalni Slice 4 — Trust Score + Moderacija |
| Nedelja 6 | Vertikalni Slice 5 — Onboarding + UX Polish |
| Nedelja 7–14 | Zatvorena beta — Novi Sad |

> Detalji po danu: @ContextDocs/implementacija-strategija.md

---

## Lokalni Build

```bash
# Backend
cd src/API
dotnet run

# Frontend Web  
cd frontend
npm run dev

# Migracije (EF Core)
dotnet ef migrations add [NazivMigracije] --project src/Infrastructure --startup-project src/API
dotnet ef database update --project src/Infrastructure --startup-project src/API

# Testovi
dotnet test
```
