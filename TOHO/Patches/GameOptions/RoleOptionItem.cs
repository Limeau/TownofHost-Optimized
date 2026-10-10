using System;
using System.Collections.Generic;
using TMPro;
using TOHO;
using UnityEngine;
using Object = UnityEngine.Object;

public class RoleOptionItem
{
    private static readonly Dictionary<ToggleOption, RoleOptionItem> OptionMap = new();
    
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
        Role.Enabled = false;
        DefaultValue = false;
        OnValueChanged = onValueChanged;
    }

    public void SetValue(bool value)
    {
        if (Equals(Value, value))
            return;

        Value = value;
        Role.Enabled = value;
        OnValueChanged?.Invoke(Value);
        OptionSaver.RoleValues[Id] = value;
        OptionSaver.Save();
    }
}