namespace W81KinectRegionDrawing
{
  using Microsoft.Graphics.Canvas;
  using Microsoft.Graphics.Canvas.DirectX;
  using Microsoft.Graphics.Canvas.UI.Xaml;
  using System;
  using Windows.UI.Xaml.Controls;
  using WindowsPreview.Kinect;
  public sealed partial class KinectVideoControl : UserControl
  {
    public KinectVideoControl()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }

    void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      this.sensor = KinectSensor.GetDefault();
      this.sensor.Open();

      this.colorReader = this.sensor.ColorFrameSource.OpenReader();

      this.colorFrameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(
        ColorImageFormat.Bgra);

      this.colorBytes = new byte[
        this.colorFrameDescription.BytesPerPixel *
        this.colorFrameDescription.LengthInPixels];

      this.previousColorFrameTimeSpan = TimeSpan.FromSeconds(0);
    }
    public void Update(ICanvasResourceCreator resourceCreator)
    {
      using (ColorFrame colorFrame = this.colorReader.AcquireLatestFrame())
      {
        if ((colorFrame != null) &&
          (colorFrame.RelativeTime != this.previousColorFrameTimeSpan))
        {
          colorFrame.CopyConvertedFrameDataToArray(this.colorBytes, ColorImageFormat.Bgra);

          if (this.canvasBitmap != null)
          {
            this.canvasBitmap.Dispose();
          }
          this.canvasBitmap = CanvasBitmap.CreateFromBytes(
            resourceCreator,
            this.colorBytes,
            this.colorFrameDescription.Width,
            this.colorFrameDescription.Height,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            96.0f,
            CanvasAlphaMode.Premultiplied);

          this.previousColorFrameTimeSpan = colorFrame.RelativeTime;
        }
      }
    }
    public void Render(CanvasDrawingSession session)
    {
      if (this.canvasBitmap != null)
      {
        session.DrawImage(this.canvasBitmap);
      }
    }
    void canvasControl_Draw(
      CanvasControl sender, 
      CanvasDrawEventArgs args)
    {
      using (CanvasDrawingSession drawSession = args.DrawingSession)
      {
        this.Update(drawSession.Device);
        this.Render(drawSession);
      }
      sender.Invalidate();
    }
    KinectSensor sensor;
    FrameDescription colorFrameDescription;
    CanvasBitmap canvasBitmap;
    TimeSpan previousColorFrameTimeSpan;
    byte[] colorBytes;
    ColorFrameReader colorReader;
  }
}
