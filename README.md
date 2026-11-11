# SmartProxyPAD – Lab 2 PAD (Web Proxy)

## Arhitectura

- **Proxy** (Ocelot, `:9000`): singurul punct de acces pentru client. Face load balancing round robin între noduri, ține în cache doar cererile GET (10 s, cheie separată pentru JSON și XML) și servește clientul web (`Proxy/wwwroot/index.html`).
- **MovieAPI** (nodurile DW, `:9001`, `:9002`): Controller → Service → Repository, fiecare nod cu baza lui MongoDB (`movie1`, `movie2`). Răspunde în JSON sau XML după header-ul `Accept`, cu paginare `?offset=&limit=` și cu header-ul `X-Served-By` (portul nodului).
- **SyncNode** (`:5000`): primește modificările de la noduri și le replică în fundal la celelalte noduri, la fiecare 5 s (eventual consistency).
- **MongoDB** (`:27017`)

```
client ──► Proxy :9000 ──► node1 :9001 ──► movie1
                     └───► node2 :9002 ──► movie2
             node1/node2 ──► SyncNode :5000 ──► /api/sync/movie pe celelalte noduri
```

## Pornire

```
mongod                                   (sau serviciul MongoDB)
dotnet run --project SyncNode
dotnet run --project MovieAPI --launch-profile node1
dotnet run --project MovieAPI --launch-profile node2
dotnet run --project Proxy
```

Clientul web se deschide la http://localhost:9000.

### Al treilea nod

Nu necesită modificări de cod, doar două linii de config:

1. `Proxy/ocelot.json`: în `DownstreamHostAndPorts` al fiecărei rute se adaugă `{ "Host": "localhost", "Port": 9003 }`;
2. `SyncNode/appsettings.json`: în `Hosts` se adaugă `"http://localhost:9003/api"`.

Apoi `dotnet run --project MovieAPI --launch-profile node3` (baza `movie3`).

## Demonstrație (Postman)

Colecția se importă în Postman din `SmartProxyPAD.postman_collection.json` și se rulează cu **Run collection**, sau din linia de comandă:

```
npx newman run SmartProxyPAD.postman_collection.json
```

| # | Scenariu | Rezultat așteptat |
|---|----------|-------------------|
| 1–2 | POST pe node1, după 6 s GET direct pe node2 | filmul există și în `movie2` |
| 3–4 | 2 × POST prin proxy | `X-Served-By` alternează 9001 / 9002 (round robin) |
| 5–6 | același GET de două ori în 10 s | al doilea vine din cache (același `X-Served-By`) |
| 7 | același GET cu `Accept: application/xml` | răspuns XML, nu JSON-ul din cache |
| 8–9 | `?offset=0&limit=2`, `?offset=-1` | cel mult 2 filme; 400 |
| 10 | GET pe un id inexistent | 404 |
| 11 | PUT `/api/sync/movie` prin proxy | 404: endpoint-urile interne nu sunt expuse |
| 12–15 | DELETE pe node1, apoi un update mai vechi trimis direct pe node2 | filmul rămâne șters pe ambele noduri |

Scenarii manuale (necesită oprirea unui proces):

- **node2 oprit**: POST pe node1, apoi node2 repornit. SyncNode loghează eșecul, reîncearcă la fiecare 5 s, iar node2 primește filmul după repornire.
- **SyncNode oprit**: POST prin proxy întoarce tot 200; nodul loghează `SyncNode unavailable`, iar filmul nu se replică.

## Decizii

- **Ștergere = tombstone** (`IsDeleted = true`): ștergerea se replică precum un update, iar la conflict câștigă modificarea cu `LastChangedAt` mai nou. Un update vechi nu poate readuce un film șters.
- **SyncNode fără pierderi**: dacă un receptor nu răspunde sau întoarce eroare, modificarea revine în coadă, cu cel mult 10 încercări.
- **Concurență**: Kestrel servește cererile în paralel, iar coada din SyncNode e un `ConcurrentDictionary` cu `AddOrUpdate` atomic.

## Limite

- SyncNode e single point of failure, iar coada lui e în memorie: un restart pierde modificările încă nesincronizate.
- Conflictele se rezolvă după timestamp, deci rezultatul depinde de ceasurile serverelor; pe o singură mașină e corect.
- Cache-ul poate întoarce date vechi cu până la 10 s după o scriere.
- `IsDeleted` apare în răspunsuri, pentru că același model e folosit și la sync.
