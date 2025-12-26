using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.Core.Infrastructure
{
    /// <summary>
    /// Walks the visual tree and attaches event handlers for recording
    /// </summary>
    public class ViewInstrumenter
    {
        private readonly HashSet<int> _instrumentedElements = new HashSet<int>();
        private readonly RecordingHub _hub;

        /// <summary>
        /// Creates a new view instrumenter
        /// </summary>
        public ViewInstrumenter()
        {
            _hub = RecordingHub.Instance;
        }

        /// <summary>
        /// Instruments a window and all its child controls
        /// </summary>
        public void InstrumentWindow(Window window)
        {
            if (window == null) return;

            window.Loaded += (s, e) => InstrumentElement(window);
            window.ContentRendered += (s, e) => InstrumentElement(window);

            // Also hook window events
            window.Closing += (s, e) =>
            {
                if (_hub.IsRecording)
                {
                    _hub.AddEntry(new RecordEntry
                    {
                        EntryType = RecordEntryType.UIWindowClose,
                        UIInfo = new UIInfo
                        {
                            ControlType = "Window",
                            WindowTitle = window.Title,
                            WindowType = window.GetType().Name
                        }
                    });
                }
            };

            // Instrument if already loaded
            if (window.IsLoaded)
            {
                InstrumentElement(window);
            }
        }

        /// <summary>
        /// Instruments a visual element and its children
        /// </summary>
        public void InstrumentElement(DependencyObject element)
        {
            if (element == null) return;

            // Skip if already instrumented
            var hashCode = element.GetHashCode();
            if (_instrumentedElements.Contains(hashCode))
            {
                return;
            }
            _instrumentedElements.Add(hashCode);

            // Instrument based on control type
            switch (element)
            {
                case TextBox textBox:
                    InstrumentTextBox(textBox);
                    break;

                case ComboBox comboBox:
                    InstrumentComboBox(comboBox);
                    break;

                case ListBox listBox:
                    InstrumentListBox(listBox);
                    break;

                case CheckBox checkBox:
                    InstrumentToggleButton(checkBox);
                    break;

                case RadioButton radioButton:
                    InstrumentToggleButton(radioButton);
                    break;

                case Button button:
                    InstrumentButton(button);
                    break;

                case DatePicker datePicker:
                    InstrumentDatePicker(datePicker);
                    break;

                case Slider slider:
                    InstrumentSlider(slider);
                    break;

                case DataGrid dataGrid:
                    InstrumentDataGrid(dataGrid);
                    break;

                case TabControl tabControl:
                    InstrumentTabControl(tabControl);
                    break;
            }

            // Recursively instrument children
            int childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                InstrumentElement(child);
            }
        }

        private void InstrumentTextBox(TextBox textBox)
        {
            string oldValue = textBox.Text;

            textBox.LostFocus += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                if (textBox.Text == oldValue) return;

                RecordTextInput(textBox, oldValue, textBox.Text);
                oldValue = textBox.Text;
            };

            textBox.TextChanged += (s, e) =>
            {
                // Just track for LostFocus comparison
            };
        }

        private void InstrumentComboBox(ComboBox comboBox)
        {
            object oldSelection = comboBox.SelectedItem;

            comboBox.SelectionChanged += (s, e) =>
            {
                if (!_hub.IsRecording) return;

                var newSelection = comboBox.SelectedItem;
                RecordSelectionChange(comboBox, oldSelection?.ToString(), newSelection?.ToString());
                oldSelection = newSelection;
            };
        }

        private void InstrumentListBox(ListBox listBox)
        {
            listBox.SelectionChanged += (s, e) =>
            {
                if (!_hub.IsRecording) return;

                var oldItems = string.Join(", ", e.RemovedItems);
                var newItems = string.Join(", ", e.AddedItems);
                RecordSelectionChange(listBox, oldItems, newItems);
            };
        }

        private void InstrumentToggleButton(ToggleButton toggleButton)
        {
            toggleButton.Checked += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                RecordToggle(toggleButton, false, true);
            };

            toggleButton.Unchecked += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                RecordToggle(toggleButton, true, false);
            };
        }

        private void InstrumentButton(Button button)
        {
            button.Click += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                RecordClick(button);
            };
        }

        private void InstrumentDatePicker(DatePicker datePicker)
        {
            DateTime? oldDate = datePicker.SelectedDate;

            datePicker.SelectedDateChanged += (s, e) =>
            {
                if (!_hub.IsRecording) return;

                RecordSelectionChange(datePicker,
                    oldDate?.ToString("yyyy-MM-dd"),
                    datePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                oldDate = datePicker.SelectedDate;
            };
        }

        private void InstrumentSlider(Slider slider)
        {
            double oldValue = slider.Value;

            slider.ValueChanged += (s, e) =>
            {
                // Debounce slider changes - only record on LostFocus or significant change
            };

            slider.LostMouseCapture += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                if (Math.Abs(slider.Value - oldValue) < 0.001) return;

                RecordValueChange(slider, oldValue.ToString("F2"), slider.Value.ToString("F2"));
                oldValue = slider.Value;
            };
        }

        private void InstrumentDataGrid(DataGrid dataGrid)
        {
            dataGrid.SelectionChanged += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                RecordSelectionChange(dataGrid, null, dataGrid.SelectedItem?.ToString());
            };

            dataGrid.CellEditEnding += (s, e) =>
            {
                if (!_hub.IsRecording) return;

                var cell = e.EditingElement as TextBox;
                if (cell != null)
                {
                    _hub.AddEntry(new RecordEntry
                    {
                        EntryType = RecordEntryType.UITextInput,
                        UIInfo = new UIInfo
                        {
                            ControlType = "DataGridCell",
                            ControlName = GetControlName(dataGrid),
                            AutomationId = GetAutomationId(dataGrid),
                            NewValue = cell.Text,
                            WindowTitle = GetWindowTitle(dataGrid),
                            Properties = new Dictionary<string, string>
                            {
                                { "Column", e.Column.Header?.ToString() ?? "" },
                                { "RowIndex", e.Row.GetIndex().ToString() }
                            }
                        }
                    });
                }
            };
        }

        private void InstrumentTabControl(TabControl tabControl)
        {
            tabControl.SelectionChanged += (s, e) =>
            {
                if (!_hub.IsRecording) return;
                if (e.Source != tabControl) return;

                var newTab = tabControl.SelectedItem as TabItem;
                RecordSelectionChange(tabControl, null, newTab?.Header?.ToString());
            };
        }

        private void RecordTextInput(Control control, string oldValue, string newValue)
        {
            _hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.UITextInput,
                UIInfo = new UIInfo
                {
                    ControlType = control.GetType().Name,
                    ControlName = GetControlName(control),
                    AutomationId = GetAutomationId(control),
                    OldValue = oldValue,
                    NewValue = newValue,
                    WindowTitle = GetWindowTitle(control),
                    VisualTreePath = GetBindingPath(control)
                }
            });
        }

        private void RecordSelectionChange(Control control, string oldValue, string newValue)
        {
            _hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.UISelectionChange,
                UIInfo = new UIInfo
                {
                    ControlType = control.GetType().Name,
                    ControlName = GetControlName(control),
                    AutomationId = GetAutomationId(control),
                    OldValue = oldValue,
                    NewValue = newValue,
                    WindowTitle = GetWindowTitle(control),
                    VisualTreePath = GetBindingPath(control)
                }
            });
        }

        private void RecordToggle(ToggleButton control, bool oldValue, bool newValue)
        {
            _hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.UIToggle,
                UIInfo = new UIInfo
                {
                    ControlType = control.GetType().Name,
                    ControlName = GetControlName(control),
                    AutomationId = GetAutomationId(control),
                    OldValue = oldValue.ToString(),
                    NewValue = newValue.ToString(),
                    ContentText = control.Content?.ToString(),
                    WindowTitle = GetWindowTitle(control)
                }
            });
        }

        private void RecordClick(Button button)
        {
            _hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.UIClick,
                UIInfo = new UIInfo
                {
                    ControlType = "Button",
                    ControlName = GetControlName(button),
                    AutomationId = GetAutomationId(button),
                    ContentText = button.Content?.ToString(),
                    WindowTitle = GetWindowTitle(button)
                }
            });
        }

        private void RecordValueChange(Control control, string oldValue, string newValue)
        {
            _hub.AddEntry(new RecordEntry
            {
                EntryType = RecordEntryType.UISelectionChange,
                UIInfo = new UIInfo
                {
                    ControlType = control.GetType().Name,
                    ControlName = GetControlName(control),
                    AutomationId = GetAutomationId(control),
                    OldValue = oldValue,
                    NewValue = newValue,
                    WindowTitle = GetWindowTitle(control)
                }
            });
        }

        private string GetControlName(DependencyObject element)
        {
            if (element is FrameworkElement fe)
            {
                return fe.Name;
            }
            return null;
        }

        private string GetAutomationId(DependencyObject element)
        {
            return System.Windows.Automation.AutomationProperties.GetAutomationId(element);
        }

        private string GetWindowTitle(DependencyObject element)
        {
            var window = Window.GetWindow(element);
            return window?.Title;
        }

        private string GetBindingPath(DependencyObject element)
        {
            // Try to get binding path for common properties
            var bindingPaths = new List<string>();

            DependencyProperty[] propsToCheck = element switch
            {
                TextBox => new[] { TextBox.TextProperty },
                ComboBox => new[] { Selector.SelectedItemProperty, Selector.SelectedValueProperty },
                CheckBox => new[] { ToggleButton.IsCheckedProperty },
                _ => Array.Empty<DependencyProperty>()
            };

            foreach (var prop in propsToCheck)
            {
                var binding = BindingOperations.GetBinding(element, prop);
                if (binding?.Path?.Path != null)
                {
                    bindingPaths.Add($"{prop.Name}:{binding.Path.Path}");
                }
            }

            return bindingPaths.Count > 0 ? string.Join(", ", bindingPaths) : null;
        }
    }
}
