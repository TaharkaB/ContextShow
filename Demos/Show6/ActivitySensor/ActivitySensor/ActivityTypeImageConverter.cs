namespace ActivitySensorCode
{
  using System;
  using Windows.Devices.Sensors;
  using Windows.UI.Xaml.Data;
  using Windows.UI.Xaml.Media.Imaging;

  class ActivityTypeImageConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      var activityType = (ActivityType)value;
     
      var bitmapImage = new BitmapImage(
        new Uri($"ms-appx:///Assets/{activityType}.png"));

      return (bitmapImage);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
