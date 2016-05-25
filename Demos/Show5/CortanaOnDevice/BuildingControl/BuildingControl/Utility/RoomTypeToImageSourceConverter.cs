using BuildingControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BuildingControl.Utility
{
  class RoomTypeToImageSourceConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if ((!(value is RoomType)) || (targetType != typeof(ImageSource)))
      {
        throw new InvalidOperationException();
      }
      var name = value.ToString();

      return (new BitmapImage(new Uri($"ms-appx:///Assets/{name}.png")));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
