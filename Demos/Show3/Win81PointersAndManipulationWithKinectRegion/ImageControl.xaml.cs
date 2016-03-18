namespace Win81PointersAndManipulation
{
  using System;
  using Microsoft.Kinect.Toolkit.Input;
  using Microsoft.Kinect.Xaml.Controls;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  using WindowsPreview.Kinect.Input;
  using Windows.UI.Xaml;
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
      // this is 'something of a hack' right now in terms of using this
      // size here.
      var size = Window.Current.Bounds;

      this.transform.ScaleX *= args.Delta.Scale;
      this.transform.ScaleY *= args.Delta.Scale;
      this.transform.TranslateX += args.Delta.Translation.X * size.Width;
      this.transform.TranslateY += args.Delta.Translation.Y * size.Height;
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