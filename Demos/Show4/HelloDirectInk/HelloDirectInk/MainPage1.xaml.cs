namespace HelloDirectInk
{
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Core;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      // The InkCanvas puts a stroke container together with an ink presenter
      // and takes a tonne of work out of dealing with ink while also reducing
      // the latency involved in capturing/rendering the ink.
      this.inkPresenter = this.inkCanvas.InkPresenter;
      this.inkStrokeContainer = this.inkPresenter.StrokeContainer;

      // By default, the canvas only handles pen so we are switching on mouse
      // and touch too here.
      this.AddAllInputDevices();

      // Change the default drawing attributes.
      this.SetDefaultInkPresentation();

      // Add handlers to watch as strokes come/go.
      this.AddStrokeEventHandlers();

      // We want to tell the control to ignore strokes which happen with
      // 'the button pressed' and leave them for us to process and we'll
      // use them as the basis for selection.
      this.AddProcessingForRightButtonStrokes();
    }
    void AddAllInputDevices()
    {
      this.inkPresenter.InputDeviceTypes =
        CoreInputDeviceTypes.Mouse |
        CoreInputDeviceTypes.Pen |
        CoreInputDeviceTypes.Touch;
    }
    void SetDefaultInkPresentation()
    {
      var drawingAttrs = this.inkPresenter.CopyDefaultDrawingAttributes();

      drawingAttrs.Color = Colors.Orange;
      drawingAttrs.PenTip = PenTipShape.Rectangle;
      drawingAttrs.Size = new Size(2, 2);
      drawingAttrs.IgnorePressure = false;
      drawingAttrs.FitToCurve = true;

      this.inkPresenter.UpdateDefaultDrawingAttributes(drawingAttrs);
    }
    InkStrokeContainer inkStrokeContainer;
    InkPresenter inkPresenter;
  }
}
