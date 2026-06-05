using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Configures the <see cref="DbSender"/> entity for Entity Framework Core.
    /// </summary>
    internal class DbSenderConfiguration : IEntityTypeConfiguration<DbSender>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<DbSender> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Name).IsUnique();

            builder.Property(e => e.Id)
                .HasMaxLength(50)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.DisplayName)
                .HasMaxLength(200);

            builder.Property(e => e.Address)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Type)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.IsActive);

            builder.Property(e => e.CreatedAt);

            builder.Property(e => e.UpdatedAt);

            builder.Ignore(e => e.EndpointType);
        }
    }
}
