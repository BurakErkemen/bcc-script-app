using System.Collections.ObjectModel;
using BccScriptApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BccScriptApp.ViewModels;

public partial class KategoriSatiri : ObservableObject
{
    [ObservableProperty]
    private string ad = string.Empty;

    /// <summary>Var olan kategori; yeni eklenen satırlarda null.</summary>
    public Kategori? Mevcut { get; init; }
}

public partial class KategoriDuzenleViewModel : ObservableObject
{
    public ObservableCollection<KategoriSatiri> Satirlar { get; } = [];
    public List<Kategori> Silinecekler { get; } = [];

    public KategoriDuzenleViewModel(IEnumerable<Kategori> kategoriler)
    {
        foreach (var kategori in kategoriler)
        {
            Satirlar.Add(new KategoriSatiri { Ad = kategori.Ad, Mevcut = kategori });
        }
    }

    [RelayCommand]
    private void Ekle() => Satirlar.Add(new KategoriSatiri());

    [RelayCommand]
    private void Sil(KategoriSatiri? satir)
    {
        if (satir is null)
        {
            return;
        }

        if (satir.Mevcut is not null)
        {
            Silinecekler.Add(satir.Mevcut);
        }

        Satirlar.Remove(satir);
    }
}
