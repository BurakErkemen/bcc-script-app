using System.Windows;

namespace BccScriptApp.Views;

public partial class RehberWindow : Window
{
    public RehberWindow()
    {
        InitializeComponent();
    }

    private void Basla_Click(object sender, RoutedEventArgs e) => Close();
}
