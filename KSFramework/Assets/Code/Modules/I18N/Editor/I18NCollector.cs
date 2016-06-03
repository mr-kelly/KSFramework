using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSFramework.Editor
{
    public interface I18NCollector
    {
        void Collect(ref I18NItems i18List);
    }
}