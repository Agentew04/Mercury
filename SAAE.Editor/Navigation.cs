using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace SAAE.Editor;

public class Navigation
{
    private static Navigation? _instance;

    public Navigation(TabControl tabControl)
    {
        host = tabControl;
        _instance = this;
        
        // Initialize targets
        ItemCollection children = tabControl.Items;
        foreach (object? child in children)
        {
            if (child is not TabItem tabItem)
            {
                continue;
            }
            NavigationTarget? target = (NavigationTarget?)tabItem.Tag;
            if (target is null)
            {
                throw new NotSupportedException("The Tag of a TabItem must be set to a NavigationTarget enum value.");
            }
            if (!targets.TryAdd(target.Value, tabItem))
            {
                throw new NotSupportedException($"The TabItem with Tag '{target}' is already registered.");
            }
        }
    }
    
    public static void NavigateTo(NavigationTarget target)
    {
        if (_instance is null)
        {
            throw new InvalidOperationException("Navigation instance is not initialized. Ensure that Navigation is created with a TabControl.");
        }
        
        if (!_instance.targets.TryGetValue(target, out TabItem? tabItem))
        {
            throw new NotSupportedException($"No TabItem registered for NavigationTarget '{target}'.");
        }
        _instance.host.SelectedItem = tabItem;
    }

    private readonly TabControl host;
    private readonly Dictionary<NavigationTarget, TabItem> targets = [];
}

public enum NavigationTarget
{
    CodeView,
    ExecuteView,
    DesignView
}