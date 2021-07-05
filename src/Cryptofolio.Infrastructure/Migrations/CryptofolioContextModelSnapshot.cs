﻿// <auto-generated />
using System;
using Cryptofolio.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Cryptofolio.Infrastructure.Migrations
{
    [DbContext(typeof(CryptofolioContext))]
    partial class CryptofolioContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Cryptofolio.Core.Entities.Asset", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)")
                        .HasColumnName("name");

                    b.Property<string>("Symbol")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("symbol");

                    b.HasKey("Id");

                    b.ToTable("asset");
                });

            modelBuilder.Entity("Cryptofolio.Core.Entities.AssetTicker", b =>
                {
                    b.Property<string>("asset_id")
                        .HasColumnType("character varying(100)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric")
                        .HasColumnName("value");

                    b.Property<string>("VsCurrency")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("vs_currency");

                    b.HasKey("asset_id", "Timestamp");

                    b.HasIndex("Timestamp");

                    b.HasIndex("VsCurrency");

                    b.ToTable("asset_ticker");
                });

            modelBuilder.Entity("Cryptofolio.Core.Entities.Exchange", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Image")
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("image");

                    b.Property<string>("Name")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)")
                        .HasColumnName("name");

                    b.Property<string>("Url")
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("url");

                    b.Property<long?>("YearEstablished")
                        .HasColumnType("bigint")
                        .HasColumnName("year_established");

                    b.HasKey("Id");

                    b.ToTable("exchange");
                });

            modelBuilder.Entity("Cryptofolio.Core.Entities.AssetTicker", b =>
                {
                    b.HasOne("Cryptofolio.Core.Entities.Asset", "Asset")
                        .WithMany()
                        .HasForeignKey("asset_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Asset");
                });
#pragma warning restore 612, 618
        }
    }
}
