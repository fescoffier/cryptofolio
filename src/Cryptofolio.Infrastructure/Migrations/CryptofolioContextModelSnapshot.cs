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
                .HasDefaultSchema("data")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Cryptofolio.Infrastructure.Entities.Asset", b =>
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

            modelBuilder.Entity("Cryptofolio.Infrastructure.Entities.AssetTicker", b =>
                {
                    b.Property<string>("asset_id")
                        .HasColumnType("character varying(100)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.Property<string>("VsCurrency")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("vs_currency");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric")
                        .HasColumnName("value");

                    b.HasKey("asset_id", "Timestamp", "VsCurrency");

                    b.HasIndex("Timestamp");

                    b.HasIndex("VsCurrency");

                    b.ToTable("asset_ticker");
                });

            modelBuilder.Entity("Cryptofolio.Infrastructure.Entities.Exchange", b =>
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

            modelBuilder.Entity("Cryptofolio.Infrastructure.Entities.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("key");

                    b.Property<string>("Group")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("group");

                    b.Property<string>("Value")
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("Key");

                    b.ToTable("setting");
                });

            modelBuilder.Entity("Cryptofolio.Infrastructure.Entities.Wallet", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)")
                        .HasColumnName("name");

                    b.Property<bool>("Selected")
                        .HasColumnType("boolean")
                        .HasColumnName("selected");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("wallet");
                });

            modelBuilder.Entity("Cryptofolio.Infrastructure.Entities.AssetTicker", b =>
                {
                    b.HasOne("Cryptofolio.Infrastructure.Entities.Asset", "Asset")
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
