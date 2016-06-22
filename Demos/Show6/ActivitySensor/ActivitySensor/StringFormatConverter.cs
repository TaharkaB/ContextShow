namespace ActivitySensorCode
{
  using System;
  using Windows.UI.Xaml.Data;

  class DateTimeOffsetFormatter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      var format = (string)parameter;
      return (((DateTimeOffset)value).ToString(format));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
