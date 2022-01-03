using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Skatech.ImageViewer;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        Components.Presentation.WindowBoundsKeeper.Register(this, "WindowBounds");
    }

    private void SwitchFullScreen() {
        if (WindowState == WindowState.Maximized) {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            Background = Brushes.AliceBlue;
        }
        else {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Background = Brushes.Black;
        }
    }

    private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e) {
        if (sender is ScrollViewer scw) {
            var ctr = (MainWindowController) DataContext;
            ctr.SetDisplay(scw.ActualWidth, scw.ActualHeight);
            scw.ScrollToHorizontalOffset(scw.ScrollableWidth / 2);
            scw.ScrollToVerticalOffset(scw.ScrollableHeight / 2);
        }
    }
    
    private Point? _origin;
    private void ScrollViewer_OnPreviewMouseLeftButtonUpOrDown(object sender, MouseButtonEventArgs e) {
        if (e.ButtonState == MouseButtonState.Pressed) {
            var ctr = (MainWindowController) DataContext;
            _origin = e.GetPosition((ScrollViewer)sender) - new Vector(
                ctr.TranslateX * ctr.Scale, ctr.TranslateY * ctr.Scale);
        }
        else _origin = null;
    }

    private void ScrollViewer_OnMouseMove(object sender, MouseEventArgs e) {
        if (_origin.HasValue && e.LeftButton == MouseButtonState.Pressed) {
            var ctr = (MainWindowController) DataContext;
            var vec = e.GetPosition((ScrollViewer)sender) - _origin.Value;
            ctr.SetTranslate(vec.X / ctr.Scale, vec.Y / ctr.Scale);
        }  
    }

    private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        var ctr = (MainWindowController) DataContext;
        var val = 4.0 * Math.Pow(Math.Sqrt(ctr.Scale / 4.0) + e.Delta / 5000.0, 2);
        ctr.SetScale(Math.Clamp(val, 0.1, 10.0));
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e) {
        var ctr = (MainWindowController) DataContext;
        switch (e.Key) {
            case Key.Left:
                ctr.Shift(-1);
                break;
            case Key.Right:
                ctr.Shift(1);
                break;
            case Key.F:
                SwitchFullScreen();
                break;
            case Key.Escape:
                if (WindowState == WindowState.Maximized) {
                    SwitchFullScreen();
                } else Close();
                break;
        }
    }

    private void ScrollViewer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        _origin = null;
        SwitchFullScreen();
    }
}
