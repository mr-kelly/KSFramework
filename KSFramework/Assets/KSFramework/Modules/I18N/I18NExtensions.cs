using KSFramework;
using TableML;


/// <summary>
/// Extenions for I18NModule
/// </summary>
public static class I18NExtensions
{

    /// <summary>
    /// Extensions for I18N parsing Settings
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static I18N Get_I18N(this TableFileRow tableRow, string value, string defaultValue)
    {
        var str = tableRow.Get_string(value, defaultValue);
        return new I18N(str);

    }
    /// <summary>
    /// Extension for String
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToI18N(this string str)
    {
        return I18NModule.Str(str);
    }
}

public class I18N : KSFramework.I18N
{
    internal I18N(string translated, string origin) : base(translated, origin)
    {
    }

    internal I18N(string origin) : base(origin)
    {
    }
}
