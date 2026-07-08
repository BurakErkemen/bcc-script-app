using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BccScriptApp.Models;

public class Script : ObservableObject
{
    private string _baslik = string.Empty;
    private string _etiketler = string.Empty;
    private bool _acik;
    private int? _kategoriId;
    private Kategori? _kategori;
    private DateTime _guncellemeTarihi;

    public int Id { get; set; }

    public string Baslik
    {
        get => _baslik;
        set => SetProperty(ref _baslik, value);
    }

    public string Etiketler
    {
        get => _etiketler;
        set => SetProperty(ref _etiketler, value);
    }

    public int? KategoriId
    {
        get => _kategoriId;
        set => SetProperty(ref _kategoriId, value);
    }

    public Kategori? Kategori
    {
        get => _kategori;
        set
        {
            if (SetProperty(ref _kategori, value))
            {
                OnPropertyChanged(nameof(KategoriAdi));
            }
        }
    }

    public ObservableCollection<ScriptMaddesi> Maddeler { get; set; } = [];

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime GuncellemeTarihi
    {
        get => _guncellemeTarihi;
        set => SetProperty(ref _guncellemeTarihi, value);
    }

    public string KategoriAdi => Kategori?.Ad ?? "Kategorisiz";

    /// <summary>Kategori adı dışarıdan değiştiğinde görünen adı tazeler.</summary>
    public void KategoriYenile() => OnPropertyChanged(nameof(KategoriAdi));

    /// <summary>Mesaj menüsünün açık olup olmadığı; veritabanına yazılmaz.</summary>
    [NotMapped]
    public bool Acik
    {
        get => _acik;
        set => SetProperty(ref _acik, value);
    }

    /// <summary>Arama metniyle eşleşen mesajlar; listede başlık altında doğrudan gösterilir.</summary>
    [NotMapped]
    public IReadOnlyList<ScriptMaddesi> EslesenMaddeler
    {
        get => _eslesenMaddeler;
        set => SetProperty(ref _eslesenMaddeler, value);
    }

    private IReadOnlyList<ScriptMaddesi> _eslesenMaddeler = [];
}
