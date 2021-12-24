using System;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using Skatech.Components.Settings;

namespace Skatech.Components.Presentation;

static class WindowBoundsKeeper {
    const string DefaultStateValue = "Default";
    static readonly Dictionary<Window, string> Names = new();
    
    public static void Register(Window window, string name) {
        Names.Add(window, name);
        window.Closing += Window_Closing;
        LoadWindowState(window);
    }
    
    public static bool Remove(Window window) {
        if (Names.Remove(window)) { 
            window.Closing -= Window_Closing;
            return true;
        }
        return false;
    }
    
    static void LoadWindowState(Window window) {
        var state = ServiceLocator.Resolve<ISettings>().GetString(Names[window], DefaultStateValue);
        if (!state.Equals(DefaultStateValue, StringComparison.OrdinalIgnoreCase)) {
            var bounds = Rect.Parse(state);
            window.BeginInit();
            window.WindowState = WindowState.Normal;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = bounds.Left;
            window.Top = bounds.Top;
            window.Width = bounds.Width;
            window.Height = bounds.Height;
            window.EndInit();
        }
    }
    
    static void SaveWindowState(Window window) {
        var bounds = window.WindowState == WindowState.Normal
            ? new Rect(window.Left, window.Top, window.Width, window.Height)
            : window.RestoreBounds;
        ServiceLocator.Resolve<ISettings>().Set(Names[window],
            bounds.ToString(CultureInfo.InvariantCulture));
        Remove(window);
    }
    
    static void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
        if (!e.Cancel && sender is Window window) {
            SaveWindowState(window);
        }
    }
}
