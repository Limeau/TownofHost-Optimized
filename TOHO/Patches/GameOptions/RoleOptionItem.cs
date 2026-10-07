using System;
using System.Collections.Generic;
using TMPro;
using TOHO;
using UnityEngine;
using Object = UnityEngine.Object;

public class RoleOptionItem
{
    private static readonly Dictionary<ToggleOption, RoleOptionItem> OptionMap = new();
    public static List<BooleanOptionItem> SubBooleanOptions = [];
    public static List<NumberOptionItem> SubNumberOptions = [];
    public static List<StringOptionItem> SubStringOptions = [];
    
    public int Id;

    public CustomRoles Role;
    
    public RoleCategories OptionTab;
    public string OptionName;

    public GameObject obj;
    public ToggleOption Toggle;

    public bool Value;
    public Action<object> OnValueChanged;
    public bool DefaultValue;
    
    public RoleOptionItem(CustomRoles role, int id, string name, RoleCategories tab, Action<object> onValueChanged = null)
    {
        Role = role;
        Id = id;
        OptionName = name;
        OptionTab = tab;
        Value = false;
        DefaultValue = false;
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