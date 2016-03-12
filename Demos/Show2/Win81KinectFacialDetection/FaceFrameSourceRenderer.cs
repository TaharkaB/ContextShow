namespace Win81KinectFacialDetection
{
  using Microsoft.Graphics.Canvas;
  using Microsoft.Graphics.Canvas.UI.Xaml;
  using Microsoft.Kinect.Face;
  using System.Linq;
  using System.Numerics;
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Xaml;
  using WindowsPreview.Kinect;

  class FaceFrameSourceRenderer : IRenderKinectFrames
  {
    public void Initialise(KinectSensor sensor)
    {
      this.sensor = sensor;
      this.bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
      this.bodyReader = this.sensor.BodyFrameSource.OpenReader();
      this.currentTrackedBodyId = ulong.MaxValue;
    }
    public void Update(ICanvasResourceCreator resourceCreator)
    {
      using (BodyFrame bodyFrame = this.bodyReader.AcquireLatestFrame())
      {
        if (bodyFrame != null)
        {
          bodyFrame.GetAndRefreshBodyData(this.bodies);

          var firstBody = this.bodies.FirstOrDefault(b => b.IsTracked);

          if (firstBody != null)
          {
            if (firstBody.TrackingId != this.currentTrackedBodyId)
            {
              this.LostBody();

              this.currentTrackedBodyId = firstBody.TrackingId;

              this.currentFaceFrameSource = new FaceFrameSource(
                this.sensor, firstBody.TrackingId, FACE_FEATURES);

              this.currentFaceFrameReader =
                this.currentFaceFrameSource.OpenReader();
            }
            using (var faceFrame = this.currentFaceFrameReader.AcquireLatestFrame())
            {
              if (faceFrame != null)
              {
                this.currentFaceFrameResult = faceFrame.FaceFrameResult;
              }
            }
          }
        }
        else
        {
          this.LostBody();
        }
      }
    }
    public void Render(
      CanvasDrawingSession session, 
      Face f,
      int width, 
      int height)
    {
      VisualStateManager.GoToState(f, "_default", false);

      // Have we picked up data on our last frame?
      if (this.currentFaceFrameResult != null)
      {
        // If we are seeing the bounding box of the face, draw it.
        if (this.currentFaceFrameResult.FaceFrameFeatures.HasFlag(
          FaceFrameFeatures.BoundingBoxInColorSpace))
        {
          var faceBoxInVideoSpace = 
            this.currentFaceFrameResult.FaceBoundingBoxInColorSpace.ToRect();

          var faceBoxInDrawSpace =
            this.ScaleColorSpaceRectToDisplayRect(faceBoxInVideoSpace,
              width, height);

          session.DrawRectangle(faceBoxInDrawSpace, Colors.Red, 3.0f);
        }
        // If we are seeing the features of the face, draw them
        // these are eyes, nose, mouth.
        if (this.currentFaceFrameResult.FaceFrameFeatures.HasFlag(
          FaceFrameFeatures.PointsInColorSpace))
        {
          foreach (var faceFeature in 
            this.currentFaceFrameResult.FacePointsInColorSpace.Values)
          {
            session.FillCircle(
              new Vector2(
                 (float)this.ScaleColorSpaceXToDisplayX(faceFeature.X, width)  - DIAMETER / 2.0f, 
                 (float)this.ScaleColorSpaceYToDisplayY(faceFeature.Y, height) - DIAMETER / 2.0f),
              DIAMETER,
              Colors.Yellow);
          }
        }
        foreach (var faceProperty in this.currentFaceFrameResult.FaceProperties)
        {
          if (faceProperty.Value == DetectionResult.Yes)
          {
            VisualStateManager.GoToState(f,
              faceProperty.Key.ToString(), false);
          }
        }
      }
    }
    Rect ScaleColorSpaceRectToDisplayRect(Rect colorSpaceRect,
      int width, int height)
    {
      var colorHeight = this.sensor.ColorFrameSource.FrameDescription.Height;

      Rect r = new Rect(
        this.ScaleColorSpaceXToDisplayX(colorSpaceRect.Left, width),
        this.ScaleColorSpaceYToDisplayY(colorSpaceRect.Top, height),
        this.ScaleColorSpaceXToDisplayX(colorSpaceRect.Width, width),
        this.ScaleColorSpaceYToDisplayY(colorSpaceRect.Height, height)
      );
      return (r);
    }
    double ScaleColorSpaceXToDisplayX(double colorSpaceX, int displayWidth)
    {
      var colorWidth = this.sensor.ColorFrameSource.FrameDescription.Width;
      return ((colorSpaceX / colorWidth) * displayWidth);
    }
    double ScaleColorSpaceYToDisplayY(double colorSpaceY, int displayHeight)
    {
      var colorHeight = this.sensor.ColorFrameSource.FrameDescription.Height;
      return ((colorSpaceY / colorHeight) * displayHeight);
    }
    void LostBody()
    {
      this.currentTrackedBodyId = null;
      this.currentFaceFrameReader?.Dispose();
      this.currentFaceFrameReader = null;
      this.currentFaceFrameSource = null;
    }
    public void CreateResources(CanvasControl canvasControl)
    {

    }
    ulong? currentTrackedBodyId;
    FaceFrameReader currentFaceFrameReader;
    FaceFrameSource currentFaceFrameSource;
    FaceFrameResult currentFaceFrameResult;
    BodyFrameReader bodyReader;
    Body[] bodies;
    KinectSensor sensor;

    static readonly FaceFrameFeatures FACE_FEATURES =
      FaceFrameFeatures.BoundingBoxInColorSpace |
      FaceFrameFeatures.PointsInColorSpace |
      FaceFrameFeatures.FaceEngagement |
      FaceFrameFeatures.Glasses |
      FaceFrameFeatures.Happy |
      FaceFrameFeatures.LeftEyeClosed |
      FaceFrameFeatures.LookingAway |
      FaceFrameFeatures.MouthMoved |
      FaceFrameFeatures.MouthOpen |
      FaceFrameFeatures.RightEyeClosed |
      FaceFrameFeatures.RotationOrientation;

    const float DIAMETER = 5.0f;
  }
}