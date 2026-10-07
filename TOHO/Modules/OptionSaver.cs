using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TOHO;

public static class OptionSaver
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };
    
    public static string Folder = Path.Combine(Main.TohoData, "Options");

    public static Dictionary<int, bool> BoolValues = [];
    public static Dictionary<int, bool> RoleValues = [];
    public static Dictionary<int, float> NumberValues = [];
    public static Dictionary<int, int> StringValues = [];

    public static void Save()
    {
        File.WriteAllText(Path.Combine(Folder, "bools.json"), JsonSerializer.Serialize(BoolValues, Options));
        File.WriteAllText(Path.Combine(Folder, "roles.json"), JsonSerializer.Serialize(RoleValues, Options));
        File.WriteAllText(Path.Combine(Folder, "floats.json"), JsonSerializer.Serialize(NumberValues, Options));
        File.WriteAllText(Path.Combine(Folder, "strings.json"), JsonSerializer.Serialize(StringValues, Options));
    }

    public static void Load()
    {
        Directory.CreateDirectory(Folder);

        BoolValues = LoadFile<Dictionary<int, bool>>(Path.Combine(Folder, "bools.json"));
        RoleValues = LoadFile<Dictionary<int, bool>>(Path.Combine(Folder, "roles.json"));
        NumberValues = LoadFile<Dictionary<int, float>>(Path.Combine(Folder, "floats.json"));
        StringValues = LoadFile<Dictionary<int, int>>(Path.Combine(Folder, "strings.json"));
    }

    private static T LoadFile<T>(string fileName) where T : new()
    {
        string path = Path.Combine(Folder, fileName);

        if (!File.Exists(path)) return new T();

        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? new T();
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    public static void SetInitialBooleanValue(this BooleanOptionItem option)
    {
        if (option == null) return;
        if (BoolValues.ContainsKey(option.Id)) option.SetValue(BoolValues[option.Id]);
        else option.SetValue(option.DefaultValue);
    }
    public static void SetInitialRoleValue(this RoleOptionItem option)
    {
        if (option == null) return;
        if (RoleValues.ContainsKey(option.Id)) option.SetValue(RoleValues[option.Id]);
        else option.SetValue(option.DefaultValue);
    }
    
    public static void SetInitialNumberValue(this NumberOptionItem option)
    {
        if (option == null) return;
        if (NumberValues.ContainsKey(option.Id)) option.SetValue(NumberValues[option.Id]);
        else option.SetValue(option.DefaultValue);
    }
    public static void SetInitialStringValue(this StringOptionItem option)
    {
        if (option == null) return;
        if (StringValues.ContainsKey(option.Id)) option.SetValue(StringValues[option.Id]);
        else option.SetValue(0);
    }
}