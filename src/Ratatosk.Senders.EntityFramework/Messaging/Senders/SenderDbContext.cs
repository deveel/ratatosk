//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.EntityFrameworkCore;

namespace Ratatosk
{
    /// <summary>
    /// The Entity Framework Core <see cref="DbContext"/> used to persist
    /// <see cref="SenderEntity"/> instances.
    /// </summary>
    public class SenderDbContext : DbContext
    {
        /// <summary>
        /// Constructs the context with the given options.
        /// </summary>
        /// <param name="options">The options for the context.</param>
        public SenderDbContext(DbContextOptions<SenderDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{T}"/> of sender entities.
        /// </summary>
        public DbSet<SenderEntity> Senders { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SenderEntity>(entity =>
            {
                entity.ToTable("senders");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(200);

                entity.Property(e => e.Address)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.EndpointType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt);

                entity.Property(e => e.UpdatedAt);
            });
        }
    }
}
