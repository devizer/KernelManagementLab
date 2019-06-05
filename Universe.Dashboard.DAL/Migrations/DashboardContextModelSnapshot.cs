﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.DAL.Migrations
{
    [DbContext(typeof(DashboardContext))]
    partial class DashboardContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity("Universe.Dashboard.DAL.DbInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Version");

                    b.HasKey("Id");

                    b.ToTable("DbInfo");
                });

            modelBuilder.Entity("Universe.Dashboard.DAL.DiskBenchmarkEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Args");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("MountPath");

                    b.Property<string>("Report");

                    b.Property<string>("Token");

                    b.HasKey("Id");

                    b.ToTable("DiskBenchmark");
                });

            modelBuilder.Entity("Universe.Dashboard.DAL.HistoryCopy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("JsonBlob");

                    b.Property<string>("Key");

                    b.HasKey("Id");

                    b.ToTable("HistoryCopy");
                });
#pragma warning restore 612, 618
        }
    }
}
