namespace Win81KinectFacialDetection
{
  using Microsoft.Kinect.Face;
  using Windows.Foundation;

  static class FaceBoxExtensions
  {
    public static Rect ToRect(this RectI rectangle)
    {
      Rect r = new Rect(
        rectangle.Left,
        rectangle.Top,
        rectangle.Right - rectangle.Left,
        rectangle.Bottom - rectangle.Top
      );
      return (r);
    } 
  }
}
