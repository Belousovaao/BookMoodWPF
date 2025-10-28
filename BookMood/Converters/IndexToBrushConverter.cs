using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using System.Windows.Media;

namespace BookMood.Converters
{
    public class IndexToBrushConverter : IMultiValueConverter
    {
        private static readonly Random _rand = new Random();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1) return Brushes.Transparent;

            var brushes = (IEnumerable<Brush>)values[0];

            List<Brush> brushList = new List<Brush>(brushes);
            int index = _rand.Next(brushList.Count);
            return brushList[index];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
