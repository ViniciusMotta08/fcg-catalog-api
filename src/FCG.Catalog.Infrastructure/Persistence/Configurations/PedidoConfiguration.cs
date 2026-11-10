using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.EmailDoComprador).HasMaxLength(200).IsRequired();
        builder.Property(p => p.TituloDoJogo).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Valor)
            .HasConversion(valor => valor.Valor, valor => Dinheiro.Criar(valor))
            .HasPrecision(10, 2)
            .IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.MotivoDaRecusa).HasMaxLength(Pedido.TamanhoMaximoMotivo);

        builder.HasIndex(p => p.UsuarioId);

        // Reforço no banco da regra "um único pedido aguardando pagamento por usuário/jogo".
        builder.HasIndex(p => new { p.UsuarioId, p.JogoId })
            .HasFilter("[Status] = 'AguardandoPagamento'")
            .IsUnique();

        builder.Ignore(p => p.EstaAguardandoPagamento);
    }
}
