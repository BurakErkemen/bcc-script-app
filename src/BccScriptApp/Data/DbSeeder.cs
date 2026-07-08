using BccScriptApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BccScriptApp.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        var yeniOlusturuldu = db.Database.EnsureCreated();

        // Eski şemadan kalan veritabanında madde tablosu yoksa sıfırdan oluştur.
        if (!yeniOlusturuldu && !TabloVarMi(db, "ScriptMaddeleri"))
        {
            // Dosya silinebilsin diye açık bağlantı ve havuzdaki tutamaçlar bırakılmalı.
            db.Database.CloseConnection();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        else if (!yeniOlusturuldu && !SutunVarMi(db, "ScriptMaddeleri", "SonKullanimTarihi"))
        {
            // Şema güncellemesi: mevcut veriyi koruyarak yeni sütunu ekle.
            db.Database.ExecuteSqlRaw(
                "ALTER TABLE \"ScriptMaddeleri\" ADD COLUMN \"SonKullanimTarihi\" TEXT NULL");
        }

        if (!db.Kategoriler.Any())
        {
            var karsilama = new Kategori { Ad = "Karşılama" };
            var sorun = new Kategori { Ad = "Sorun Çözme" };
            var bekletme = new Kategori { Ad = "Bekletme" };
            var kapanis = new Kategori { Ad = "Kapanış" };
            db.Kategoriler.AddRange(karsilama, sorun, bekletme, kapanis);

            var simdi = DateTime.Now;
            db.Scriptler.AddRange(
                YeniScript("Giriş", karsilama, "karşılama, selamlama", true, simdi,
                    "Merhabalar! Ben müşteri temsilcinizim. Size nasıl yardımcı olabilirim?",
                    "Tekrar merhaba! Görüşmemize kaldığımız yerden devam edebiliriz.",
                    "Hoş geldiniz! Talebinizi dinliyorum, size en kısa sürede yardımcı olacağım."),
                YeniScript("Sorun Çözme", sorun, "özür, empati, bilgi", true, simdi,
                    "Yaşadığınız sorun için üzgünüm. Konuyu hemen inceliyorum, en kısa sürede çözüme ulaştıracağım.",
                    "Talebinizi daha hızlı sonuçlandırabilmem için işlem numaranızı ve kayıtlı telefon numaranızı paylaşabilir misiniz?",
                    "Kontrollerimi tamamladım, sorunun kaynağını tespit ettim. Hemen düzeltiyorum."),
                YeniScript("Bekletme", bekletme, "bekletme, kontrol", true, simdi,
                    "Konuyu kontrol edebilmem için sizi kısa bir süre bekletebilir miyim? En geç 2-3 dakika içinde döneceğim.",
                    "Beklediğiniz için teşekkür ederim. Kontrollerimi tamamladım, hemen bilgi veriyorum."),
                YeniScript("Kapanış", kapanis, "kapanış, veda, anket", true, simdi,
                    "Yardımcı olabildiysem ne mutlu! Başka bir sorunuz yoksa görüşmeyi sonlandırıyorum. İyi günler dilerim.",
                    "Görüşme sonunda size iletilecek kısa anketi doldurursanız hizmet kalitemizi artırmamıza destek olursunuz. İyi günler!"));
        }

        db.SaveChanges();
    }

    private static Script YeniScript(
        string baslik, Kategori kategori, string etiketler, bool favori, DateTime tarih, params string[] mesajlar)
    {
        var script = new Script
        {
            Baslik = baslik,
            Kategori = kategori,
            Etiketler = etiketler,
            Favori = favori,
            OlusturmaTarihi = tarih,
            GuncellemeTarihi = tarih
        };

        for (var i = 0; i < mesajlar.Length; i++)
        {
            script.Maddeler.Add(new ScriptMaddesi { Metin = mesajlar[i], Sira = i });
        }

        return script;
    }

    private static bool SutunVarMi(AppDbContext db, string tabloAdi, string sutunAdi)
    {
        var baglanti = db.Database.GetDbConnection();
        db.Database.OpenConnection();
        try
        {
            using var komut = baglanti.CreateCommand();
            komut.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{tabloAdi}') WHERE name = @ad";
            var parametre = komut.CreateParameter();
            parametre.ParameterName = "@ad";
            parametre.Value = sutunAdi;
            komut.Parameters.Add(parametre);
            return Convert.ToInt32(komut.ExecuteScalar()) > 0;
        }
        finally
        {
            db.Database.CloseConnection();
        }
    }

    private static bool TabloVarMi(AppDbContext db, string tabloAdi)
    {
        var baglanti = db.Database.GetDbConnection();
        db.Database.OpenConnection();
        try
        {
            using var komut = baglanti.CreateCommand();
            komut.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @ad";
            var parametre = komut.CreateParameter();
            parametre.ParameterName = "@ad";
            parametre.Value = tabloAdi;
            komut.Parameters.Add(parametre);
            return Convert.ToInt32(komut.ExecuteScalar()) > 0;
        }
        finally
        {
            db.Database.CloseConnection();
        }
    }
}
