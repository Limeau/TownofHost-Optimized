using System;
using System.Collections.Generic;
using TMPro;
using TOHO;
using UnityEngine;
using Object = UnityEngine.Object;

public class BooleanOptionItem
{
    private static readonly Dictionary<ToggleOption, BooleanOptionItem> OptionMap = new();

    public int Id;
    
    public OptionTabs OptionTab;
    public string OptionName;

    public GameObject obj;
    public ToggleOption Toggle;

    public bool Value;
    public Action<object> OnValueChanged;
    public bool DefaultValue;
    
    public BooleanOptionItem(int id, string name, OptionTabs tab, bool defaultValue, Action<object> onValueChanged = null)
    {
        Id = id;
        OptionName = name;
        OptionTab = tab;
        Value = defaultValue;
        DefaultValue = defaultValue;
        OnValueChanged = onValueChanged;
    }

    public void SetValue(bool value)
    {
        if (Equals(Value, value))
            return;

        Value = value;
        OnValueChanged?.Invoke(Value);
        OptionSaver.BoolValues[Id] = value;
        OptionSaver.Save();
    }
}