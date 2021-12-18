﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using Skatech.Components;
using Skatech.Components.Settings;
using Skatech.Extensions.Runtime;

namespace Skatech.ImageViewer;

public partial class App : Application {
    public App() {
        Startup += OnStartup;
        Exit += OnExit;
    }
    
    private void OnStartup(object sender, StartupEventArgs e) {
        Directory.CreateDirectory(Assembly.GetExecutingAssembly().CreateAppDataPath());
        ServiceLocator.Register<ISettings>(typeof(SettingsService));
    }
    
    private void OnExit(object sender, ExitEventArgs e) {
        if (ServiceLocator.Resolve<ISettings>() is IDisposable settings) {
            settings.Dispose();
        }
    }
}
