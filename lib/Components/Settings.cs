using System;
using System.Globalization;
using System.Collections.Generic;
using Skatech.Configuration;
using Skatech.Extensions.Runtime;

namespace Skatech.Components.Settings;

interface ISettings {
    string? Get(string name, string? defaultValue = default);
    string? Set(string name, string? value);
}

static class SettingsExtensions {
    public static string GetString(this ISettings settings,
            string name, string? defaultValue = default, bool propagateDefaultValue = false) {
        return settings.Get(name) 
            ?? (propagateDefaultValue ? settings.Set(name, defaultValue) : defaultValue)
            ?? throw new InvalidOperationException($"Setting not found: \"{name}\"."); 
    }

    public static bool GetBoolean(this ISettings settings,
            string name, bool defaultValue = false, bool propagateDefaultValue = false) {
        var text = settings.Get(name);
        if (text != null) {
            return bool.Parse(text);
        }
        if (propagateDefaultValue) {
            settings.Set(name, defaultValue.ToString(CultureInfo.InvariantCulture));
        }
        return defaultValue;
    }
    
    public static int GetInteger(this ISettings settings,
            string name, int defaultValue = 0, bool propagateDefaultValue = false) {
        var text = settings.Get(name);
        if (text != null) {
            return int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        if (propagateDefaultValue) {
            settings.Set(name, defaultValue.ToString(CultureInfo.InvariantCulture));
        }
        return defaultValue;
    }

    public static double GetDouble(this ISettings settings,
            string name, double defaultValue = 0, bool propagateDefaultValue = false) {
        var text = settings.Get(name);
        if (text != null) {
            return double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        if (propagateDefaultValue) {
            settings.Set(name, defaultValue.ToString(CultureInfo.InvariantCulture));
        }
        return defaultValue;
    }
    
    public static TEnum GetEnumeration<TEnum>(this ISettings settings, string name,
            TEnum defaultValue = default, bool propagateDefaultValue = false) where TEnum : struct {
        var text = settings.Get(name);
        if (text != null) {
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }
        if (propagateDefaultValue) {
            settings.Set(name, defaultValue.ToString());
        }
        return defaultValue;
    }

    public static string[] GetStrings(this ISettings settings, string name,
            string separator, string[] defaultValue, bool propagateDefaultValue = false) {
        var value = settings.Get(name);
        if (value != null) {
            return value.Split(separator);
        }
        if (propagateDefaultValue) {
            settings.Set(name, separator, defaultValue);
        }
        return defaultValue;
    }
    
    public static void Set(this ISettings settings, string name, bool value) {
        settings.Set(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public static void Set(this ISettings settings, string name, int value) {
        settings.Set(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public static void Set(this ISettings settings, string name, double value) {
        settings.Set(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public static void Set(this ISettings settings, string name, Enum value) {
        settings.Set(name, value.ToString());
    }
    
    public static void Set(this ISettings settings, string name, string separator, IEnumerable<string> values) {
        settings.Set(name, String.Join(separator, values));
    }
}

class SettingsService : ISettings, IDisposable {
    private readonly Dictionary<string, string> _values = new();

    public bool Modified { get; private set; }
    public string Path { get; }

    public SettingsService() : this(
        System.Reflection.Assembly.GetExecutingAssembly().CreateAppDataPath("Settings.ini")) {
    }

    public SettingsService(string path) {
        Path = path;
        Load();
    }

    ~SettingsService() {
        Dispose();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        if (Modified) {
            Save();
        }
    }

    public string? Get(string name, string? defaultValue = default) {
        return (_values.TryGetValue(name, out string? value))
            ? value
            : defaultValue;
    }

    public string? Set(string name, string? value) {
        ConfigWriter.ValidateKey(name);
        ConfigWriter.ValidateValue(value);
        if (value == null) {
            _values.Remove(name);
            return value;
        }
        var previous = Get(name);
        if (value == previous) {
            return previous;
        }
        _values[name] = value;
        Modified = true;
        return value;
    }

    public void Load() {
        ConfigReader.FromFile(Path).AsDictionary(_values);
        Modified = false;
    }

    public void Save() {
        ConfigWriter.FromEnumerable(_values).WriteFile(Path);
        Modified = false;
    }
}
