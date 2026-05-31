//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.EntityFrameworkCore;

namespace Ratatosk.Senders
{
    /// <summary>
    /// The Entity Framework Core <see cref="DbContext"/> used to persist
    /// <see cref="DbSender"/> instances.
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
        public DbSet<DbSender> Senders { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DbSenderConfiguration());
        }
    }
}
