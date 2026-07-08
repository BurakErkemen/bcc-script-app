using System.IO;
using BccScriptApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BccScriptApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Script> Scriptler => Set<Script>();
    public DbSet<ScriptMaddesi> ScriptMaddeleri => Set<ScriptMaddesi>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();

    public static string DbPath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BccScriptApp");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "bccscripts.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Script>()
            .HasOne(s => s.Kategori)
            .WithMany(k => k.Scriptler)
            .HasForeignKey(s => s.KategoriId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ScriptMaddesi>()
            .HasOne<Script>()
            .WithMany(s => s.Maddeler)
            .HasForeignKey(m => m.ScriptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
