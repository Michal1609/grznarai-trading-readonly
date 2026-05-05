# Omezení eToro API

Tento dokument popisuje funkcionality, které **nejsou v knihovně implementovány**, protože je eToro API nepodporuje navzdory tomu, co uvádí oficiální dokumentace. Všechny závěry byly ověřeny integračními testy na produkčním API.

---

## Market Data — `SearchInstrumentsAsync`

### Parametr sort není podporován
**Postižený endpoint:** `GET /api/v1/market-data/search`

Oficiální dokumentace uvádí query parametr `sort`. Přidání jakékoliv hodnoty `sort` vrátí **HTTP 404**. Parametr byl odstraněn z `InstrumentSearchRequest` a není odesílán.

### Parametr pageNumber nemá žádný efekt
**Postižený endpoint:** `GET /api/v1/market-data/search`

Query parametr `pageNumber` je v dokumentaci uveden, ale API ho zcela ignoruje — všechna čísla stránek vrátí stejnou sadu výsledků. Parametr byl odstraněn z `InstrumentSearchRequest`.

### PageSize nad 100 vrací nekonzistentní výsledky
**Postižený endpoint:** `GET /api/v1/market-data/search`

Požadavek s `pageSize=200` (dokumentovaný maximální limit) vrací pokaždé jiný počet záznamů (např. 147, 190, …). Knihovna omezuje `pageSize` na **100** pro tento endpoint (`EToroRequestLimits.MaxSearchPageSize`).

---

## Watchlist — `GetDefaultWatchlistItemsAsync`

### Parametr itemsPerPage nemá žádný efekt
**Postižený endpoint:** `GET /api/v1/watchlists/default-watchlists/items`

Query parametr `itemsPerPage` je v dokumentaci uveden, ale nemá žádný pozorovatelný efekt — API vždy vrátí stejný počet položek bez ohledu na hodnotu parametru. Parametr byl odstraněn z podpisu metody.

Parametr `itemsLimit` **funguje správně** a byl zachován.

---

## User Info — `SearchUsersAsync`

### Filtr popularInvestor vrací HTTP 404
**Postižený endpoint:** `GET /api/v1/user-info/people/search`

Odeslání `popularInvestor=true` (nebo `false`) v query stringu způsobí, že API odpoví **HTTP 404**. Vlastnost `PopularInvestor` byla odstraněna z `UserSearchRequest`.

### Pole customerId chybí v odpovědi
**Postižený endpoint:** `GET /api/v1/user-info/people/search`

Dokumentace uvádí, že položky odpovědi obsahují pole `customerId`. V praxi toto pole **není v odpovědi API přítomno**. Vlastnost `CustomerId` v `UserSearchItem` je nyní typu `int?` (nullable) a při deserializaci z reálného API bude vždy `null`.

---

## Přehledná tabulka

| Oblast | Funkcionalita | Dokumentováno | Funguje | Přijatá akce |
|--------|--------------|--------------|---------|--------------|
| Market / Search | parametr `sort` | Ano | Ne — HTTP 404 | Odstraněno z modelu a klienta |
| Market / Search | parametr `pageNumber` | Ano | Ne — ignorováno | Odstraněno z modelu a klienta |
| Market / Search | `pageSize` až 200 | Ano | Ne — náhodné výsledky nad 100 | Omezeno na 100 |
| Watchlist / DefaultItems | parametr `itemsPerPage` | Ano | Ne — ignorováno | Odstraněno z podpisu metody |
| UserInfo / Search | filtr `popularInvestor` | Ano | Ne — HTTP 404 | Odstraněno z modelu a klienta |
| UserInfo / Search | `customerId` v odpovědi | Ano | Ne — chybí | Změněno na nullable (`int?`) |
