using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Tipo).HasMaxLength(200).IsRequired();
        builder.Property(m => m.RoutingKey).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Conteudo).IsRequired();
        builder.Property(m => m.UltimoErro).HasMaxLength(OutboxMessage.TamanhoMaximoErro);

        builder.HasIndex(m => new { m.ProcessadaEm, m.OcorreuEm });
    }
}
