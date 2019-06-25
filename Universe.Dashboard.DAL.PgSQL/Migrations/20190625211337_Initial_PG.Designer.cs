﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.DAL.PgSQL.Migrations
{
    [DbContext(typeof(DashboardContext))]
    [Migration("20190625211337_Initial_PG")]
    partial class Initial_PG
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Universe.Dashboard.DAL.DbInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Version");

                    b.HasKey("Id");

                    b.ToTable("W3Top_DbInfo");
                });

            modelBuilder.Entity("Universe.Dashboard.DAL.DiskBenchmarkEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Args");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("ErrorInfo");

                    b.Property<string>("MountPath");

                    b.Property<string>("Report");

                    b.Property<Guid>("Token");

                    b.HasKey("Id");

                    b.ToTable("W3Top_DiskBenchmark");
                });

            modelBuilder.Entity("Universe.Dashboard.DAL.HistoryCopy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("JsonBlob");

                    b.Property<string>("Key");

                    b.HasKey("Id");

                    b.ToTable("W3Top_HistoryCopy");
                });
#pragma warning restore 612, 618
        }
    }
}
