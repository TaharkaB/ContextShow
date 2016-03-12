namespace Win81KinectHandTracking
{
  using Windows.UI;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media;
  using WindowsPreview.Kinect;
  using System.Linq;
  using System;
  using Windows.UI.Xaml.Shapes;
  using Windows.Foundation;
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
      this.brushes = new SolidColorBrush[]
      {
        new SolidColorBrush(Colors.Red),
        new SolidColorBrush(Colors.Green),
        new SolidColorBrush(Colors.Blue)
      };
      this.brushIndex = 0;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.sensor = KinectSensor.GetDefault();
      this.sensor.Open();

      this.bodyFrameSource = this.sensor.BodyFrameSource;

      this.bodies = new Body[this.bodyFrameSource.BodyCount];

      this.bodyReader = this.bodyFrameSource.OpenReader();
      this.bodyReader.FrameArrived += OnBodyFrameArrived;         
    }
    void OnBodyFrameArrived(BodyFrameReader sender, BodyFrameArrivedEventArgs args)
    {
      bool drawnHands = false;

      using (var frame = args.FrameReference?.AcquireFrame())
      {
        // Get an array of all the bodies (up to 6) that we can see.
        frame.GetAndRefreshBodyData(this.bodies);

        // We're going to ignore all but the first one.
        var firstBody = this.bodies.FirstOrDefault(b => b.IsTracked);

        if (firstBody != null)
        {
          // We have 25 joints to play with including 3 for each hand
          // (wrist, tip, center). We'll draw a circle around the tip
          // to keep it simple.
          var leftHandTip = firstBody.Joints[JointType.HandTipLeft];
          var rightHandTip = firstBody.Joints[JointType.HandTipRight];

          // do we *really* know where these are?
          if (JointsAreTracked(leftHandTip, rightHandTip))
          {
            drawnHands = true;
            this.MoveHandToPosition(this.leftHandEllipse, leftHandTip.Position);
            this.MoveHandToPosition(this.rightHandEllipse, rightHandTip.Position);

            this.UpdateForLeftHandState(firstBody.HandLeftState);
            this.DrawWithRightHand(firstBody.HandRightState, rightHandTip.Position);
          }
        }
      }
      this.ShowHands(drawnHands);
    }
    void DrawWithRightHand(HandState newHandState, CameraSpacePoint rightHandPosition)
    {
      var displayPoint = this.ScaleCameraPointToDisplayPoint(rightHandPosition);

      if (newHandState == HandState.Closed)
      {
        if (this.lastDrawPoint.HasValue)
        {
          Line line = new Line()
          {
            X1 = this.lastDrawPoint.Value.X,
            Y1 = this.lastDrawPoint.Value.Y,
            X2 = displayPoint.X,
            Y2 = displayPoint.Y,
            Stroke = this.brushes[this.brushIndex],
            StrokeThickness = 25
          };
          this.drawCanvas.Children.Add(line);
        }
        this.lastDrawPoint = displayPoint;
      }
      else
      {
        this.lastDrawPoint = null;
      }  
    }
    void UpdateForLeftHandState(HandState newHandState)
    {
      if (this.previousLeftHandState.HasValue)
      {
        if ((this.previousLeftHandState != HandState.Lasso) &&
          (newHandState == HandState.Lasso))
        {
          if (++this.brushIndex == this.brushes.Length)
          {
            this.brushIndex = 0;
          }
          this.leftHandEllipse.Fill = this.brushes[this.brushIndex];
        }
        if ((this.previousLeftHandState != HandState.Open) &&
          (newHandState == HandState.Open))
        {
          foreach (var item in
            this.drawCanvas.Children.Where(c => !(c is Ellipse)).ToList())
          {
            this.drawCanvas.Children.Remove(item);
          }
        }
      }
      this.previousLeftHandState = newHandState;
    }
    void MoveHandToPosition(Ellipse ellipse, CameraSpacePoint position)
    {
      var displayPoint = this.ScaleCameraPointToDisplayPoint(position);

      Canvas.SetLeft(ellipse, displayPoint.X - (ellipse.ActualWidth / 2.0));
      Canvas.SetTop(ellipse, displayPoint.Y - (ellipse.ActualHeight / 2.0));
    }
    Point ScaleCameraPointToDisplayPoint(CameraSpacePoint cameraPoint)
    {
      var colorPoint =
        this.sensor.CoordinateMapper.MapCameraPointToColorSpace(cameraPoint);

      var colorWidth =
        this.sensor.ColorFrameSource.FrameDescription.Width;

      var colorHeight =
        this.sensor.ColorFrameSource.FrameDescription.Height;

      var point = new Point()
      {
        X = (colorPoint.X / colorWidth) * this.drawCanvas.ActualWidth,
        Y = (colorPoint.Y / colorHeight) * this.drawCanvas.ActualHeight
      };
      return (point);
    }
    void ShowHands(bool show = true)
    {
      this.leftHandEllipse.Visibility = 
        show ? Visibility.Visible : Visibility.Collapsed;

      this.rightHandEllipse.Visibility = 
        show ? Visibility.Visible : Visibility.Collapsed;

      if (!show)
      {
        this.previousLeftHandState = null;
      }
    }
    static bool JointsAreTracked(params Joint[] joints)
    {
      return (joints.All(j => j.TrackingState == TrackingState.Tracked));
    }
    HandState? previousLeftHandState;
    Body[] bodies;
    BodyFrameReader bodyReader;
    BodyFrameSource bodyFrameSource;
    KinectSensor sensor;
    SolidColorBrush[] brushes;
    Point? lastDrawPoint;
    int brushIndex;
  }
}
