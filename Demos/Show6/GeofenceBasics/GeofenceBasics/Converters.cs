using System;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace GeofenceBasics
{
  public class DateFormatter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      return (
        ((DateTimeOffset)value).ToString("d MMM, HH:mm:ss"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
  public class FenceStatusToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      return (
        ((GeofenceState)value) == GeofenceState.Entered ? Colors.Green : Colors.Red);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
  public class InverseBoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      return ((bool)value ? Visibility.Collapsed : Visibility.Visible);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      return ((bool)value ? Visibility.Visible : Visibility.Collapsed);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
