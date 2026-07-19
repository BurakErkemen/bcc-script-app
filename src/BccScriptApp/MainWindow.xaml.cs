using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using BccScriptApp.Views;

namespace BccScriptApp;

public partial class MainWindow : Window
{
    private const double PanelGenislik = 340;
    private const double PanelEnAzGenislik = 280;
    private const double PanelEnAzYukseklik = 340;
    private const double BalonBoyut = 54;

    // Baloncuk, tarayıcının sağ üstteki kapat düğmesini örtmeyecek kadar aşağıda durur.
    private const double BalonUstBosluk = 110;

    private bool _genis = true;
    private Rect? _panelYer;
    private Point? _balonYer;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Yerlestir(genis: true);
        RehberGoster();
    }

    /// <summary>İlk açılışta bir kez "Nasıl kullanılır?" penceresini gösterir.</summary>
    private void RehberGoster()
    {
        var dizin = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BccScriptApp");
        Directory.CreateDirectory(dizin);
        var bayrak = Path.Combine(dizin, "rehber.ok");
        if (File.Exists(bayrak))
        {
            return;
        }

        new RehberWindow { Owner = this }.ShowDialog();
        File.WriteAllText(bayrak, "1");
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        if (PresentationSource.FromVisual(this) is HwndSource kaynak)
        {
            kaynak.AddHook(PencereMesaji);

            // Windows 11 Snap'i (kenara sürükleyince ekrana yayılma, Snap Layouts)
            // bu pencere için kapat: WS_MAXIMIZEBOX kaldırılır, boyutlandırma etkilenmez.
            var stil = GetWindowLong(kaynak.Handle, GWL_STYLE);
            SetWindowLong(kaynak.Handle, GWL_STYLE, stil & ~WS_MAXIMIZEBOX);
        }
    }

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x00010000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private void Yerlestir(bool genis)
    {
        var alan = SystemParameters.WorkArea;
        _genis = genis;

        if (genis)
        {
            _balonYer = new Point(Left, Top);
            PanelGorunum.Visibility = Visibility.Visible;
            BalonGorunum.Visibility = Visibility.Collapsed;
            MinWidth = PanelEnAzGenislik;
            MinHeight = PanelEnAzYukseklik;

            if (_panelYer is { } yer)
            {
                Left = yer.Left;
                Top = yer.Top;
                Width = yer.Width;
                Height = yer.Height;
            }
            else
            {
                Width = PanelGenislik;
                Height = Math.Min(720, alan.Height - BalonUstBosluk - 20);
                Left = alan.Right - Width - 10;
                Top = BalonUstBosluk;
            }
        }
        else
        {
            _panelYer = new Rect(Left, Top, Width, Height);
            PanelGorunum.Visibility = Visibility.Collapsed;
            BalonGorunum.Visibility = Visibility.Visible;
            MinWidth = 0;
            MinHeight = 0;
            Width = BalonBoyut;
            Height = BalonBoyut;

            if (_balonYer is { } konum)
            {
                Left = konum.X;
                Top = konum.Y;
            }
            else
            {
                Left = alan.Right - BalonBoyut - 8;
                Top = BalonUstBosluk;
            }
        }
    }

    private void Kucult_Click(object sender, RoutedEventArgs e) => Yerlestir(genis: false);

    private void Balon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Sürükleme bitene kadar DragMove bloklar; yer değişmediyse tıklama kabul edilir.
        var once = new Point(Left, Top);
        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // Fare düğmesi bu arada bırakıldıysa yok say.
        }

        var fark = new Point(Left, Top) - (Vector)once;
        if (Math.Abs(fark.X) < 3 && Math.Abs(fark.Y) < 3)
        {
            Yerlestir(genis: true);
        }
        else
        {
            _balonYer = new Point(Left, Top);
        }
    }

    private void Kapat_Click(object sender, RoutedEventArgs e) => Close();

    private void Bfe_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.DuzenlemeModuDegistirCommand.Execute(null);
        }
    }

    private void Baslik_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    #region Kenardan boyutlandırma (çerçevesiz pencere)

    private const int WM_NCHITTEST = 0x0084;
    private const int KenarPayi = 8;

    private const int HTCLIENT = 1;
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    private IntPtr PencereMesaji(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_NCHITTEST || !_genis)
        {
            return IntPtr.Zero;
        }

        if (!GetWindowRect(hwnd, out var r))
        {
            return IntPtr.Zero;
        }

        // lParam ekran koordinatlarında imleç konumunu taşır (düşük söz: x, yüksek söz: y).
        int x = unchecked((short)(lParam.ToInt64() & 0xFFFF));
        int y = unchecked((short)((lParam.ToInt64() >> 16) & 0xFFFF));

        bool sol = x >= r.Left && x < r.Left + KenarPayi;
        bool sag = x <= r.Right && x > r.Right - KenarPayi;
        bool ust = y >= r.Top && y < r.Top + KenarPayi;
        bool alt = y <= r.Bottom && y > r.Bottom - KenarPayi;

        int sonuc = HTCLIENT;
        if (ust && sol) sonuc = HTTOPLEFT;
        else if (ust && sag) sonuc = HTTOPRIGHT;
        else if (alt && sol) sonuc = HTBOTTOMLEFT;
        else if (alt && sag) sonuc = HTBOTTOMRIGHT;
        else if (sol) sonuc = HTLEFT;
        else if (sag) sonuc = HTRIGHT;
        else if (ust) sonuc = HTTOP;
        else if (alt) sonuc = HTBOTTOM;

        if (sonuc != HTCLIENT)
        {
            handled = true;
            return new IntPtr(sonuc);
        }

        return IntPtr.Zero;
    }

    #endregion
}
