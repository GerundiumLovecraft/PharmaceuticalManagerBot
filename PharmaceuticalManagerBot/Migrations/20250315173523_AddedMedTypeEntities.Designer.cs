﻿// <auto-generated />
using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PharmaceuticalManagerBot.Data;

#nullable disable

namespace PharmaceuticalManagerBot.Migrations
{
    [DbContext(typeof(PharmaceuticalManagerBotContext))]
    [Migration("20250315173523_AddedMedTypeEntities")]
    partial class AddedMedTypeEntities
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("medbot")
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.ActivePharmIngredient", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("ActivePharmIngredientName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("api");

                    b.HasKey("ID");

                    b.ToTable("ActivePharmIngedients", "medbot");
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.MedType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("ID");

                    b.ToTable("MedTypes", "medbot");

                    b.HasData(
                        new
                        {
                            ID = 1,
                            Type = "Обезболивающие и противовоспалительные"
                        },
                        new
                        {
                            ID = 2,
                            Type = "Жаропонижающие"
                        },
                        new
                        {
                            ID = 3,
                            Type = "Антибиотики"
                        },
                        new
                        {
                            ID = 4,
                            Type = "Противовирусные"
                        },
                        new
                        {
                            ID = 5,
                            Type = "Антигистаминные"
                        },
                        new
                        {
                            ID = 6,
                            Type = "Сердечно-сосудистые"
                        },
                        new
                        {
                            ID = 7,
                            Type = "Желудочно-кишечные"
                        },
                        new
                        {
                            ID = 8,
                            Type = "Антидепрессанты и анксиолитики"
                        },
                        new
                        {
                            ID = 9,
                            Type = "Гормональные препараты"
                        },
                        new
                        {
                            ID = 10,
                            Type = "Витамины и минералы"
                        },
                        new
                        {
                            ID = 11,
                            Type = "Противогрибковые"
                        },
                        new
                        {
                            ID = 12,
                            Type = "Препараты для дыхательной системы"
                        },
                        new
                        {
                            ID = 13,
                            Type = "Онкологические (противоопухолевые)"
                        },
                        new
                        {
                            ID = 14,
                            Type = "Анестетики"
                        },
                        new
                        {
                            ID = 15,
                            Type = "Иммуномодуляторы"
                        },
                        new
                        {
                            ID = 16,
                            Type = "Спазмолитик"
                        },
                        new
                        {
                            ID = 17,
                            Type = "Другое"
                        });
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.Medicine", b =>
                {
                    b.Property<int>("MedicineId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("MedicineId"));

                    b.Property<int>("ActivePharmIngredientId")
                        .HasColumnType("integer")
                        .HasColumnName("api_id");

                    b.Property<DateOnly>("ExpiryDate")
                        .HasColumnType("date")
                        .HasColumnName("expiry_date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("TypeId")
                        .HasColumnType("integer")
                        .HasColumnName("type_id");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("MedicineId");

                    b.HasIndex("ActivePharmIngredientId");

                    b.HasIndex("TypeId");

                    b.HasIndex("UserId");

                    b.ToTable("Medicines", "medbot");
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.User", b =>
                {
                    b.Property<int>("UID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("uid");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UID"));

                    b.Property<bool>("CheckRequired")
                        .HasColumnType("boolean")
                        .HasColumnName("check_req");

                    b.Property<BigInteger>("TgChatId")
                        .HasColumnType("numeric")
                        .HasColumnName("telegram_chat_id");

                    b.Property<BigInteger>("TgId")
                        .HasColumnType("numeric")
                        .HasColumnName("telegram_id");

                    b.HasKey("UID");

                    b.HasIndex("TgId")
                        .IsUnique();

                    b.HasIndex("UID")
                        .IsUnique();

                    b.ToTable("Users", "medbot");
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.Medicine", b =>
                {
                    b.HasOne("PharmaceuticalManagerBot.Models.ActivePharmIngredient", "ActivePharmIngredient")
                        .WithMany("Medicines")
                        .HasForeignKey("ActivePharmIngredientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PharmaceuticalManagerBot.Models.MedType", "MedType")
                        .WithMany("Medicines")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PharmaceuticalManagerBot.Models.User", "User")
                        .WithMany("Medicines")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ActivePharmIngredient");

                    b.Navigation("MedType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.ActivePharmIngredient", b =>
                {
                    b.Navigation("Medicines");
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.MedType", b =>
                {
                    b.Navigation("Medicines");
                });

            modelBuilder.Entity("PharmaceuticalManagerBot.Models.User", b =>
                {
                    b.Navigation("Medicines");
                });
#pragma warning restore 612, 618
        }
    }
}
