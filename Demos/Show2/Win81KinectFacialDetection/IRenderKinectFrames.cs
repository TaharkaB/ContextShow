namespace Win81KinectFacialDetection
{
  using Microsoft.Graphics.Canvas;
  using Microsoft.Graphics.Canvas.UI.Xaml;
  using WindowsPreview.Kinect;

  interface IRenderKinectFrames
  {
    void Initialise(KinectSensor sensor);
    void CreateResources(CanvasControl canvasControl);
    void Update(ICanvasResourceCreator resourceCreator);
    void Render(CanvasDrawingSession session, Face face);
  }
}
