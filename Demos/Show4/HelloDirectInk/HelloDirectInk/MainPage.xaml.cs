namespace HelloDirectInk
{
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Core;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media;
  using Windows.UI.Xaml.Shapes;

  public sealed partial class MainPage : Page
  {
    Rectangle MakeBackingCanvasRectangle(Rect rectangle, Color color, int thickness, bool dash = false)
    {
      var r = new Rectangle()
      {
        Stroke = new SolidColorBrush(color),
        Width = rectangle.Width,
        Height = rectangle.Height,
        StrokeThickness = thickness
      };
      Canvas.SetLeft(r, rectangle.Left);
      Canvas.SetTop(r, rectangle.Top);

      if (dash)
      {
        var dashes = new DoubleCollection();
        dashes.Add(5.0d);
        r.StrokeDashArray = dashes;
      }
      return (r);
    }
    Polyline MakeBackingCanvasLine(Color color, int thickness,
      bool dash = false)
    {
      var polyline =
        new Polyline()
        {
          Stroke = new SolidColorBrush(color),
          StrokeThickness = thickness
        };

      if (dash)
      {
        var dashes = new DoubleCollection();
        dashes.Add(5.0d);
        polyline.StrokeDashArray = dashes;
      }
      return (polyline);
    }
  }
}