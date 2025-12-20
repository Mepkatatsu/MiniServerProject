using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Infrastructure.Persistence.Configurations
{
    public sealed class StageClearLogConfiguration : IEntityTypeConfiguration<StageClearLog>
    {
        public void Configure(EntityTypeBuilder<StageClearLog> entity)
        {
            entity.ToTable("stage_clear_logs");

            entity.HasKey(x => x.StageClearLogId);
            entity.Property(x => x.StageClearLogId)
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

            entity.Property(x => x.ClearedDateTime)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.HasIndex(x => new { x.UserId, x.RequestId })
                .IsUnique();
        }
    }
}
