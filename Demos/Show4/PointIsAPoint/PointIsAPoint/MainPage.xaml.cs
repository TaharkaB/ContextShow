namespace PointIsAPoint
{
  using Windows.Foundation;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
    }
    void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
      this.lastPoint = e.GetCurrentPoint(this.grid).Position;
    }
    void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
      if (this.lastPoint != null)
      {
        var newPoint = e.GetCurrentPoint(this.grid).Position;

        this.transform.TranslateX += (int)newPoint.X - (int)this.lastPoint.Value.X;
        this.transform.TranslateY += (int)newPoint.Y - (int)this.lastPoint.Value.Y;

        this.lastPoint = newPoint;
      }
    }
    void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
      if (this.lastPoint != null)
      {
        this.lastPoint = null;
        this.transform.TranslateX = 0;
        this.transform.TranslateY = 0;
      }
    }  
    Point? lastPoint;
  }
}

