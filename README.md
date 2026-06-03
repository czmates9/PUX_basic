# PUX Directory Change Detector

Cvičné řešení backendového úkolu pro detekci změn v lokálním adresáři.

## Splnění zadání

Aplikace splňuje požadavky zadání:

- jednoduchá ASP.NET Core MVC aplikace v C#,
- UI obsahuje textbox pro zadání cesty k adresáři,
- UI obsahuje tlačítko pro ruční spuštění analýzy,
- výsledek analýzy se vypisuje přímo ve webovém rozhraní,
- první spuštění vytvoří baseline snapshot,
- další spuštění hlásí změny od posledního spuštění,
- vypisuje nové soubory,
- vypisuje změněné soubory,
- vypisuje odstraněné soubory,
- vypisuje odstraněné podadresáře,
- eviduje verzi souboru,
- nepoužívá databázi,
- snapshot se ukládá do JSON souboru.

## Spuštění

Ze složky se solution souborem:

```bash
cd PUX.DirectoryChangeDetector
dotnet run --project PUX.DirectoryChangeDetector
```

Alternativně přímo ze složky projektu:

```bash
cd PUX.DirectoryChangeDetector/PUX.DirectoryChangeDetector
dotnet run
```

Aplikace se spustí například na:

```text
https://localhost:7214
http://localhost:5214
```

## Princip řešení

Identita souboru je v této verzi daná jeho relativní cestou vůči analyzovanému adresáři.

Snapshot souboru obsahuje:

- relativní cestu,
- SHA256 hash obsahu,
- velikost souboru,
- číslo verze.

První spuštění vytvoří snapshot a nastaví všem souborům verzi 1.

Při dalším spuštění:

- pokud relativní cesta v předchozím snapshotu neexistovala, soubor je nový,
- pokud relativní cesta v aktuálním snapshotu neexistuje, soubor je odstraněný,
- pokud relativní cesta existuje v obou snapshotech a změnil se SHA256 hash, soubor je změněný,
- při změně obsahu se verze souboru navýší o 1,
- pokud se hash nezměnil, verze zůstává stejná.

Adresáře jsou evidované podle relativní cesty. Díky tomu se detekují i odstraněné prázdné podadresáře.

## Proč SHA256

Zadání říká, že změnou souboru se rozumí změna obsahu. Proto aplikace neporovnává pouze čas poslední změny, ale počítá SHA256 hash obsahu souboru.

To je přesnější než `LastWriteTime`, protože čas změny může být upraven bez změny obsahu a naopak.

## Slabá místa řešení

### Přejmenování souboru

Přejmenování souboru není v zadání požadované jako samostatná kategorie. V této verzi je proto vyhodnoceno jako:

- odstranění původního souboru,
- vytvoření nového souboru.

Toto chování je jednoduché, deterministické a odpovídá zadání, protože program má hlásit nové, změněné a odstraněné soubory.

### Kopie souboru

Kopie souboru je vyhodnocena jako nový soubor, protože má novou relativní cestu.

### Přejmenování adresáře

Přejmenování adresáře není v zadání požadované jako samostatná kategorie. V této verzi se projeví jako:

- odstranění původního podadresáře,
- nové soubory v nové cestě.

Zadání výslovně požaduje seznam odstraněných souborů a podadresářů, nikoliv seznam přejmenovaných adresářů.

### Stabilní identita souboru

Pro přesnou detekci přejmenování a přesunů by bylo možné použít stabilní identifikátor ze souborového systému:

- Windows: Volume Serial Number + File Index přes WinAPI,
- Linux/macOS: device id + inode.

Tuto nadstavbovou variantu jsem si ověřil samostatně. Výhodou je přesné rozlišení kopie, přejmenování a změny obsahu. Nevýhodou je vyšší složitost, závislost na konkrétním souborovém systému a fakt, že taková logika už přesahuje rozsah tohoto zadání.

### Zamčené soubory a oprávnění

Pokud je soubor zamčený nebo k němu aplikace nemá práva, analýza může skončit chybou. V produkčním řešení by bylo vhodné doplnit detailní zpracování chyb po jednotlivých souborech.

### Velikost a výkon

Zadání předpokládá soubory do 50 MB a nejvýše 100 souborů v každém adresáři. Pro tento rozsah je výpočet SHA256 v pořádku. U větších adresářů by bylo vhodné řešit paralelizaci, průběžné zpracování a odolnější práci s chybami.

## Možné budoucí rozšíření

- detekce přejmenování souborů pomocí stabilní identity filesystemu,
- detekce přejmenování adresářů,
- zpracování chyb po jednotlivých souborech bez pádu celé analýzy,
- export výsledku do souboru,
- REST API varianta,
- unit testy pro porovnávací logiku,
- více snapshotů v historii.
