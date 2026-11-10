using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Comum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Persistence.Configurations;

public sealed class BibliotecaConfiguration : IEntityTypeConfiguration<Biblioteca>
{
    public void Configure(EntityTypeBuilder<Biblioteca> builder)
    {
        builder.ToTable("Bibliotecas");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("UsuarioId").ValueGeneratedNever();
        builder.Ignore(b => b.UsuarioId);

        builder.HasMany(b => b.Itens)
            .WithOne()
            .HasForeignKey("UsuarioId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ItemDaBibliotecaConfiguration : IEntityTypeConfiguration<ItemDaBiblioteca>
{
    public void Configure(EntityTypeBuilder<ItemDaBiblioteca> builder)
    {
        builder.ToTable("ItensDaBiblioteca");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.TituloDoJogo).HasMaxLength(200).IsRequired();
        builder.Property(i => i.PrecoPago)
            .HasConversion(valor => valor.Valor, valor => Dinheiro.Criar(valor))
            .HasPrecision(10, 2)
            .IsRequired();

        // Reforço no banco da invariante "um jogo no máximo uma vez por biblioteca".
        builder.HasIndex("UsuarioId", nameof(ItemDaBiblioteca.JogoId)).IsUnique();
    }
}
