namespace Win81PointersAndManipulation
{
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;

  public sealed partial class ImageControl : UserControl
  {
    public ImageControl()
    {
      this.InitializeComponent();

      this.ManipulationMode =
        ManipulationModes.All &
        ~(ManipulationModes.TranslateRailsX | ManipulationModes.TranslateRailsY);
    }

    void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
      this.transform.ScaleX *= e.Delta.Scale;
      this.transform.ScaleY *= e.Delta.Scale;
      this.transform.Rotation += e.Delta.Rotation;
      this.transform.TranslateX += e.Delta.Translation.X;
      this.transform.TranslateY += e.Delta.Translation.Y;
    }
    void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
      e.Handled = true;
    }
  }
}
