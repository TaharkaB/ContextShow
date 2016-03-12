namespace W81KinectRegionDrawing
{
  using Microsoft.Kinect.Toolkit.Input;
  using Microsoft.Kinect.Xaml.Controls;
  using Windows.UI.Xaml;

  public class ManipulatableController : IKinectManipulatableController
  {
    IInputModel model;
    public ManipulatableController(IInputModel model)
    {
      this.model = model;
    }
    public FrameworkElement Element
    {
      get
      {
        return (this.model.Element as FrameworkElement);
      }
    }

    public ManipulatableModel ManipulatableInputModel
    {
      get
      {
        return (this.model as ManipulatableModel);
      }
    }
  }
}
