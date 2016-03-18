using System;
namespace Demo1_SpeechDictation
{
  using Windows.UI.Xaml.Data;

  public class StringFormatConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      string result = result = string.Format(
        (string)parameter, value);

      return (result);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
