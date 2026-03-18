# Business Rules Reference

## Trust Score Sistem

### Faza 0 — Lite (dekorativni, ne gate-uje ništa)

| Akcija | Poeni | Napomena |
|---|---|---|
| Verifikovan identitet | +20 | Jednokratno |
| Objavljen oglas sa svim obaveznim poljima | +2 | |
| Pozitivne reakcije zajednice | +2 | Cap: +10/mesec |
| Zatvaranje "Izgubljen" sa "Pas nađen" | +5 | |
| Potvrđena prijava od zajednice | -30 | |
| Neodgovaranje na poruke >48h (aktivan oglas) | -5 | |

### Faza 1 — Aktivni (gate-uje feature-e)

Dodatne akcije:
- Uspešno udomljavanje (potvrđeno obe strane): +25
- Završen foster: +15
- Validna prijava zloupotrebe: +10
- Nezatvaranje oglasa kad je razrešen: -10
- Prodaja maskirana kao udomljavanje: -50 + ban upozorenje

Gate-ovi:
- Progresivni Amber Alert radijus (5→10→20km): zahteva 50+ poena
- Viši score = blagi boost u feed-u

### Kategorije (korisnik vidi samo kategoriju, ne tačan broj)

| Score | Kategorija | UI oznaka |
|---|---|---|
| 0-19 | New | "Nov korisnik" |
| 20-49 | Active | "Aktivan" |
| 50-99 | Trusted | "Pouzdan" |
| 100+ | LocalHero | "Lokalni heroj" |

### Anti-gaming pravila

- Score ne može biti negativan (minimum 0)
- Isti korisnik ne može "udomljavati" od istog korisnika više puta (Faza 1)
- Device fingerprinting za detekciju alt naloga (Faza 1)
- CommunityReaction cap: max +10 poena mesečno od reakcija
- IdentityVerified: jednokratno, ne može se ponoviti

---

## Amber Alert Sistem

### Ključna arhitekturna odluka

**Nema zasebnog Amber Alert sistema.** Objava kategorije `PostCategory.Lost` automatski:
1. Kreira Post sa `IsUrgent = true` i `ExpiresAt = UtcNow + 7 dana`
2. Emituje `AmberAlertActivatedEvent` koji handler preuzima i šalje push notifikacije

### Geo logika — ko dobija push

```
Korisnik objavi "Izgubljen" sa lokacijom incidenta (GpsCoordinates)
    → Backend SELECT korisnici WHERE:
        a) LastKnownLocation u radijusu 10km od IncidentLocation (precizni)
        b) ILI LocationZone.Municipality = ista opština (fallback za korisnike bez GPS-a)
    → Filtrirati: IsActive=true, AmberAlertNotificationsEnabled=true
    → SendPushToManyAsync()
```

### Ograničenja (Faza 0)

- Max **1 "Izgubljen" objava na 24h** po korisniku (rate limiting u Application sloju)
- Objava traje **7 dana**, posle toga prompt za produženje ili zatvaranje
- Svi registrovani korisnici mogu objaviti "Izgubljen" — bez Trust Score gate-a u Fazi 0
- Community report za lažne prijave → moderator uklanja

### Faza 1 — Progresivni radijus

```
Dan 1: 5km radijus
Dan 2 (24h bez razrešenja): automatski širi na 10km
Dan 3 (48h bez razrešenja): automatski širi na 20km
```

Zahteva Trust Score 50+ za aktivaciju (gate uveden u Fazi 1).

### Životni ciklus objave "Izgubljen"

```
Active → [korisnik ažurira] → Updated
Active → [korisnik zatvori "pas nađen"] → Resolved (+5 Trust Score)
Active → [7 dana bez akcije] → Expired (Hangfire job, -10 Trust Score u Fazi 1)
Expired → [korisnik produži] → Active (re-verifikacija)
```

### "Vidim ovog psa" dugme

Svaka "Izgubljen" objava ima dugme "Vidim ovog psa":
1. Korisnik klikne → otvara mini-formular (GPS auto ili ručni unos, opcioni komentar)
2. Kreira `PetSighting` entitet
3. Emituje `PetSightingReportedEvent`
4. Handler šalje push notifikaciju vlasniku: "Neko je video vašeg psa!"
5. Sighting se prikazuje na objavi kao update (lokacija vidljiva svima)

### Push notifikacija sadržaj

```
Naslov: "Izgubljen pas u vašem kraju"
Telo: "{ime psa} — {rasa}, {boja}. {opština}. Videli ste ga?"
Data: { "postId": "{id}", "type": "amber_alert" }
```

---

## Moderacija — Troslojna Strategija

### Sloj 1: Automatska moderacija (pre objavljivanja)

| Mehanizam | Pravilo |
|---|---|
| Obavezna polja | Bez slike + lokacije + opisa → nema objavljivanja |
| Rate limiting | Max 3 objave/dan po korisniku (Udruženja izuzeta) |
| Keyword filter | "prodajem", "cena", "dinara", "evra", "simbolična nadoknada" → flag |
| Image hashing | Ista slika na dva profila → automatski flag |
| Azure Content Safety | Neprimerene slike → odbijanje uploada |
| Lost post rate limit | Max 1 "Izgubljen" na 24h po korisniku |

**VAŽNO:** Nikada pre-moderacija (čekanje manuelnog odobrenja pre objavljivanja).
Objava ide live odmah, filteri rade post-hoc.

### Sloj 2: Community moderacija (post-objavljivanja)

| Mehanizam | Pravilo |
|---|---|
| "Prijavi objavu" dugme | Svaki korisnik, bira razlog (ReportReason enum) |
| Auto-skrivanje | 3 prijave od **različitih** korisnika → `IsHidden = true` → admin red |
| Trust Score težina | Prijava od Trusted korisnika = 2 prijave od New korisnika |
| Udruženja | Njihove prijave imaju veću težinu (Faza 1 — formalizovano) |

### Sloj 3: Admin (15 min/dan ritual)

Admin dashboard prikazuje:
- Flagovane objave (keyword + community report) sa dugmićima: Odobri / Ukloni / Upozori / Suspenduj / Banuj
- Prijavljene korisnike sa istorijom prijava
- Osnovne brojke: ukupno korisnika, objava danas, aktivnih "Izgubljen" oglasa

### Sankcije

| Nivo | Opis | Trajanje |
|---|---|---|
| Warning | Notifikacija korisniku, prvi prekršaj | — |
| TemporarySuspension | Ne može da objavljuje, može da čita | 7–30 dana |
| PermanentBan | Potpuno uklanjanje, podaci sačuvani | Trajno |

### Skaliranje moderacije

| Korisnika | Pristup |
|---|---|
| 0-300 | Admin + automatski filteri + community |
| 300-1,000 | +2-3 volontera iz udruženja kao moderatori |
| 1,000-5,000 | Formalizovani moderator tim |
| 5,000+ | AI moderacija, eskalirani slučajevi ljudima |

---

## Notifikacije — Pravila

### Kategorije i defaulti

| Kategorija | Može se isključiti | Default | Kanal |
|---|---|---|---|
| AmberAlert | Da | Uključen | Push + in-app |
| ChatMessage | NE | Uvek uključen | Push + in-app |
| StatusChange | Da | Uključen | Push + in-app |
| TrustScore | N/A | — | Samo in-app |

### Šta se ne šalje u Fazi 0

- Notifikacije o novim udomljavanjima (Faza 1 sa personalizacijom)
- Weekly digest (premalo korisnika)
- Marketinške notifikacije (nikada do 10,000+ korisnika)

### Notifikacioni tekstovi

```
AmberAlert push:
  Naslov: "Izgubljen pas u vašem kraju"
  Telo: "[Ime] — [Rasa], [boja]. [Opština]. Jeste li ga videli?"

Sighting push (vlasniku):
  Naslov: "Neko je video vašeg psa!"
  Telo: "Viđen u [zona], [pre X minuta]. Kliknite za detalje."

Chat push:
  Naslov: "Nova poruka o [naslov oglasa]"
  Telo: "[Ime pošiljaoca]: [prvih 50 karaktera poruke]"

TrustScore in-app:
  "+5 poena: Pas nađen — oglas zatvoren"
  "Napredak: Nov → Aktivan korisnik!"
```

---

## Feed Algoritam

### Prioriteti (apsolutni, ne mogu se promeniti)

1. **"Izgubljen" objave** — uvek na vrhu, sortirane po datumu (najnovije prvo)
2. **Ostale urgentne** (UrgentHelp u Fazi 1)
3. **Sve ostalo** — hronološki, sa blagim boostom za Trusted/LocalHero autore

### Filtriranje

- Default: objave iz iste opštine kao LocationZone korisnika
- Korisnik može da proširi filter na susedne opštine (Faza 1)
- Kategorizacija: Udomljavanje / Izgubljen / Nađen (Faza 0 tabovi)

### Paginacija

- Cursor-based (efikasnije od offset za real-time feed)
- Page size: 20 objava
- "Izgubljen" objave su uvek prisutne bez obzira na cursor

---

## Onboarding Kviz — Logika

```
Pitanje 1: "Tražiš psa za udomljavanje?"
  → DA: feed filtriran na Adoption kategoriju

Pitanje 2: "Izgubio si ljubimca?"
  → DA: preskočiti ostala pitanja, direktno na "Prijavi nestanak" CTA
        registracija minimalna (samo email + ime)

Pitanje 3: "Želiš da pomogneš kao foster/volonter?"
  → DA: označi korisnika za Foster onboarding u Fazi 1

Bez odgovora ili sve NE:
  → Default feed (sve kategorije, lokacija korisnika)
```

**Kritično za Pitanje 2:** Registracija mora biti 2 polja max.
Korisnik u panici ne sme da prolazi kroz standard flow.
