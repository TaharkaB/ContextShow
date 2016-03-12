namespace UWPPointersAndManipulation
{
  using System;
  using Windows.UI.Core;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media.Imaging;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs args)
    {
      this.inkCanvas.InkPresenter.InputDeviceTypes =
        CoreInputDeviceTypes.Pen |
        CoreInputDeviceTypes.Mouse |
        CoreInputDeviceTypes.Touch;

      this.inkCanvas.InkPresenter.IsInputEnabled = true;

      this.inkCanvas.InkPresenter.UnprocessedInput.PointerEntered +=
        OnPointerEntered;

      this.inkCanvas.InkPresenter.UnprocessedInput.PointerExited +=
        OnPointerExited;
    }
    void OnPointerEntered(
      InkUnprocessedInput sender,
      PointerEventArgs args)
    {
      var typeOfPointer = args.CurrentPoint.PointerDevice.PointerDeviceType;

      BitmapImage image = new BitmapImage(
       new Uri($"ms-appx:///Assets/{typeOfPointer}.png"));

      this.imgInput.Source = image;
    }
    void OnPointerExited(InkUnprocessedInput sender, PointerEventArgs args)
    {
      this.imgInput.Source = null;
    }
    void OnClear(object sender, RoutedEventArgs e)
    {
      this.inkCanvas.InkPresenter.StrokeContainer.Clear();
      this.canvas.Children.Clear();
      this.inkCanvas.Visibility = Visibility.Visible;
    }
    void OnChoosePaperType(object sender, RoutedEventArgs e)
    {
      MenuFlyoutItem flyoutItem = (MenuFlyoutItem)sender;

      BitmapImage image = new BitmapImage(
        new Uri($"ms-appx:///Assets/{flyoutItem.Text}.png"));

      this.backdrop.Source = image;
    }
    void OnAddPhoto(object sender, RoutedEventArgs e)
    {
      ImageControl control = new ImageControl();
      this.canvas.Children.Add(control);
      this.inkCanvas.Visibility = Visibility.Collapsed;
    }
  }
}
