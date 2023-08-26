﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizer.Database;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BalanceLedger> BalanceLedger { get; set; }
    public virtual DbSet<CostType> CostType { get; set; }
    public virtual DbSet<CropField> CropField { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder()
        {
            DataSource = DatabaseFile.FullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        };
        optionsBuilder.UseSqlite(builder.ToString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BalanceLedger>(entity =>
        {
            entity.ToTable("balanceLedger");

            entity.HasIndex(e => e.Id, "IX_balanceLedger_id")
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.BalanceChange)
                .HasColumnType("REAL (8, 2)")
                .HasColumnName("balanceChange")
                .HasDefaultValueSql("0.0");

            entity.Property(e => e.DateAdded)
                .IsRequired()
                .HasColumnType("DATETIME")
                .HasColumnName("dateAdded")
                .HasDefaultValueSql("datetime()");

            entity.Property(e => e.IdCostType).HasColumnName("id_costType");

            entity.Property(e => e.IdCropField).HasColumnName("id_cropField");

            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(d => d.IdCostTypeNavigation)
                .WithMany(p => p.BalanceLedger)
                .HasForeignKey(d => d.IdCostType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdCropFieldNavigation)
                .WithMany(p => p.BalanceLedger)
                .HasForeignKey(d => d.IdCropField)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CostType>(entity =>
        {
            entity.ToTable("costType");

            entity.HasIndex(e => e.Id, "IX_costType_id")
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
        });

        modelBuilder.Entity<CropField>(entity =>
        {
            entity.ToTable("cropField");

            entity.HasIndex(e => e.Id, "IX_cropField_id")
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Hectares)
                .HasColumnType("REAL (8, 2)")
                .HasColumnName("hectares")
                .HasDefaultValueSql("0.0");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasDefaultValueSql("\"Nowe pole\"");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}