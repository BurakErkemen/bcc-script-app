using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using BccScriptApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BccScriptApp.ViewModels;

public partial class ScriptListViewModel : ObservableObject
{
    public ObservableCollection<Script> Scriptler { get; } = [];
    public ObservableCollection<Kategori> Kategoriler { get; } = [];
    public ICollectionView ScriptGorunumu { get; }

    [ObservableProperty]
    private string aramaMetni = string.Empty;

    [ObservableProperty]
    private Kategori? seciliKategori;

    public ScriptListViewModel()
    {
        ScriptGorunumu = CollectionViewSource.GetDefaultView(Scriptler);
        ScriptGorunumu.Filter = FiltreUygula;
        ScriptGorunumu.SortDescriptions.Add(
            new SortDescription(nameof(Script.Baslik), ListSortDirection.Ascending));
    }

    partial void OnAramaMetniChanged(string value)
    {
        EslesmeleriGuncelle(value);
        ScriptGorunumu.Refresh();
    }

    /// <summary>Arama metniyle eşleşen mesajları her başlık için hesaplar;
    /// eşleşenler listede başlığın altında doğrudan gösterilir.</summary>
    private void EslesmeleriGuncelle(string arama)
    {
        arama = arama.Trim();
        foreach (var script in Scriptler)
        {
            script.EslesenMaddeler = arama.Length == 0
                ? []
                : script.Maddeler
                    .Where(m => m.Metin.Contains(arama, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
        }
    }
    partial void OnSeciliKategoriChanged(Kategori? value) => ScriptGorunumu.Refresh();

    private bool FiltreUygula(object obj)
    {
        if (obj is not Script script)
        {
            return false;
        }

        if (SeciliKategori is not null && script.KategoriId != SeciliKategori.Id)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(AramaMetni))
        {
            var arama = AramaMetni.Trim();
            return script.Baslik.Contains(arama, StringComparison.CurrentCultureIgnoreCase)
                || script.Etiketler.Contains(arama, StringComparison.CurrentCultureIgnoreCase)
                || script.Maddeler.Any(m => m.Metin.Contains(arama, StringComparison.CurrentCultureIgnoreCase));
        }

        return true;
    }
}
