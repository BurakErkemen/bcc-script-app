using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BccScriptApp.Models;

public class ScriptMaddesi : ObservableObject
{
    private string _metin = string.Empty;
    private bool _kopyalandi;

    public int Id { get; set; }
    public int ScriptId { get; set; }
    public int Sira { get; set; }

    public string Metin
    {
        get => _metin;
        set => SetProperty(ref _metin, value);
    }

    /// <summary>Kopyalama sonrası kısa süreli yeşil vurgu; veritabanına yazılmaz.</summary>
    [NotMapped]
    public bool Kopyalandi
    {
        get => _kopyalandi;
        set => SetProperty(ref _kopyalandi, value);
    }
}
