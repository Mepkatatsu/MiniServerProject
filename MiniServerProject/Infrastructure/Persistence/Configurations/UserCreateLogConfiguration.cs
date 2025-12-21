using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Infrastructure.Persistence.Configurations
{
    public sealed class UserCreateLogConfiguration : IEntityTypeConfiguration<UserCreateLog>
    {
        public void Configure(EntityTypeBuilder<UserCreateLog> entity)
        {
            entity.ToTable("user_create_logs");

            entity.HasKey(x => x.UserCreateLogId);
            entity.Property(x => x.UserCreateLogId)
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.AccountId)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Nickname)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.HasIndex(x => new { x.AccountId })
                .IsUnique();
        }
    }
}
