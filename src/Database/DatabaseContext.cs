﻿using FarmOrganizer.IO;
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

    public virtual DbSet<BalanceLedger> BalanceLedgers { get; set; }
    public virtual DbSet<CostType> CostTypes { get; set; }
    public virtual DbSet<CropField> CropFields { get; set; }
    public virtual DbSet<FieldEfficiency> FieldEfficiencies { get; set; }
    public virtual DbSet<Season> Seasons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        SqliteConnectionStringBuilder builder = new()
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

            entity.HasIndex(e => e.Id, "IX_balanceLedger_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BalanceChange)
                .HasDefaultValueSql("0.0")
                .HasColumnType("REAL (8, 2)")
                .HasColumnName("balanceChange");
            entity.Property(e => e.DateAdded)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME")
                .HasColumnName("dateAdded");
            entity.Property(e => e.IdCostType).HasColumnName("id_costType");
            entity.Property(e => e.IdCropField).HasColumnName("id_cropField");
            entity.Property(e => e.IdSeason).HasColumnName("id_season");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(d => d.IdCostTypeNavigation).WithMany(p => p.BalanceLedgers)
                .HasForeignKey(d => d.IdCostType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdCropFieldNavigation).WithMany(p => p.BalanceLedgers)
                .HasForeignKey(d => d.IdCropField)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdSeasonNavigation).WithMany(p => p.BalanceLedgers).HasForeignKey(d => d.IdSeason);
        });

        modelBuilder.Entity<CostType>(entity =>
        {
            entity.ToTable("costType");

            entity.HasIndex(e => e.Id, "IX_costType_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsExpense).HasColumnName("isExpense");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
        });

        modelBuilder.Entity<CropField>(entity =>
        {
            entity.ToTable("cropField");

            entity.HasIndex(e => e.Id, "IX_cropField_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Hectares)
                .HasDefaultValueSql("0.0")
                .HasColumnType("REAL (8, 2)")
                .HasColumnName("hectares");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasDefaultValueSql("\"Nowe pole\"")
                .HasColumnName("name");
        });

        modelBuilder.Entity<FieldEfficiency>(entity =>
        {
            entity.ToTable("fieldEfficiency");

            entity.HasIndex(e => e.Id, "IX_fieldEfficiency_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CropAmount)
                .HasColumnType("REAL (8, 2)")
                .HasColumnName("cropAmount");
            entity.Property(e => e.Hectares)
                .HasColumnType("REAL (8, 2)")
                .HasColumnName("hectares");
            entity.Property(e => e.IdCropField).HasColumnName("id_cropField");
            entity.Property(e => e.IdSeason).HasColumnName("id_season");

            entity.HasOne(d => d.IdCropFieldNavigation).WithMany(p => p.FieldEfficiencies)
                .HasForeignKey(d => d.IdCropField)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdSeasonNavigation).WithMany(p => p.FieldEfficiencies)
                .HasForeignKey(d => d.IdSeason)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.ToTable("season");

            entity.HasIndex(e => e.Id, "IX_season_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateEnd)
                .IsRequired()
                .HasDefaultValueSql("DATE('now', 'start of year', '+1 year', '-1 day')")
                .HasColumnType("DATETIME")
                .HasColumnName("dateEnd");
            entity.Property(e => e.DateStart)
                .IsRequired()
                .HasDefaultValueSql("datetime()")
                .HasColumnType("DATETIME")
                .HasColumnName("dateStart");
            entity.Property(e => e.HasConcluded)
                .HasDefaultValueSql("0")
                .HasColumnName("hasConcluded");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasDefaultValueSql("\"Nowy sezon\"")
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
