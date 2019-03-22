﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.DAL.Migrations
{
    [DbContext(typeof(DashboardContext))]
    [Migration("20190322024148_InitialCreate")]
    partial class InitialCreate
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

                    b.ToTable("Info");
                });
#pragma warning restore 612, 618
        }
    }
}
