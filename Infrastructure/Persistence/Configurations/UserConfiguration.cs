using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniServerProject.Domain.Entities;

namespace MiniServerProject.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("users");

            entity.Property(x => x.UserId)
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();
            entity.HasKey(x => x.UserId);

            entity.Property(x => x.Nickname)
                .HasMaxLength(32)
                .IsRequired();
            entity.HasIndex(x => x.Nickname);

            entity.Property(x => x.Level)
                .HasColumnType("smallint unsigned")
                .HasDefaultValue((ushort)1)
                .IsRequired();

            entity.Property(x => x.Stamina)
                .HasColumnType("smallint unsigned")
                .IsRequired();

            entity.Property(x => x.Gold)
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.Exp)
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.Property(x => x.LastStaminaUpdateTime)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.Property(x => x.CurrentStageId)
                .HasMaxLength(32)
                .IsRequired(false);
        }
    }
}
