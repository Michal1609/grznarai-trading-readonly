# eToro API Limitations

This document lists features that are **not implemented** in this library because the eToro API does not support them as described in the official documentation. All findings were verified through integration tests against the production API.

---

## Market Data — `SearchInstrumentsAsync`

### Sort parameter not supported
**Affected endpoint:** `GET /api/v1/market-data/search`

The official documentation lists a `sort` query parameter. Adding any `sort` value returns **HTTP 404**. The parameter has been removed from `InstrumentSearchRequest` and is not sent.

### PageNumber parameter has no effect
**Affected endpoint:** `GET /api/v1/market-data/search`

The `pageNumber` query parameter is documented but completely ignored by the API — all page numbers return the same result set. The parameter has been removed from `InstrumentSearchRequest`.

### PageSize above 100 returns inconsistent results
**Affected endpoint:** `GET /api/v1/market-data/search`

Requesting `pageSize=200` (the documented maximum) returns a random number of records on each call (e.g. 147, 190, …). The library caps `pageSize` at **100** for this endpoint (`EToroRequestLimits.MaxSearchPageSize`).

---

## Watchlist — `GetDefaultWatchlistItemsAsync`

### itemsPerPage parameter has no effect
**Affected endpoint:** `GET /api/v1/watchlists/default-watchlists/items`

The `itemsPerPage` query parameter is listed in the documentation but has no observable effect — the API always returns the same number of items regardless of the value. The parameter has been removed from the method signature.

The `itemsLimit` parameter **does work** and is retained.

---

## User Info — `SearchUsersAsync`

### popularInvestor filter returns HTTP 404
**Affected endpoint:** `GET /api/v1/user-info/people/search`

Sending `popularInvestor=true` (or `false`) in the query string causes the API to respond with **HTTP 404**. The `PopularInvestor` property has been removed from `UserSearchRequest`.

### customerId field missing from response
**Affected endpoint:** `GET /api/v1/user-info/people/search`

The documentation states that the response items include a `customerId` field. In practice the field is **not present** in the API response. The `CustomerId` property in `UserSearchItem` is typed as `int?` (nullable) and will always be `null` when deserialized from the real API.

---

## Summary table

| Area | Feature | Documented | Works | Action taken |
|------|---------|-----------|-------|--------------|
| Market / Search | `sort` parameter | Yes | No — HTTP 404 | Removed from model and client |
| Market / Search | `pageNumber` parameter | Yes | No — ignored | Removed from model and client |
| Market / Search | `pageSize` up to 200 | Yes | No — random results above 100 | Capped at 100 |
| Watchlist / DefaultItems | `itemsPerPage` parameter | Yes | No — ignored | Removed from method signature |
| UserInfo / Search | `popularInvestor` filter | Yes | No — HTTP 404 | Removed from model and client |
| UserInfo / Search | `customerId` in response | Yes | No — absent | Made nullable (`int?`) |
