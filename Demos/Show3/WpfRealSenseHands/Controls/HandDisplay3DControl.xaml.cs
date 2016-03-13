namespace WpfRealSenseHands.Controls
{
  using HelixToolkit.Wpf;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Media.Media3D;
  using BoneVisualMap =
      System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<HelixToolkit.Wpf.TubeVisual3D>>;
  using HandMap =
      System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<PXCMHandData.JointType, PXCMPoint3DF32>>;
  using HandVisualMap =
      System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<PXCMHandData.JointType, System.Windows.Media.Media3D.ModelVisual3D>>;
  using JointPositionMap =
      System.Collections.Generic.Dictionary<PXCMHandData.JointType, PXCMPoint3DF32>;
  using JointVisual3DMap =
      System.Collections.Generic.Dictionary<PXCMHandData.JointType, System.Windows.Media.Media3D.ModelVisual3D>;

  static class Constants
  {
    static public readonly float SphereRadius = 0.005f;
    static public readonly float TubeDiameter = 0.003f;
  }
  public partial class HandDisplay3DControl : UserControl, IModuleProcessor
  {
    public HandDisplay3DControl()
    {
      InitializeComponent();
      this.spheresInColours = brushes.Select(b => MakeSphereForBrush(b)).ToArray();
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

          handConfiguration.EnableStabilizer(true).ThrowOnFail();

          handConfiguration.EnableTrackedJoints(true).ThrowOnFail();

          handConfiguration.ApplyChanges().ThrowOnFail();
        }
        this.handData = handModule.CreateOutput();
      }
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      // this is me being very lazy and simply letting the GC get rid of 
      // this instance every time around.
      this.handMap = new HandMap();

      if (this.handData.Update().Succeeded())
      {
        var handsInfoFromAlerts = this.alertManager.GetHandsInfo();

        var goodHands =
          handsInfoFromAlerts?.Where(
            (entry => entry.Value == HandAlertManager.HandStatus.Ok));

        if (goodHands != null)
        {
          foreach (var entry in goodHands)
          {
            PXCMHandData.IHand iHand;

            // gather the data to display that hand.
            if (this.handData.QueryHandDataById(entry.Key, out iHand).Succeeded())
            {
              foreach (PXCMHandData.JointType joint in Enum.GetValues(
                typeof(PXCMHandData.JointType)))
              {
                PXCMHandData.JointData jointData;

                if (iHand.QueryTrackedJoint(joint, out jointData).Succeeded())
                {
                  if (!this.handMap.ContainsKey(entry.Key))
                  {
                    this.handMap[entry.Key] = new JointPositionMap();
                  }
                  this.handMap[entry.Key][joint] =
                    new PXCMPoint3DF32(
                      0.0f - jointData.positionWorld.x,
                      jointData.positionWorld.y,
                      jointData.positionWorld.z);
                }
              }
            }
          }
        }
      }
    }
    public void DrawUI(PXCMCapture.Sample sample)
    {
      if ((this.handMap == null) || (this.handMap.Count == 0))
      {
        this.ClearAll();
      }
      else
      {
        if (this.drawnHandJointVisualMap == null)
        {
          this.drawnHandJointVisualMap = new HandVisualMap();
        }
        if (this.drawnHandBoneVisualMap == null)
        {
          this.drawnHandBoneVisualMap = new BoneVisualMap();
        }
        // get rid of anything we drew for a previous hand that is not
        // present in the current frame of data.
        this.ClearLostHands();

        // draw the stuff that *is* present in the current frame of
        // data...
        this.DrawHands();
      }
    }
    void DrawHands()
    {
      foreach (var handId in this.handMap.Keys)
      {
        if (!this.drawnHandJointVisualMap.ContainsKey(handId))
        {
          this.drawnHandJointVisualMap[handId] = new JointVisual3DMap();
        }
        foreach (var joint in this.handMap[handId].Keys)
        {
          this.DrawJoint(handId, joint);
        }
        this.DrawBones(handId);
      }
    }
    void DrawBones(int handId)
    {
      if (!this.drawnHandBoneVisualMap.ContainsKey(handId))
      {
        this.drawnHandBoneVisualMap[handId] = new List<TubeVisual3D>();
      }
      int tubeCount = 0;

      foreach (var joints in jointConnections)
      {
        for (int i = 0; i < joints.Length - 1; i++)
        {
          var point = this.handMap[handId][joints[i]];
          var next = this.handMap[handId][joints[i + 1]];

          if (this.drawnHandBoneVisualMap[handId].Count() <= tubeCount)
          {
            var tube = MakeTubeForPositions(point, next);
            this.drawnHandBoneVisualMap[handId].Add(tube);
            this.modelVisual.Children.Add(tube);
          }
          else
          {
            var tube = this.drawnHandBoneVisualMap[handId][tubeCount];
            tube.Path[0] = new Point3D(point.x, point.y, point.z);
            tube.Path[1] = new Point3D(next.x, next.y, next.z);
          }
          tubeCount++;
        }
      }
    }
    void DrawJoint(int handId, PXCMHandData.JointType joint)
    {
      var position = this.handMap[handId][joint];
      ModelVisual3D visual;

      if (!this.drawnHandJointVisualMap[handId].TryGetValue(joint, out visual))
      {
        visual =
          MakeSphereForHandPosition(handId, position.x, position.y, position.z);

        this.drawnHandJointVisualMap[handId][joint] = visual;

        this.modelVisual.Children.Add(visual);
      }
      var transform = ((TranslateTransform3D)visual.Transform);
      transform.OffsetX = position.x;
      transform.OffsetY = position.y;
      transform.OffsetZ = position.z;
    }
    void ClearLostHands()
    {
      foreach (var oldHandId in this.drawnHandJointVisualMap.Keys.Where(
        k => !this.handMap.ContainsKey(k)).ToList())
      {
        foreach (var joint in this.drawnHandJointVisualMap[oldHandId])
        {
          this.modelVisual.Children.Remove(joint.Value);
        }
        this.drawnHandJointVisualMap.Remove(oldHandId);

        foreach (var bone in this.drawnHandBoneVisualMap[oldHandId])
        {
          this.modelVisual.Children.Remove(bone);
        }
        this.drawnHandBoneVisualMap.Remove(oldHandId);
      }
    }
    void ClearAll()
    {
      this.modelVisual.Children.Clear();
      this.drawnHandJointVisualMap = null;
      this.drawnHandBoneVisualMap = null;
    }
    ModelVisual3D MakeSphereForHandPosition(int handId, float x, float y, float z)
    {
      var modelVisual3d = new ModelVisual3D()
      {
        Content = this.spheresInColours[handId % brushes.Length].Model
      };
      modelVisual3d.Transform = new TranslateTransform3D()
      {
        OffsetX = x,
        OffsetY = y,
        OffsetZ = z
      };
      return (modelVisual3d);
    }
    static SphereVisual3D MakeSphereForBrush(Brush brush)
    {
      var sphere = new SphereVisual3D();
      sphere.Radius = Constants.SphereRadius;
      sphere.Fill = brush;
      return (sphere);
    }
    static TubeVisual3D MakeTubeForPositions(PXCMPoint3DF32 p1, PXCMPoint3DF32 p2)
    {
      var tube = new TubeVisual3D();
      tube.Diameter = Constants.TubeDiameter;
      Point3DCollection points = new Point3DCollection();
      points.Add(new Point3D(p1.x, p1.y, p1.z));
      points.Add(new Point3D(p2.x, p2.y, p2.z));
      tube.Path = points;
      tube.Fill = Brushes.Silver;
      return (tube);
    }
    static Brush[] brushes =
    {
      Brushes.Red,
      Brushes.Green,
      Brushes.Blue,
      Brushes.Yellow,
      Brushes.Cyan,
      Brushes.Purple
    };
    static PXCMHandData.JointType[][] jointConnections =
    {
      new []
        {
          PXCMHandData.JointType.JOINT_WRIST,
          PXCMHandData.JointType.JOINT_PINKY_BASE,
          PXCMHandData.JointType.JOINT_PINKY_JT1,
          PXCMHandData.JointType.JOINT_PINKY_JT2,
          PXCMHandData.JointType.JOINT_PINKY_TIP
        },
      new []
        {
          PXCMHandData.JointType.JOINT_WRIST,
          PXCMHandData.JointType.JOINT_RING_BASE,
          PXCMHandData.JointType.JOINT_RING_JT1,
          PXCMHandData.JointType.JOINT_RING_JT2,
          PXCMHandData.JointType.JOINT_RING_TIP
        },
      new []
        {
          PXCMHandData.JointType.JOINT_WRIST,
          PXCMHandData.JointType.JOINT_CENTER,
          PXCMHandData.JointType.JOINT_MIDDLE_BASE,
          PXCMHandData.JointType.JOINT_MIDDLE_JT1,
          PXCMHandData.JointType.JOINT_MIDDLE_JT2,
          PXCMHandData.JointType.JOINT_MIDDLE_TIP
        },
      new []
        {
          PXCMHandData.JointType.JOINT_WRIST,
          PXCMHandData.JointType.JOINT_INDEX_BASE,
          PXCMHandData.JointType.JOINT_INDEX_JT1,
          PXCMHandData.JointType.JOINT_INDEX_JT2,
          PXCMHandData.JointType.JOINT_INDEX_TIP
        },
      new []
      {
        PXCMHandData.JointType.JOINT_WRIST,
        PXCMHandData.JointType.JOINT_THUMB_BASE,
        PXCMHandData.JointType.JOINT_THUMB_JT1,
        PXCMHandData.JointType.JOINT_THUMB_JT2,
        PXCMHandData.JointType.JOINT_THUMB_TIP
      }
    };
    SphereVisual3D[] spheresInColours;
    HandMap handMap;
    HandVisualMap drawnHandJointVisualMap;
    BoneVisualMap drawnHandBoneVisualMap;
    HandAlertManager alertManager;
    PXCMHandData handData;
    PXCMSenseManager senseManager;
  }
}
