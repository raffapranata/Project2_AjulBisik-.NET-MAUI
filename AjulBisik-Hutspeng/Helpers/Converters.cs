using System;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.Helpers
{
    public class NextButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int position)
            {
                return position < 2 ? "Selanjutnya" : "Mulai";
            }
            return "Selanjutnya";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            if (parameter is string invert && invert.ToLower() == "invert")
                return isNull;
            return !isNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            return "ic_account.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }
    }

    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHidden)
            {
                return isHidden ? "ic_eye_closed.png" : "ic_eye_open.png";
            }
            return "ic_eye_closed.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PrimarySoftToGrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSettingsTab = (bool)value;
            string tab = parameter as string;

            if (tab == "text")
            {
                return Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
            }

            bool isActive = (tab == "settings" && isSettingsTab) || (tab == "security" && !isSettingsTab);

            if (isActive)
            {
                return Application.Current.Resources.TryGetValue("PrimarySoft", out var color) ? color : Application.Current.Resources["Primary"];
            }

            return Application.Current.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#333333") : Color.FromArgb("#E0E0E0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
