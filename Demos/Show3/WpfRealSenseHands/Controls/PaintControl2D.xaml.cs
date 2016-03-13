using System;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media;

namespace WpfRealSenseHands.Controls
{
  public partial class PaintControl2D : UserControl, IModuleProcessor
  {
    public PaintControl2D()
    {
      InitializeComponent();

      this.brushes = new SolidColorBrush[]
      {
        new SolidColorBrush(Colors.Red),
        new SolidColorBrush(Colors.Green),
        new SolidColorBrush(Colors.Blue)
      };
      this.brushIndex = 0;
    }

    public int RealSenseModuleId
    {
      get
      {
        return (PXCMHandModule.CUID);
      }
    }
    public void Initialise(PXCMSenseManager senseManager)
    {
      this.senseManager = senseManager;

      this.senseManager.EnableHand().ThrowOnFail();

      using (var handModule = this.senseManager.QueryHand())
      {
        using (var handConfiguration = handModule.CreateActiveConfiguration())
        {
          this.alertManager = new HandAlertManager(handConfiguration);

          handConfiguration.EnableGesture("thumb_up", false).ThrowOnFail();
          handConfiguration.EnableGesture("wave", false).ThrowOnFail();
          handConfiguration.EnableGesture("fist").ThrowOnFail();
          handConfiguration.SubscribeGesture(this.OnGestureFired);

          handConfiguration.ApplyChanges().ThrowOnFail();
        }
        this.handData = handModule.CreateOutput();
      }
    }
    void OnGestureFired(PXCMHandData.GestureData gestureData)
    {
      if ((gestureData.name == "thumb_up") &&
        (gestureData.state == PXCMHandData.GestureStateType.GESTURE_STATE_END))
      {
        this.OnChooseNextBrushColour();
      }
      else if (gestureData.name == "fist")
      {
        if (gestureData.state == PXCMHandData.GestureStateType.GESTURE_STATE_START)
        {
          this.draw = true;
        }
        else if (gestureData.state == PXCMHandData.GestureStateType.GESTURE_STATE_END)
        {
          this.draw = false;
          this.lastPoint = null;
        }
      }
      else if ((gestureData.name == "wave") &&
        (gestureData.state == PXCMHandData.GestureStateType.GESTURE_STATE_END))
      {
        this.clearPending = true;
      }
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      this.leftTipPosition = this.rightTipPosition = null;

      if (this.handData.Update().Succeeded())
      {
        var imageSize = sample.depth.QueryInfo();

        var handsInfoFromAlerts = this.alertManager.GetHandsInfo();

        var goodHands =
          handsInfoFromAlerts?.Where(
            (entry => entry.Value == HandAlertManager.HandStatus.Ok));

        if (goodHands != null)
        {
          foreach (var entry in goodHands)
          {
            PXCMHandData.IHand iHand;

            if (this.handData.QueryHandDataById(entry.Key, out iHand).Succeeded())
            {
              var bodySide = iHand.QueryBodySide();

              // I'm not 100% sure but BODY_SIDE_RIGHT seems to be the inverse
              // of how I would interpret it as a human regardless of mirroring
              // on the camera.
              PXCMHandData.JointData jointData;

              // Get the tip of the index finger
              if (iHand.QueryTrackedJoint(
                  PXCMHandData.JointType.JOINT_INDEX_TIP,
                  out jointData).Succeeded())
              {
                var position = new PXCMPointF32(
                  jointData.positionImage.x / imageSize.width,
                  jointData.positionImage.y / imageSize.height);

                if (bodySide == PXCMHandData.BodySideType.BODY_SIDE_RIGHT)
                {
                  this.leftTipPosition = position;
                }
                else
                {
                  this.rightTipPosition = position;
                }
              }
            }
          }
        }
      }
    }
    public void DrawUI(PXCMCapture.Sample sample)
    {
      if (this.clearPending)
      {
        foreach (var line in this.drawCanvas.Children.OfType<Line>().ToList())
        {
          this.drawCanvas.Children.Remove(line);
        }
        this.clearPending = false;
      }
      if (this.leftTipPosition.HasValue)
      {
        this.leftEllipse.Visibility = System.Windows.Visibility.Visible;
        this.leftEllipse.Fill = this.brushes[this.brushIndex];

        Canvas.SetLeft(
          this.leftEllipse, 
          (this.leftTipPosition.Value.x * this.drawCanvas.ActualWidth) - this.leftEllipse.ActualWidth / 2);
        Canvas.SetTop(
          this.leftEllipse, 
          (this.leftTipPosition.Value.y * this.drawCanvas.ActualHeight) - this.leftEllipse.ActualHeight / 2);
      }
      else
      {
        this.leftEllipse.Visibility = System.Windows.Visibility.Collapsed;
      }

      if (this.draw && this.rightTipPosition.HasValue)
      {
        if (lastPoint.HasValue)
        {
          var line = this.MakeLine(
            this.lastPoint.Value, this.rightTipPosition.Value);

          this.drawCanvas.Children.Add(line);
        }
        this.lastPoint = this.rightTipPosition.Value;
      }
    }
    Line MakeLine(PXCMPointF32 pt1, PXCMPointF32 pt2)
    {
      var line = new Line()
      {
        X1 = pt1.x * this.drawCanvas.ActualWidth,
        Y1 = pt1.y * this.drawCanvas.ActualHeight,
        X2 = pt2.x * this.drawCanvas.ActualWidth,
        Y2 = pt2.y * this.drawCanvas.ActualHeight,
        Stroke = this.brushes[this.brushIndex],
        StrokeThickness = 10
      };
      return (line);
    }
    void OnChooseNextBrushColour()
    {
      if (++this.brushIndex == this.brushes.Length)
      {
        this.brushIndex = 0;
      }
    }
    PXCMPointF32? leftTipPosition;
    PXCMPointF32? rightTipPosition;
    PXCMHandData handData;
    PXCMSenseManager senseManager;
    HandAlertManager alertManager;
    Brush rightBrush;
    Brush leftBrush;
    SolidColorBrush[] brushes;
    int brushIndex;
    bool draw;
    PXCMPointF32? lastPoint;
    bool clearPending;
  }
}
