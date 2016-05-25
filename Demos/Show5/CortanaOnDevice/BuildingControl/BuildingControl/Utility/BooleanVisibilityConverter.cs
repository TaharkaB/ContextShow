namespace BuildingControl.Utility
{

  using System;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Data;
  public class VisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if ((!(value is bool)) || (targetType != typeof(Visibility)))
      {
        throw new InvalidOperationException();
      }
      return ((bool)value ? Visibility.Visible : Visibility.Collapsed);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
