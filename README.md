# FCG — CatalogAPI

Microsserviço de **Catálogo**: CRUD de jogos e promoções, biblioteca do usuário e **início do fluxo de compra**.

## Modelo de domínio
- **Jogo** (Raiz de Agregação) com **Promocao** (entidade interna): título, gênero, preço e promoções sem sobreposição; calcula o preço vigente.
- **Pedido** (Raiz de Agregação): retrato do jogo/valor no momento da compra e o estado do pagamento (`AguardandoPagamento`, `Pago`, `PagamentoRecusado`).
- **Biblioteca** (Raiz de Agregação, identidade = Id do usuário) com **ItemDaBiblioteca**: um jogo no máximo uma vez.
- **Dinheiro**, **PercentualDeDesconto**, **PeriodoDeVigencia** (Value Objects).
- **ServicoDeCompraDeJogos** (Serviço de Domínio): regras que envolvem Jogo, Biblioteca e Pedido (jogo ativo, não possuído, sem pedido em andamento).

## Eventos
| Direção | Evento | Routing key | Fila |
|---|---|---|---|
| Publica | `OrderPlacedEvent` (`OrderId`, `UserId`, `UserEmail`, `GameId`, `GameTitle`, `Price`) | `order.placed` | — |
| Consome | `PaymentProcessedEvent` (`Approved`/`Rejected`) | `payment.processed` | `catalog.payment-processed` |

Ao receber `Approved`, o pedido passa a `Pago` e o jogo é adicionado à biblioteca (mesma transação local). Processamento idempotente.

## Endpoints
| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/jogos`, `/api/jogos/{id}` | autenticado |
| POST/PUT/DELETE | `/api/jogos`, `/api/jogos/{id}` | Administrador |
| PATCH | `/api/jogos/{id}/ativar` | Administrador |
| POST | `/api/jogos/{id}/promocoes` | Administrador |
| PATCH | `/api/jogos/{id}/promocoes/{promocaoId}/encerrar` | Administrador |
| POST | `/api/biblioteca/pedidos` | autenticado — inicia a compra (202) |
| GET | `/api/biblioteca/pedidos`, `/api/biblioteca/pedidos/{id}` | autenticado |
| GET | `/api/biblioteca` | autenticado |

## Camadas (DDD)

```
src/
  FCG.Catalog.Domain          Entidades, Value Objects, Agregados, Eventos de Domínio, Serviços de Domínio e interfaces de Repositório
  FCG.Catalog.Application     Casos de uso, DTOs, Mappers, eventos de integração e seus handlers, portas de saída
  FCG.Catalog.Infrastructure  EF Core (SQL Server), repositórios, RabbitMQ (RabbitMQ.Client)
  FCG.Catalog.Api             Apresentação: controllers, middleware de erros, JWT, Swagger e health checks
tests/
  FCG.Catalog.Domain.Tests    Testes de unidade das regras de domínio (xUnit)
k8s/                       Deployment, Service, ConfigMap e Secret
Dockerfile                 Build multi-stage (sdk:8.0 -> aspnet:8.0, usuário não-root)
```

O Domínio não referencia nenhum framework. A Infraestrutura implementa as interfaces definidas no Domínio/Aplicação.

## Executar

```powershell
dotnet build FCG.Catalog.slnx
dotnet test  FCG.Catalog.slnx
```

- **Local (dotnet run):** suba a infraestrutura com o compose do repositório `fcg-orchestration`
  (`docker compose up sqlserver rabbitmq`) e rode `dotnet run --project src/FCG.Catalog.Api` (ambiente Development, porta 5102).
- **Docker:** `docker build -t fcg/catalog-api:1.0.0 .`
- **Kubernetes:** `kubectl apply -f k8s/` (após aplicar namespace e infraestrutura do `fcg-orchestration`).

Health checks: `/health/live` (liveness) e `/health/ready` (readiness: SQL Server e RabbitMQ).
