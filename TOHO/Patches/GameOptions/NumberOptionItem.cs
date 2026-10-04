using System;
using System.Collections.Generic;
using TMPro;
using TOHO;
using UnityEngine;
using Object = UnityEngine.Object;

public class NumberOptionItem
{
    private static readonly Dictionary<ToggleOption, NumberOptionItem> OptionMap = new();

    public OptionTabs OptionTab;
    public string OptionName;

    public int Id;
    
    public GameObject obj;
    public ToggleOption Toggle;
    public Action<object> OnValueChanged;

    public float Value;
    public float DefaultValue;
    public float MinValue;
    public float MaxValue;
    public float Step;
    
    public NumberOptionItem(int id, string name, OptionTabs tab, float defaultValue, float minValue, float maxValue, float step, Action<object> onValueChanged = null)
    {
        Id = id;
        OptionName = name;
        OptionTab = tab;
        Value = defaultValue;
        DefaultValue = defaultValue;
        MinValue = minValue;
        MaxValue = maxValue;
        OnValueChanged = onValueChanged;
        Step = step;
    }

    public void SetValue(float value)
    {
        if (Equals(Value, value))
            return;

        Value = value;
        OnValueChanged?.Invoke(Value);
        OptionSaver.NumberValues[Id] = value;
        OptionSaver.Save();
    }
}