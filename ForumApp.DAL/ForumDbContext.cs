using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ForumApp.Data;

namespace ForumApp.DAL
{
    /// <summary>
    /// Database context for the Forum application using EF Core
    /// </summary>
    public class ForumDbContext : IdentityDbContext<IdentityUser>
    {
        public ForumDbContext(DbContextOptions<ForumDbContext> options)
            : base(options)
        {
        }

        // DbSets for forum entities
        public DbSet<Forum> Forums { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Forum entity
            builder.Entity<Forum>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Seed 3 forums
            builder.Entity<Forum>().HasData(
                new Forum { Id = 1, Name = "General Discussion", Description = "Talk about anything", CreatedAt = new DateTime(2025, 1, 1) },
                new Forum { Id = 2, Name = "Tech Talk", Description = "Discuss technology and programming", CreatedAt = new DateTime(2025, 1, 1) },
                new Forum { Id = 3, Name = "Off Topic", Description = "Casual conversations and fun", CreatedAt = new DateTime(2025, 1, 1) }
            );

            // Configure Post entity
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt);

                entity.HasOne(e => e.Forum)
                    .WithMany(f => f.Posts)
                    .HasForeignKey(e => e.ForumId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ForumId);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure Comment entity
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt);

                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically set UpdatedAt timestamps.
        /// </summary>
        public override int SaveChanges()
        {
            ApplyTimestamps();

            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically set UpdatedAt timestamps.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Helper method to set UpdatedAt for modified entities.
        /// </summary>
        private void ApplyTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Forum || e.Entity is Post || e.Entity is Comment);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    ((dynamic)entry.Entity).UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

