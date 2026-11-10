using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.Jogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Persistence.Configurations;

public sealed class JogoConfiguration : IEntityTypeConfiguration<Jogo>
{
    public void Configure(EntityTypeBuilder<Jogo> builder)
    {
        builder.ToTable("Jogos");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedNever();

        builder.Property(j => j.Titulo).HasMaxLength(Jogo.TamanhoMaximoTitulo).IsRequired();
        builder.HasIndex(j => j.Titulo).IsUnique();
        builder.Property(j => j.Descricao).HasMaxLength(Jogo.TamanhoMaximoDescricao);
        builder.Property(j => j.Genero).HasMaxLength(Jogo.TamanhoMaximoGenero).IsRequired();

        builder.Property(j => j.Preco)
            .HasConversion(preco => preco.Valor, valor => Dinheiro.Criar(valor))
            .HasPrecision(10, 2)
            .IsRequired();

        // Promoções só existem dentro do agregado Jogo (sem DbSet próprio).
        builder.HasMany(j => j.Promocoes)
            .WithOne()
            .HasForeignKey("JogoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(j => j.Promocoes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PromocaoConfiguration : IEntityTypeConfiguration<Promocao>
{
    public void Configure(EntityTypeBuilder<Promocao> builder)
    {
        builder.ToTable("Promocoes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nome).HasMaxLength(Promocao.TamanhoMaximoNome).IsRequired();

        builder.Property(p => p.Desconto)
            .HasConversion(desconto => desconto.Valor, valor => PercentualDeDesconto.Criar(valor))
            .HasColumnName("PercentualDesconto")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.OwnsOne(p => p.Vigencia, vigencia =>
        {
            vigencia.Property(v => v.Inicio).HasColumnName("DataInicio").IsRequired();
            vigencia.Property(v => v.Fim).HasColumnName("DataFim").IsRequired();
        });
        builder.Navigation(p => p.Vigencia).IsRequired();
    }
}
