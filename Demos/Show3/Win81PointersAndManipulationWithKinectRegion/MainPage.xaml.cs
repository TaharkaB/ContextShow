namespace Win81PointersAndManipulation
{
  using System;
  using System.Collections.Generic;
  using Windows.Foundation;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media.Imaging;
  using Windows.UI.Xaml.Shapes;
  using WindowsPreview.Kinect.Input;
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();

      this.pointerPositions = new Dictionary<uint, Point>();

      this.brushManager = new BrushManager();

      this.AddEventHandlers();

      this.drawCanvas.IsEnabled = true;
    }
    void AddEventHandlers()
    {
      // NB: these are POINTER events, not MOUSE events, not PEN events, etc.
      this.drawCanvas.PointerPressed += (s, e) =>
      {
        this.TrackDown(
          e.Pointer.PointerId,
          e.Pointer.PointerDeviceType.ToString(),
          e.GetCurrentPoint(this.drawCanvas).Position);
      };
      this.drawCanvas.PointerReleased += (s, e) =>
      {
        this.TrackUp(e.Pointer.PointerId);
      };
      this.drawCanvas.PointerMoved += (s, e) =>
      {
        this.TrackMove(
          e.Pointer.PointerId,
          e.GetCurrentPoint(this.drawCanvas).Position,
          ((int)e.Pointer.PointerDeviceType + 1) * 3);
      };
      this.drawCanvas.PointerExited += (s, e) =>
      {
        this.TrackUp(e.Pointer.PointerId);
      };

      var window = KinectCoreWindow.GetForCurrentThread();
      window.PointerMoved += (s, e) =>
      {
        if (this.drawCanvas.IsGripping)
        {
          var canvasPosition = new Point()
          {
            X = e.CurrentPoint.Position.X * this.drawCanvas.ActualWidth,
            Y = e.CurrentPoint.Position.Y * this.drawCanvas.ActualHeight
          };
          this.TrackDown(
            e.CurrentPoint.PointerId, 
            e.CurrentPoint.PointerDevice.PointerDeviceType.ToString(),
            canvasPosition);

          this.TrackMove(
            e.CurrentPoint.PointerId,
            canvasPosition,
            ((int)e.CurrentPoint.PointerDevice.PointerDeviceType) * 3);
        }
        else
        {
          this.TrackUp(e.CurrentPoint.PointerId);
        }
      };
      window.PointerExited += (s, e) =>
      {
        this.TrackUp(e.CurrentPoint.PointerId);
      };
    }
    void TrackDown(uint pointerId, string deviceType, Point position)
    {
      if (this.pointerPositions.Count == 0)
      {
        this.imgInput.Source =
          new BitmapImage(
            new Uri($"ms-appx:///Assets/{deviceType}.png"));
      }
      if (!this.pointerPositions.ContainsKey(pointerId))
      {
        this.pointerPositions[pointerId] = position;
      }
    }
    void TrackUp(uint pointerId)
    {
      if (this.pointerPositions.ContainsKey(pointerId))
      {
        this.pointerPositions.Remove(pointerId);
      }
      if (this.pointerPositions.Count == 0)
      {
        this.imgInput.Source = null;
      }
    }
    void TrackMove(uint pointerId, Point newPoint, int thickness)
    {
      Point oldPoint;

      if (this.pointerPositions.TryGetValue(pointerId, out oldPoint))
      {
        this.drawCanvas.Children.Add(
          new Line()
          {
            X1 = oldPoint.X,
            Y1 = oldPoint.Y,
            X2 = newPoint.X,
            Y2 = newPoint.Y,
            StrokeThickness = thickness,
            Stroke = this.brushManager.DrawBrush
          }
        );
        this.pointerPositions[pointerId] = newPoint;
      }
    }
    void OnClear(object sender, RoutedEventArgs e)
    {
      this.drawCanvas.Children.Clear();
      this.drawCanvas.IsEnabled = true;
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
      this.drawCanvas.Children.Add(control);
      this.drawCanvas.IsEnabled = false;
    }
    void OnRed(object sender, RoutedEventArgs e)
    {
      ((Button)sender).Opacity =
        this.brushManager.ToggleRed() ? 1.0 : 0.5;
    }
    void OnGreen(object sender, RoutedEventArgs e)
    {
      ((Button)sender).Opacity =
        this.brushManager.ToggleGreen() ? 1.0 : 0.5;
    }
    void OnBlue(object sender, RoutedEventArgs e)
    {
      ((Button)sender).Opacity =
        this.brushManager.ToggleBlue() ? 1.0 : 0.5;
    }
    BrushManager brushManager;
    Dictionary<uint, Point> pointerPositions;
  }
}
