using CommunityToolkit.Mvvm.ComponentModel;

namespace BccScriptApp.Models;

public class Kategori : ObservableObject
{
    private string _ad = string.Empty;

    public int Id { get; set; }

    public string Ad
    {
        get => _ad;
        set => SetProperty(ref _ad, value);
    }

    public List<Script> Scriptler { get; set; } = [];
}
