using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace WinClient
{
    public class LoginStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                bool loggedIn = (bool) value;
                if (loggedIn)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Colors.DarkRed);
                }
            }

            return new SolidColorBrush(Colors.DarkRed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                SolidColorBrush currentColor = (SolidColorBrush) value;

                return (currentColor.Color == Colors.Green) ;
            }

            return false;
        }
    }
}
