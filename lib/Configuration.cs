using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Skatech.Configuration;

internal readonly struct ConfigWriter {
    private readonly StringBuilder _builder = new();

    public static ConfigWriter FromEnumerable(IEnumerable<KeyValuePair<string, string>> pairs) {
        var instance = new ConfigWriter();
        instance.AddRange(pairs);
        return instance;
    }

    public void Clear() {
        _builder.Clear();
    }
    
    public void AddRange(IEnumerable<KeyValuePair<string, string>> pairs) {
        foreach (var pair in pairs) {
            Add(pair.Key, pair.Value);
        }
    }

    public void Add(string key, string? value) {
        ValidateKey(key);
        ValidateValue(value);
        _builder.Append(key);
        _builder.Append('=');
        _builder.AppendLine(value);
    }
    
    public void WriteFile(string path) {
        File.WriteAllText(path, _builder.ToString(), Encoding.UTF8);
        Clear();
    }
    
    public static void ValidateKey(string key) {
        if (key.Length < 1)
            throw new FormatException("Config key must not be an empty string.");
        if (key.All(char.IsLetterOrDigit) is false)
            throw new FormatException("Config key must contain only letter and digit characters.");
    }
    
    public static void ValidateValue(string? value) {
        if (value == null || (value.Contains('\r') || value.Contains('\n')))
            throw new FormatException("Config value must not contain new line characters.");
    }
}

internal struct ConfigReader {
    private readonly string _data;
    private int _position;

    public static ConfigReader FromFile(string path) {
        return new ConfigReader(File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : "");
    }

    public ConfigReader(string data) {
        _position = 0;
        _data = data;
    }

    public Dictionary<string, string> AsDictionary(Dictionary<string, string>? result = default) {
        if (result is null) {
            result = new Dictionary<string, string>();
        }
        else result.Clear();
        
        while (Next(out var key, out var value)) {
            result[key.ToString()] = value.ToString();
        }
        return result;
    }

    public bool Next(out ReadOnlySpan<char> key, out ReadOnlySpan<char> value) {
        int org, sep;
        for (;; ++_position) {
            if (_position == _data.Length) {
                key = value = ReadOnlySpan<char>.Empty;
                return false;
            }
            if (_data[_position] is not '\r' and not '\n') {
                org = _position;
                break;
            }
        }

        for (;; ++_position) {
            if (_position == _data.Length) {
                throw new FormatException("Config record must contain separator character.");
            }
            var c = _data[_position];
            if (c is '=') {
                sep = _position;
                if (sep == org) {
                    throw new FormatException("Config key must not be an empty string.");
                }
                break;
            }
            if (!char.IsLetterOrDigit(c)) {
                throw new FormatException("Config key must contain only letter and digit characters.");
            }
        }

        for (;; ++_position) {
            if (_position < _data.Length && _data[_position] is not '\r' and not '\n') {
                continue;
            }
            key = _data.AsSpan(org, sep - org);
            value = _data.AsSpan(++sep , _position - sep);
            return true;
        }
    }
}
