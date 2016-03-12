namespace Win81KinectFacialDetection
{
  using Microsoft.Graphics.Canvas;
  using Microsoft.Graphics.Canvas.DirectX;
  using Microsoft.Graphics.Canvas.UI.Xaml;
  using System;
  using Windows.Foundation;
  using WindowsPreview.Kinect;

  class ColorFrameSourceRenderer : IRenderKinectFrames
  {
    public void Initialise(KinectSensor sensor)
    {
      this.sensor = sensor;

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
    public void Render(CanvasDrawingSession session, Face f, 
      int width, int height)
    {
      if (this.canvasBitmap != null)
      {
        session.DrawImage(this.canvasBitmap,
          new Rect(0, 0, width, height),
          new Rect(0, 0, this.colorFrameDescription.Width, this.colorFrameDescription.Height));
      }
    }
    public void CreateResources(CanvasControl control)
    {

    }
    KinectSensor sensor;
    FrameDescription colorFrameDescription;
    CanvasBitmap canvasBitmap;
    TimeSpan previousColorFrameTimeSpan;
    byte[] colorBytes;
    ColorFrameReader colorReader;
  }
}