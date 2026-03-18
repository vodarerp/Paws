# DB Šema Reference

## Tehnologija

- **SQL Server** (Azure SQL Database)
- **EF Core** sa Code First Migrations
- **SQL Server `geography`** tip za geo upite (Amber Alert radijus)
- Slike se čuvaju u **Azure Blob Storage** — u bazi samo URL-ovi (nikad binary)

---

## Tabele

### Users
```sql
Users
├── Id                  UNIQUEIDENTIFIER  PK
├── Email               NVARCHAR(256)     NOT NULL, UNIQUE
├── DisplayName         NVARCHAR(100)     NOT NULL
├── AvatarUrl           NVARCHAR(500)     NULL
├── Role                INT               NOT NULL  -- UserRole enum
├── Municipality        NVARCHAR(100)     NOT NULL  -- LocationZone VO
├── Settlement          NVARCHAR(100)     NULL      -- LocationZone VO
├── LastKnownLatitude   FLOAT             NULL      -- GpsCoordinates VO (NIKAD u API response)
├── LastKnownLongitude  FLOAT             NULL      -- GpsCoordinates VO
├── GeoLocation         geography         NULL      -- Computed za spatial upite
├── TrustScoreValue     INT               NOT NULL  DEFAULT 0
├── IsVerified          BIT               NOT NULL  DEFAULT 0
├── IsActive            BIT               NOT NULL  DEFAULT 1
├── LastActiveAt        DATETIME2         NULL
├── AmberAlertNotif     BIT               NOT NULL  DEFAULT 1
├── StatusChangeNotif   BIT               NOT NULL  DEFAULT 1
├── CreatedBy           UNIQUEIDENTIFIER  NOT NULL
├── UpdatedBy           UNIQUEIDENTIFIER  NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (Municipality, IsActive)
INDEX: (GeoLocation)  -- SPATIAL INDEX za radijus upite
```

### Pets
```sql
Pets
├── Id                  UNIQUEIDENTIFIER  PK
├── OwnerId             UNIQUEIDENTIFIER  FK → Users.Id
├── Name                NVARCHAR(100)     NOT NULL
├── Breed               NVARCHAR(100)     NOT NULL
├── AgeMonths           INT               NULL
├── Gender              INT               NOT NULL  -- PetGender enum
├── Size                INT               NOT NULL  -- PetSize enum
├── ChipNumber          NVARCHAR(50)      NULL
├── IsSterilized        BIT               NOT NULL  DEFAULT 0
├── Color               NVARCHAR(100)     NULL
├── DistinctiveMarks    NVARCHAR(500)     NULL
├── PhotoUrls           NVARCHAR(MAX)     NOT NULL  -- JSON array URL-ova
├── Status              INT               NOT NULL  -- PetStatus enum
├── AiDetectedBreed     NVARCHAR(100)     NULL      -- Faza 1.5
├── AiConfidence        FLOAT             NULL      -- Faza 1.5
├── CreatedBy           UNIQUEIDENTIFIER  NOT NULL
├── UpdatedBy           UNIQUEIDENTIFIER  NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (OwnerId)
INDEX: (Status)
```

### Posts
```sql
Posts
├── Id                  UNIQUEIDENTIFIER  PK
├── AuthorId            UNIQUEIDENTIFIER  FK → Users.Id
├── PetId               UNIQUEIDENTIFIER  FK → Pets.Id  NULL
├── Category            INT               NOT NULL  -- PostCategory enum
├── Title               NVARCHAR(200)     NOT NULL
├── Content             NVARCHAR(2000)    NOT NULL
├── MediaUrls           NVARCHAR(MAX)     NOT NULL  -- JSON array
├── PrimaryImageHash    NVARCHAR(64)      NULL      -- Za detekciju duplikata
├── Municipality        NVARCHAR(100)     NOT NULL
├── Settlement          NVARCHAR(100)     NULL
├── IncidentLatitude    FLOAT             NULL      -- Za Lost/Found
├── IncidentLongitude   FLOAT             NULL
├── IsUrgent            BIT               NOT NULL  DEFAULT 0
├── Status              INT               NOT NULL  DEFAULT 0  -- PostStatus.Active
├── ExpiresAt           DATETIME2         NULL      -- Samo za Lost (7 dana)
├── ReportCount         INT               NOT NULL  DEFAULT 0
├── IsHidden            BIT               NOT NULL  DEFAULT 0
├── CreatedBy           UNIQUEIDENTIFIER  NOT NULL
├── UpdatedBy           UNIQUEIDENTIFIER  NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (AuthorId, CreatedAt DESC)
INDEX: (Category, Status, CreatedAt DESC)  -- Osnova feed query-ja
INDEX: (IsUrgent, CreatedAt DESC)          -- Hitni oglasi na vrhu
INDEX: (Municipality, Status)
INDEX: (PrimaryImageHash)                  -- Detekcija duplikata
INDEX: (ExpiresAt) WHERE ExpiresAt IS NOT NULL  -- Hangfire expired job
```

### PetSightings
```sql
PetSightings
├── Id                  UNIQUEIDENTIFIER  PK
├── PostId              UNIQUEIDENTIFIER  FK → Posts.Id
├── ReporterId          UNIQUEIDENTIFIER  FK → Users.Id
├── Latitude            FLOAT             NOT NULL
├── Longitude           FLOAT             NOT NULL
├── SeenAt              DATETIME2         NOT NULL
├── Comment             NVARCHAR(500)     NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (PostId, SeenAt DESC)
```

### ChatConversations
```sql
ChatConversations
├── Id                  UNIQUEIDENTIFIER  PK
├── PostId              UNIQUEIDENTIFIER  FK → Posts.Id
├── InitiatorId         UNIQUEIDENTIFIER  FK → Users.Id
├── PostOwnerId         UNIQUEIDENTIFIER  FK → Users.Id
├── PhoneShared         BIT               NOT NULL  DEFAULT 0
├── LastMessageAt       DATETIME2         NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (InitiatorId, LastMessageAt DESC)
INDEX: (PostOwnerId, LastMessageAt DESC)
UNIQUE: (PostId, InitiatorId)  -- Jedna konverzacija po paru za isti oglas
```

### ChatMessages
```sql
ChatMessages
├── Id                  UNIQUEIDENTIFIER  PK
├── ConversationId      UNIQUEIDENTIFIER  FK → ChatConversations.Id
├── SenderId            UNIQUEIDENTIFIER  FK → Users.Id
├── Content             NVARCHAR(2000)    NOT NULL
├── IsRead              BIT               NOT NULL  DEFAULT 0
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (ConversationId, CreatedAt ASC)
INDEX: (SenderId, IsRead)
```

### TrustScoreHistory
```sql
TrustScoreHistory
├── Id                  UNIQUEIDENTIFIER  PK
├── UserId              UNIQUEIDENTIFIER  FK → Users.Id
├── Action              INT               NOT NULL  -- TrustScoreAction enum
├── Points              INT               NOT NULL
├── BalanceBefore       INT               NOT NULL
├── BalanceAfter        INT               NOT NULL
├── Description         NVARCHAR(200)     NULL
├── ReferenceId         UNIQUEIDENTIFIER  NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (UserId, CreatedAt DESC)
```

### Reports
```sql
Reports
├── Id                  UNIQUEIDENTIFIER  PK
├── ReporterId          UNIQUEIDENTIFIER  FK → Users.Id
├── TargetType          NVARCHAR(50)      NOT NULL  -- "Post", "User"
├── TargetId            UNIQUEIDENTIFIER  NOT NULL
├── Reason              INT               NOT NULL  -- ReportReason enum
├── Evidence            NVARCHAR(1000)    NULL
├── Status              NVARCHAR(50)      NOT NULL  DEFAULT 'Pending'
├── AdminNotes          NVARCHAR(500)     NULL
├── ResolvedAt          DATETIME2         NULL
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (Status, CreatedAt DESC)  -- Admin dashboard query
INDEX: (TargetType, TargetId)
```

### Notifications
```sql
Notifications
├── Id                  UNIQUEIDENTIFIER  PK
├── UserId              UNIQUEIDENTIFIER  FK → Users.Id
├── Category            INT               NOT NULL  -- NotificationCategory enum
├── Title               NVARCHAR(200)     NOT NULL
├── Body                NVARCHAR(500)     NOT NULL
├── ReferenceId         UNIQUEIDENTIFIER  NULL
├── ReferenceType       NVARCHAR(50)      NULL
├── IsRead              BIT               NOT NULL  DEFAULT 0
├── IsPush              BIT               NOT NULL  DEFAULT 0
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

INDEX: (UserId, IsRead, CreatedAt DESC)
```

### FcmTokens
```sql
FcmTokens
├── Id                  UNIQUEIDENTIFIER  PK
├── UserId              UNIQUEIDENTIFIER  FK → Users.Id
├── Token               NVARCHAR(500)     NOT NULL
├── DeviceType          NVARCHAR(50)      NULL  -- "ios", "android", "web"
├── CreatedAt           DATETIME2         NOT NULL
└── UpdatedAt           DATETIME2         NULL

UNIQUE: (UserId, Token)
```

---

## Geo Upiti (Amber Alert radijus)

### SQL Server geography tip

```sql
-- Dodati kolonu na Users tabelu (via EF Core migration)
ALTER TABLE Users
ADD GeoLocation AS geography::Point(LastKnownLatitude, LastKnownLongitude, 4326)
    PERSISTED;

CREATE SPATIAL INDEX SI_Users_GeoLocation
    ON Users(GeoLocation)
    USING GEOGRAPHY_AUTO_GRID;
```

### Radijus upit (EF Core raw SQL)

```csharp
// Infrastructure/Persistence/Repositories/UserRepository.cs
public async Task<IEnumerable<Guid>> GetUsersInRadiusAsync(
    GpsCoordinates center, double radiusMeters, CancellationToken ct = default)
{
    var point = $"geography::Point({center.Latitude}, {center.Longitude}, 4326)";

    return await _context.Users
        .FromSqlRaw($@"
            SELECT Id FROM Users
            WHERE IsActive = 1
              AND AmberAlertNotificationsEnabled = 1
              AND GeoLocation IS NOT NULL
              AND GeoLocation.STDistance({point}) <= {{0}}",
            radiusMeters)
        .Select(u => u.Id)
        .ToListAsync(ct);
}

public async Task<IEnumerable<Guid>> GetUsersByZoneAsync(
    LocationZone zone, CancellationToken ct = default)
{
    return await _context.Users
        .Where(u => u.IsActive
                 && u.AmberAlertNotificationsEnabled
                 && u.Municipality == zone.Municipality)
        .Select(u => u.Id)
        .ToListAsync(ct);
}
```

---

## Feed Query (optimizovani)

```csharp
// Infrastructure/Persistence/Repositories/PostRepository.cs
public async Task<IEnumerable<Post>> GetFeedAsync(
    Guid userId, LocationZone zone, PostCategory? filter,
    int page, CancellationToken ct = default)
{
    var query = _context.Posts
        .Include(p => p.Author)
        .Where(p => p.Municipality == zone.Municipality
                 && p.Status == PostStatus.Active
                 && !p.IsHidden);

    if (filter.HasValue)
        query = query.Where(p => p.Category == filter.Value);

    // KRITIČNO: Lost uvek na vrhu
    return await query
        .OrderByDescending(p => p.IsUrgent)    // Lost/UrgentHelp prvo
        .ThenByDescending(p => p.CreatedAt)    // Unutar grupe — hronološki
        .Skip((page - 1) * 20)
        .Take(20)
        .ToListAsync(ct);
}
```

---

## Važne DB Napomene

1. **Binary podaci nikad u SQL-u** — slike/videi idu u Azure Blob, baza čuva samo URL
2. **LocationZone je Owned Entity** — Municipality i Settlement su kolone na tabeli, ne FK
3. **GpsCoordinates je Owned Entity** — Latitude/Longitude su kolone (IncidentLatitude, IncidentLongitude)
4. **GeoLocation kolona** je computed/persisted na Users tabeli za spatial indexing
5. **JSON kolone** za liste (MediaUrls, PhotoUrls) — dovoljno za MVP, Faza 3 razmatranje za normalizaciju
6. **Soft delete** nije implementiran u Fazi 0 — `IsActive` flag na Users, `Status = Removed` na Posts
7. **Svi Id-evi su GUID** — ne sequence int (lakše za buduće šarding i import podataka)
