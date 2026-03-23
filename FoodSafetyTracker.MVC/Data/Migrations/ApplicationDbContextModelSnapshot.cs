using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FoodSafetyTracker.MVC.Data;

#nullable disable

namespace FoodSafetyTracker.MVC.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.2");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
            {
                b.Property<string>("Id").HasColumnType("TEXT");
                b.Property<string>("ConcurrencyStamp").HasColumnType("TEXT");
                b.Property<string>("Name").HasMaxLength(256).HasColumnType("TEXT");
                b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex");
                b.ToTable("AspNetRoles", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
            {
                b.Property<string>("Id").HasColumnType("TEXT");
                b.Property<int>("AccessFailedCount").HasColumnType("INTEGER");
                b.Property<string>("ConcurrencyStamp").HasColumnType("TEXT");
                b.Property<string>("Email").HasMaxLength(256).HasColumnType("TEXT");
                b.Property<bool>("EmailConfirmed").HasColumnType("INTEGER");
                b.Property<bool>("LockoutEnabled").HasColumnType("INTEGER");
                b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("TEXT");
                b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("TEXT");
                b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("TEXT");
                b.Property<string>("PasswordHash").HasColumnType("TEXT");
                b.Property<string>("PhoneNumber").HasColumnType("TEXT");
                b.Property<bool>("PhoneNumberConfirmed").HasColumnType("INTEGER");
                b.Property<string>("SecurityStamp").HasColumnType("TEXT");
                b.Property<bool>("TwoFactorEnabled").HasColumnType("INTEGER");
                b.Property<string>("UserName").HasMaxLength(256).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
                b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex");
                b.ToTable("AspNetUsers", (string)null);
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.Premises", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
                b.Property<string>("Address").IsRequired().HasMaxLength(300).HasColumnType("TEXT");
                b.Property<string>("Town").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<int>("RiskRating").HasColumnType("INTEGER");
                b.HasKey("Id");
                b.ToTable("Premises");
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.Inspection", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<int>("PremisesId").HasColumnType("INTEGER");
                b.Property<DateTime>("InspectionDate").HasColumnType("TEXT");
                b.Property<int>("Score").HasColumnType("INTEGER");
                b.Property<int>("Outcome").HasColumnType("INTEGER");
                b.Property<string>("Notes").HasMaxLength(1000).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("PremisesId");
                b.ToTable("Inspections");
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.FollowUp", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<int>("InspectionId").HasColumnType("INTEGER");
                b.Property<DateTime>("DueDate").HasColumnType("TEXT");
                b.Property<int>("Status").HasColumnType("INTEGER");
                b.Property<DateTime?>("ClosedDate").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("InspectionId");
                b.ToTable("FollowUps");
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.Inspection", b =>
            {
                b.HasOne("FoodSafetyTracker.Domain.Premises", "Premises")
                    .WithMany("Inspections")
                    .HasForeignKey("PremisesId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
                b.Navigation("Premises");
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.FollowUp", b =>
            {
                b.HasOne("FoodSafetyTracker.Domain.Inspection", "Inspection")
                    .WithMany("FollowUps")
                    .HasForeignKey("InspectionId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
                b.Navigation("Inspection");
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.Premises", b =>
            {
                b.Navigation("Inspections");
            });

            modelBuilder.Entity("FoodSafetyTracker.Domain.Inspection", b =>
            {
                b.Navigation("FollowUps");
            });
#pragma warning restore 612, 618
        }
    }
}
