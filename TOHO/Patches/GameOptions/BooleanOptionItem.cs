using System;
using System.Collections.Generic;
using TMPro;
using TOHO;
using UnityEngine;
using Object = UnityEngine.Object;

public class BooleanOptionItem
{
    private static readonly Dictionary<ToggleOption, BooleanOptionItem> OptionMap = new();

    public OptionTabs OptionTab;
    public string OptionName;

    public GameObject obj;
    public ToggleOption Toggle;

    public object Value;
    public object DefaultValue;

    public Action<object> OnValueChanged;

    public BooleanOptionItem(string name, OptionTabs tab, object defaultValue, Action<object> onValueChanged = null)
    {
        OptionName = name;
        OptionTab = tab;
        Value = defaultValue;
        DefaultValue = defaultValue;
        OnValueChanged = onValueChanged;
    }

    public void SetValue(object value)
    {
        if (Equals(Value, value))
            return;

        Value = value;
        OnValueChanged?.Invoke(Value);
    }
}