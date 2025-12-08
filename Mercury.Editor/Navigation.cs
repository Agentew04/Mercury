using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace Mercury.Editor;

/// <summary>
/// Class to manage lazy loading page navigation
/// </summary>
public class Navigation {
    private static Navigation? _instance;

    public Navigation(ContentControl contentControl, bool setAsDefault = true) {
        if (_instance is not null && setAsDefault) {
            throw new NotSupportedException("There can be only one default navigation");
        }
        host = contentControl;
        if (setAsDefault) {
            _instance = this;
        }
    }
    
    private readonly ContentControl host;
    private readonly Dictionary<NavigationTarget, Func<Control>> registeredTypes = [];
    private readonly Dictionary<NavigationTarget, Control> createdTargets = [];
    private NavigationTarget current;
    private bool hasCurrent;

    public void Register<TControl>(NavigationTarget target, bool initialize = false) where TControl : Control, new() {
        registeredTypes.Add(target, () => new TControl());
        if (initialize) {
            createdTargets[target] = new TControl();
        }
    }

    [RequiresDynamicCode("Uses reflection to create instances of pages")]
    public void Navigate(NavigationTarget target) {
        if (!createdTargets.TryGetValue(target, out Control? ctrl)) {
            ctrl = registeredTypes[target].Invoke();
            if (ctrl is null) {
                throw new NotSupportedException("Could not create dynamic page type");
            }
        }

        if (!hasCurrent || current != target) {
            host.Content = ctrl;
            current = target;
            hasCurrent = true;
        }
    }

    public static void NavigateTo(NavigationTarget target) {
        if (_instance is null)
        {
            throw new InvalidOperationException("Navigation instance is not initialized. Ensure that Navigation is created with a TabControl.");
        }
        _instance.Navigate(target);
    }
}

public enum NavigationTarget {
    CodeView,
    ExecuteView,
    DesignView
}