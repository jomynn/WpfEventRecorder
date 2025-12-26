using System.Windows;
using System.Windows.Data;

namespace WpfEventRecorder.Core.Helpers;

/// <summary>
/// Helper methods for working with WPF bindings.
/// </summary>
public static class BindingHelper
{
    /// <summary>
    /// Gets the binding path for a dependency property.
    /// </summary>
    public static string? GetBindingPath(DependencyObject obj, DependencyProperty property)
    {
        var binding = BindingOperations.GetBinding(obj, property);
        return binding?.Path.Path;
    }

    /// <summary>
    /// Gets the binding expression for a dependency property.
    /// </summary>
    public static BindingExpression? GetBindingExpression(DependencyObject obj, DependencyProperty property)
    {
        if (obj is FrameworkElement fe)
            return fe.GetBindingExpression(property);

        if (obj is FrameworkContentElement fce)
            return fce.GetBindingExpression(property);

        return null;
    }

    /// <summary>
    /// Gets the data context for an element.
    /// </summary>
    public static object? GetDataContext(DependencyObject obj)
    {
        if (obj is FrameworkElement fe)
            return fe.DataContext;

        if (obj is FrameworkContentElement fce)
            return fce.DataContext;

        return null;
    }

    /// <summary>
    /// Gets the ViewModel type name for an element.
    /// </summary>
    public static string? GetViewModelTypeName(DependencyObject obj)
    {
        var dataContext = GetDataContext(obj);
        return dataContext?.GetType().FullName;
    }

    /// <summary>
    /// Extracts the property name from a binding path.
    /// </summary>
    public static string? GetViewModelPropertyName(string? bindingPath)
    {
        if (string.IsNullOrEmpty(bindingPath))
            return null;

        // Handle simple property paths (e.g., "PropertyName")
        if (!bindingPath.Contains('.') && !bindingPath.Contains('['))
            return bindingPath;

        // Handle complex paths (e.g., "ViewModel.Property" or "Items[0].Name")
        var parts = bindingPath.Split('.');
        return parts.Length > 0 ? parts[^1] : bindingPath;
    }

    /// <summary>
    /// Gets the binding mode for a dependency property.
    /// </summary>
    public static BindingMode GetBindingMode(DependencyObject obj, DependencyProperty property)
    {
        var binding = BindingOperations.GetBinding(obj, property);
        return binding?.Mode ?? BindingMode.Default;
    }

    /// <summary>
    /// Gets the binding source for a dependency property.
    /// </summary>
    public static object? GetBindingSource(DependencyObject obj, DependencyProperty property)
    {
        var bindingExpression = GetBindingExpression(obj, property);
        return bindingExpression?.DataItem;
    }

    /// <summary>
    /// Gets all bindings on an element.
    /// </summary>
    public static IEnumerable<(DependencyProperty Property, Binding Binding)> GetAllBindings(DependencyObject obj)
    {
        var localValueEnumerator = obj.GetLocalValueEnumerator();
        while (localValueEnumerator.MoveNext())
        {
            var entry = localValueEnumerator.Current;
            var binding = BindingOperations.GetBinding(obj, entry.Property);
            if (binding != null)
            {
                yield return (entry.Property, binding);
            }
        }
    }
}
