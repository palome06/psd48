using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PSD.ClientAo
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolVisibleConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            bool isValid = (bool)value;
            return isValid ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            Visibility? visibility = value as Visibility?;
            if (visibility == Visibility.Visible)
                return true;
            else
                return false;
        }
    }

    [ValueConversion(typeof(int), typeof(SolidColorBrush))]
    public class TeamColorConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            int team = (int) value;
            if (team == 1)
                return new SolidColorBrush(Colors.Red);
            else if (team == 2)
                return new SolidColorBrush(Colors.Blue);
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            SolidColorBrush color = value as SolidColorBrush;
            if (color != null)
            {
                if (color.Color == Colors.Red)
                    return 1;
                else if (color.Color == Colors.Blue)
                    return 2;
                else
                    return 0;
            }
            else
                return 0;
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringVisibleConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            string text = (string)value;
            return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            Visibility? visibility = value as Visibility?;
            if (visibility == Visibility.Visible)
                return "0";
            else
                return "";
        }
    }

    [ValueConversion(typeof(double), typeof(Thickness))]
    public class DoubleThicknessConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            double db = (double)value;
            return new Thickness(db);
        }

        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            Thickness tn = (Thickness)value;
            return tn != null ? tn.Bottom : 0;
        }
    }

    [ValueConversion(typeof(Base.Skill), typeof(Visibility))]
    public class SkillVisibleConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            Base.Skill sk = (Base.Skill)value;
            return sk == null ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Base.Skill), typeof(string))]
    public class SkillNameConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            Base.Skill sk = (Base.Skill)value;
            return sk == null ? "" : sk.Name;
        }
        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Base.Skill), typeof(System.Windows.Controls.ToolTip))]
    public class SkillToolTopConvert : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            Base.Skill sk = (Base.Skill)value;
            return sk == null ? null : Tips.IchiDisplay.GetSkillTip(sk);
        }
        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return null;
        }
    }
}
