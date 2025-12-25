using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WpfEventRecorder.Core.Models;

/// <summary>
/// Represents a node in the visual tree hierarchy
/// </summary>
public class VisualTreeNode : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isSelected;

    public VisualTreeNode()
    {
        Children = new ObservableCollection<VisualTreeNode>();
        Properties = new ObservableCollection<PropertyItem>();
    }

    /// <summary>
    /// The type of control (Button, TextBox, Grid, etc.)
    /// </summary>
    public string ControlType { get; set; } = "Unknown";

    /// <summary>
    /// The name/content of the control
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The UI Automation ID
    /// </summary>
    public string? AutomationId { get; set; }

    /// <summary>
    /// The class name of the control
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// The framework ID (WPF, Win32, etc.)
    /// </summary>
    public string? FrameworkId { get; set; }

    /// <summary>
    /// The bounding rectangle of the control
    /// </summary>
    public Rect BoundingRectangle { get; set; }

    /// <summary>
    /// Whether this node is expanded in the tree view
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Whether this node is selected in the tree view
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Parent node in the tree
    /// </summary>
    public VisualTreeNode? Parent { get; set; }

    /// <summary>
    /// Child nodes
    /// </summary>
    public ObservableCollection<VisualTreeNode> Children { get; set; }

    /// <summary>
    /// All properties of this element
    /// </summary>
    public ObservableCollection<PropertyItem> Properties { get; set; }

    /// <summary>
    /// Display text for the tree view (e.g., "#okButton" or "'Submit'" or "")
    /// </summary>
    public string DisplayText
    {
        get
        {
            if (!string.IsNullOrEmpty(AutomationId))
                return $"#{AutomationId}";
            if (!string.IsNullOrEmpty(Name))
                return $"'{Name}'";
            return "";
        }
    }

    /// <summary>
    /// Full display string for the tree view
    /// </summary>
    public string FullDisplayText
    {
        get
        {
            var display = ControlType;
            if (!string.IsNullOrEmpty(AutomationId))
                display += $" #{AutomationId}";
            else if (!string.IsNullOrEmpty(Name))
                display += $" '{Name}'";
            return display;
        }
    }

    /// <summary>
    /// Number of child elements
    /// </summary>
    public int ChildCount => Children.Count;

    /// <summary>
    /// Expands all nodes in this subtree
    /// </summary>
    public void ExpandAll()
    {
        IsExpanded = true;
        foreach (var child in Children)
        {
            child.ExpandAll();
        }
    }

    /// <summary>
    /// Collapses all nodes in this subtree
    /// </summary>
    public void CollapseAll()
    {
        IsExpanded = false;
        foreach (var child in Children)
        {
            child.CollapseAll();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Represents a property item for display in the property explorer
/// </summary>
public class PropertyItem
{
    public PropertyItem(string category, string name, string? value)
    {
        Category = category;
        Name = name;
        Value = value ?? "(null)";
    }

    /// <summary>
    /// Category for grouping (Identity, Layout, State, Content, Patterns)
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Property name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Property value as string
    /// </summary>
    public string Value { get; set; }
}
