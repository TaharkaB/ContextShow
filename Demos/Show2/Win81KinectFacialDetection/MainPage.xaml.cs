namespace Win81KinectFacialDetection
{
  using Microsoft.Graphics.Canvas;
  using Microsoft.Graphics.Canvas.UI.Xaml;
  using System.Collections.Generic;
  using Windows.UI.Xaml.Controls;
  using WindowsPreview.Kinect;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();

      this.renderList = new List<IRenderKinectFrames>()
      {
        new ColorFrameSourceRenderer(),
        new FaceFrameSourceRenderer()
      };
    }
    void Initialise()
    {
      if (!this.initialised)
      {
        this.sensor = KinectSensor.GetDefault();
        this.sensor.Open();

        foreach (var renderer in this.renderList)
        {
          renderer.Initialise(this.sensor);
        }

        this.initialised = true;
      }
    }
    void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
      this.Initialise();

      using (CanvasDrawingSession drawSession = args.DrawingSession)
      {
        foreach (var renderer in this.renderList)
        {
          renderer.Update(sender.Device);
          renderer.Render(
            drawSession, 
            this.faceControl,
            (int)sender.ActualWidth, (int)sender.ActualHeight);
        }
      }
      sender.Invalidate();
    }
    void OnCreateResources(CanvasControl sender, object args)
    {
      foreach (var renderer in renderList)
      {
        renderer.CreateResources(sender);
      }
    }
    List<IRenderKinectFrames> renderList;
    KinectSensor sensor;
    bool initialised;
  }
}
