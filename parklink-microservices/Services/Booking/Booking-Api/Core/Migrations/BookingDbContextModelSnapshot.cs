﻿// <auto-generated />
using System;
using Booking_Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Booking_Api.Core.Migrations
{
    [DbContext(typeof(BookingDbContext))]
    partial class BookingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Booking_Domain.Entities.Booking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("BookingConfirmation")
                        .HasColumnType("bit");

                    b.Property<string>("CarRegistration")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Fees")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<bool>("FinePaid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<bool>("FineStatus")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ParkingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProviderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("QrCodeLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RecordId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("RefundAmount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(18, 2)")
                        .HasDefaultValue(0m);

                    b.Property<bool>("RefundStatus")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("SubTotal")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18, 2)");

                    b.HasKey("Id");

                    b.HasIndex("RecordId")
                        .IsUnique();

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("Booking_Domain.Entities.BookingRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("DepositStatus")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<decimal>("Fees")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<decimal>("SubTotal")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Verified")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("BookingRecords");
                });

            modelBuilder.Entity("Booking_Domain.Entities.BookingRefund", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BookingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProviderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RecordId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18, 2)");

                    b.HasKey("Id");

                    b.ToTable("BookingRefunds");
                });

            modelBuilder.Entity("Booking_Domain.Entities.Booking", b =>
                {
                    b.HasOne("Booking_Domain.Entities.BookingRecord", "BookingRecord")
                        .WithOne("Booking")
                        .HasForeignKey("Booking_Domain.Entities.Booking", "RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BookingRecord");
                });

            modelBuilder.Entity("Booking_Domain.Entities.BookingRecord", b =>
                {
                    b.Navigation("Booking")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
