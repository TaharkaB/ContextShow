namespace Win81PointersAndManipulation
{
  using Microsoft.Kinect.Toolkit.Input;
  using Microsoft.Kinect.Xaml.Controls;
  using Windows.UI.Xaml.Controls;

  class ManipulatableCanvas : Canvas, IKinectControl
  {
    public bool IsManipulatable
    {
      get
      {
        return (true);
      }
    }

    public bool IsPressable
    {
      get
      {
        return (false);
      }
    }
    public bool IsGripping
    {
      get
      {
        return (this.isGripping && this.isEnabled);
      }
    }
    public bool IsEnabled
    {
      get
      {
        return (this.isEnabled);
      }
      set
      {
        this.isEnabled = value;
      }
    }
    public IKinectController CreateController(
      IInputModel inputModel,
      KinectRegion kinectRegion)
    {
      inputModel.GestureRecognizer.ManipulationStarted += (s, e) =>
      {
        this.isGripping = true;
      };
      inputModel.GestureRecognizer.ManipulationCompleted += (s, e) =>
      {
        this.isGripping = false;
      };
      return (new ManipulatableController(inputModel));
    }
    bool isEnabled;
    bool isGripping;
  }
}
