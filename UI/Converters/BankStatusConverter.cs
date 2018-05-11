using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace GameTech.Elite.Client.Modules.SessionSummary.UI.Converters
{
    /// <summary>
    /// Converts a true/false value to an "open"/"closed" string
    /// </summary>
    public class BankStatusConverter : IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "Closed";

            if (value is bool)
            {
                if ((bool)value == false)
                {
                    result = "Open";
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string))
                return false;
            if (String.Equals("open", (string)value, StringComparison.CurrentCultureIgnoreCase))
                return true;
            else
                return false;
        }

        #endregion

    }
}
