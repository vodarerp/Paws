# 🐾 Platforma za Ljubimce (Srbija) — Proizvod & Funkcionalnosti

## 1. Vizija i Svrha

Centralizovana digitalna platforma (Web + Mobilna aplikacija) koja zamenjuje haotične FB/Viber grupe jedinstvenim ekosistemom za **udomljavanje**, **pronalaženje izgubljenih ljubimaca** i **zaštitu životinja** u Srbiji.

---

## 2. Oglasna Tabla & News Feed

### 2.1 Tipovi objava (obavezna kategorija)

Svaka objava **mora** imati jednu od sledećih kategorija — bez kategorije nema objavljivanja:

- **Udomljavanje** — obavezna polja: starost, zdravstveno stanje, razlog, fotografija
- **Izgubljen ljubimac** — lokacija nestanka, vreme, opis, fotografija
- **Nađen ljubimac** — lokacija nalaženja, opis, fotografija
- **Foster potreban** — hitnost, trajanje, tip životinje
- **Hitna pomoć** — veterinarska pomoć, hrana, transport
- **Edukativni sadržaj** — članci, saveti, iskustva

### 2.2 Feed algoritam

- **Hitni oglasi** (izgubljen pas, hitna pomoć) — uvek na vrhu
- Ostalo hronološki, sa blagim boostom za korisnike sa višim Trust Score-om
- Nije čist chronological, ali ni potpuno algoritmski

### 2.3 Moderacija sadržaja

- Automatski filteri za uvredljiv sadržaj i spam
- Community reporting (prijavi objavu)
- **Image hashing** — ista slika koja već postoji na platformi → flagovanje za pregled
- Sprečavanje prodaje maskirane kao "udomljavanje uz simboličnu nadoknadu"

### 2.4 Zloupotrebe i zaštite

| Zloupotreba | Zaštita |
|---|---|
| Dupli oglasi (ista životinja na više profila) | Image hashing + automatsko flagovanje |
| Lažne priče za skupljanje donacija | Verifikacija kampanja, prikaz računa |
| Prodaja maskirana kao udomljavanje | Obavezna polja, moderator pregled |
| Spam i botovi | Rate limiting, CAPTCHA, auto-filteri |

---

## 3. Amber Alert Sistem

### 3.1 Ko može da aktivira

- Samo korisnici sa **Trust Score iznad praga** (50+ poena) ili **verifikovani korisnici** (potvrđen identitet)
- Novi korisnici **ne mogu** da šalju Amber Alert — moraju prvo izgraditi reputaciju

### 3.2 Ograničenja

- Maksimalno **1 alert na 24h** po korisniku
- Maksimalno **3 aktivna alerta** u istom radijusu istovremeno
- Radijus: **5km** → proširenje na **10km** posle 24h → **20km** posle 48h (ako pas nije nađen)

### 3.3 Životni ciklus alerta

```
Aktivan → Ažuriran (nova info) → Razrešen (pas nađen) / Istekao (7 dana)
```

- Korisnik koji ne zatvori alert kad se pas nađe **gubi Trust Score poene**
- Posle 7 dana — opcija produžetka uz re-verifikaciju

### 3.4 Zloupotrebe i zaštite

| Zloupotreba | Zaštita |
|---|---|
| Lažne prijave nestanka | Community report lažnog alerta, admin verifikacija |
| Preuzimanje tuđeg psa | Obavezna fotografija + opis pre nestanka |
| Korišćenje alerta kao reklame | Automatska detekcija + penalizacija Trust Score |
| Notification fatigue (previše alertova) | Radijus limiti, max 3 istovremena u zoni |

---

## 4. Trust Score Sistem

### 4.1 Pozitivne akcije

| Akcija | Poeni |
|---|---|
| Uspešno udomljavanje (potvrđeno od obe strane) | +25 |
| Verifikovan identitet (jednokratno) | +20 |
| Završen foster | +15 |
| Validna prijava zloupotrebe | +10 |
| Zatvaranje alerta u roku (pas nađen) | +5 |
| Objava sa pozitivnim reakcijama | +2 (cap +10/mesec) |

### 4.2 Negativne akcije

| Akcija | Poeni |
|---|---|
| Prodaja maskirana kao udomljavanje | -50 + ban upozorenje |
| Potvrđena prijava od drugih korisnika | -30 |
| Nezatvaranje alerta/oglasa kad je razrešen | -10 |
| Neodgovaranje na poruke >48h (aktivni oglas) | -5 |

### 4.3 Vidljivost

- Korisnik vidi **svoj score i istoriju promena**
- Drugi korisnici vide **kategoriju** (ne tačan broj):
  - Nov korisnik → Aktivan → Pouzdan → Lokalni heroj
- Sprečava "gaming" sistema

### 4.4 Anti-gaming zaštite

- Isti korisnik ne može da "udomljava" od istog korisnika više od jednom
- Device fingerprinting za detekciju alt naloga
- Verifikacija putem potvrde obe strane

---

## 5. Verifikacija i Primopredaja

### 5.1 Flow primopredaje

```
1. Obe strane potvrde datum i mesto u aplikaciji
2. Na licu mesta — jedna strana generiše QR kod, druga skenira
3. Obe strane potvrđuju selfijem sa životinjom
4. Posle 7 dana — prompt "da li je sve u redu?"
5. Tek posle potvrde → udomljavanje = završeno → Trust Score poeni
```

### 5.2 Follow-up sistem

- Platforma šalje podsetnik novom vlasniku na **1 mesec**, **3 meseca** i **6 meseci**
- Korisnik postavi update (slika + kratki status)
- Dvostruka svrha:
  - Zajednica — lepe priče o uspešnom udomljavanju
  - Kontrola — ako neko nestane posle udomljavanja = red flag

### 5.3 Zloupotrebe i zaštite

| Zloupotreba | Zaštita |
|---|---|
| Uzme psa, potvrdi primopredaju, pa proda/napusti | Follow-up sistem + prethodni vlasnik može da prijavi zabrinutost |
| Lažna primopredaja (bez stvarnog susreta) | QR + selfi verifikacija na licu mesta |
| Ignorisanje follow-up-a | Gubitak Trust Score poena + flag za moderatore |

---

## 6. Crna Lista i Sistem Prijava

### 6.1 Nivoi sankcija

| Nivo | Opis | Trajanje |
|---|---|---|
| Upozorenje | Notifikacija, prvi prekršaj | — |
| Privremena suspenzija | Ne može da objavljuje, može da čita | 7–30 dana |
| Permanentni ban | Potpuno uklanjanje, čuvanje podataka | Trajno |
| Crna lista deljenja | Podaci deljeni sa partnerskim udruženjima (uz pravni okvir) | Trajno |

### 6.2 Prijava zlostavljanja — realni flow za Srbiju

```
1. Korisnik popunjava formular (šta, gde, kad, dokazi/foto)
2. Platforma generiše strukturiran PDF
3. Automatski se šalje partnerskim udruženjima (SOS Životinje, lokalna udruženja)
4. Korisnik dobija uputstvo kome da prosledi lokalno (komunalna inspekcija, policija)
5. Platforma prati status prijave
```

> **Napomena:** Direktna integracija sa državnim službama je dugoročni cilj, ne MVP. Komunalna policija i inspekcija u Srbiji nemaju API ni standardizovan prijem digitalnih prijava.

---

## 7. Foster Mreža

### 7.1 Foster profil (strukturirani podaci)

- **Kapacitet** — koliko životinja istovremeno
- **Vremenski okvir** — koliko dugo može da drži
- **Uslovi** — dvorište/stan, druge životinje, deca u kućanstvu
- **Iskustvo** — prvi put foster ili iskusan
- **Lokacija** — šira zona (naselje/opština)

### 7.2 Automatski matchmaking

Kad neko hitno traži foster, sistem predlaže dostupne fostere u blizini na osnovu kompatibilnosti (veličina psa vs uslovi fostera).

### 7.3 Digitalni foster ugovor

Obostrano potpisan u aplikaciji:
- Ko snosi troškove veterinara
- Koliko traje foster period
- Šta ako foster želi da zadrži psa (pravo prvog izbora za udomljavanje)

> Sprečava 90% sukoba koji se dešavaju u FB grupama.

---

## 8. Korisnički Profili i Privatnost

### 8.1 Tipovi naloga

| Tip | Opis | Pristup |
|---|---|---|
| Pojedinac | Obični korisnik | Osnovne funkcije |
| Spasilac/Foster | Verifikovan | Viši pristup, foster mod |
| Udruženje | Organizacioni nalog | Dashboard, statistika, bulk akcije |
| Veterinarska ambulanta | Partnerski nalog | Ažuriranje zdravstvenih kartona |
| Admin/Moderator | Interni | Pun pristup, crna lista, moderacija |

### 8.2 Privatnost i zaštita lokacije

- Nikada tačna adresa — samo šira zona (naselje/opština)
- Amber Alert — korisnik bira precizniju lokaciju (ulica) ili samo zonu
- **In-app chat** bez otkrivanja broja telefona
- Broj se deli tek kad korisnik **eksplicitno odobri** u chatu

### 8.3 Digitalna kartica ljubimca

- Ime, starost, rasa (AI prepoznata + korisnik potvrdi)
- Fotografije (galerija)
- Zdravstveni karton: vakcinacije, sterilizacija, čip broj, alergije
- Istorija: prethodni vlasnik, foster period, datum udomljavanja
- Partnerske ambulante mogu direktno popunjavati karton

### 8.4 Veterinarski podsetnici

Platforma šalje automatske podsetnike za:
- Vakcinaciju i revakcinaciju
- Antiparazitske tretmane
- Redovne preglede

> Jednostavno za implementaciju, a korisnici se vraćaju u aplikaciju redovno.

---

## 9. Onboarding

Umesto klasične registracije, korisnik na startu odgovara na 3–4 pitanja:

1. "Tražiš psa za udomljavanje?"
2. "Izgubio si ljubimca?"
3. "Želiš da pomogneš kao foster?"
4. "Predstavljaš udruženje?"

→ Personalizovan feed od prvog momenta. Drastično poboljšava retention.

---

## 10. Monetizacija

### 10.1 Besplatno (uvek)

- Objava oglasa, pretraga, chat, Amber Alert
- Osnovne funkcionalnosti za pojedince

> **Pravilo:** Čim naplatiš osnovu, gubiš korisnike i vraćaju se na FB grupe.

### 10.2 Prihodi

| Izvor | Opis |
|---|---|
| Veterinarske ambulante — "Zlatni bedž" | Plaćena promocija za klinike koje daju popuste udomljenim psima |
| Pet Shop Affiliate | Procenat od prodaje opreme/hrane preko personalizovanih kupona |
| Lokalni Boost | Simbolična naknada za isticanje oglasa na vrh feed-a |
| Premium profil za udruženja | Mesečna pretplata — dashboard, statistika, prioritetna podrška |
| Donacije sa transparentnošću | Korisnik donira za konkretnu životinju, platforma prikazuje račune, 5–10% servisna naknada |
| Sponzorisani edukativni sadržaj | Brendovi plaćaju za korisne članke u feed-u, jasno označeni kao sponzorisani |

---

## 11. Gamifikacija

### 11.1 Bedževi (vezani za potvrđene akcije)

| Bedž | Uslov |
|---|---|
| Prvi foster | Završio prvi foster period |
| Spasilac x10 | 10 uspešnih udomljavanja |
| Ambulantni heroj | Redovno vakcinisanje ljubimca |
| Detektiv | Pomogao u pronalasku izgubljenog ljubimca |
| Lokalni heroj | Najviši Trust Score u opštini |
| Šampion maženja | Najviše pozitivnih follow-up-ova |

### 11.2 Mesečna/godišnja priznanja

- Najaktivniji korisnici i udruženja
- Gradi zajednicu i daje socijalnu potvrdu

### 11.3 Anti-farming zaštite

- Bedževi vezani **isključivo za potvrđene akcije** (obe strane potvrde)
- Self-reported podaci **ne donose bedževe**

---

## 12. Vizuelni Identitet

- **Stil:** "Soft & Clean" — zaobljene ivice, pregledne kartice
- **Paleta:**
  - Prijateljska plava — poverenje
  - Topla narandžasta — akcija, CTA
  - Crvena — hitnost, Amber Alert
- **Tipografija:** Čitljiva, moderna, sa dobrim kontrastom

---

## 13. MVP Prioriteti

### Faza 1 — Lansiranje

- Oglasna tabla sa feed-om i kategorijama
- Profili korisnika i ljubimaca
- In-app chat (bez otkrivanja broja)
- Amber Alert (sa ograničenjima)
- Trust Score (bazični)
- AI prepoznavanje rasa
- Foster lista (strukturirani profili)
- Onboarding kviz

### Faza 2

- Donacije/kampanje sa transparentnošću
- Veterinarski karton (digitalni)
- Admin panel za udruženja
- Prijava zlostavljanja sa PDF generisanjem
- Veterinarski podsetnici
- Follow-up sistem posle udomljavanja

### Faza 3

- AI Detektiv (facial recognition za pse)
- AI Matchmaker (kviz uparivanja)
- Integracija sa državnim službama
- Affiliate program sa pet shopovima
- Crna lista deljenja sa partnerima
