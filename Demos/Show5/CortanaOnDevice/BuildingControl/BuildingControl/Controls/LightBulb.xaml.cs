namespace BuildingControl.Controls
{
  using System;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  public sealed partial class LightBulb : UserControl
  {
    public LightBulb()
    {
      this.InitializeComponent();
    }
    public DependencyProperty IsOnProperty = DependencyProperty.Register(
      "IsOn", typeof(bool), typeof(LightBulb), new PropertyMetadata(null, OnIsOnChanged));

    public bool IsOn
    {
      get
      {
        return ((bool)base.GetValue(IsOnProperty));
      }
      set
      {
        base.SetValue(IsOnProperty, value);
      }
    }
    static void OnIsOnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
      LightBulb bulb = (LightBulb)sender;

      Visibility visibility = Visibility.Visible;

      if (!(bool)args.NewValue)
      {
        visibility = Visibility.Collapsed;
      }
      bulb.imgLit.Visibility = visibility;
    }
  }
}
