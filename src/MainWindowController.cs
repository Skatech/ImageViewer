using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using Path = System.IO.Path;

namespace Skatech.ImageViewer;

class MainWindowController : INotifyPropertyChanged{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ImageRecord[] _images;
    private double _width, _height;
    private int _index;

    public double TranslateX { get; private set; }
    public double TranslateY { get; private set; }
    public double Scale  { get; private set; } = 1.0;

    public BitmapImage? Image =>
        (_index < _images.Length) ? _images[_index].Image : null;
    
    public string? ImageFileName => (_index < _images.Length)
            ? Path.GetFileNameWithoutExtension(_images[_index].File) : null;
    
    public MainWindowController() {
        var argument = Environment.GetCommandLineArgs().ElementAtOrDefault(1);
        string? filename = default;
        string directory = (File.Exists(argument))
            ? Path.GetDirectoryName(filename = argument)!
            : Directory.Exists(argument) ? argument : Environment.CurrentDirectory;
        
        _images = Directory.EnumerateFiles(directory, "*.jpg")
            .Select(s => new ImageRecord { File = s }).ToArray();

        Shift(filename == null ? 0 : Math.Max(0, Array.FindIndex(
            _images, r => filename.Equals(r.File,StringComparison.OrdinalIgnoreCase))));
    }

    public void SetDisplay(double width, double height) {
        if (Double.Epsilon < Math.Max(Math.Abs(width - _width), Math.Abs(height - _height))) { 
            _width = width; _height = height;
            OptimizeScale();
        }
    }
    
    public void Shift(int offset) {
        if (_images.Length > 0) {
            int index = (_index + _images.Length + offset % _images.Length) % _images.Length;
            if (_index != index || _images[index].Image == null) {
                _images[index].Image ??= LoadImage(_images[index].File);
                _index = index;
                OnPropertyChanged(nameof(Image));
                SetTranslate(0.0, 0.0);
                OptimizeScale();
                OnPropertyChanged(nameof(ImageFileName));
            }
        }
    }

    public void SetTranslate(double x, double y) {
        if (Math.Abs(x - TranslateX) > 0.01) {
            TranslateX = x;
            OnPropertyChanged(nameof(TranslateX));
        }
        if (Math.Abs(y - TranslateY) > 0.01) {
            TranslateY = y;
            OnPropertyChanged(nameof(TranslateY));
        }
    }

    public void SetScale(double scale) {
        if (Math.Abs(scale - Scale) > Double.Epsilon) {
            Scale = scale;
            OnPropertyChanged(nameof(Scale));
        }
    }
    
    void OptimizeScale() {
        var image = Image;
        if (image != null) {
            double scalex = _width / image.Width; 
            double scaley = _height / image.Height;
            double scale = Math.Min(1, Math.Min(scalex, scaley));
            SetScale(scale);
        }
    }
    
    void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    static BitmapImage LoadImage(string path) {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bitmap.UriSource = new("file:///" + System.IO.Path.GetFullPath(path));
        bitmap.EndInit();
        return bitmap;
    }

    class ImageRecord {
        public string File { get; init; } = String.Empty;
        public BitmapImage? Image { get; set; }
    }
}
