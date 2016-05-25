namespace BuildingControl.Services
{
  using System;
  using Windows.UI.Xaml.Controls;

  // This could/should be turned into a proper service and injected but
  // for demo purposes we just make it static here and keep it all very
  // simple (and short!)
  static class Navigator
  {
    public static Frame Frame
    {
      get; set;
    }
    public static void Navigate(Type pageType, object parameter)
    {
      Frame.Navigate(pageType, parameter);
    }
  }
}
