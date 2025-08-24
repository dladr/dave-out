using System;

/// <summary>
/// Extension methods for type conversion and enum parsing.
/// </summary>
public static class ConversionExtensions
{
    /// <summary>
    /// Takes a string value and converts it into an Enum value.
    /// Throws an exception if value is not an enum value of T
    /// </summary>
    /// <typeparam name="T">The Enum</typeparam>
    /// <param name="value">Input string value</param>
    /// <returns></returns>
    public static T ToEnum<T>(this string value) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    /// <summary>
    /// A simplified way to convert an object to another type
    /// Roughly the same as Int.Parse and similar methods, but a bit more robust and with less code.
    /// Throws exception if value cannot be cast, but 'returnDefaultOnFail' allows the method to instead
    /// return T's default value instead.
    /// </summary>
    /// <typeparam name="T">Type to convert value to</typeparam>
    /// <param name="value">The value to be converted</param>
    /// <param name="returnDefaultOnFail">If exception, silently return default(T) or null</param>
    /// <returns></returns>
    public static T ConvertTo<T>(this object value, bool returnDefaultOnFail = false)
    {
        try
        {
            var returnVal = (T)Convert.ChangeType(value, typeof(T));
            return returnVal;
        }
        catch (Exception ex)
        {
            if (returnDefaultOnFail) 
                return default;

            if (string.IsNullOrWhiteSpace(value?.ToString()))
                value = "null";
            throw new Exception($"[{value}] cannot be converted to <{typeof(T)}>.", ex);
        }
    }
}
