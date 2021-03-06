﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.DAL.Migrations
{
    [DbContext(typeof(DashboardContext))]
    [Migration("20190621204217_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity("Universe.Dashboard.DAL.DbInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Version")
                        .HasColumnType("NVARCHAR(32767)");

                    b.HasKey("Id");

                    b.ToTable("DbInfo");
                });

            modelBuilder.Entity("Universe.Dashboard.DAL.DiskBenchmarkEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Args")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("ErrorInfo");

                    b.Property<string>("MountPath")
                        .HasColumnType("NVARCHAR(32767)");

                    b.Property<string>("Report")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Token")
                        .HasColumnType("BINARY(16)");

                    b.HasKey("Id");

                    b.HasIndex("Token")
                        .IsUnique();

                    b.ToTable("DiskBenchmark");
                });

            modelBuilder.Entity("Universe.Dashboard.DAL.HistoryCopy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("JsonBlob")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .HasColumnType("NVARCHAR(32767)");

                    b.HasKey("Id");

                    b.ToTable("HistoryCopy");
                });
#pragma warning restore 612, 618
        }
    }
}
