using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace LightMusic.Controls {
    public class DiskplayToVisibility : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, string language) {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType,
            object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class DiskplayToRow : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, string language) {
            return (bool)value ? 1 : 0;
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType,
            object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
