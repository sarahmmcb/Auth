using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Auth.Contracts;

namespace AuthApi.Data
{
    public class UserDbContext (DbContextOptions<UserDbContext> options, IConfiguration configuration) : DbContext(options)
    {
        public DbSet<User> User => Set<User>();
        public DbSet<UserHistory> UserHistory => Set<UserHistory>();
        public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
        public DbSet<Role> Role => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("Default"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("core");

            modelBuilder.Entity<RefreshToken>(rt => {
                rt.HasKey(i => i.Id);
                rt.HasIndex(i => i.UserName);
            });
            
            modelBuilder.Entity<Role>(r => {
                r.Property(i => i.RoleName).IsRequired();
                r.HasKey(i => i.Id);
                r.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            });

            modelBuilder.Entity<User>(u => {
                u.HasKey(i => i.Id);
                u.Property(i => i.Id).ValueGeneratedOnAdd();
                u.Property(i => i.FirstName).IsRequired().HasMaxLength(128);
                u.Property(i => i.LastName).IsRequired().HasMaxLength(128);
                u.Property(i => i.Email).IsRequired().HasMaxLength(128);
                u.Property(i => i.UserName).IsRequired().HasMaxLength(128);
                u.Property(i => i.Password).IsRequired();
                u.Property(i => i.AccountStatus).IsRequired();
                u.Property(i => i.CreateDate).IsRequired();

                u.HasIndex(i => i.UserName).IsUnique();

                u.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });

            modelBuilder.Entity<UserRole>(ur => { 
                ur.Property(i => i.UserId).IsRequired();
                ur.Property(i => i.RoleId).IsRequired();
                ur.HasKey(i => new { i.UserId, i.RoleId });
            });

            modelBuilder.Entity<UserHistory>(u => {
                u.HasKey(i => i.Uniqueifier);
                u.Property(i => i.Uniqueifier).ValueGeneratedOnAdd();
                u.Property(i => i.Id).IsRequired();
                u.Property(i => i.FirstName).IsRequired().HasMaxLength(128);
                u.Property(i => i.LastName).IsRequired().HasMaxLength(128);
                u.Property(i => i.Email).IsRequired().HasMaxLength(128);
                u.Property(i => i.UserName).IsRequired().HasMaxLength(128);
                u.Property(i => i.Password).IsRequired();
                u.Property(i => i.AccountStatus).IsRequired();
                u.Property(i => i.CreateDate).IsRequired();
                u.Property(i => i.UpdateUserId).IsRequired();
                u.Property(i => i.UpdateDate).IsRequired();
                u.Property(i => i.IsDeleted).IsRequired().HasDefaultValue(false);
            });

            // Seed Data - TODO: Consider moving this to a stored proc
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin"}
             );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Bob",
                    LastName = "Terwilliger",
                    Email = "sideshow.bob@simpsons.org",
                    UserName = "sideshow.bob@simpsons.org",
                    Password = "$2a$11$yIyAzG2qqVpGYr4vvW5Rhu0zAQBlo3GuxUsc/gyyvoqIOBC98A91W", // BobsPassword
                    AccountStatus = AccountStatus.Active,
                    CreateDate = new DateTimeOffset(new DateTime(2025, 03, 15))
                });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    FirstName = "Helen",
                    LastName = "Lovejoy",
                    Email = "helen.lovejoy@simpsons.org",
                    UserName = "helen.lovejoy@simpsons.org",
                    Password = "$2a$11$jlxgg3huqAkKy9vN6enwred6wUvH.9B6LVTZWCLBZbcCmgvnXaCri", // HelensPassword
                    AccountStatus = AccountStatus.Active,
                    CreateDate = new DateTimeOffset(new DateTime(2025, 03, 15))
                });

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    UserId = 1,
                    RoleId = 1
                }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // TODO: these saves should happen within a transaction
            var modifiedEntries = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToList();

            await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // TODO: check if it's necessary to do this here in order for CurrentValue to have the updated value below

            foreach (var entry in modifiedEntries)
            {
                var history = new UserHistory();

                UserHistory.Add(history);
                history.Id = (int)(entry.Property("Id").CurrentValue ?? 0);
                history.FirstName = (string)(entry.Property("FirstName").CurrentValue ?? "");
                history.LastName = (string)(entry.Property("LastName").CurrentValue ?? "");
                history.Email = (string)(entry.Property("Email").CurrentValue ?? "");
                history.UserName = (string)(entry.Property("UserName").CurrentValue ?? "");
                history.Password = (string)(entry.Property("Password").CurrentValue ?? "");
                history.AccountStatus = (AccountStatus)(entry.Property("AccountStatus").CurrentValue ?? AccountStatus.Unknown);
                history.CreateDate = (DateTimeOffset)(entry.Property("CreateDate").CurrentValue ?? DateTimeOffset.MinValue);
                history.UpdateDate = DateTimeOffset.UtcNow;
                history.UpdateUserId = 0; // TODO: Figure out what this should be
                history.IsDeleted = entry.State == EntityState.Detached; // not kosher but whatever
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
