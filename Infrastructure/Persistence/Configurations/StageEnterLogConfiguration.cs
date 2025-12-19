using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Infrastructure.Persistence.Configurations
{
    public sealed class StageEnterLogConfiguration : IEntityTypeConfiguration<StageEnterLog>
    {
        public void Configure(EntityTypeBuilder<StageEnterLog> entity)
        {
            entity.ToTable("stage_enter_logs");

            entity.HasKey(x => x.StageEnterLogId);
            entity.Property(x => x.StageEnterLogId)
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

            entity.Property(x => x.EnteredDateTime)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.HasIndex(x => new { x.UserId, x.RequestId })
                .IsUnique();
        }
    }
}
