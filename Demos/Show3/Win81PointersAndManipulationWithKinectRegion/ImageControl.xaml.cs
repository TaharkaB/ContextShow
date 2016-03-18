namespace Win81PointersAndManipulation
{
  using System;
  using Microsoft.Kinect.Toolkit.Input;
  using Microsoft.Kinect.Xaml.Controls;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  using WindowsPreview.Kinect.Input;

  public sealed partial class ImageControl : UserControl, IKinectControl
  {
    public ImageControl()
    {
      this.InitializeComponent();

      this.ManipulationMode =
        ManipulationModes.All &
        ~(ManipulationModes.TranslateRailsX | ManipulationModes.TranslateRailsY);

      this.IsEnabled = true;
    }

    public bool IsManipulatable
    {
      get
      {
        return (true);
      }
    }

    public bool IsPressable
    {
      get
      {
        return (false);
      }
    }
    public IKinectController CreateController(
      IInputModel inputModel, 
      KinectRegion kinectRegion)
    {
      inputModel.GestureRecognizer.GestureSettings =
        KinectGestureSettings.ManipulationScale |
        KinectGestureSettings.ManipulationTranslateX |
        KinectGestureSettings.ManipulationTranslateY;

      inputModel.GestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
      return (new ManipulatableController(inputModel));
    }
    void OnManipulationUpdated(
      KinectGestureRecognizer sender, 
      KinectManipulationUpdatedEventArgs args)
    {
      this.transform.ScaleX *= args.Delta.Scale;
      this.transform.ScaleY *= args.Delta.Scale;
      this.transform.TranslateX += args.Delta.Translation.X * 1280;
      this.transform.TranslateY += args.Delta.Translation.Y * 720;
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