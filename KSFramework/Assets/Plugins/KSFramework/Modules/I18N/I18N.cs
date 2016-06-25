using UnityEngine;
using System.Collections;

namespace KSFramework
{
    /// <summary>
    /// I18NModule的Alias别名
    /// </summary>
    public class I18N
    {
        public string Translated { get; private set; }
        public string Origin { get; private set; }

        internal I18N(string translated, string origin)
        {
            Translated = translated;
            Origin = origin;
        }

        internal I18N(string origin)
        {
            Origin = origin;
        }

        public override string ToString()
        {
            return Translated;
        }

        /// <summary>
        /// return translated string
        /// </summary>
        /// <param name="i18n"></param>
        public static implicit operator string(I18N i18n)
        {
            return i18n.Translated;
        }

        /// <summary>
        /// alias of I18NModule.Str
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Str(string str)
        {
            return I18NModule.Str(str);
        }

    }

}