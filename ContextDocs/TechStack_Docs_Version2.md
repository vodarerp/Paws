# 🔧 Platforma za Ljubimce — Tehnička Dokumentacija V2

> **Status:** Finaliziran tehnički plan usklađen sa ProjectPlan V3
> **Datum:** Mart 2026
> **Baziran na:** ProjectPlan_Version3.md (Faza 0 — True MVP)
> **Princip:** Monolit za Fazu 0, postepeno razdvajanje u mikroservise

---

## 1. Tech Stack — Pregled po Fazama

### 1.1 Faza 0 — True MVP Stack

| Sloj | Tehnologija | Napomena |
|---|---|---|
| **Backend API** | ASP.NET Core Web API (C#) | Jedini backend servis (monolit) |
| **Frontend Web** | React (Vite + TypeScript) | Mobile-first responsive, Azure Static Web Apps |
| **Frontend Mobile** | React Native (iOS + Android) | FCM integracija, geolokacija |
| **Baza podataka** | SQL Server (Azure SQL Basic) | 5 DTU, 2GB |
| **Cloud** | Microsoft Azure | West Europe region |
| **Real-time** | SignalR (Azure SignalR Service Free) | Chat + Feed updates |
| **Push notifikacije** | Firebase Cloud Messaging (FCM) | Besplatno, bez limita |
| **Moderacija slika** | Azure Content Safety API (F0 Free) | 5,000 analiza/mesec |
| **Image hashing** | pHash biblioteka u .NET | Detekcija duplikata |
| **Monitoring** | Application Insights | Besplatna kvota (5GB/mesec) |
| **CI/CD** | GitHub Actions | Besplatan tier |
| **IaC** | Bicep templates | Azure native |
| **Background Jobs** | BackgroundService (built-in .NET) | Tri servisa po prioritetu |

### 1.2 Šta eksplicitno NE ulazi u Fazu 0

| Tehnologija | Faza | Razlog |
|---|---|---|
| Python FastAPI mikroservis | Faza 1.5 | AI prepoznavanje rasa, nema AI u Fazi 0 |
| Social Feed mikroservis | Faza 1.5 | Zaseban servis od rođenja, uslov: 500+ korisnika |
| Hangfire | Faza 1+ | BackgroundService dovoljan za 150-300 korisnika |
| Azure Kubernetes Service (AKS) | Faza 3 | Post-MVP orkestracija kontejnera |
| Azure Service Bus / RabbitMQ | Faza 2+ | Message queue za async procesiranje |
| Azure Cognitive Search | Faza 2+ | Full-text pretraga kad SQL LIKE ne bude dovoljan |
| Redis Cache | Faza 2+ | Keširanje feed-a, Trust Score-ova |
| GPU Instance | Faza 3 | AI Detektiv facial recognition |

### 1.3 Evolucija stack-a

| Faza | Dodaje se |
|---|---|
| **Faza 0** | .NET API + React + React Native + SQL + Azure + FCM |
| **Faza 1** | Hangfire, SignalR Standard tier, proširena baza |
| **Faza 1.5** | Social Feed API (.NET, zaseban servis) + Python FastAPI (AI) + Docker |
| **Faza 2+** | Azure CDN, Redis, Service Bus, event-driven komunikacija |
| **Faza 3** | AKS, GPU instance, AI modeli |

---

## 2. Backend Arhitektura (.NET)

### 2.1 Ključna arhitekturalna odluka: Monolit za Fazu 0

Jedan .NET API servis koji radi sve. Svesna odluka za solo developera — čist, jednostavan, lak za debug i deploy. Clean Architecture unutar monolita garantuje da se komponente mogu izvući u zasebne servise kad zatreba.

### 2.2 Struktura projekta — Clean Architecture

```
PetPlatform/
├── src/
│   ├── PetPlatform.Domain/           # Entiteti, enumi, interfejsi, business rules
│   ├── PetPlatform.Application/      # Use cases, CQRS, validacija, DTOs
│   ├── PetPlatform.Infrastructure/   # EF Core, Azure servisi, Identity, Jobs
│   └── PetPlatform.API/              # Controllers, Hubs, Middleware
│
├── tests/
│   ├── PetPlatform.Domain.Tests/
│   ├── PetPlatform.Application.Tests/
│   └── PetPlatform.API.Tests/
│
├── infra/
│   ├── main.bicep
│   ├── modules/
│   └── parameters/
│
├── PetPlatform.sln
├── .github/workflows/
└── README.md
```

### 2.3 Zavisnosti između slojeva

```
API → Application → Domain ← Infrastructure
         ↑                        |
         └────────────────────────┘
         (Infrastructure implementira
          interfejse iz Domain-a,
          registruje se u API kroz DI)
```

**Pravila:**
- Domain ne zavisi ni od čega — čist C#, nula NuGet paketa
- Application zavisi samo od Domain-a
- Infrastructure zavisi od Domain-a i Application-a
- API zavisi od svih (ali samo za DI registraciju i HTTP endpoint definicije)
- Kontroler nikad ne poziva Repository direktno (uvek kroz MediatR handler)
- Handler nikad ne poziva drugi handler (koristi domain evente)

---

### 2.4 Domain sloj

```
PetPlatform.Domain/
├── Entities/
│   ├── User.cs
│   ├── Pet.cs
│   ├── Post.cs
│   ├── PostSighting.cs
│   ├── Media.cs
│   ├── ChatConversation.cs
│   ├── ChatMessage.cs
│   ├── TrustScoreEntry.cs
│   ├── Report.cs
│   ├── FcmToken.cs
│   ├── InAppNotification.cs
│   └── UserNotificationSettings.cs
│
├── Enums/
│   ├── UserRole.cs               (Individual, Organization, Admin)
│   ├── PostCategory.cs           (Udomljavanje, Izgubljen, Nadjen)
│   ├── PostStatus.cs             (Aktivan, UProcesu, Zatvoren, Uklonjen)
│   ├── ResolutionType.cs         (Udomljen, PasNadjen, Istekao)
│   ├── PetStatus.cs              (SaVlasnikom, ZaUdomljavanje, Izgubljen, Nadjen)
│   ├── PetGender.cs              (Musko, Zensko, Nepoznato)
│   ├── PetSize.cs                (Mali, Srednji, Veliki)
│   ├── MediaEntityType.cs        (Post, Pet, PostSighting)
│   ├── MediaType.cs              (Image, Video)
│   ├── ModerationStatus.cs       (Uploading, Pending, Approved, Rejected, PendingReview)
│   ├── ContactPreference.cs      (ChatOnly, PhoneOk)
│   ├── ReportTargetType.cs       (Post, User)
│   ├── ReportReason.cs           (Spam, ProdajaMaskirana, LaznaPrijava, Uvredljivo, Duplikat, Drugo)
│   ├── ReportStatus.cs           (Pending, Reviewed, Resolved, Dismissed)
│   ├── TrustScoreActionType.cs   (OglasKreiran, PasNadjenZatvoren, PrijavaPotvrdjena, ...)
│   ├── NotificationType.cs       (AmberAlert, PetSighting, ChatMessage, ...)
│   ├── NotificationPriority.cs   (High, Normal, Low)
│   └── OnboardingIntent.cs       (TraziPsa, IzgubioLjubimca, ZeliDaPomogne)
│
├── Interfaces/
│   ├── Repositories/
│   │   ├── IUserRepository.cs
│   │   ├── IPostRepository.cs
│   │   ├── IPetRepository.cs
│   │   ├── IMediaRepository.cs
│   │   ├── IChatRepository.cs
│   │   ├── IReportRepository.cs
│   │   ├── ITrustScoreRepository.cs
│   │   ├── INotificationSettingsRepository.cs
│   │   └── IFcmTokenRepository.cs
│   └── Services/
│       ├── IBlobStorageService.cs
│       ├── IImageHashingService.cs
│       ├── IContentModerationService.cs
│       ├── INotificationService.cs
│       ├── IPushService.cs
│       └── IGeoQueryService.cs
│
├── ValueObjects/
│   ├── Location.cs               (Latitude, Longitude — immutable)
│   ├── TrustScoreCategory.cs     (logika Nov/Aktivan/Pouzdan/LokalniHeroj)
│   └── ImageHash.cs              (pHash wrapper)
│
├── Constants/
│   ├── TrustScorePoints.cs
│   ├── RateLimits.cs
│   └── PostExpiration.cs
│
└── Exceptions/
    ├── DomainException.cs
    ├── PostLimitExceededException.cs
    ├── UnauthorizedPostAccessException.cs
    └── InvalidPostCategoryException.cs
```

---

### 2.5 Application sloj

CQRS sa MediatR — svaki use case je zaseban Command ili Query. Folder-per-feature organizacija.

```
PetPlatform.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   └── ICurrentUserService.cs
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs      (MediatR pipeline)
│   │   └── LoggingBehavior.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── Models/
│       ├── PaginatedList.cs
│       └── Result.cs
│
├── Auth/
│   ├── Commands/ (Register, Login, RefreshToken, GoogleLogin, TwoFactor/)
│   └── DTOs/
│
├── Posts/
│   ├── Commands/ (CreatePost, UpdatePost, ChangePostStatus, ReportPost, CreateSighting)
│   ├── Queries/ (GetFeed, GetPostById, GetPostSightings)
│   ├── DTOs/
│   └── EventHandlers/
│       ├── PostCreatedHandler.cs       (triggeruje Amber Alert ako Izgubljen)
│       ├── PostStatusChangedHandler.cs (notifikacije, Trust Score)
│       └── SightingCreatedHandler.cs   (notifikacija vlasniku)
│
├── Pets/
│   ├── Commands/ (CreatePet, UpdatePet)
│   ├── Queries/ (GetMyPets, GetPetById)
│   └── DTOs/
│
├── Media/
│   ├── Commands/ (RequestUpload, ConfirmUpload, DeleteMedia)
│   └── DTOs/
│
├── Chat/
│   ├── Commands/ (CreateConversation, SendMessage, SharePhone, MarkAsRead)
│   ├── Queries/ (GetConversations, GetMessages)
│   └── DTOs/
│
├── Users/
│   ├── Commands/ (UpdateProfile, UpdateLocation, ReportUser, DeleteAccount)
│   ├── Queries/ (GetMyProfile, GetPublicProfile)
│   └── DTOs/
│
├── Notifications/
│   ├── Commands/ (UpdateNotificationSettings, RegisterFcmToken)
│   ├── Queries/ (GetNotificationSettings, GetInAppNotifications)
│   └── DTOs/
│
├── Admin/
│   ├── Commands/ (ApprovePost, RemovePost, WarnUser, SuspendUser, BanUser)
│   ├── Queries/ (GetDashboard, GetFlaggedPosts, GetReportedUsers)
│   └── DTOs/
│
└── TrustScore/
    ├── Commands/ (UpdateTrustScore)
    ├── Queries/ (GetTrustScoreHistory)
    └── Services/
        └── TrustScoreCalculator.cs
```

---

### 2.6 Infrastructure sloj

```
PetPlatform.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/          (EF Core Fluent API — svaki entitet zasebno)
│   ├── Repositories/            (implementacija IRepository interfejsa)
│   ├── Migrations/
│   └── Seed/
│       └── SeedData.cs
│
├── Services/
│   ├── BlobStorageService.cs        (upload, SAS generisanje, delete)
│   ├── ImageHashingService.cs       (pHash generisanje i poređenje)
│   ├── ContentModerationService.cs  (Azure Content Safety API)
│   ├── NotificationService.cs       (centralni routing — SignalR/FCM/Popup)
│   ├── FcmPushService.cs           (Firebase Cloud Messaging)
│   ├── GeoQueryService.cs          (SQL geography za Amber Alert)
│   └── KeywordFilterService.cs     (regex + keyword liste)
│
├── Identity/
│   ├── IdentityService.cs
│   ├── JwtTokenGenerator.cs
│   ├── RefreshTokenService.cs
│   └── TwoFactorService.cs         (TOTP za admin)
│
├── BackgroundJobs/
│   ├── CriticalEventProcessor.cs    (Amber Alert dispatch)
│   ├── HighPriorityEventProcessor.cs (Image processing, Trust Score)
│   └── ScheduledTaskRunner.cs       (periodični poslovi)
│
└── DependencyInjection.cs
```

---

### 2.7 API sloj

```
PetPlatform.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── PostsController.cs
│   ├── PetsController.cs
│   ├── MediaController.cs
│   ├── ChatController.cs
│   ├── UsersController.cs
│   ├── NotificationsController.cs
│   └── AdminController.cs
│
├── Hubs/
│   ├── ChatHub.cs
│   └── FeedHub.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── BannedUserMiddleware.cs
│
├── Filters/
│   ├── RateLimitFilter.cs
│   └── AdminAuthorizationFilter.cs    (Role + 2FA)
│
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── WebApplicationExtensions.cs
│
├── Configuration/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
│
├── Program.cs
└── Dockerfile
```

**Middleware pipeline redosled:**
```
Request → ExceptionHandling → RequestLogging → Authentication (JWT)
→ BannedUserMiddleware → Authorization → RateLimitFilter → Controller → MediatR → Handler
```

### 2.8 NuGet paketi po sloju

| Sloj | Paketi |
|---|---|
| **Domain** | Nema (čist C#) |
| **Application** | MediatR, FluentValidation, AutoMapper/Mapster |
| **Infrastructure** | EF Core SqlServer, ASP.NET Identity, Azure.Storage.Blobs, Azure.AI.ContentSafety, Microsoft.Azure.SignalR, FirebaseAdmin |
| **API** | Swashbuckle (Swagger), Serilog, Microsoft.ApplicationInsights |

---

## 3. Baza Podataka (SQL Server)

### 3.1 Ključne odluke

| Odluka | Vrednost | Razlog |
|---|---|---|
| TrustScoreCategory | Application Layer (computed u kodu) | Jednostavnost, fleksibilnost pragova, Clean Architecture |
| Media Storage | Polymorphic Media tabela (samo Core) | Image hashing, moderacija, proširivost |
| Social Feed media | Zasebne tabele u zasebnom schema-i (Faza 1.5) | Izolacija od Core-a |
| Age polje na Pets | `string?` (nullable) | Fleksibilnost ("štenad", "oko 2 godine") |

### 3.2 Faza 0 — Tabele

| Tabela | Status | Opis |
|---|---|---|
| Users | Modifikovana | Dvoslojni geo, pojednostavljene role |
| Pets | Modifikovana | Bez AI polja, dodat Size/Color/SpecialMarks |
| Posts | Značajno promenjena | Integrisani Amber Alert |
| PostSightings | **NOVA** | "Vidim ovog psa" funkcionalnost |
| Media | **NOVA** (Polymorphic) | Centralizovani media za Core |
| ChatConversations | Modifikovana | Dodat PostId za KPI tracking |
| ChatMessages | Isto kao V1 | Enkriptovane at rest |
| TrustScoreHistory | Modifikovana | Dodat RelatedEntityId |
| Reports | Isto kao V1 | Community reporting |
| UserNotificationSettings | **NOVA** | Kontrole notifikacija |
| FcmTokens | **NOVA** | Više uređaja po korisniku |
| InAppNotifications | **NOVA** | Trust Score i buduće in-app notifikacije |

### 3.3 Konceptualni model

```
Users
├── Id (GUID)
├── Email, PasswordHash, DisplayName, AvatarUrl
├── Role (enum: Individual | Organization | Admin)
├── TrustScore (int, default 0)
├── LocationZone (string, obavezno — opština iz dropdown-a)
├── LastKnownLatitude (decimal?, nullable)
├── LastKnownLongitude (decimal?, nullable)
├── GpsConsentGiven (bool, default false)
├── OrganizationName (string?, nullable)
├── OrganizationUrl (string?, nullable)
├── IsVerified (bool), IsBanned (bool)
├── OnboardingCompleted (bool)
├── OnboardingIntent (enum?: TraziPsa | IzgubioLjubimca | ZeliDaPomogne)
└── CreatedAt, LastActiveAt

Pets
├── Id (GUID), OwnerId (FK → Users)
├── Name, Breed (string), Age (string?)
├── Gender (enum), Size (enum), Color (string)
├── SpecialMarks (string?), ChipNumber (string?), IsSterilized (bool?)
├── PhotoUrls — NE (koristi Media tabelu)
├── Status (enum: SaVlasnikom | ZaUdomljavanje | Izgubljen | Nadjen)
└── CreatedAt, UpdatedAt

Posts
├── Id (GUID), AuthorId (FK → Users), PetId (FK → Pets, nullable)
├── Category (enum: Udomljavanje | Izgubljen | Nadjen)
├── Title, Description
├── LocationZone (string), Latitude (decimal?), Longitude (decimal?)
├── LastSeenAt (datetime?), ContactPreference (enum?)
├── AlertRadiusKm (int, default 10), AlertSentAt (datetime?)
├── Status (enum: Aktivan | UProcesu | Zatvoren | Uklonjen)
├── ResolutionType (enum?: Udomljen | PasNadjen | Istekao)
├── ReportCount (int), IsHidden (bool)
└── CreatedAt, UpdatedAt, ExpiresAt

PostSightings
├── Id (GUID), PostId (FK → Posts), ReporterId (FK → Users)
├── Latitude (decimal), Longitude (decimal)
├── LocationDescription (string?), SeenAt (datetime)
├── Comment (string?)
└── CreatedAt

Media (Polymorphic — SAMO za Core)
├── Id (GUID)
├── EntityType (enum: Post | Pet | PostSighting)
├── EntityId (GUID)
├── Url (string), MediaType (enum: Image | Video)
├── ImageHash (string?), ModerationStatus (enum)
├── SortOrder (int)
└── UploadedAt

ChatConversations
├── Id (GUID), PostId (FK → Posts?, opciono)
├── Participant1Id, Participant2Id (FK → Users)
├── PhoneSharedByP1, PhoneSharedByP2 (bool)
├── LastMessageAt
└── CreatedAt

ChatMessages
├── Id (GUID), ConversationId (FK), SenderId (FK)
├── Content (string, encrypted at rest)
├── IsRead (bool)
└── CreatedAt

TrustScoreHistory
├── Id (GUID), UserId (FK)
├── ActionType (enum), Points (int), Description (string)
├── RelatedEntityId (GUID?)
└── CreatedAt

Reports
├── Id (GUID), ReporterId (FK)
├── TargetType (enum: Post | User), TargetId (GUID)
├── Reason (enum), Description (string?)
├── Status (enum), AdminNotes (string?)
└── CreatedAt, ResolvedAt

UserNotificationSettings
├── Id (GUID), UserId (FK)
├── AmberAlertEnabled (bool, default true)
├── StatusChangesEnabled (bool, default true)
└── UpdatedAt

FcmTokens
├── Id (GUID), UserId (FK)
├── Token (string), Platform (enum: Android | iOS | Web)
├── CreatedAt, LastUsedAt

InAppNotifications
├── Id (GUID), UserId (FK)
├── Title, Body (string), Type (enum)
├── IsRead (bool, default false)
└── CreatedAt
```

### 3.4 Šta NE ulazi u Fazu 0

| Tabela | Faza |
|---|---|
| FosterProfiles | Faza 1 |
| Handovers (QR, selfi) | Faza 1 |
| HealthRecords | Faza 1 |
| DonationCampaigns / Donations | Faza 1 |
| AbuseReports (sa PDF) | Faza 1 |
| Badges | Faza 1 |
| SocialPosts / SocialPostMedia / SocialLikes / SocialComments | Faza 1.5 (zaseban schema) |

### 3.5 Geo upiti — Amber Alert

Dvoslojni sistem sa fallback-om. SQL Server `geography` tip za spatial upite.

```sql
-- Korisnici sa GPS-om u radijusu 10km
SELECT u.Id FROM Users u
WHERE u.GpsConsentGiven = 1
  AND geography::Point(@lostLat, @lostLng, 4326)
      .STDistance(geography::Point(u.LastKnownLatitude, u.LastKnownLongitude, 4326)) <= 10000

UNION

-- Fallback: korisnici bez GPS-a ali u istoj opštini
SELECT u.Id FROM Users u
WHERE u.GpsConsentGiven = 0
  AND u.LocationZone = @lostLocationZone
```

### 3.6 Skaliranje baze

| Korisnika | Tier | DTU | Cena |
|---|---|---|---|
| 0-500 | Basic | 5 | ~$5/mesec |
| 500-2,000 | Standard S0 | 10 | ~$15/mesec |
| 2,000-5,000 | Standard S1 | 20 | ~$30/mesec |
| 5,000+ | Standard S2 ili vCore model | 50+ | ~$75+/mesec |

---

## 4. API Endpointi (Faza 0)

### 4.1 Principi dizajna

- **Verzionisanje:** `/api/v1/` od prvog dana
- **Cursor-based pagination** za feed
- **Konzistentna error struktura:** `{ "error": { "code": "...", "message": "...", "details": {} } }`
- **Swagger/OpenAPI** dokumentacija automatski generisana
- Svaki endpoint mapira na konkretan user journey iz ProjectPlan V3

### 4.2 Auth (7 endpointa)

```
POST   /api/v1/auth/register           # Email + password + LocationZone + OnboardingIntent
POST   /api/v1/auth/login              # Email + password → JWT + refresh token
POST   /api/v1/auth/refresh-token      # Refresh → novi JWT
POST   /api/v1/auth/google             # Google OAuth login
POST   /api/v1/auth/2fa/setup          # Generiši TOTP secret + QR (admin)
POST   /api/v1/auth/2fa/verify         # Verifikuj TOTP i aktiviraj 2FA
POST   /api/v1/auth/2fa/validate       # Proveri TOTP kod pri loginu
```

Dva registraciona flow-a: standardni (sva polja) i skraćeni za hitne slučajeve ("Izgubio sam psa" — minimum polja, ostalo naknadno).

### 4.3 Posts (7 endpointa)

```
GET    /api/v1/posts                    # Feed (paginirano, filtrirano)
POST   /api/v1/posts                    # Kreiraj objavu (sa mediaIds)
GET    /api/v1/posts/{id}               # Detalji objave
PUT    /api/v1/posts/{id}               # Ažuriraj (samo autor)
PUT    /api/v1/posts/{id}/status        # Promeni status
POST   /api/v1/posts/{id}/report        # Prijavi objavu
POST   /api/v1/posts/{id}/sightings     # "Vidim ovog psa" (samo Izgubljen)
GET    /api/v1/posts/{id}/sightings     # Lista viđenja
```

**Feed logika:** Kategorija "Izgubljen" uvek na vrhu, ostalo hronološki sa blagim boostom za viši TrustScore. Skriveni postovi se ne prikazuju.

**Kreiranje "Izgubljen" posta:** Backend automatski setuje ExpiresAt +7 dana, triggeruje push u radijusu 10km (background job), rate limit: max 1 na 24h.

**Nema zasebnog `/api/v1/alerts`** — Amber Alert je integrisani deo Posts sistema.

### 4.4 Pets (4 endpointa)

```
GET    /api/v1/pets                     # Lista ljubimaca korisnika
POST   /api/v1/pets                     # Registruj ljubimca (sa mediaIds)
GET    /api/v1/pets/{id}                # Detalji
PUT    /api/v1/pets/{id}                # Ažuriraj
```

### 4.5 Media (3 endpointa) — SAS Token pristup

```
POST   /api/v1/media/request-upload     # Dobij SAS token + mediaId
POST   /api/v1/media/confirm-upload     # Potvrdi da je upload završen
DELETE /api/v1/media/{id}               # Obriši (samo vlasnik parent entiteta)
```

**Hibridni upload flow:**
1. Frontend: `POST /request-upload` → dobije `mediaId` + `uploadUrl` (SAS token, write-only, 15 min expiry)
2. Frontend: `PUT` direktno na Azure Blob Storage (progress bar, paralelni upload)
3. Frontend: `POST /confirm-upload` → backend verifikuje, generiše hash, šalje na Content Safety
4. Frontend: `POST /posts` sa `mediaIds` u body-ju → linkuje media sa postom

**Prednosti:** API nikad ne procesira binarne podatke, skalira se do 100k+ korisnika bez promene, bolji UX (paralelni upload dok korisnik kuca tekst).

**Orphaned cleanup:** Background job briše Media redove bez parent-a starije od 24h + Blob lifecycle policy.

### 4.6 Chat (6 endpointa)

```
GET    /api/v1/chat/conversations                   # Lista konverzacija
POST   /api/v1/chat/conversations                   # Pokreni (sa opcionim PostId)
GET    /api/v1/chat/conversations/{id}/messages      # Poruke (paginirano)
POST   /api/v1/chat/conversations/{id}/messages      # Pošalji poruku
POST   /api/v1/chat/conversations/{id}/share-phone   # Eksplicitno deli broj
PUT    /api/v1/chat/conversations/{id}/read          # Označi kao pročitane
```

Real-time poruke kroz SignalR ChatHub, REST endpointi za istoriju i inicijaciju.

### 4.7 Users (5 endpointa)

```
GET    /api/v1/users/me                  # Moj profil
PUT    /api/v1/users/me                  # Ažuriraj profil
PUT    /api/v1/users/me/location         # Ažuriraj GPS (mobilna app šalje periodično)
GET    /api/v1/users/{id}/profile        # Javni profil (ograničeni podaci)
POST   /api/v1/users/{id}/report         # Prijavi korisnika
```

`GET /{id}/profile` vraća samo javne podatke: DisplayName, Avatar, TrustScoreCategory, OrganizationName. Nikad email, telefon, tačnu lokaciju.

### 4.8 Notifications (3 endpointa)

```
GET    /api/v1/notifications/settings    # Trenutna podešavanja
PUT    /api/v1/notifications/settings    # Ažuriraj
POST   /api/v1/notifications/fcm-token   # Registruj/ažuriraj Firebase token
```

### 4.9 Admin (7 endpointa)

```
GET    /api/v1/admin/dashboard           # Osnovne brojke
GET    /api/v1/admin/flagged-posts       # Flagovane objave
POST   /api/v1/admin/posts/{id}/approve  # Odobri
POST   /api/v1/admin/posts/{id}/remove   # Ukloni
GET    /api/v1/admin/reported-users      # Prijavljeni korisnici
POST   /api/v1/admin/users/{id}/warn     # Upozori
POST   /api/v1/admin/users/{id}/suspend  # Suspenduj (7-30 dana)
POST   /api/v1/admin/users/{id}/ban      # Permanentni ban
```

Svi admin endpointi zahtevaju `Role = Admin` + `2fa_verified = true`.

### 4.10 Pregled

| Grupa | Endpointi |
|---|---|
| Auth | 7 |
| Posts | 8 |
| Pets | 4 |
| Media | 3 |
| Chat | 6 |
| Users | 5 |
| Notifications | 3 |
| Admin | 8 |
| **Ukupno** | **44** |

### 4.11 Izbačeno iz V1

| V1 Endpoint | Razlog |
|---|---|
| `/api/v1/alerts/*` | Integrisano u Posts |
| `/api/v1/foster/*` | Faza 1 |
| `/api/v1/handover/*` | Faza 1 |
| `/api/v1/pets/{id}/health-record` | Faza 1 |
| `/api/v1/donations/*` | Faza 1 |
| `/api/v1/abuse-reports/*` | Faza 1 |

---

## 5. Real-time Komunikacija (SignalR)

### 5.1 Dva hub-a

**ChatHub** — privatna komunikacija između dva korisnika.

| Metoda | Opis |
|---|---|
| SendMessage | Pošalji poruku u konverzaciji |
| SendTyping | Typing indicator |
| MarkAsRead | Označi poruke kao pročitane |
| SharePhone | Deljenje broja telefona |

**FeedHub** — javni sadržaj i notifikacije.

| Metoda | Opis |
|---|---|
| NewPost | Nov oglas u opštini (server → klijent) |
| PostStatusChanged | Promena statusa oglasa |
| NewSighting | "Vidim ovog psa" (server → vlasnik + pratioce posta) |
| PostHidden | Moderacija sakrila post |
| JoinPostGroup | Korisnik otvara detalje posta |
| LeavePostGroup | Korisnik napušta stranicu posta |

### 5.2 Grupna logika — tri nivoa

| Grupa | Pridružuje se | Svrha |
|---|---|---|
| `chat_{conversationId}` | Automatski pri konekciji | Chat poruke |
| `feed_{locationZone}` | Automatski pri konekciji | Novi oglasi u opštini |
| `post_{postId}` | Kad otvori detalje posta | Sightings i status promene |

### 5.3 Presence Tracking

In-memory `ConcurrentDictionary` za Fazu 0 (jedan server). Kad korisnik pošalje poruku, provera da li je primalac online — ako da, SignalR; ako ne, FCM push. Redis-backed tracking za post-MVP sa više instanci.

### 5.4 Infrastruktura

Azure SignalR Service Free tier: 20 istovremenih konekcija, 20,000 poruka/dan. Dovoljno za Fazu 0. Prelaz na Standard (~$49/mesec) kad DAU preraste 20 istovremenih korisnika.

---

## 6. Push Notifikacije i In-App Popup

### 6.1 Tri kanala za notifikacije

| Kanal | Kad se koristi | Prioritet |
|---|---|---|
| **In-app Popup** (banner) | Online, hitni eventi | Amber Alert, Sighting, Chat (dok nije u tom chatu) |
| **SignalR tihi update** | Online, informativni eventi | Feed refresh, status promena |
| **FCM Push** | Offline (app zatvorena) | Amber Alert, Sighting, Chat |

In-app popup je treći kanal koji pokriva scenario: korisnik je u app-u ali na drugom ekranu (čita chat, gleda profil) — ne vidi feed update, ali popup banner na vrhu ekrana ga odmah obaveštava o hitnom eventu.

### 6.2 Centralni NotificationService — routing logika

```
Event → NotificationService.NotifyUser()
  ├── Korisnik ONLINE?
  │   ├── Priority HIGH → Frontend: POPUP banner
  │   └── Priority NORMAL → Frontend: TIHI update (feed refresh, badge)
  └── Korisnik OFFLINE? → FCM Push
```

Nikad duplo slanje — ili SignalR ili FCM za istu notifikaciju.

### 6.3 Notifikacione kategorije (V3 sekcija 7)

**Kategorija 1: Hitne (popup + push)**

| Notifikacija | Okidač | Primalac |
|---|---|---|
| "Izgubljen pas u vašem kraju" | Nova objava "Izgubljen" | Svi u radijusu 10km |
| "Neko je video vašeg psa!" | "Vidim ovog psa" klik | Vlasnik izgubljenog psa |

**Kategorija 2: Komunikacija (popup + push)**

| Notifikacija | Okidač | Primalac |
|---|---|---|
| Nova poruka u chat-u | Svaka nova poruka | Primalac (ako nije u tom chatu) |
| "Neko želi da udomi vašeg psa" | Prva poruka od novog korisnika | Autor oglasa za udomljavanje |

**Kategorija 3: Status (tihi push)**

| Notifikacija | Okidač | Primalac |
|---|---|---|
| "Vaš oglas ističe za 24h" | 6. dan od objave | Autor |
| "Oglas koji ste pratili je zatvoren" | Zatvaranje oglasa | Korisnici koji su slali poruke |

**Kategorija 4: Trust Score (samo in-app, nikad push)**

| Notifikacija | Okidač | Primalac |
|---|---|---|
| "Vaš status je sada: Aktivan!" | Promena kategorije | Korisnik |
| "+N poena: [opis]" | Svaka Trust Score promena | Korisnik |

### 6.4 FCM konfiguracija

| Aspekt | Detalji |
|---|---|
| Android channels | `urgent` (Amber Alert, Chat) + `updates` (status promena) |
| iOS | `time-sensitive` interruption level za Amber Alert (probija DND) |
| Batch sending | Max 500 tokena po batch-u za Amber Alert |
| Token cleanup | Background job briše nekorišćene tokene >60 dana |
| Deep linking | `action` + `entityId` u data payload-u za navigaciju |

### 6.5 Korisničke kontrole

| Tip notifikacije | Default | Može da isključi |
|---|---|---|
| Amber Alert | Uključen | Da |
| Chat poruke | Uključen | Ne |
| Status promena | Uključen | Da |

### 6.6 Rate Limiting notifikacija

Max 5 Amber Alert-ova po korisniku na sat. Max 20 chat push-eva na sat (posle toga samo badge count). Zaštita od notification fatigue.

### 6.7 In-App Popup — UX detalji

Popup queue sa prioritetom: urgent popup prekida normal popup. Max 3 u queue-u. Auto-dismiss: 8 sekundi za urgent, 5 sekundi za normal. Urgent = svetlo crvena pozadina, normal = svetlo plava.

---

## 7. Background Jobs

### 7.1 Tehnologija

`BackgroundService` (built-in .NET) za Fazu 0. Migracija na Hangfire na 500+ korisnika.

### 7.2 Tri servisa po prioritetu

**CriticalEventProcessor** — in-memory `Channel<T>`, instant procesiranje.

| Job | Max kašnjenje |
|---|---|
| Amber Alert Dispatch (geo query + push) | < 5 sekundi |
| Report Threshold (auto-hide na 3+ prijave) | < 5 sekundi |

**HighPriorityEventProcessor** — in-memory `Channel<T>`, FIFO.

| Job | Max kašnjenje |
|---|---|
| Image Processing (verifikacija, hash, Content Safety) | < 30 sekundi |
| Trust Score Update | < 30 sekundi |
| Duplicate Image Check | < 30 sekundi |

**ScheduledTaskRunner** — periodični timer.

| Job | Učestalost |
|---|---|
| Post Expiration Check | Svakih 6h |
| Post Cleanup (istekli "Izgubljen") | Jednom dnevno |
| Orphaned Media Cleanup | Jednom dnevno |
| Inactive User Nudge | Jednom nedeljno |
| FCM Token Cleanup | Jednom nedeljno |
| Daily Stats Snapshot | Jednom dnevno |

### 7.3 Failure handling

Princip: **nikad ne blokirati korisnikov UX zbog failed background job-a.**

| Job | Retry strategija |
|---|---|
| Amber Alert Dispatch | 3x eksponencijalni backoff (1s, 5s, 15s) |
| Image Processing | 2x, Pending ostaje za admin |
| Content Safety API | 2x, pusti kao Pending |
| Trust Score | 3x, periodična rekalkulacija ispravlja |
| Scheduled jobs | Sledeći ciklus, nema akumulacije |

### 7.4 Skaliranje

| Korisnika | Pristup |
|---|---|
| 0-500 | BackgroundService + in-memory Channel |
| 500-5,000 | Hangfire (persistent queue, dashboard, retry) |
| 5,000-50,000 | Azure Service Bus za kritične, Hangfire za ostalo |
| 50,000+ | Azure Functions event-driven |

---

## 8. Moderacija i Content Safety

### 8.1 Troslojna arhitektura

**Sloj 1 — Automatska moderacija (pre/pri objavljivanju)**

| Mehanizam | Tip | Akcija |
|---|---|---|
| Obavezna polja (min 1 slika, opis 20-30 char) | Sinhrono | Blokira post |
| Rate limiting (3 objave/dan, 1 Izgubljen/24h) | Sinhrono | Blokira request |
| Keyword filter ("prodajem", "cena", telefon pattern) | Sinhrono | Flag za admina |
| Image hashing — pHash | Asinhrono | Flag za admina |
| Azure Content Safety API | Asinhrono | Reject → sakrij, ostalo → flag |

**Sloj 2 — Community moderacija (posle objavljivanja)**

| Mehanizam | Opis |
|---|---|
| "Prijavi objavu" | Korisnik bira razlog iz liste, max 1 prijava po postu |
| Auto-hide | Kumulativna težina prijava ≥ 5 → post sakriven |
| Trust Score težina | Nov/Aktivan: 1, Pouzdan/Lokalni heroj: 2, Udruženje: 3 |
| Anti-zloupotreba | Neosnovane prijave = gubitak Trust Score poena za prijavitelja |

**Sloj 3 — Admin moderacija (dnevni ritual, ~15 min)**

Prioritetni redosled u admin queue-u:
1. Content Safety reject (kritičan)
2. Community auto-hide (visok)
3. Keyword flag (normalan)
4. Image hash duplikat (nizak)

Eskalacija sankcija: Upozorenje → Suspenzija 7d → Suspenzija 30d → Permanentni ban. Admin može da preskoči eskalaciju za ozbiljne prekršaje.

### 8.2 Image moderacija pipeline

```
Upload potvrđen → pHash generisanje (1-2s) → Duplikat provera (instant)
→ Content Safety API (2-5s) → Rezultat:
    Sve čisto → Approved
    Hash duplikat → Flag, post ostaje vidljiv
    Content Safety reject → Rejected, post sakriven
    Neodlučno → PendingReview, flag za admina
```

Post je vidljiv odmah — "publish first, moderate async" pristup (target: <10s od upload-a do odluke).

### 8.3 Skaliranje moderacije

| Korisnika | Pristup |
|---|---|
| 0-300 | Solo admin + automatski filteri + community |
| 300-1,000 | +2-3 volontera iz udruženja |
| 1,000-5,000 | Formalizovani moderator tim |
| 5,000+ | AI moderacija teksta, eskalacija ljudima |

---

## 9. Sigurnost i ZZPL Compliance

### 9.1 API Sigurnost

| Oblast | Implementacija |
|---|---|
| Autentifikacija | JWT (15min expiry) + Refresh token (7d, rotacija) + 2FA (TOTP) za admin |
| Autorizacija | Role-based (Individual, Organization, Admin) + resource-based za sopstvene resurse |
| Rate Limiting | Globalni po IP + per-endpoint per-user (ASP.NET Core built-in) |
| Input validacija | FluentValidation, parametrizovani EF Core upiti (SQL injection zaštita) |
| CORS | Striktno — samo naš web frontend domain |
| Security headers | HSTS, X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy |

### 9.2 Zaštita podataka

| Podatak | Enkripcija | Ko vidi |
|---|---|---|
| Lozinke | bcrypt/PBKDF2 (ASP.NET Identity) | Niko |
| Podaci u tranzitu | TLS 1.2+ (HTTPS everywhere) | — |
| Podaci at rest | Azure TDE (automatski) | — |
| Chat poruke | Application-level enkripcija + TDE | Samo učesnici |
| GPS koordinate | Enkriptovano u bazi | Nikad u API response-u |
| Slike | SAS tokeni sa expiry-jem, EXIF strip | Kontrolisan pristup |

### 9.3 Upload sigurnost

Validacija: samo slike (JPEG, PNG, WebP) i video (MP4), magic bytes provera, max 10MB/slika, max 4096x4096px, EXIF strip, virus scan (Azure Defender), random GUID blob naming, SAS write-only 15min expiry.

### 9.4 ZZPL Compliance

| Obaveza | Implementacija |
|---|---|
| Pravni osnov | Izvršenje ugovora (osnovno), Pristanak za GPS, Legitiman interes za Trust Score |
| Pravo na pristup | JSON export endpoint |
| Pravo na brisanje | Deaktivacija odmah + anonimizacija posle 30 dana |
| Pravo na ispravku | Korisnik edituje profil, postove, kartice |
| Consent management | GPS consent sa timestampom, povlačenje u settings-u |
| Privacy Policy | Javno dostupna stranica na platformi |

**Data Retention Policy:**

| Podatak | Čuvanje |
|---|---|
| Neaktivni nalog (>12 meseci) | Notifikacija + 30 dana + anonimizacija |
| Zatvoreni oglasi | 6 meseci, pa anonimizacija |
| Chat poruke | Dok obe strane imaju aktivan nalog |
| Flagovani/banovani podaci | 3 godine |
| Logovi | 90 dana |

---

## 10. Infrastruktura (Azure)

### 10.1 Faza 0 — Kompletna arhitektura

```
rg-petplatform-prod
├── App Service Plan (Linux, B1)
│   └── App Service: api-petplatform (.NET Core API + SignalR + BackgroundServices)
├── Azure SQL Server
│   └── Database: sqldb-petplatform (Basic, 5 DTU, 2GB)
├── Azure SignalR Service (Free tier)
└── Azure Content Safety (F0 Free)

rg-petplatform-shared
├── Storage Account: stpetplatform
│   ├── Container: core-media (slike oglasa, ljubimaca, sightings)
│   ├── Container: social-media (prazan, priprema za Fazu 1.5)
│   └── Container: admin (budući PDF-ovi, exporti)
├── Azure Static Web Apps: web-petplatform (React frontend, Free tier, CDN)
└── DNS Zone: petplatform.rs
```

### 10.2 Konfiguracija servisa

| Servis | Parametar | Vrednost |
|---|---|---|
| App Service | OS: Linux, Runtime: .NET 8, Plan: B1, Always On: Da | Region: West Europe |
| SQL Database | Tier: Basic (5 DTU), Backup: Azure managed 7d, TDE: On | Firewall: samo Azure servisi |
| Blob Storage | GPv2, Standard, LRS, Hot tier | Lifecycle policy za cleanup |
| Static Web Apps | Free, React/Vite, custom domain, SSL managed | CDN ugrađen |
| SignalR | Free tier (20 konekcija, 20k poruka/dan) | Default service mode |
| Firebase | FCM (besplatno), Analytics (besplatno), Crashlytics (besplatno) | Google servis |

### 10.3 CI/CD Pipeline (GitHub Actions)

```
.github/workflows/
├── api-deploy.yml          # .NET API → App Service
├── web-deploy.yml          # React → Static Web Apps
├── mobile-build.yml        # React Native → build artifacts
└── db-migration.yml        # EF Core migracije
```

**api-deploy.yml:** Checkout → .NET build → Unit tests → Publish → Deploy → Health check → Auto rollback ako fails.

**web-deploy.yml:** Checkout → npm install → npm build → Deploy (Azure Static Web Apps built-in action).

### 10.4 IaC — Bicep

```
infra/
├── main.bicep
├── modules/
│   ├── appservice.bicep
│   ├── sql.bicep
│   ├── storage.bicep
│   ├── signalr.bicep
│   ├── staticwebapp.bicep
│   └── monitoring.bicep
└── parameters/
    ├── prod.parameters.json
    └── staging.parameters.json
```

### 10.5 Health Checks i Monitoring

Health endpoint `/health` proverava: SQL konekcija, Blob Storage pristup, SignalR konekcija, BackgroundService-i.

**Application Insights metrike:** Request duration (<200ms target), Failed requests (%), Dependency duration, Custom events (`oglas_kreiran`, `amber_alert_poslat`, `chat_pokrenut`, `udomljavanje_zavrseno`), Exceptions.

**Alerting:** API down (3x fail → email+SMS), Error rate >5% (email), Spor API >2s (email), SQL DTU >80% (email).

### 10.6 Evolucija infrastrukture

| Faza | Dodaje se |
|---|---|
| Faza 0 | App Service B1, SQL Basic, Blob LRS, SignalR Free, Static Web Apps |
| Faza 1 | SQL → Standard S0, SignalR → Standard (~$49), Content Safety → S0 |
| Faza 1.5 | +App Service (Social Feed API), +Azure Container Instance (Python AI) |
| Faza 2+ | Azure CDN, Redis Cache, Azure Service Bus |
| Faza 3 | GPU instance, AKS |

---

## 11. Social Feed — Zasebni Mikroservis (Faza 1.5)

### 11.1 Ključna odluka

Social Feed je zaseban .NET API servis od prvog dana svog postojanja. Nikad ne ulazi u Core monolit.

**Razlozi:**
- Core platforma (Amber Alert, udomljavanje) mora da radi i kad Social Feed padne
- Različit scaling profil (Social = write-heavy, Core = read-heavy)
- Različiti SLA zahtevi
- Eksplozija media sadržaja ne sme da utiče na Core performance

### 11.2 Arhitektura (Faza 1.5)

```
Core API (.NET)                     Social Feed API (.NET)
├── Posts, Pets, Chat,              ├── SocialPosts, Likes,
│   Media, Amber Alert,             │   Comments, SocialMedia
│   Auth, Admin, Notifications      │
├── dbo.* schema                    ├── social.* schema (isti SQL server)
├── core-media/ Blob kontejner      ├── social-media/ Blob kontejner
└── Izdaje JWT                      └── Validira isti JWT (read-only)
```

### 11.3 Komunikacija između servisa

**Faza 1.5:** REST pozivi (Social → Core za user profil, Core → Social za follow-up evente).

**Faza 2+:** Event-driven (Azure Service Bus) za slabo spregnute evente (FollowUpCompleted, PostReachedThreshold).

### 11.4 Media razdvajanje

Polymorphic Media tabela pokriva **SAMO Core** (`EntityType: Post | Pet | PostSighting`). Social Feed ima sopstvenu `SocialPostMedia` tabelu u `social` schema-i. Image hashing radi samo na Core medijima.

---

## 12. Procena Troškova

### 12.1 Faza 0 — True MVP (150-300 korisnika)

| Servis | Tier | Mesečno (USD) |
|---|---|---|
| Azure App Service | B1 Linux | ~$13 |
| Azure SQL Database | Basic (5 DTU) | ~$5 |
| Azure Blob Storage | Hot LRS | ~$2 |
| Azure SignalR Service | Free | $0 |
| Firebase (FCM + Analytics) | Free | $0 |
| Azure Content Safety | F0 Free | $0 |
| Application Insights | Free quota | $0 |
| Static Web Apps | Free | $0 |
| Domen (.rs) | Godišnji | ~$1.5 |
| App Store nalozi | Apple + Google | ~$10 |
| **UKUPNO** | | **~$30-35/mesec** |

### 12.2 Faza 1 (500-2,000 korisnika)

| Promena | Nova cena |
|---|---|
| App Service → B2 | ~$26 |
| SQL → Standard S0 | ~$15 |
| SignalR → Standard | ~$49 |
| Content Safety → S0 | ~$5-15 |
| **UKUPNO** | **~$120-135/mesec** |

### 12.3 Faza 1.5 (2,000-5,000 korisnika)

| Dodatak | Cena |
|---|---|
| +App Service (Social Feed) | +$13 |
| +Azure Container Instance (Python AI) | +$15-25 |
| +Blob Storage (social) | +$5-10 |
| **UKUPNO** | **~$155-185/mesec** |

### 12.4 Optimizacija troškova

- **Azure Free Account:** $200 kredita + besplatne kvote prvih 12 meseci
- **Microsoft for Startups:** Do $150,000 Azure kredita za social impact projekte
- **Reserved instances:** 30-40% popusta na 1-godišnji commitment
- **SignalR optimizacija:** Zatvaranje konekcije u background-u (mobile), oslanjanje na FCM

---

## 13. Frontend Arhitektura (React / React Native)

### 13.1 Zajednički sloj

- **API layer** — deljeni HTTP klijent (axios/fetch wrapper) za Web i Mobile
- **State management** — Zustand ili Redux Toolkit
- **Server state** — TanStack Query (React Query) za keširanje, pagination, optimistic updates
- **Deljeni tipovi** — TypeScript interfejsi za API response-ove

### 13.2 React (Web)

- Vite kao build tool
- React Router za navigaciju
- Responsive dizajn (mobile-first)
- Azure Static Web Apps hosting (besplatno, CDN, SSL)

### 13.3 React Native (Mobile)

- React Navigation za navigaciju
- Push notifikacije preko FCM
- Kamera pristup (upload slika)
- Geolokacija (Amber Alert radijus, GPS consent flow)
- SignalR klijent (@microsoft/signalr)
- In-app popup banner komponenta (globalni NotificationProvider)

---

## 14. API Design Principi

- **Verzionisanje:** `/api/v1/` od prvog dana
- **Konzistentna error struktura:**
  ```json
  {
    "error": {
      "code": "ALERT_LIMIT_EXCEEDED",
      "message": "Već imate aktivan oglas za izgubljenog ljubimca u poslednjih 24h.",
      "details": {}
    }
  }
  ```
- **Pagination:** Cursor-based za feed (efikasnije od offset-a)
- **Swagger/OpenAPI** dokumentacija automatski generisana
- **Autentifikacija:** JWT Bearer token u Authorization header-u
- **Rate limiting:** Per-endpoint per-user, globalni per-IP

---

## 15. Monitoring i Logging

- **Application Insights** (Azure) — performance monitoring, custom events, alerting
- **Structured logging** (Serilog) — JSON format, queryable
- **Health checks** — `/health` endpoint za svaki servis (SQL, Blob, SignalR, BackgroundServices)
- **Firebase Analytics** — mobile metrike, kohorte, retention
- **Firebase Crashlytics** — crash reporting za React Native
- **Custom eventi:** `oglas_kreiran`, `amber_alert_aktiviran`, `chat_pokrenut`, `udomljavanje_zavrseno`, `vidim_ovog_psa_klik`
- **Alerting:** API down, error rate >5%, response >2s, SQL DTU >80%

---

## 16. Test strategija

### 16.1 Prioritet za Fazu 0

| Tip testa | Šta se testira | Prioritet |
|---|---|---|
| Unit (Domain) | TrustScore kalkulacija, value objects, domain pravila | Visok |
| Unit (Application) | CQRS handleri, validatori, feed algoritam | Visok |
| Integration (API) | Kontroleri sa in-memory DB, middleware | Srednji |
| E2E | Kompletni user journey-ji | Faza 1+ |

### 16.2 Ključni testovi

- TrustScoreCategory pragovi (0-19 → Nov, 20-49 → Aktivan, 50-99 → Pouzdan, 100+ → Lokalni heroj)
- Feed sorting (Izgubljen uvek na vrhu, TrustScore boost)
- Amber Alert geo query (GPS + LocationZone fallback)
- Rate limiting pravila (1 Izgubljen/24h, 3 objave/dan)
- Keyword filter (prodaja, kontakt van app-a)
- Community report auto-hide (težinska kalkulacija)
- Validaciona pravila za sve commands (obavezna polja, min dužine)

---

## 17. Buduće razmatranje — van scope-a Faze 0

| Tema | Faza | Napomena |
|---|---|---|
| AI prepoznavanje rasa | 1.5 | Python FastAPI + pretrained ResNet/EfficientNet |
| AI Detektiv (facial recognition) | 3 | Custom model, zahteva veliku bazu slika |
| AI Matchmaker | 3 | ML scoring kviz za uparivanje |
| Elasticsearch | 2+ | Kad SQL LIKE ne bude dovoljan za full-text pretragu |
| CDN za slike | 2+ | Azure CDN za brže serviranje |
| Redis Cache | 2+ | Keširanje feed-a, Trust Score-ova |
| Multi-region | 3 | Kad platforma izađe iz Srbije |
| Kubernetes (AKS) | 3 | Orkestracija svih mikroservisa |
