using System.Collections.ObjectModel;
using BccScriptApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BccScriptApp.ViewModels;

public partial class MaddeDuzenleme : ObservableObject
{
    [ObservableProperty]
    private string metin = string.Empty;
}

public partial class ScriptEditViewModel : ObservableObject
{
    public ObservableCollection<Kategori> Kategoriler { get; }
    public ObservableCollection<MaddeDuzenleme> Maddeler { get; } = [];

    [ObservableProperty]
    private string baslik = string.Empty;

    [ObservableProperty]
    private string etiketler = string.Empty;

    [ObservableProperty]
    private string kategoriAdi = string.Empty;

    public string PencereBasligi { get; }

    public ScriptEditViewModel(IEnumerable<Kategori> kategoriler, Script? mevcut)
    {
        Kategoriler = new ObservableCollection<Kategori>(kategoriler);
        PencereBasligi = mevcut is null ? "Yeni Script" : "Script Düzenle";

        if (mevcut is not null)
        {
            Baslik = mevcut.Baslik;
            Etiketler = mevcut.Etiketler;
            KategoriAdi = mevcut.Kategori?.Ad ?? string.Empty;

            foreach (var madde in mevcut.Maddeler.OrderBy(m => m.Sira))
            {
                Maddeler.Add(new MaddeDuzenleme { Metin = madde.Metin });
            }
        }

        if (Maddeler.Count == 0)
        {
            Maddeler.Add(new MaddeDuzenleme());
        }
    }

    [RelayCommand]
    private void MaddeEkle() => Maddeler.Add(new MaddeDuzenleme());

    [RelayCommand]
    private void MaddeSil(MaddeDuzenleme? madde)
    {
        if (madde is not null && Maddeler.Count > 1)
        {
            Maddeler.Remove(madde);
        }
    }

    public bool GecerliMi =>
        !string.IsNullOrWhiteSpace(Baslik)
        && Maddeler.Any(m => !string.IsNullOrWhiteSpace(m.Metin));
}
