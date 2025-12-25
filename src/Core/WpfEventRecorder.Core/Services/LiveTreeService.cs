using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using WpfEventRecorder.Core.Models;

namespace WpfEventRecorder.Core.Services;

/// <summary>
/// Service for building live visual trees from running applications
/// </summary>
public static class LiveTreeService
{
    /// <summary>
    /// Builds a visual tree from a window handle
    /// </summary>
    /// <param name="windowHandle">The window handle to build the tree from</param>
    /// <returns>The root node of the visual tree, or null if failed</returns>
    public static VisualTreeNode? BuildVisualTree(IntPtr windowHandle)
    {
        try
        {
            var element = AutomationElement.FromHandle(windowHandle);
            if (element == null)
                return null;

            return BuildNodeFromElement(element, null, 0, maxDepth: 50);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Builds a visual tree from an AutomationElement
    /// </summary>
    public static VisualTreeNode? BuildVisualTree(AutomationElement rootElement)
    {
        try
        {
            return BuildNodeFromElement(rootElement, null, 0, maxDepth: 50);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all properties for a visual tree node
    /// </summary>
    public static ObservableCollection<PropertyItem> GetElementProperties(VisualTreeNode node)
    {
        var properties = new ObservableCollection<PropertyItem>();

        // Identity properties
        properties.Add(new PropertyItem("Identity", "ControlType", node.ControlType));
        properties.Add(new PropertyItem("Identity", "Name", node.Name));
        properties.Add(new PropertyItem("Identity", "AutomationId", node.AutomationId));
        properties.Add(new PropertyItem("Identity", "ClassName", node.ClassName));
        properties.Add(new PropertyItem("Identity", "FrameworkId", node.FrameworkId));

        // Layout properties
        if (!node.BoundingRectangle.IsEmpty)
        {
            properties.Add(new PropertyItem("Layout", "X", node.BoundingRectangle.X.ToString("F0")));
            properties.Add(new PropertyItem("Layout", "Y", node.BoundingRectangle.Y.ToString("F0")));
            properties.Add(new PropertyItem("Layout", "Width", node.BoundingRectangle.Width.ToString("F0")));
            properties.Add(new PropertyItem("Layout", "Height", node.BoundingRectangle.Height.ToString("F0")));
        }

        // Add properties from the node's Properties collection
        foreach (var prop in node.Properties)
        {
            properties.Add(prop);
        }

        return properties;
    }

    private static VisualTreeNode BuildNodeFromElement(AutomationElement element, VisualTreeNode? parent, int depth, int maxDepth)
    {
        if (depth > maxDepth)
            return new VisualTreeNode { ControlType = "..." };

        var node = new VisualTreeNode
        {
            Parent = parent,
            IsExpanded = depth < 2 // Auto-expand first two levels
        };

        try
        {
            // Basic properties
            node.ControlType = element.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
            node.Name = element.Current.Name;
            node.AutomationId = element.Current.AutomationId;
            node.ClassName = element.Current.ClassName;
            node.FrameworkId = element.Current.FrameworkId;
            node.BoundingRectangle = element.Current.BoundingRectangle;

            // Collect additional properties
            CollectElementProperties(element, node);

            // Build children
            var walker = TreeWalker.ControlViewWalker;
            var child = walker.GetFirstChild(element);

            while (child != null)
            {
                try
                {
                    var childNode = BuildNodeFromElement(child, node, depth + 1, maxDepth);
                    node.Children.Add(childNode);
                }
                catch
                {
                    // Skip elements that throw exceptions
                }

                child = walker.GetNextSibling(child);
            }
        }
        catch
        {
            // If we can't access the element, return a minimal node
            node.ControlType = "Unknown";
        }

        return node;
    }

    private static void CollectElementProperties(AutomationElement element, VisualTreeNode node)
    {
        try
        {
            // State properties
            node.Properties.Add(new PropertyItem("State", "IsEnabled", element.Current.IsEnabled.ToString()));
            node.Properties.Add(new PropertyItem("State", "IsOffscreen", element.Current.IsOffscreen.ToString()));
            node.Properties.Add(new PropertyItem("State", "IsKeyboardFocusable", element.Current.IsKeyboardFocusable.ToString()));
            node.Properties.Add(new PropertyItem("State", "HasKeyboardFocus", element.Current.HasKeyboardFocus.ToString()));

            // Try to get content text
            if (!string.IsNullOrEmpty(element.Current.Name))
            {
                node.Properties.Add(new PropertyItem("Content", "Text", element.Current.Name));
            }

            // ValuePattern
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
            {
                var vp = (ValuePattern)valuePattern;
                node.Properties.Add(new PropertyItem("Patterns", "Value", vp.Current.Value));
                node.Properties.Add(new PropertyItem("Patterns", "IsReadOnly", vp.Current.IsReadOnly.ToString()));
            }

            // TogglePattern
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? togglePattern))
            {
                var tp = (TogglePattern)togglePattern;
                node.Properties.Add(new PropertyItem("Patterns", "ToggleState", tp.Current.ToggleState.ToString()));
            }

            // SelectionItemPattern
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionItemPattern))
            {
                var sip = (SelectionItemPattern)selectionItemPattern;
                node.Properties.Add(new PropertyItem("Patterns", "IsSelected", sip.Current.IsSelected.ToString()));
            }

            // SelectionPattern
            if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out object? selectionPattern))
            {
                var sp = (SelectionPattern)selectionPattern;
                node.Properties.Add(new PropertyItem("Patterns", "CanSelectMultiple", sp.Current.CanSelectMultiple.ToString()));
                node.Properties.Add(new PropertyItem("Patterns", "IsSelectionRequired", sp.Current.IsSelectionRequired.ToString()));
            }

            // RangeValuePattern
            if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out object? rangeValuePattern))
            {
                var rvp = (RangeValuePattern)rangeValuePattern;
                node.Properties.Add(new PropertyItem("Patterns", "RangeValue", rvp.Current.Value.ToString("F2")));
                node.Properties.Add(new PropertyItem("Patterns", "Minimum", rvp.Current.Minimum.ToString("F2")));
                node.Properties.Add(new PropertyItem("Patterns", "Maximum", rvp.Current.Maximum.ToString("F2")));
            }

            // GridPattern
            if (element.TryGetCurrentPattern(GridPattern.Pattern, out object? gridPattern))
            {
                var gp = (GridPattern)gridPattern;
                node.Properties.Add(new PropertyItem("Patterns", "RowCount", gp.Current.RowCount.ToString()));
                node.Properties.Add(new PropertyItem("Patterns", "ColumnCount", gp.Current.ColumnCount.ToString()));
            }

            // GridItemPattern
            if (element.TryGetCurrentPattern(GridItemPattern.Pattern, out object? gridItemPattern))
            {
                var gip = (GridItemPattern)gridItemPattern;
                node.Properties.Add(new PropertyItem("Patterns", "Row", gip.Current.Row.ToString()));
                node.Properties.Add(new PropertyItem("Patterns", "Column", gip.Current.Column.ToString()));
            }

            // ExpandCollapsePattern
            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandCollapsePattern))
            {
                var ecp = (ExpandCollapsePattern)expandCollapsePattern;
                node.Properties.Add(new PropertyItem("Patterns", "ExpandCollapseState", ecp.Current.ExpandCollapseState.ToString()));
            }

            // ScrollPattern
            if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out object? scrollPattern))
            {
                var sp = (ScrollPattern)scrollPattern;
                node.Properties.Add(new PropertyItem("Patterns", "HorizontalScrollPercent", sp.Current.HorizontalScrollPercent.ToString("F1") + "%"));
                node.Properties.Add(new PropertyItem("Patterns", "VerticalScrollPercent", sp.Current.VerticalScrollPercent.ToString("F1") + "%"));
            }

            // Additional useful properties
            if (!string.IsNullOrEmpty(element.Current.HelpText))
            {
                node.Properties.Add(new PropertyItem("Content", "HelpText", element.Current.HelpText));
            }

            if (!string.IsNullOrEmpty(element.Current.AccessKey))
            {
                node.Properties.Add(new PropertyItem("Content", "AccessKey", element.Current.AccessKey));
            }

            if (!string.IsNullOrEmpty(element.Current.AcceleratorKey))
            {
                node.Properties.Add(new PropertyItem("Content", "AcceleratorKey", element.Current.AcceleratorKey));
            }

            if (element.Current.ProcessId > 0)
            {
                node.Properties.Add(new PropertyItem("Process", "ProcessId", element.Current.ProcessId.ToString()));
            }
        }
        catch
        {
            // Ignore property collection errors
        }
    }
}
