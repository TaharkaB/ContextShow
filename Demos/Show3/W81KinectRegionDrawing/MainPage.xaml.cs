namespace W81KinectRegionDrawing
{
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media;
  using Windows.UI.Xaml.Shapes;
  using WindowsPreview.Kinect.Input;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
      var window = KinectCoreWindow.GetForCurrentThread();
      window.PointerMoved += OnPointerMoved;
    }

    void OnPointerMoved(KinectCoreWindow sender, KinectPointerEventArgs args)
    {
      if (args.CurrentPoint.Properties.HandType == HandType.RIGHT)
      {
        if (this.drawCanvas.IsGripping)
        {
          if (this.lastPoint.HasValue)
          {
            Line line = new Line()
            {
              X1 = this.lastPoint.Value.X * this.drawCanvas.ActualWidth,
              Y1 = this.lastPoint.Value.Y * this.drawCanvas.ActualHeight,
              X2 = args.CurrentPoint.Position.X * this.drawCanvas.ActualWidth,
              Y2 = args.CurrentPoint.Position.Y * this.drawCanvas.ActualHeight,
              Stroke = this.brush,
              StrokeThickness = 10
            };
            this.drawCanvas.Children.Add(line);
          }
          this.lastPoint = args.CurrentPoint.Position;
        }
        else
        {
          this.lastPoint = null;
        }
      }
    }
    void OnRed(object sender, RoutedEventArgs e)
    {
      byte newValue =
        this.brushColour.R == 0 ? (byte)0xFF : (byte)0x00;

      this.brushColour = Color.FromArgb(0xFF, newValue,
        this.brushColour.G, this.brushColour.B);

      ((Button)sender).Opacity = newValue == 0 ? 0.5f : 1.0f;

      this.RecreateBrush();
    }
    void OnGreen(object sender, RoutedEventArgs e)
    {
      byte newValue =
        this.brushColour.G == 0 ? (byte)0xFF : (byte)0x00;

      this.brushColour = Color.FromArgb(0xFF, this.brushColour.R,
        newValue, this.brushColour.B);

      ((Button)sender).Opacity = newValue == 0 ? 0.5f : 1.0f;

      this.RecreateBrush();
    }
    void OnBlue(object sender, RoutedEventArgs e)
    {
      byte newValue =
        this.brushColour.B == 0 ? (byte)0xFF : (byte)0x00;

      this.brushColour = Color.FromArgb(0xFF, this.brushColour.R,
        this.brushColour.G, newValue);

      ((Button)sender).Opacity = newValue == 0 ? 0.5f : 1.0f;

      this.RecreateBrush();
    }
    void OnClear(object sender, RoutedEventArgs e)
    {
      this.drawCanvas.Children.Clear();
    }
    void RecreateBrush()
    {
      this.brush = new SolidColorBrush(this.brushColour);
    }
    SolidColorBrush brush;
    Color brushColour;
    Point? lastPoint;
  }
}
