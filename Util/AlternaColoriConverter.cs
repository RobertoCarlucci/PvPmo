using System.Globalization;

namespace PvPmo.Util
{
    public class AlternaColoriConverter : IValueConverter
    {
        static int index = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            index++;
            return index % 2 == 0 ? Colors.White : Color.FromArgb("#E0F7FA");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
