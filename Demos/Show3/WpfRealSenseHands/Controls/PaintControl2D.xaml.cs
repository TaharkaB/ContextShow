namespace WpfRealSenseHands.Controls
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Shapes;
  using JointPositionMap =
    System.Collections.Generic.Dictionary<PXCMHandData.JointType, PXCMPointF32>;

  public partial class PaintControl2D : UserControl, ISampleProcessor
  {
    enum DisplayMode
    {
      Pictures,
      Highlight
    }
    public PaintControl2D()
    {
      InitializeComponent();

      this.displayMode = DisplayMode.Pictures;

      this.jointPositions = new JointPositionMap();

      // very simple re-use of ellipses. could easily make this so much smarter
      this.ellipseCache = new List<Ellipse>();

      this.ellipseBrush = new SolidColorBrush(
        Color.FromArgb(0x88, 0x00, 0x00, 0x00));

      this.whiteBrush = new SolidColorBrush(Colors.White);
    }
    public void Initialise(PXCMSenseManager senseManager)
    {
      this.senseManager = senseManager;

      this.senseManager.EnableHand().ThrowOnFail();

      using (var handModule = this.senseManager.QueryHand())
      {
        using (var handConfiguration = handModule.CreateActiveConfiguration())
        {
          handConfiguration.EnableGesture("click", false).ThrowOnFail();
          handConfiguration.EnableGesture("wave", false).ThrowOnFail();

          handConfiguration.SubscribeGesture(
            new PXCMHandConfiguration.OnFiredGestureDelegate(
              OnGesture));

          handConfiguration.ApplyChanges().ThrowOnFail();

        }
        this.handData = handModule.CreateOutput();
      }
    }
    void OnGesture(PXCMHandData.GestureData gestureData)
    {
      if ((gestureData.name == "click") && 
        (this.displayMode == DisplayMode.Pictures))
      {
        this.clickWaiting = true;
      }
      else if ((gestureData.name == "wave") && 
        (this.displayMode == DisplayMode.Highlight))
      {
        this.swipeDownWaiting = true;
      }
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      this.jointPositions.Clear();

      if (this.handData.Update().Succeeded())
      {
        foreach (var handId in this.GetHandIdentifiersInFrame())
        {
          if (!this.currentHandId.HasValue ||
            (this.currentHandId.Value == handId))
          {
            this.currentHandId = handId;

            PXCMHandData.IHand handInfo;

            if (this.handData.QueryHandDataById(handId, out handInfo).IsSuccessful())
            {
              PXCMHandData.JointData jointData;

              foreach (var jointType in jointTypes)
              {
                if (handInfo.QueryTrackedJoint(jointType, out jointData).IsSuccessful())
                {
                  var scaledPosition = new PXCMPointF32(
                    (float)jointData.positionImage.x / sample.depth.info.width,
                    (float)jointData.positionImage.y / sample.depth.info.height);

                  this.jointPositions[jointType] = scaledPosition;
                }
              }
            }
          }
        }
      }
      if (this.jointPositions.Count() == 0)
      {
        this.currentHandId = null;
      }
    }
    IEnumerable<int> GetHandIdentifiersInFrame()
    {
      var handCount = this.handData.QueryNumberOfHands();

      for (int i = 0; i < handCount; i++)
      {
        int handId;

        if (this.handData.QueryHandId(
          PXCMHandData.AccessOrderType.ACCESS_ORDER_NEAR_TO_FAR,
          i,
          out handId).Succeeded())
        {
          PXCMHandData.IHand handInfo;

          if (this.handData.QueryHandDataById(
            handId,
            out handInfo).IsSuccessful())
          {
            yield return handId;
          }
        }
      }
    }
    public void DrawUI(PXCMCapture.Sample sample)
    {
      if (this.displayMode == DisplayMode.Pictures)
      {
        int ellipseCount = 0;

        foreach (var joint in this.jointPositions)
        {
          var isTip = joint.Key == PXCMHandData.JointType.JOINT_INDEX_TIP;

          var ellipse = this.MakeOrReuseEllipse(ellipseCount++, isTip);

          var point = new Point(
            (joint.Value.x * this.drawCanvas.ActualWidth),
            (joint.Value.y * this.drawCanvas.ActualHeight));

          Canvas.SetLeft(ellipse, point.X - ellipse.ActualHeight / 2);
          Canvas.SetTop(ellipse, point.Y - ellipse.ActualWidth / 2);

          if (isTip && this.clickWaiting)
          {
            Image hitImage = this.HitTestPageForFirstOfType<Image>(point);

            if (hitImage != null)
            {
              this.rectangleBrush.ImageSource = hitImage.Source;

              this.SwitchDisplayMode();
            }
            this.clickWaiting = false;
          }
        }
        this.FlushEllipses(ellipseCount);

      }
      else if (this.swipeDownWaiting)
      {
        this.SwitchDisplayMode();
        this.swipeDownWaiting = false;
      }
    }
    void SwitchDisplayMode()
    {
      // this should be binding and visual states but isn't yet.
      this.displayMode = this.displayMode == DisplayMode.Highlight ?
        DisplayMode.Pictures : DisplayMode.Highlight;

      if (this.displayMode == DisplayMode.Highlight)
      {
        this.highlightGrid.Visibility = Visibility.Visible;
        this.drawCanvas.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.highlightGrid.Visibility = Visibility.Collapsed;
        this.drawCanvas.Visibility = Visibility.Visible;
      }
    }
    T HitTestPageForFirstOfType<T>(Point point) where T : DependencyObject
    {
      T hitTested = default(T);

      VisualTreeHelper.HitTest(this,
          hitObject =>
          {
            bool carryOn = !(hitObject is T);

            if (!carryOn)
            {
              hitTested = (T)hitObject;
            }
            return (carryOn ? HitTestFilterBehavior.Continue : HitTestFilterBehavior.Stop);
          },
          result =>
          {
            bool carryOn = !(result.VisualHit is T);

            return (carryOn ? HitTestResultBehavior.Continue : HitTestResultBehavior.Stop);
          },
          new PointHitTestParameters(point));

      return (hitTested);
    }
    Ellipse MakeOrReuseEllipse(int n, bool large)
    {
      Ellipse ellipse = null;
      var size = large ? LARGE_ELLIPSE : SMALL_ELLIPSE;

      if (n < this.ellipseCache.Count)
      {
        ellipse = this.ellipseCache[n];
        ellipse.Width = ellipse.Height = size;
      }
      else
      {
        ellipse = new Ellipse()
        {
          Width = size,
          Height = size,
          Fill = this.ellipseBrush,
          Stroke = this.whiteBrush
        };
        this.ellipseCache.Add(ellipse);
        this.drawCanvas.Children.Add(ellipse);
      }
      return (ellipse);
    }
    void FlushEllipses(int n)
    {
      for (int i = (this.ellipseCache.Count - 1); i >= n; i--)
      {
        this.drawCanvas.Children.Remove(this.ellipseCache[i]);
        this.ellipseCache.RemoveAt(i);
      }
    }
    static PXCMHandData.JointType[] jointTypes =
    {
      PXCMHandData.JointType.JOINT_INDEX_TIP,
      PXCMHandData.JointType.JOINT_MIDDLE_TIP,
      PXCMHandData.JointType.JOINT_RING_TIP,
      PXCMHandData.JointType.JOINT_PINKY_TIP,
      PXCMHandData.JointType.JOINT_THUMB_TIP
    };
    DisplayMode displayMode;
    List<Ellipse> ellipseCache;
    JointPositionMap jointPositions;
    int? currentHandId;
    PXCMHandData handData;
    PXCMSenseManager senseManager;
    SolidColorBrush ellipseBrush;
    SolidColorBrush whiteBrush;
    const int SMALL_ELLIPSE = 25;
    const int LARGE_ELLIPSE = 50;
    bool clickWaiting;
    bool swipeDownWaiting;
  }
}
