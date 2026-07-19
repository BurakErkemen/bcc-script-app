using System.IO;
using System.Windows;
using System.Windows.Threading;
using BccScriptApp.Data;
using BccScriptApp.Models;
using BccScriptApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace BccScriptApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _db;
    private readonly DispatcherTimer _durumZamanlayici;
    private readonly DispatcherTimer _bildirimZamanlayici;
    private ScriptMaddesi? _sonKopyalanan;

    public ScriptListViewModel Liste { get; }

    [ObservableProperty]
    private string durumMesaji = "İpucu: Başlığa tıklayın, mesajlar sola açılır; mesaja tıklamak kopyalar.";

    [ObservableProperty]
    private bool kopyalandiBildirimi;

    /// <summary>Ekleme/çıkarma kontrollerinin görünürlüğü; BFE rozetine tıklayarak açılır.</summary>
    [ObservableProperty]
    private bool duzenlemeModu;

    public MainViewModel(AppDbContext db)
    {
        _db = db;
        Liste = new ScriptListViewModel();

        _durumZamanlayici = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _durumZamanlayici.Tick += (_, _) =>
        {
            _durumZamanlayici.Stop();
            DurumMesaji = "Hazır";
        };

        _bildirimZamanlayici = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        _bildirimZamanlayici.Tick += (_, _) =>
        {
            _bildirimZamanlayici.Stop();
            KopyalandiBildirimi = false;
            if (_sonKopyalanan is not null)
            {
                _sonKopyalanan.Kopyalandi = false;
                _sonKopyalanan = null;
            }
        };

        VerileriYukle();
    }

    private void VerileriYukle()
    {
        Liste.Kategoriler.Clear();
        foreach (var kategori in _db.Kategoriler.OrderBy(k => k.Ad).ToList())
        {
            Liste.Kategoriler.Add(kategori);
        }

        Liste.Scriptler.Clear();
        foreach (var script in _db.Scriptler
                     .Include(s => s.Kategori)
                     .Include(s => s.Maddeler.OrderBy(m => m.Sira))
                     .ToList())
        {
            Liste.Scriptler.Add(script);
        }
    }

    [RelayCommand]
    private void MenuAcKapa(Script? script)
    {
        if (script is null)
        {
            return;
        }

        var yeniDurum = !script.Acik;
        MenuleriKapat();
        script.Acik = yeniDurum;
    }

    private void MenuleriKapat()
    {
        foreach (var script in Liste.Scriptler)
        {
            script.Acik = false;
        }
    }

    [RelayCommand]
    private void Kopyala(ScriptMaddesi? madde)
    {
        if (madde is null)
        {
            return;
        }

        try
        {
            Clipboard.SetDataObject(madde.Metin, true);
        }
        catch
        {
            // Pano başka bir uygulama tarafından kilitliyse tek seferlik yeniden dene.
            try
            {
                Clipboard.SetDataObject(madde.Metin, true);
            }
            catch
            {
                DurumBildir("Pano meşgul, tekrar deneyin");
                return;
            }
        }

        MenuleriKapat();

        // Belirgin geri bildirim: tıklanan kutu yeşil yanar, panelde toast görünür.
        if (_sonKopyalanan is not null)
        {
            _sonKopyalanan.Kopyalandi = false;
        }

        _sonKopyalanan = madde;
        madde.Kopyalandi = true;
        KopyalandiBildirimi = true;
        _bildirimZamanlayici.Stop();
        _bildirimZamanlayici.Start();

        DurumBildir("Panoya kopyalandı ✓");
    }

    [RelayCommand]
    private void YeniScript()
    {
        var vm = new ScriptEditViewModel(Liste.Kategoriler, null);
        if (DuzenlePenceresiAc(vm) != true)
        {
            return;
        }

        var simdi = DateTime.Now;
        var script = new Script
        {
            Baslik = vm.Baslik.Trim(),
            Etiketler = vm.Etiketler.Trim(),
            Kategori = KategoriBulVeyaOlustur(vm.KategoriAdi),
            OlusturmaTarihi = simdi,
            GuncellemeTarihi = simdi
        };
        MaddeleriAktar(vm, script);

        _db.Scriptler.Add(script);
        _db.SaveChanges();
        Liste.Scriptler.Add(script);
        DurumBildir($"\"{script.Baslik}\" eklendi");
    }

    [RelayCommand]
    private void ScriptDuzenle(Script? script)
    {
        if (script is null)
        {
            return;
        }

        var vm = new ScriptEditViewModel(Liste.Kategoriler, script);
        if (DuzenlePenceresiAc(vm) != true)
        {
            return;
        }

        script.Baslik = vm.Baslik.Trim();
        script.Etiketler = vm.Etiketler.Trim();
        script.Kategori = KategoriBulVeyaOlustur(vm.KategoriAdi);
        script.KategoriId = script.Kategori?.Id;
        script.GuncellemeTarihi = DateTime.Now;

        // Maddeleri baştan kur: eskiler öksüz kalınca EF tarafından silinir.
        script.Maddeler.Clear();
        MaddeleriAktar(vm, script);

        _db.SaveChanges();
        Liste.ScriptGorunumu.Refresh();
        DurumBildir($"\"{script.Baslik}\" güncellendi");
    }

    [RelayCommand]
    private void ScriptSil(Script? script)
    {
        if (script is null)
        {
            return;
        }

        var onay = MessageBox.Show(
            $"\"{script.Baslik}\" başlığı ve altındaki {script.Maddeler.Count} mesaj silinsin mi?",
            "Script Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (onay != MessageBoxResult.Yes)
        {
            return;
        }

        _db.Scriptler.Remove(script);
        _db.SaveChanges();
        Liste.Scriptler.Remove(script);
        DurumBildir($"\"{script.Baslik}\" silindi");
    }

    [RelayCommand]
    private void DisaAktar()
    {
        var dlg = new SaveFileDialog
        {
            FileName = "bcc-scriptler.db",
            Filter = "SQLite Veritabanı (*.db)|*.db|Tüm Dosyalar (*.*)|*.*",
            Title = "Scriptleri Dışa Aktar"
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        try
        {
            // VACUUM INTO hedef dosya varsa hata verir; önce temizle.
            if (File.Exists(dlg.FileName))
            {
                File.Delete(dlg.FileName);
            }

            // VACUUM INTO parametre almaz; tek tırnaklar kaçırılarak yol gömülür.
            var yol = dlg.FileName.Replace("'", "''");
#pragma warning disable EF1002
            _db.Database.ExecuteSqlRaw($"VACUUM INTO '{yol}'");
#pragma warning restore EF1002
            DurumBildir($"Scriptler dışa aktarıldı: {Path.GetFileName(dlg.FileName)} ✓");
        }
        catch (Exception hata)
        {
            MessageBox.Show($"Dışa aktarma başarısız oldu:\n{hata.Message}",
                "Dışa Aktar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void IceAktar()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "SQLite Veritabanı (*.db;*.sqlite)|*.db;*.sqlite|Tüm Dosyalar (*.*)|*.*",
            Title = "Scriptleri İçe Aktar"
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        var secim = MessageBox.Show(
            "Dosyadaki scriptler mevcut scriptlere EKLENSİN mi?\n\n" +
            "Evet — Mevcutlar korunur, dosyadakiler eklenir (aynı başlıklar atlanır).\n" +
            "Hayır — Mevcut TÜM scriptler silinir, yalnızca dosyadakiler kalır.",
            "İçe Aktar",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (secim == MessageBoxResult.Cancel)
        {
            return;
        }

        try
        {
            List<Script> gelenler;
            using (var kaynak = new AppDbContext(dlg.FileName))
            {
                gelenler = kaynak.Scriptler
                    .Include(s => s.Kategori)
                    .Include(s => s.Maddeler.OrderBy(m => m.Sira))
                    .ToList();
            }

            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); // dosya kilitli kalmasın

            if (secim == MessageBoxResult.No)
            {
                Liste.SeciliKategori = null;
                _db.Scriptler.RemoveRange(_db.Scriptler);
                _db.Kategoriler.RemoveRange(_db.Kategoriler);
                _db.SaveChanges();
            }

            // Kategori eşlemesi: ada göre bul, yoksa oluştur.
            var kategoriler = _db.Kategoriler.ToList()
                .ToDictionary(k => k.Ad.Trim(), k => k, StringComparer.CurrentCultureIgnoreCase);
            var mevcutBasliklar = new HashSet<string>(
                _db.Scriptler.Select(s => s.Baslik).ToList(), StringComparer.CurrentCultureIgnoreCase);

            var eklenen = 0;
            var atlanan = 0;
            var simdi = DateTime.Now;

            foreach (var gelen in gelenler)
            {
                if (!mevcutBasliklar.Add(gelen.Baslik.Trim()))
                {
                    atlanan++;
                    continue;
                }

                Kategori? kategori = null;
                var kategoriAdi = gelen.Kategori?.Ad.Trim();
                if (!string.IsNullOrEmpty(kategoriAdi) && !kategoriler.TryGetValue(kategoriAdi, out kategori))
                {
                    kategori = new Kategori { Ad = kategoriAdi };
                    kategoriler[kategoriAdi] = kategori;
                }

                var yeni = new Script
                {
                    Baslik = gelen.Baslik.Trim(),
                    Etiketler = gelen.Etiketler,
                    Kategori = kategori,
                    OlusturmaTarihi = simdi,
                    GuncellemeTarihi = simdi
                };
                foreach (var madde in gelen.Maddeler)
                {
                    yeni.Maddeler.Add(new ScriptMaddesi { Metin = madde.Metin, Sira = madde.Sira });
                }

                _db.Scriptler.Add(yeni);
                eklenen++;
            }

            _db.SaveChanges();
            VerileriYukle();

            var mesaj = $"{eklenen} script içe aktarıldı";
            if (atlanan > 0)
            {
                mesaj += $", {atlanan} aynı başlık atlandı";
            }

            DurumBildir($"{mesaj} ✓");
        }
        catch (Exception hata)
        {
            MessageBox.Show(
                $"İçe aktarma başarısız oldu. Dosya geçerli bir script veritabanı olmayabilir.\n\nAyrıntı: {hata.Message}",
                "İçe Aktar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DuzenlemeModuDegistir()
    {
        DuzenlemeModu = !DuzenlemeModu;
        DurumBildir(DuzenlemeModu
            ? "Düzenleme modu açıldı: ekleme/çıkarma kontrolleri görünür"
            : "Düzenleme modu kapatıldı");
    }

    [RelayCommand]
    private void TumKategoriler() => Liste.SeciliKategori = null;

    [RelayCommand]
    private void KategorileriDuzenle()
    {
        var vm = new KategoriDuzenleViewModel(Liste.Kategoriler);
        var pencere = new KategoriDuzenleWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };

        if (pencere.ShowDialog() != true)
        {
            return;
        }

        // Silinenler: bağlı scriptler "Kategorisiz" kalır.
        foreach (var kategori in vm.Silinecekler)
        {
            foreach (var script in Liste.Scriptler.Where(s => s.KategoriId == kategori.Id))
            {
                script.Kategori = null;
                script.KategoriId = null;
            }

            if (Liste.SeciliKategori == kategori)
            {
                Liste.SeciliKategori = null;
            }

            _db.Kategoriler.Remove(kategori);
        }

        // Ad değişiklikleri ve yeni kategoriler (boş ve yinelenen adlar atlanır).
        var kullanilan = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var satir in vm.Satirlar)
        {
            var ad = satir.Ad.Trim();
            if (ad.Length == 0 || !kullanilan.Add(ad))
            {
                continue;
            }

            if (satir.Mevcut is { } kategori)
            {
                if (!string.Equals(kategori.Ad, ad, StringComparison.CurrentCulture))
                {
                    kategori.Ad = ad;
                    foreach (var script in Liste.Scriptler.Where(s => s.KategoriId == kategori.Id))
                    {
                        script.KategoriYenile();
                    }
                }
            }
            else
            {
                _db.Kategoriler.Add(new Kategori { Ad = ad });
            }
        }

        _db.SaveChanges();

        // Çip listesini sıralı olarak yeniden kur; seçimi mümkünse koru.
        var secili = Liste.SeciliKategori;
        Liste.Kategoriler.Clear();
        foreach (var kategori in _db.Kategoriler.OrderBy(k => k.Ad).ToList())
        {
            Liste.Kategoriler.Add(kategori);
        }

        Liste.SeciliKategori = secili is not null && Liste.Kategoriler.Contains(secili) ? secili : null;
        Liste.ScriptGorunumu.Refresh();
        DurumBildir("Kategoriler güncellendi");
    }

    [RelayCommand]
    private void FiltreleriTemizle()
    {
        Liste.SeciliKategori = null;
        Liste.AramaMetni = string.Empty;
    }

    private static void MaddeleriAktar(ScriptEditViewModel vm, Script script)
    {
        var sira = 0;
        foreach (var madde in vm.Maddeler.Where(m => !string.IsNullOrWhiteSpace(m.Metin)))
        {
            script.Maddeler.Add(new ScriptMaddesi { Metin = madde.Metin.Trim(), Sira = sira++ });
        }
    }

    private Kategori? KategoriBulVeyaOlustur(string ad)
    {
        ad = ad.Trim();
        if (ad.Length == 0)
        {
            return null;
        }

        var mevcut = Liste.Kategoriler.FirstOrDefault(
            k => string.Equals(k.Ad, ad, StringComparison.CurrentCultureIgnoreCase));
        if (mevcut is not null)
        {
            return mevcut;
        }

        var yeni = new Kategori { Ad = ad };
        _db.Kategoriler.Add(yeni);
        _db.SaveChanges();

        var eklenecekIndeks = 0;
        while (eklenecekIndeks < Liste.Kategoriler.Count
               && string.Compare(Liste.Kategoriler[eklenecekIndeks].Ad, ad, StringComparison.CurrentCulture) < 0)
        {
            eklenecekIndeks++;
        }

        Liste.Kategoriler.Insert(eklenecekIndeks, yeni);
        return yeni;
    }

    private static bool? DuzenlePenceresiAc(ScriptEditViewModel vm)
    {
        var pencere = new ScriptEditWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };
        return pencere.ShowDialog();
    }

    private void DurumBildir(string mesaj)
    {
        DurumMesaji = mesaj;
        _durumZamanlayici.Stop();
        _durumZamanlayici.Start();
    }
}
