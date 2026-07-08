using System.Windows;

namespace BccScriptApp.Views;

public partial class KategoriDuzenleWindow : Window
{
    public KategoriDuzenleWindow()
    {
        InitializeComponent();
    }

    private void Kaydet_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
