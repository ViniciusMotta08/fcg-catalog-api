using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Persistence;

/// <summary>Banco exclusivo do contexto de Catálogo (database-per-service).</summary>
public sealed class CatalogDbContext : ContextoComOutbox
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IIntegrationEventMapper mapeador)
        : base(options, mapeador) { }

    public DbSet<Jogo> Jogos => Set<Jogo>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<Biblioteca> Bibliotecas => Set<Biblioteca>();
}
