using System.Windows;
using BccScriptApp.ViewModels;

namespace BccScriptApp.Views;

public partial class ScriptEditWindow : Window
{
    public ScriptEditWindow()
    {
        InitializeComponent();
    }

    private void Kaydet_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ScriptEditViewModel vm && !vm.GecerliMi)
        {
            MessageBox.Show(
                "Başlık boş bırakılamaz ve en az bir mesaj girilmelidir.",
                "Eksik Bilgi",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }
}
