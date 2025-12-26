using System.Reflection;
using WpfEventRecorder.Core.Attributes;

namespace WpfEventRecorder.Core.Helpers;

/// <summary>
/// Helper methods for type reflection and attribute handling.
/// </summary>
public static class TypeHelper
{
    /// <summary>
    /// Checks if a type has the RecordViewModel attribute.
    /// </summary>
    public static bool HasRecordViewModelAttribute(Type type)
    {
        return type.GetCustomAttribute<RecordViewModelAttribute>() != null;
    }

    /// <summary>
    /// Gets the RecordViewModel attribute from a type.
    /// </summary>
    public static RecordViewModelAttribute? GetRecordViewModelAttribute(Type type)
    {
        return type.GetCustomAttribute<RecordViewModelAttribute>();
    }

    /// <summary>
    /// Checks if a property has the RecordProperty attribute.
    /// </summary>
    public static bool HasRecordPropertyAttribute(PropertyInfo property)
    {
        return property.GetCustomAttribute<RecordPropertyAttribute>() != null;
    }

    /// <summary>
    /// Gets the RecordProperty attribute from a property.
    /// </summary>
    public static RecordPropertyAttribute? GetRecordPropertyAttribute(PropertyInfo property)
    {
        return property.GetCustomAttribute<RecordPropertyAttribute>();
    }

    /// <summary>
    /// Checks if a member has the IgnoreRecording attribute.
    /// </summary>
    public static bool HasIgnoreRecordingAttribute(MemberInfo member)
    {
        return member.GetCustomAttribute<IgnoreRecordingAttribute>() != null;
    }

    /// <summary>
    /// Gets all properties with the RecordProperty attribute.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetRecordedProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => HasRecordPropertyAttribute(p) && !HasIgnoreRecordingAttribute(p));
    }

    /// <summary>
    /// Gets the display name for a type or property.
    /// </summary>
    public static string GetDisplayName(MemberInfo member)
    {
        if (member is Type type)
        {
            var vmAttr = GetRecordViewModelAttribute(type);
            return vmAttr?.DisplayName ?? type.Name;
        }

        if (member is PropertyInfo property)
        {
            var propAttr = GetRecordPropertyAttribute(property);
            return propAttr?.DisplayName ?? property.Name;
        }

        return member.Name;
    }

    /// <summary>
    /// Checks if a property should be masked as sensitive.
    /// </summary>
    public static bool IsSensitiveProperty(PropertyInfo property)
    {
        var attr = GetRecordPropertyAttribute(property);
        return attr?.IsSensitive == true;
    }

    /// <summary>
    /// Gets the mask to use for a sensitive property.
    /// </summary>
    public static string GetSensitiveMask(PropertyInfo property)
    {
        var attr = GetRecordPropertyAttribute(property);
        return attr?.SensitiveMask ?? "***";
    }

    /// <summary>
    /// Checks if a type is a ViewModel (ends with ViewModel or has the attribute).
    /// </summary>
    public static bool IsViewModel(Type type)
    {
        if (HasRecordViewModelAttribute(type))
            return true;

        return type.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all ViewModel types in an assembly.
    /// </summary>
    public static IEnumerable<Type> GetViewModelTypes(Assembly assembly)
    {
        return assembly.GetTypes().Where(IsViewModel);
    }

    /// <summary>
    /// Safely gets a property value, returning null on error.
    /// </summary>
    public static object? SafeGetPropertyValue(object obj, string propertyName)
    {
        try
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a dictionary of property values for an object.
    /// </summary>
    public static Dictionary<string, object?> GetPropertyValues(object obj, bool includeNulls = false)
    {
        var result = new Dictionary<string, object?>();
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (HasIgnoreRecordingAttribute(property))
                continue;

            try
            {
                var value = property.GetValue(obj);

                if (IsSensitiveProperty(property))
                {
                    value = GetSensitiveMask(property);
                }

                if (includeNulls || value != null)
                {
                    result[property.Name] = value;
                }
            }
            catch
            {
                // Skip properties that throw
            }
        }

        return result;
    }
}
