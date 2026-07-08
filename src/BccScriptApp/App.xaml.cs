using System.Windows;
using BccScriptApp.Data;
using BccScriptApp.ViewModels;

namespace BccScriptApp;

public partial class App : Application
{
    private AppDbContext? _db;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _db = new AppDbContext();
        DbSeeder.Seed(_db);

        var pencere = new MainWindow
        {
            DataContext = new MainViewModel(_db)
        };
        MainWindow = pencere;
        pencere.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _db?.Dispose();
        base.OnExit(e);
    }
}
