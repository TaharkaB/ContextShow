namespace HelloDirectInk
{
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    void OnToggleMode(object sender, RoutedEventArgs e)
    {
      if (this.inkPresenter.InputProcessingConfiguration.Mode ==
        InkInputProcessingMode.Inking)
      {
        this.inkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
        this.btnErase.Visibility = Visibility.Visible;
        this.btnDraw.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.inkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
        this.btnErase.Visibility = Visibility.Collapsed;
        this.btnDraw.Visibility = Visibility.Visible;
      }
    }
    void OnClearStrokes(object sender, RoutedEventArgs e)
    {
      this.inkStrokeContainer.Clear();
      this.UpdateStrokesView();
    }
  }
}
