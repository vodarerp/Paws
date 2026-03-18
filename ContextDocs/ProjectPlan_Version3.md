# 🐾 Platforma za Ljubimce (Srbija) — ProjectPlan v3.0

> **Status:** Finaliziran plan za implementaciju
> **Datum:** Mart 2026
> **Tim:** Solo developer + Claude ekosistem
> **Grad lansiranja:** Novi Sad (zatvorena beta → meki launch → širenje)

---

## 1. Vizija i Svrha

Centralizovana digitalna platforma (Web + Mobilna aplikacija) koja zamenjuje haotične FB/Viber grupe jedinstvenim ekosistemom za **udomljavanje**, **pronalaženje izgubljenih ljubimaca** i **zaštitu životinja** u Srbiji.

**Ključni diferenciator:** Amber Alert sistem sa push notifikacijama baziranim na lokaciji — nešto što FB grupe nikada ne mogu da ponude.

---

## 2. Strategija Lansiranja

### 2.1 Zašto Novi Sad prvo

- Postojeći kontakti sa udruženjima i FB grupama
- Rešava cold start problem — platforma živi od gustine korisnika
- Manji grad = lakše postići kritičnu masu nego Beograd
- Kontrolisano okruženje za testiranje pre širenja

### 2.2 Tri koraka lansiranja

**Korak 1 — Zatvorena beta (4-8 nedelja pre lansiranja)**
- Pozvati 3-5 udruženja iz Novog Sada i najaktivnije ljude iz FB grupa
- Oni popune platformu realnim oglasima pre javnog lansiranja
- Kad novi korisnik uđe, vidi živ feed — ne praznu aplikaciju

**Korak 2 — Meki launch u Novom Sadu**
- Otvoriti za javnost, promocija kroz FB grupe i udruženja
- Poruka: "Koristite ovo KAO DODATAK za Amber Alert, verifikaciju i foster mrežu" — ne "zamenite FB grupu"
- Cilj: 150-300 korisnika u prvih 30 dana

**Korak 3 — Širenje na sledeći grad**
- Tek kad metrike u NS pokažu traction (videti KPI sekciju)
- Ista metoda: najpre anchor udruženja, pa javnost
- Verovatno Beograd kao drugi grad

---

## 3. Faze Razvoja

### 3.0 Faza 0 — True MVP (Lansiranje)

| Feature | Opis |
|---|---|
| **Oglasna tabla** | 3 kategorije: Udomljavanje, Izgubljen, Nađen |
| **Amber Alert (integrisani)** | Objava "Izgubljen" automatski šalje push notifikaciju u radijusu 10km |
| **Profili korisnika** | Osnovni profil sa lokacijom (opština + opcioni GPS) |
| **Profili ljubimaca** | Osnovna kartica: ime, rasa (dropdown), starost, slike |
| **In-app chat** | Bez otkrivanja broja telefona dok korisnik ne odobri |
| **Onboarding kviz** | 3 pitanja za personalizaciju feed-a |
| **Trust Score Lite** | Vidljiv, akumulira se, ali ne gate-uje ništa |
| **Moderacija** | Automatski filteri + community reporting + admin panel (minimalni) |
| **Push notifikacije** | FCM integracija za Amber Alert i chat poruke |

### 3.1 Faza 1

| Feature | Opis |
|---|---|
| Trust Score "aktivan" | Gate-ovi, uticaj na feed ranking |
| Foster lista | Strukturirani profili fostera, osnovno matchmaking |
| QR verifikacija primopredaje | QR + selfi potvrda |
| Follow-up sistem | Podsetnici 1/3/6 meseci posle udomljavanja |
| Prijava zlostavljanja | Formular + PDF generisanje |
| Veterinarski karton | Digitalna zdravstvena kartica ljubimca |
| Veterinarski podsetnici | Automatski podsetnici za vakcinacije, tretmane |
| Admin panel za udruženja | Dashboard, statistika |
| Osnovno poređenje Nađen ↔ Izgubljen | Rule-based: ista rasa + boja + lokacija + datum |
| Pametni push za "Nađen" | Push samo korisnicima sa sličnim aktivnim "Izgubljen" oglasom |
| Donacije/kampanje | Sa transparentnošću troškova |
| Progresivni Amber Alert radijus | 5km → 10km → 20km posle 24h/48h |

### 3.2 Faza 1.5

| Feature | Opis |
|---|---|
| AI prepoznavanje rasa | Python FastAPI + pretrained model (ResNet/EfficientNet) |
| Pametni push za "Nađen" (AI) | AI-baziran matching umesto rule-based |
| Social feed — "Zajednica" tab | Instagram-stil feed za slike/videe ljubimaca, odvojen od glavnog feed-a |

> **Uslov za aktiviranje Faze 1.5:** minimum 500+ aktivnih korisnika mesečno.

### 3.3 Faza 3

| Feature | Opis |
|---|---|
| AI Detektiv | Facial recognition za pse — pet re-identification modeli |
| AI Matchmaker | ML scoring kviz za uparivanje korisnik-ljubimac |
| Integracija sa državnim službama | Komunalna inspekcija, policija |
| Affiliate program | Pet shopovi, personalizovani kuponi |
| Crna lista deljenja | Deljenje sa partnerskim udruženjima |

---

## 4. Amber Alert Sistem (Faza 0)

### 4.1 Ključna odluka: Integrisani model

Nema zasebnog "Amber Alert" sistema. Objava kategorije "Izgubljen" automatski aktivira push notifikaciju. Iz perspektive korisnika: postavljaš oglas za izgubljenog psa, sistem odradi ostalo.

**Razlika između obične objave i Amber Alert-a:**

| Aspekt | Obična objava (Udomljavanje, Nađen) | "Izgubljen" objava (Amber Alert) |
|---|---|---|
| Push notifikacija | NE | DA — svima u radijusu 10km |
| Pozicija u feed-u | Hronološki | Uvek na vrhu |
| Vizuelni dizajn | Standardni | Crvena, ikonica hitnosti |
| Poseban CTA | "Pošalji poruku" | "Vidim ovog psa" (sa lokacijom) |
| Trajanje | Bez limita | 7 dana, pa prompt za produženje |

### 4.2 Ograničenja

- **1 objava "Izgubljen" na 24h** po korisniku (rate limiting)
- **Objava traje 7 dana**, posle toga korisnik bira: produži ili zatvori
- **Community report** za lažne prijave → moderator uklanja

### 4.3 Pristup

- Svi registrovani korisnici mogu da objave "Izgubljen" — nema Trust Score gate-a u Fazi 0
- Rate limiting je dovoljna zaštita na malom broju korisnika
- Trust Score gate (50+ za aktivaciju) se uvodi u Fazi 1 kad baza poraste

### 4.4 Dugme "Vidim ovog psa"

Korisnik koji vidi izgubljenog psa klikne dugme → otvara se mini-formular:
- Lokacija (automatski GPS ili ručni unos)
- Vreme viđenja
- Kratki komentar (opciono)
- Šalje se kao notifikacija vlasniku + pojavljuje se na objavi kao update

### 4.5 Životni ciklus

```
Objavljen → [opciono: Ažuriran novim info] → Razrešen (pas nađen) / Istekao (7 dana)
```

- Korisnik koji zatvori oglas sa "Pas nađen" → dobija Trust Score poene
- Korisnik koji ne zatvori posle 7 dana → prompt za produženje ili zatvaranje
- Brend "Amber Alert" se koristi u UI-u kao korisnički termin

---

## 5. Trust Score Sistem (Faza 0 — Lite verzija)

### 5.1 Princip

Trust Score je **vidljiv od dana 1** ali **ne utiče na funkcionalnost** u Fazi 0. Korisnici od početka grade reputaciju, što ih vezuje za platformu i priprema za Fazu 1 kad Score počinje da gate-uje feature-e.

### 5.2 Faza 0 — Pozitivne akcije

| Akcija | Poeni |
|---|---|
| Verifikovan identitet (jednokratno) | +20 |
| Objavljen oglas sa svim obaveznim poljima | +2 |
| Pozitivne reakcije zajednice na objavu | +2 (cap +10/mesec) |
| Zatvaranje "Izgubljen" oglasa sa "Pas nađen" | +5 |

### 5.3 Faza 0 — Negativne akcije

| Akcija | Poeni |
|---|---|
| Potvrđena prijava od drugih korisnika | -30 |
| Neodgovaranje na poruke >48h (aktivan oglas) | -5 |

### 5.4 Vidljivost

- Korisnik vidi **svoj score i istoriju promena**
- Drugi korisnici vide **kategoriju** (ne tačan broj):
  - **Nov korisnik** (0-19) → **Aktivan** (20-49) → **Pouzdan** (50-99) → **Lokalni heroj** (100+)
- Sprečava "gaming" sistema

### 5.5 Faza 1 — Trust Score "aktivan"

- Amber Alert (kad se uvede progresivni radijus) zahteva 50+ poena
- Viši score daje blagi boost u feed-u
- Dodatne akcije: uspešno udomljavanje (+25), završen foster (+15), validna prijava zloupotrebe (+10)
- Dodatne penalizacije: prodaja maskirana kao udomljavanje (-50 + ban upozorenje), nezatvaranje oglasa (-10)
- Anti-gaming: device fingerprinting, verifikacija obe strane

---

## 6. Korisnički Flow-ovi (User Journey)

### 6.1 Journey: "Želim da udomim psa"

```
Prva poseta → Onboarding: "Tražiš psa za udomljavanje?" → DA
  → Registracija (email + lozinka ili Google login)
  → Feed filtriran na "Udomljavanje" u Novom Sadu
  → Pregleda oglase (slike, starost, opis, zdravstveno stanje)
  → Klikne na oglas
      → Detalji psa + profil autora + Trust Score kategorija
      → "Pošalji poruku" → in-app chat
  → Dogovaraju se o upoznavanju (van aplikacije)
  → Odluka o udomljavanju
      → Autor menja status u "U procesu udomljavanja"
      → Posle primopredaje → autor zatvara oglas: "Udomljen"
      → Obe strane dobijaju Trust Score poene
  → Korisnik ima ljubimca u profilu (osnovna kartica)
```

**Napomene:**
- Ako više ljudi želi istog psa: autor vidi sve poruke, bira sam. Oglas ostaje vidljiv dok ga autor ne stavi u "U procesu"
- Udomljavanje mimo platforme je OK — ne forsirati da sve ide kroz aplikaciju

### 6.2 Journey: "Izgubio sam psa"

```
Korisnik (registrovan ili nov u panici)
  → Onboarding (ako nov): "Izgubio si ljubimca?" → DA
  → Brza registracija (minimum: email + ime)
  → "Prijavi nestanak" (prominentan CTA)
      → Formular:
        - Fotografija psa (obavezno)
        - Ime, rasa, boja, posebni znaci
        - Lokacija nestanka (mapa ili adresa)
        - Vreme nestanka
        - Kontakt preferenca (chat / telefon)
      → Objava na feed: "Izgubljen" (vizuelno istaknuta, uvek na vrhu)
      → AUTOMATSKI: push notifikacija korisnicima u radijusu 10km
  → Korisnici u blizini vide push → kliknu "Vidim ovog psa" (lokacija + info)
  → Vlasnik dobija notifikaciju → chat → dogovor
  → Pas nađen → korisnik zatvara oglas → Trust Score poeni
```

**Kritično:** Registracija za ovaj flow mora biti MAKSIMALNO kratka. Korisnik u panici ne sme da prolazi kroz 5 koraka. Minimum polja, sve ostalo se popunjava naknadno.

### 6.3 Journey: "Našao sam psa"

```
Korisnik (registrovan ili nov)
  → "Prijavi nađenog psa"
      → Formular:
        - Fotografija (obavezno)
        - Lokacija nalaženja
        - Opis (veličina, boja, ogrlica, stanje)
        - "Možeš li privremeno da ga zadržiš?" DA/NE
            → NE: objava se taguje "Hitno — potreban smeštaj"
      → Objava na feed: "Nađen"
  → [Faza 0] Poređenje sa "Izgubljen" oglasima: ručno (korisnici sami prepoznaju)
  → [Faza 1] Rule-based matching: ista rasa + boja + lokacija + datum → notifikacija
  → Vlasnik prepozna → chat → dogovor → oglas zatvoren → Trust Score poeni
```

### 6.4 Journey: "Udruženje sa psima za udomljavanje"

```
Predstavnik udruženja
  → Onboarding: "Predstavljaš udruženje?" → DA
  → Registracija sa podacima udruženja (ime, lokacija, kontakt, link)
  → Funkcioniše kao običan korisnik, ali sa oznakom "Udruženje"
  → Bedž udruženja na svakom oglasu → instant poverenje
  → Može objaviti više oglasa (pojedinačno, nema bulk u Fazi 0)
  → [Faza 1] Admin panel, statistika, bulk akcije
```

**Udruženja su ključni "content generatori"** — jedno udruženje sa 20 pasa popuni feed bolje nego 50 individualnih korisnika.

### 6.5 Journey: "Korisnik se vraća" (Retention loop)

```
Korisnik otvara aplikaciju posle N dana
  → Šta vidi:
    → Feed sa novim oglasima od poslednje posete
    → [Ako ima aktivan oglas] Broj novih poruka
    → [Ako je u razgovoru] Nepročitane poruke
    → Trust Score promene (in-app notifikacija)
  → Razlozi za povratak:
    → Amber Alert push notifikacija
    → Nova poruka u chat-u (push)
    → Novi oglasi na feed-u (organski)
    → Trust Score napredak
    → Emotivni sadržaj (slike pasa)
```

---

## 7. Notifikacije (Faza 0)

### 7.1 Princip

Svaka notifikacija mora imati jasan razlog za akciju. Korisnik u jednoj sekundi mora znati: zašto me ovo zanima i šta treba da uradim.

### 7.2 Kategorija 1: Hitne (push + in-app)

| Notifikacija | Primalac | Okidač |
|---|---|---|
| "Izgubljen pas u vašem kraju" | Svi u radijusu 10km | Nova objava "Izgubljen" |
| "Neko je video vašeg psa!" | Vlasnik izgubljenog psa | Klik na "Vidim ovog psa" |

### 7.3 Kategorija 2: Komunikacija (push + in-app)

| Notifikacija | Primalac | Okidač |
|---|---|---|
| Nova poruka u chat-u | Primalac poruke | Svaka nova poruka |
| "Neko je zainteresovan za vašeg psa" | Autor oglasa za udomljavanje | Prva poruka od novog korisnika |

### 7.4 Kategorija 3: Status promena (in-app + opcioni push)

| Notifikacija | Primalac | Okidač |
|---|---|---|
| "Vaš oglas ističe za 24h" | Autor objave | 6. dan od objave |
| "Oglas koji ste pratili je zatvoren" | Korisnici koji su slali poruke autoru | Zatvaranje oglasa |

### 7.5 Kategorija 4: Trust Score (samo in-app)

| Notifikacija | Primalac | Okidač |
|---|---|---|
| "Vaš Trust Score je porastao!" | Korisnik | Promena kategorije (Nov → Aktivan) |
| "+N poena: [opis akcije]" | Korisnik | Svaka Trust Score promena |

### 7.6 Šta NE šaljemo u Fazi 0

- Notifikacije o novim oglasima za udomljavanje (kandidat za Fazu 1 sa personalizacijom)
- Weekly digest (premalo sadržaja za 150-300 korisnika)
- Marketinške notifikacije (nikada ili tek na 10,000+ korisnika)

### 7.7 Korisničke kontrole (settings)

| Tip notifikacije | Default | Može da isključi |
|---|---|---|
| Amber Alert (izgubljeni psi) | Uključen | Da |
| Chat poruke | Uključen | Ne |
| Status promena oglasa | Uključen | Da |

---

## 8. Lokacija Korisnika — Dvoslojni Sistem

### 8.1 Sloj 1: Bazna lokacija (obavezna)

- Korisnik bira opštinu/naselje iz liste pri registraciji
- Čuva se u profilu kao `LocationZone`
- Koristi se za feed filtriranje ("oglasi u Novom Sadu")
- Maksimalna privatnost — korisnik sam bira, nema GPS

### 8.2 Sloj 2: GPS lokacija (opciona)

- Pri instalaciji pitamo: "Želiš da primaš Amber Alert za izgubljene pse u tvom kraju?"
- Ako DA → aplikacija traži GPS dozvolu
- Koristi se **poslednja poznata lokacija** (low battery, significant location changes)
- Čuva se kao `LastKnownLatLng` (nullable)
- Ako NE → korisnik dobija Amber Alert na nivou cele opštine (manje precizno)

### 8.3 Amber Alert geo query logika

```
1. Korisnik objavi "Izgubljen" sa tačnom lokacijom nestanka
2. Backend: SELECT korisnici WHERE
     a) LastKnownLatLng u radijusu 10km (precizni korisnici)
     b) ILI LocationZone = ista opština (fallback za korisnike bez GPS-a)
3. Pošalji push notifikaciju svim pogođenim korisnicima
```

### 8.4 Privatnost

- Nikada tačna adresa u profilu — samo opština/naselje
- GPS koordinate se NE prikazuju drugim korisnicima
- Lokacija nestanka: korisnik sam bira preciznost (ulica ili samo zona)
- In-app chat bez otkrivanja broja telefona — broj se deli eksplicitnim klikom

---

## 9. Moderacija (Faza 0)

### 9.1 Strategija: Automatizacija + Zajednica + Admin

Solo developer ne može da ručno pregleda svaki oglas. Moderacija je troslojna.

### 9.2 Sloj 1 — Automatska moderacija

| Mehanizam | Opis |
|---|---|
| Obavezna polja | Bez slike, lokacije i opisa nema objavljivanja — eliminiše 50% spam-a |
| Rate limiting | Max 3 objave/dan po korisniku (udruženja izuzeta) |
| Keyword filter | "prodajem", "cena", "dinara", "evra", "simbolična nadoknada" → flag za pregled |
| Image hashing | Ista slika sa dva profila → automatski flag |
| Content Safety API | Azure Content Safety za detekciju neprimerenih slika |

### 9.3 Sloj 2 — Community moderacija

| Mehanizam | Opis |
|---|---|
| "Prijavi objavu" | Svaki korisnik može da prijavi oglas sa razlogom |
| Auto-skrivanje | 3 prijave od različitih korisnika → oglas se skriva, ide u red za pregled |
| Trust Score težina | Prijava od "Pouzdanog" korisnika = 2 prijave od "Novog" |
| Udruženja kao super-moderatori | Njihove prijave imaju veću težinu (neformalno u Fazi 0) |

### 9.4 Sloj 3 — Admin (vi)

**Dnevni ritual: 15 minuta ujutru**
- Pregled flagovanih objava
- Akcije: Odobri / Ukloni / Upozori autora / Suspenduj / Banuj
- Na 150-300 korisnika: realistično 5-10 stavki dnevno

### 9.5 Admin Dashboard (minimalni scope za Fazu 0)

- Lista flagovanih objava (keyword + community report) + dugmići za akciju
- Lista prijavljenih korisnika sa istorijom prijava
- Osnovne brojke: ukupno korisnika, objava danas, aktivnih "Izgubljen" oglasa
- Ništa više za Fazu 0

### 9.6 Šta NE moderišemo u Fazi 0

- Svaki oglas pre objavljivanja (pre-moderacija) — ne skalirate, usporava UX
- Chat sadržaj (privatna komunikacija)
- Verifikacija identiteta korisnika (Faza 1)
- Donacijske kampanje (Faza 1)

### 9.7 Skaliranje moderacije

| Korisnika | Pristup |
|---|---|
| 0-300 | Vi + automatski filteri + community reporting |
| 300-1,000 | +2-3 volontera iz udruženja kao moderatori |
| 1,000-5,000 | Formalizovani moderator tim, prioritetni redovi |
| 5,000+ | AI moderacija sadržaja, eskalirani slučajevi ljudima |

---

## 10. AI Plan

### 10.1 Princip

Nema custom AI modela u Fazi 0. Python mikroservis se dodaje tek u Fazi 1.5. Za Fazu 0: dropdown za rasu, gotovi API servisi za moderaciju.

### 10.2 Rasa psa u Fazi 0

- Dropdown sa 30-40 najčešćih rasa
- Opcija "Mešanac" i "Ne znam"
- Opcija slobodnog unosa
- Implementacija: 2 sata umesto 2 nedelje

### 10.3 AI po fazama

| Faza | Feature | Pristup |
|---|---|---|
| **Faza 0** | Moderacija slika | Azure Content Safety API |
| **Faza 0** | Keyword filteri | Regex + keyword liste |
| **Faza 1** | Poređenje Nađen ↔ Izgubljen | Rule-based (rasa + boja + lokacija + datum) |
| **Faza 1.5** | AI prepoznavanje rasa | Python FastAPI + pretrained ResNet/EfficientNet |
| **Faza 1.5** | Pametni push za "Nađen" | AI-bazirano poređenje |
| **Faza 3** | AI Detektiv (facial recognition) | Custom model, zahteva veliku bazu slika |
| **Faza 3** | AI Matchmaker | ML scoring + kviz uparivanja |

### 10.4 Priprema za buduće AI feature-e

Od Faze 0 čuvati slike u dobrom kvalitetu u Blob Storage sa strukturiranim metapodacima (rasa, boja, veličina, lokacija). Kad dođe vreme za AI Detektiva, dataset je spreman.

### 10.5 Tech stack implikacija

Faza 0 stack (bez Python servisa):
- .NET API + React + React Native + SQL Server + Azure + FCM

Od Faze 1.5 se dodaje:
- Python FastAPI + Docker + Azure Container Instance

---

## 11. Monetizacija

### 11.1 Pravilo

Osnovne funkcionalnosti su **zauvek besplatne** za pojedince. Čim se naplati osnova, korisnici se vraćaju na FB grupe.

### 11.2 Prihodi (od Faze 1+)

| Izvor | Opis |
|---|---|
| Veterinarske ambulante — "Zlatni bedž" | Plaćena promocija za klinike sa popustima za udomljene pse |
| Pet Shop Affiliate | Procenat od prodaje preko personalizovanih kupona |
| Lokalni Boost | Simbolična naknada za isticanje oglasa na vrh feed-a |
| Premium profil za udruženja | Mesečna pretplata: dashboard, statistika, prioritetna podrška |
| Donacije sa transparentnošću | 5-10% servisna naknada |
| Sponzorisani edukativni sadržaj | Brendovi plaćaju za korisne članke, jasno označeni |

> Monetizacija NIJE prioritet za Fazu 0. Fokus je na korisnicima i vrednosti.

---

## 12. KPI Metrike

### 12.1 Akvizicija — da li ljudi dolaze?

| Metrika | Target (prvih 30 dana) |
|---|---|
| Registrovani korisnici | 150-300 |
| Izvor registracija | Pratiti: FB grupa, udruženje, word of mouth |

### 12.2 Engagement — da li ostaju?

| Metrika | Target |
|---|---|
| DAU/MAU ratio | 15-25% |
| Objave nedeljno | Min 10 novih posle prvog meseca |
| Chat konverzacije | Pratiti: % objava koje rezultiraju porukom |
| 7-dnevni retention | 40% |
| 30-dnevni retention | 25% |

### 12.3 Ishod — da li rešava problem?

| Metrika | Target (prvih 90 dana) |
|---|---|
| Uspešna udomljavanja (North Star) | Min 10 |
| Amber Alert → razrešen | 30%+ |
| Prosečno vreme do odgovora na oglas | Pratiti, cilj: brže od FB |

### 12.4 Zdravlje platforme

| Metrika | Šta pratiti |
|---|---|
| Trust Score distribucija | Da li korisnici napreduju (Nov → Aktivan → Pouzdan) |
| Prijave (reports) | Trend — na početku blizu nule |

### 12.5 Alarm signali

| Period | Signal | Akcija |
|---|---|---|
| 30 dana | <50 registrovanih | Problem distribucije — promeniti kanale promocije |
| 60 dana | 7-dnevni retention <20% | Korisnici ne nalaze vrednost — anketa, direktni razgovori |
| 90 dana | 0 uspešnih udomljavanja | Fundamentalni problem u flow-u — pregled user journey-a |

### 12.6 Analytics alati

- **Application Insights** (Azure) — backend metrike, custom eventi, funnels
- **Firebase Analytics** — mobile/frontend, kohorte, retention
- Custom eventi za praćenje: `oglas_kreiran`, `amber_alert_aktiviran`, `chat_pokrenut`, `udomljavanje_zavrseno`, `vidim_ovog_psa_klik`

---

## 13. Onboarding

Korisnik na startu odgovara na 3 pitanja:

1. "Tražiš psa za udomljavanje?"
2. "Izgubio si ljubimca?"
3. "Želiš da pomogneš kao foster/volonter?"

→ Personalizovan feed od prvog momenta
→ Drastično poboljšava retention
→ Za kategoriju "Izgubio si ljubimca" — skraćena registracija (minimum polja)

---

## 14. Vizuelni Identitet

- **Stil:** "Soft & Clean" — zaobljene ivice, pregledne kartice
- **Paleta:**
  - Prijateljska plava — poverenje
  - Topla narandžasta — akcija, CTA
  - Crvena — hitnost, "Izgubljen" objave (Amber Alert)
- **Tipografija:** Čitljiva, moderna, sa dobrim kontrastom

---

## 15. Gamifikacija (od Faze 1)

### 15.1 Bedževi (vezani za potvrđene akcije)

| Bedž | Uslov |
|---|---|
| Prvi foster | Završio prvi foster period |
| Spasilac x10 | 10 uspešnih udomljavanja |
| Ambulantni heroj | Redovno vakcinisanje ljubimca |
| Detektiv | Pomogao u pronalasku izgubljenog ljubimca |
| Lokalni heroj | Najviši Trust Score u opštini |
| Šampion maženja | Najviše pozitivnih follow-up-ova |

### 15.2 Anti-farming zaštite

- Bedževi vezani isključivo za potvrđene akcije (obe strane potvrde)
- Self-reported podaci ne donose bedževe

---

## 16. Social Feed — "Zajednica" Tab (Faza 1.5)

### 16.1 Koncept

Instagram-stil feed gde korisnici kače slike, videe i priče o svojim ljubimcima. Čist engagement i retention feature — emotivni sadržaj koji vraća korisnike u aplikaciju svaki dan.

### 16.2 Zašto odvojen tab

Social feed je potpuno razdvojen od glavnog feed-a (Udomljavanje, Izgubljen, Nađen). Dva razloga:
- Sprečava razvodnjavanje core feed-a — hitan oglas za izgubljenog psa ne sme da potone ispod slatkih slika
- Korisnik jasno zna: glavni feed = akcija, "Zajednica" = sadržaj i druženje

### 16.3 Uslov za aktiviranje

**Minimum 500+ aktivnih korisnika mesečno.** Na tom broju ima dovoljno sadržaja da feed izgleda živo (10-20 objava dnevno). Sa manje korisnika, social feed deluje prazno i kontraproduktivan je.

### 16.4 Tipovi sadržaja

- Slike i videi ljubimaca (svakodnevni momenti)
- Update priče posle udomljavanja ("Evo kako izgleda Reks 3 meseca kasnije")
- Saveti i iskustva (neformalni, ne edukativni članci)
- Smešni momenti, meme sadržaj vezan za ljubimce

### 16.5 Veza sa ostalim sistemima

- **Trust Score:** pozitivne reakcije na social objave donose poene (+2, cap +10/mesec)
- **Follow-up sistem (Faza 1):** follow-up update posle udomljavanja se automatski deli u "Zajednica" tab — najjači emotivni sadržaj i dokaz da platforma radi
- **Retention:** korisnici koji nemaju aktivan oglas i dalje imaju razlog da otvore aplikaciju

### 16.6 Moderacija social feed-a

- Isti mehanizmi kao za glavni feed (community report, keyword filteri, image hashing)
- Dodatno: Content Safety API za neprimerene slike (nasilje nad životinjama)
- Rate limit: max 5 social objava dnevno po korisniku

---

## 17. Sumarni Pregled — Šta se implementira u Fazi 0

### Implementirati:
- [x] Oglasna tabla (3 kategorije: Udomljavanje, Izgubljen, Nađen)
- [x] Integrisani Amber Alert (push notifikacija za "Izgubljen" u radijusu 10km)
- [x] Dugme "Vidim ovog psa" sa lokacijom
- [x] Profili korisnika (osnovno + dvoslojni sistem lokacije)
- [x] Profili ljubimaca (osnovna kartica sa dropdown-om za rasu)
- [x] In-app chat (bez otkrivanja broja)
- [x] Onboarding kviz (3 pitanja)
- [x] Trust Score Lite (vidljiv, akumulira se, ne gate-uje)
- [x] Push notifikacije (FCM) — Amber Alert + chat + status promene
- [x] Automatska moderacija (obavezna polja, rate limit, keyword filter, image hash)
- [x] Community reporting (prijavi objavu)
- [x] Admin dashboard (minimalni — flagovi, prijave, osnovne brojke)
- [x] Korisničke kontrole notifikacija (settings sa 2-3 toggle-a)

### NE implementirati u Fazi 0:
- [ ] AI prepoznavanje rasa (dropdown je dovoljan)
- [ ] Python AI mikroservis
- [ ] Trust Score gate-ovi (score se akumulira ali ne blokira ništa)
- [ ] Foster lista (strukturirana)
- [ ] QR verifikacija primopredaje
- [ ] Follow-up sistem (1/3/6 meseci)
- [ ] Veterinarski karton
- [ ] Prijava zlostavljanja sa PDF
- [ ] Donacije/kampanje
- [ ] Progresivni Amber Alert radijus
- [ ] Admin panel za udruženja
- [ ] Monetizacija
- [ ] Social feed "Zajednica" tab (Faza 1.5, uslov: 500+ korisnika)
