using System.Windows;
using System.Windows.Media;

namespace WpfEventRecorder.Core.Helpers;

/// <summary>
/// Helper for walking the WPF visual tree.
/// </summary>
public static class VisualTreeWalker
{
    /// <summary>
    /// Walks the visual tree starting from the given element.
    /// </summary>
    /// <param name="element">The starting element.</param>
    /// <param name="visitor">A function that receives each element. Return true to continue, false to stop.</param>
    public static void Walk(DependencyObject element, Func<DependencyObject, bool> visitor)
    {
        if (element == null) return;

        if (!visitor(element))
            return;

        int childCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            Walk(child, visitor);
        }
    }

    /// <summary>
    /// Walks the visual tree and collects elements matching a predicate.
    /// </summary>
    public static List<T> FindAll<T>(DependencyObject element, Func<T, bool>? predicate = null)
        where T : DependencyObject
    {
        var results = new List<T>();

        Walk(element, child =>
        {
            if (child is T typedChild && (predicate?.Invoke(typedChild) ?? true))
            {
                results.Add(typedChild);
            }
            return true;
        });

        return results;
    }

    /// <summary>
    /// Finds the first element of a given type.
    /// </summary>
    public static T? FindFirst<T>(DependencyObject element, Func<T, bool>? predicate = null)
        where T : DependencyObject
    {
        T? result = null;

        Walk(element, child =>
        {
            if (child is T typedChild && (predicate?.Invoke(typedChild) ?? true))
            {
                result = typedChild;
                return false; // Stop walking
            }
            return true;
        });

        return result;
    }

    /// <summary>
    /// Finds a parent of a given type.
    /// </summary>
    public static T? FindParent<T>(DependencyObject element) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(element);

        while (parent != null)
        {
            if (parent is T typedParent)
                return typedParent;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    /// <summary>
    /// Finds an element by name.
    /// </summary>
    public static FrameworkElement? FindByName(DependencyObject element, string name)
    {
        return FindFirst<FrameworkElement>(element, fe => fe.Name == name);
    }

    /// <summary>
    /// Finds an element by automation ID.
    /// </summary>
    public static FrameworkElement? FindByAutomationId(DependencyObject element, string automationId)
    {
        return FindFirst<FrameworkElement>(element, fe =>
            System.Windows.Automation.AutomationProperties.GetAutomationId(fe) == automationId);
    }

    /// <summary>
    /// Gets the path from the root to an element.
    /// </summary>
    public static string GetPath(DependencyObject element)
    {
        var parts = new List<string>();
        var current = element;

        while (current != null)
        {
            var name = GetElementName(current);
            parts.Insert(0, name);
            current = VisualTreeHelper.GetParent(current);
        }

        return string.Join(" > ", parts);
    }

    /// <summary>
    /// Gets a descriptive name for an element.
    /// </summary>
    public static string GetElementName(DependencyObject element)
    {
        if (element is FrameworkElement fe)
        {
            if (!string.IsNullOrEmpty(fe.Name))
                return $"{fe.GetType().Name}#{fe.Name}";

            var automationId = System.Windows.Automation.AutomationProperties.GetAutomationId(fe);
            if (!string.IsNullOrEmpty(automationId))
                return $"{fe.GetType().Name}[{automationId}]";
        }

        return element.GetType().Name;
    }

    /// <summary>
    /// Counts all descendants of an element.
    /// </summary>
    public static int CountDescendants(DependencyObject element)
    {
        int count = 0;
        Walk(element, _ =>
        {
            count++;
            return true;
        });
        return count - 1; // Exclude the root
    }

    /// <summary>
    /// Gets the depth of an element in the tree.
    /// </summary>
    public static int GetDepth(DependencyObject element)
    {
        int depth = 0;
        var current = VisualTreeHelper.GetParent(element);

        while (current != null)
        {
            depth++;
            current = VisualTreeHelper.GetParent(current);
        }

        return depth;
    }
}
