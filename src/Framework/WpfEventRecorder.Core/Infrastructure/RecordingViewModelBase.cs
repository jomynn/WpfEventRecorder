using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.Core.Models;
using WpfEventRecorder.Core.Services;

namespace WpfEventRecorder.Core.Infrastructure
{
    /// <summary>
    /// Base ViewModel class that automatically records property changes
    /// </summary>
    public abstract class RecordingViewModelBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> _propertyValues = new Dictionary<string, object>();
        private readonly HashSet<string> _ignoredProperties = new HashSet<string>();
        private readonly Dictionary<string, string> _propertyDisplayNames = new Dictionary<string, string>();
        private readonly string _viewModelName;
        private readonly bool _recordAllProperties;

        /// <summary>
        /// Event raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets whether recording is currently enabled for this ViewModel
        /// </summary>
        public bool IsRecordingEnabled { get; set; } = true;

        /// <summary>
        /// Creates a new RecordingViewModelBase
        /// </summary>
        protected RecordingViewModelBase()
        {
            var type = GetType();
            var recordAttr = type.GetCustomAttribute<RecordViewModelAttribute>();

            _viewModelName = recordAttr?.Name ?? type.Name;
            _recordAllProperties = recordAttr?.RecordAllProperties ?? true;

            // Scan properties for attributes
            foreach (var prop in type.GetProperties())
            {
                if (prop.GetCustomAttribute<IgnoreRecordingAttribute>() != null)
                {
                    _ignoredProperties.Add(prop.Name);
                }

                var recordPropAttr = prop.GetCustomAttribute<RecordPropertyAttribute>();
                if (recordPropAttr?.DisplayName != null)
                {
                    _propertyDisplayNames[prop.Name] = recordPropAttr.DisplayName;
                }
            }
        }

        /// <summary>
        /// Sets a property value and records the change
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Name of the property (auto-filled)</param>
        /// <returns>True if the value changed</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            var oldValue = field;
            field = value;

            RecordPropertyChange(propertyName, oldValue, value);
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Sets a property value and records the change with additional actions
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="onChanged">Action to invoke when value changes</param>
        /// <param name="propertyName">Name of the property (auto-filled)</param>
        /// <returns>True if the value changed</returns>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged,
            [CallerMemberName] string propertyName = null)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                onChanged?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Records a property change event
        /// </summary>
        protected virtual void RecordPropertyChange<T>(string propertyName, T oldValue, T newValue)
        {
            if (!IsRecordingEnabled) return;
            if (!ShouldRecordProperty(propertyName)) return;

            var hub = RecordingHub.Instance;
            if (!hub.IsRecording) return;

            var displayName = _propertyDisplayNames.TryGetValue(propertyName, out var name)
                ? name
                : propertyName;

            var entry = new RecordEntry
            {
                EntryType = RecordEntryType.Custom,
                Metadata = "PropertyChange",
                UIInfo = new UIInfo
                {
                    ControlType = "ViewModel",
                    ControlName = $"{_viewModelName}.{displayName}",
                    OldValue = oldValue?.ToString(),
                    NewValue = newValue?.ToString()
                }
            };

            hub.AddEntry(entry);
        }

        /// <summary>
        /// Determines if a property should be recorded
        /// </summary>
        protected virtual bool ShouldRecordProperty(string propertyName)
        {
            if (_ignoredProperties.Contains(propertyName)) return false;

            var prop = GetType().GetProperty(propertyName);
            if (prop == null) return false;

            // Check for IgnoreRecording attribute
            if (prop.GetCustomAttribute<IgnoreRecordingAttribute>() != null)
            {
                return false;
            }

            // If RecordAllProperties is false, only record properties with RecordProperty attribute
            if (!_recordAllProperties)
            {
                return prop.GetCustomAttribute<RecordPropertyAttribute>() != null;
            }

            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises PropertyChanged for multiple properties
        /// </summary>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                OnPropertyChanged(name);
            }
        }
    }
}
