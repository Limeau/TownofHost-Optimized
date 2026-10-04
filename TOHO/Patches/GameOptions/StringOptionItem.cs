using System;
using System.Collections.Generic;
using TMPro;
using TOHO;
using UnityEngine;
using Object = UnityEngine.Object;

public class StringOptionItem
{
    public static readonly Dictionary<StringOption, StringOptionItem> OptionMap = new();

    public OptionTabs OptionTab;
    public string OptionName;

    public int Id;
    
    public GameObject obj;
    public ToggleOption Toggle;
    public Action<object> OnValueChanged;

    
    public int Value;
    public Dictionary<int, string> AllValues = [];
    
    public StringOptionItem(int id, string name, OptionTabs tab, Dictionary<int, string> allValues, Action<object> onValueChanged = null)
    {
        Id = id;
        OptionName = name;
        OptionTab = tab;
        AllValues = allValues;
        OnValueChanged = onValueChanged;
    }

    public void SetValue(int value)
    {
        if (AllValues.Count == 0)
            return;

        int maxIndex = AllValues.Count - 1;

        if (value < 0)
            value = maxIndex;
        else if (value > maxIndex)
            value = 0;

        if (Value == value)
            return;

        Value = value;

        OnValueChanged?.Invoke(Value);

        OptionSaver.StringValues[Id] = value;
        OptionSaver.Save();
    }
}