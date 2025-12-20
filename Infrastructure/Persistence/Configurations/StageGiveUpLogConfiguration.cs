using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Infrastructure.Persistence.Configurations
{
    public sealed class StageGiveUpLogConfiguration : IEntityTypeConfiguration<StageGiveUpLog>
    {
        public void Configure(EntityTypeBuilder<StageGiveUpLog> entity)
        {
            entity.ToTable("stage_giveup_logs");

            entity.HasKey(x => x.StageGiveUpLogId);
            entity.Property(x => x.StageGiveUpLogId)
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.UserId)
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.StageId)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.RequestId)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.GaveUpDateTime)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.HasIndex(x => new { x.UserId, x.RequestId })
                .IsUnique();
        }
    }
}
