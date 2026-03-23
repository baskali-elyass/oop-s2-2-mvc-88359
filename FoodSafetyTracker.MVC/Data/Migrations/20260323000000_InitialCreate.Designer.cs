using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using FoodSafetyTracker.MVC.Data;

#nullable disable

namespace FoodSafetyTracker.MVC.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260323000000_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.2");

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
