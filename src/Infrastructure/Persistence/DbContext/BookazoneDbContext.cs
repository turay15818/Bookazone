using Bookazone.Application.Interfaces.Context;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Entities.Others;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Domain.Entities.Reviews;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Entities.Subscription;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.DbContext;

    public class BookazoneDbContext(DbContextOptions<BookazoneDbContext> options, IUserContext userContext) : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        // ================================================
        // CORE / PLATFORM TABLES
        // ================================================

        public DbSet<StoredFile> StoredFiles { get; set; } = null!;
        public DbSet<AppConfig>? AppConfig { get; set; }
        public DbSet<RefreshToken>? RefreshTokens { get; set; }

        public DbSet<Tenants> Tenants { get; set; } = default!;
        public DbSet<UserTenant> UserTenants { get; set; } = default!;
        public DbSet<TenantSettings> TenantSettings { get; set; } = default!;
        public DbSet<TenantWorkingHour> TenantWorkingHours { get; set; } = default!;

        public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; } = default!;
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<AuthProvider> AuthProviders { get; set; } = default!;
        public DbSet<UserPassword> UserPasswords { get; set; } = default!;
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        public DbSet<Device> Devices { get; set; } = default!;
        public DbSet<OneTimePassword> OneTimePassword { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<RolePermission> RolePermissions { get; set; } = default!;
        public DbSet<Permission> Permissions { get; set; } = default!;
        public DbSet<UserPermission> UserPermissions { get; set; } = default!;
        public DbSet<PermissionGroup> PermissionGroups { get; set; } = default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;

        // ================================================
        // BOOKING / SPORTS TABLES
        // ================================================

        public DbSet<BookingCategory> BookingCategories { get; set; } = default!;
        public DbSet<TenantBookingCategory> TenantBookingCategories { get; set; } = default!;
        public DbSet<TenantSportType> TenantSportTypes { get; set; } = default!;
        public DbSet<SportResource> SportResources { get; set; } = default!;
        public DbSet<SportResourceAvailability> SportResourceAvailabilities { get; set; } = default!;
        public DbSet<SportResourceBlackout> SportResourceBlackouts { get; set; } = default!;
        public DbSet<SportResourcePricingRule> SportResourcePricingRules { get; set; } = default!;
        public DbSet<Booking> Bookings { get; set; } = default!;
        public DbSet<BookingApproval> BookingApprovals { get; set; } = default!;
        public DbSet<TenantSportMedia> TenantSportMedia { get; set; } = default!;
        public DbSet<SportResourceMedia> SportResourceMedia { get; set; } = default!;
        public DbSet<EventCategory> EventCategories { get; set; } = default!;
        public DbSet<EventCategorySubscription> EventCategorySubscriptions { get; set; } = default!;
        public DbSet<Event> Events { get; set; } = default!;
        public DbSet<EventTicketType> EventTicketTypes { get; set; } = default!;
        public DbSet<EventScheduleItem> EventScheduleItems { get; set; } = default!;
        public DbSet<EventPolicy> EventPolicies { get; set; } = default!;
        public DbSet<EventMedia> EventMedias { get; set; } = default!;
        public DbSet<EventVenue> EventVenues { get; set; } = default!;
        public DbSet<EventOrder> EventOrders { get; set; } = default!;
        public DbSet<EventOrderItem> EventOrderItems { get; set; } = default!;
        public DbSet<Vehicle> Vehicles { get; set; } = default!;
        public DbSet<VehiclePricingRule> VehiclePricingRules { get; set; } = default!;
        public DbSet<VehicleSpec> VehicleSpecs { get; set; } = default!;
        public DbSet<VehiclePolicy> VehiclePolicies { get; set; } = default!;
        public DbSet<VehicleMedia> VehicleMedia { get; set; } = default!;
        public DbSet<VehicleOrder> VehicleOrders { get; set; } = default!;
        public DbSet<Equipment> Equipments { get; set; } = default!;
        public DbSet<EquipmentPricingRule> EquipmentPricingRules { get; set; } = default!;
        public DbSet<EquipmentSpec> EquipmentSpecs { get; set; } = default!;
        public DbSet<EquipmentPolicy> EquipmentPolicies { get; set; } = default!;
        public DbSet<EquipmentMedia> EquipmentMedia { get; set; } = default!;
        public DbSet<EquipmentOrder> EquipmentOrders { get; set; } = default!;
        public DbSet<Rental> Rentals { get; set; } = default!;
        public DbSet<RentalPricingRule> RentalPricingRules { get; set; } = default!;
        public DbSet<RentalSpec> RentalSpecs { get; set; } = default!;
        public DbSet<RentalPolicy> RentalPolicies { get; set; } = default!;
        public DbSet<RentalMedia> RentalMedia { get; set; } = default!;
        public DbSet<RentalOrder> RentalOrders { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<ReviewAspectRating> ReviewAspectRatings { get; set; } = default!;

        // ================================================
        // MODEL CONFIGURATION
        // ================================================
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<UserBaseEntity>();
            var now = DateTime.UtcNow;
            var user = userContext.Username ?? "system";
            foreach (var e in entries)
            {
                if (e.State == EntityState.Added)
                {
                    e.Entity.DateCreated = now;
                    e.Entity.CreatedBy = user;
                    e.Entity.FkUserId = userContext.UserId ?? null;
                    e.Entity.Active = e.Entity.Active;
                    e.Entity.Deleted = false;
                    // e.Entity.RowVersion = 1;
                }
                else if (e.State == EntityState.Modified)
                {
                    e.Entity.DateUpdated = now;
                    e.Entity.UpdatedBy = user;
                    // e.Entity.RowVersion = +1;
                }
                else if (e.State == EntityState.Deleted)
                {
                    e.State = EntityState.Modified;
                    e.Entity.Deleted = true;
                    e.Entity.DateDeleted = now;
                    e.Entity.DeletedBy = user;
                    // e.Entity.RowVersion = +1;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
        
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookazoneDbContext).Assembly);
            var baseEntityTypes = modelBuilder.Model
                .GetEntityTypes()
                .Where(t =>
                    typeof(BaseEntity).IsAssignableFrom(t.ClrType) &&
                    t.ClrType != typeof(Users));

            var userOwnedTypes = modelBuilder.Model
                .GetEntityTypes()
                .Where(t => typeof(UserBaseEntity).IsAssignableFrom(t.ClrType));

            foreach (var type in userOwnedTypes)
            {
                modelBuilder.Entity(type.ClrType)
                    .Navigation(nameof(UserBaseEntity.FkUser))
                    .AutoInclude();
            }



            modelBuilder.Entity<Users>()
                .Ignore(u => u.FkTenant)
                .Ignore(u => u.FkTenantId);
            
        

            // ==========================
            // CORE NAVIGATIONS
            // ==========================
          //  modelBuilder.Entity<Users>().Navigation(u => u.FkTenant).AutoInclude();
            modelBuilder.Entity<UserPassword>().Navigation(u => u.FkUser).AutoInclude();
            modelBuilder.Entity<Device>().Navigation(u => u.FkUser).AutoInclude();
            modelBuilder.Entity<UserPermission>().Navigation(u => u.FkPermission).AutoInclude();
            modelBuilder.Entity<UserPermission>().Navigation(u => u.FkUser).AutoInclude();
            modelBuilder.Entity<UserRole>().Navigation(u => u.FkUser).AutoInclude();
            modelBuilder.Entity<UserRole>().Navigation(u => u.FkRole).AutoInclude();
            modelBuilder.Entity<RolePermission>().Navigation(u => u.FkPermission).AutoInclude();
            modelBuilder.Entity<RolePermission>().Navigation(u => u.FkRole).AutoInclude();

            modelBuilder.Entity<RefreshToken>().Navigation(u => u.FkDevice).AutoInclude();
            modelBuilder.Entity<RefreshToken>().Navigation(u => u.FkUser).AutoInclude();

            modelBuilder.Entity<UserRole>().HasIndex(ur => new { ur.FkUserId, ur.FkRoleId }).IsUnique();
            modelBuilder.Entity<UserPermission>().HasIndex(up => new { up.FkUserId, up.FkPermissionId }).IsUnique();

        }
    }
