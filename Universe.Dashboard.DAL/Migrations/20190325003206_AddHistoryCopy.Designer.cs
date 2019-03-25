﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.DAL.Migrations
{
    [DbContext(typeof(DashboardContext))]
    [Migration("20190325003206_AddHistoryCopy")]
    partial class AddHistoryCopy
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity("Universe.Dashboard.DAL.DbInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Version");

                    b.HasKey("Id");

                    b.ToTable("DbInfo");
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
