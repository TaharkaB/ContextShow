namespace OldSchoolInk
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using Windows.Devices.Input;
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Input;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  using Windows.UI.Xaml.Media;
  using Windows.UI.Xaml.Shapes;

  public sealed partial class MainPage : Page, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public MainPage()
    {
      this.InitializeComponent();

      this.lineBrush = new SolidColorBrush(Colors.Yellow);
      this.pathBrush = new SolidColorBrush(Colors.Navy);

      this.Loaded += OnLoaded;
    }
    public Dictionary<string, PropertyValue> Properties
    {
      get
      {
        return (this.properties);
      }
      set
      {
        this.SetProperty(ref this.properties, value);
      }
    }
    void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      this.DataContext = this;
      this.inkManager = new InkManager();
      this.inkManager.Mode = InkManipulationMode.Inking;
    }
    void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
      PointerPoint pointerPt = e.GetCurrentPoint(this.canvas);

      this.lastPoint = pointerPt.Position;

      this.inkManager.ProcessPointerDown(pointerPt);

      this.UpdatePenProperties(pointerPt);
    }
    void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
      if (this.lastPoint != null)
      {
        PointerPoint newPoint = e.GetCurrentPoint(this.canvas);

        if (this.inkManager.Mode == InkManipulationMode.Inking)
        {
          this.canvas.Children.Add(
            new Line()
            {
              X1 = this.lastPoint.Value.X,
              Y1 = this.lastPoint.Value.Y,
              X2 = newPoint.Position.X,
              Y2 = newPoint.Position.Y,
              StrokeThickness = 5.0 * newPoint.Properties.Pressure,
              Stroke = this.lineBrush
            }
          );
        }
        this.lastPoint = newPoint.Position;

        this.inkManager.ProcessPointerUpdate(newPoint);

        this.UpdatePenProperties(newPoint);
      }
    }
    void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
      this.Properties = null;

      if (this.lastPoint != null)
      {
        this.inkManager.ProcessPointerUp(e.GetCurrentPoint(this.canvas));

        if (this.inkManager.Mode == InkManipulationMode.Inking)
        {
          this.ClearCanvasLines();
          this.RedrawLastStroke();
        }
        else
        {
          this.RedrawAllStrokes();
        }
      }
      this.lastPoint = null;
    }
    void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
      this.OnPointerReleased(sender, e);
    }
    void ClearCanvasLines()
    {
      var lines = this.canvas.Children.Where(c => c is Line).ToList();

      foreach (var line in lines)
      {
        this.canvas.Children.Remove(line);
      }
    }
    void DrawStroke(InkStroke stroke)
    {
      var segments = stroke.GetRenderingSegments();

      var pathGeometry = new PathGeometry()
      {
        Figures = new PathFigureCollection()
      };

      var path = new Path()
      {
        Data = pathGeometry,
        Stroke = this.pathBrush,
        StrokeThickness = 2
      };

      var figure = new PathFigure()
      {
        StartPoint = segments.First().Position
      };
      pathGeometry.Figures.Add(figure);

      foreach (var segment in segments)
      {
        figure.Segments.Add(new BezierSegment()
        {
          Point1 = segment.BezierControlPoint1,
          Point2 = segment.BezierControlPoint2,
          Point3 = segment.Position
        });
      }
      this.canvas.Children.Add(path);
    }
    void RedrawAllStrokes()
    {
      this.canvas.Children.Clear();

      foreach (var stroke in this.inkManager.GetStrokes())
      {
        this.DrawStroke(stroke);
      }
    }
    void RedrawLastStroke()
    {
      var lastStroke = this.inkManager.GetStrokes()?.LastOrDefault();

      if (lastStroke != null)
      {
        this.DrawStroke(lastStroke);
      }
    }
    void OnModeToggled(object sender, RoutedEventArgs e)
    {
      if (this.inkManager != null)
      {
        if (this.inkManager.Mode == InkManipulationMode.Inking)
        {
          this.inkManager.Mode = InkManipulationMode.Erasing;
        }
        else
        {
          this.inkManager.Mode = InkManipulationMode.Inking;
        }
      }
    }
    void UpdatePenProperties(PointerPoint pt)
    {
      if (pt.PointerDevice.PointerDeviceType == PointerDeviceType.Pen)
      {
        Dictionary<string, PropertyValue> list = this.Properties;

        if (list == null)
        {
          list = new Dictionary<string, PropertyValue>();
        }
        AddToDictionary(list, "IsInContact", pt.IsInContact);
        AddToDictionary(list, "IsIntegrated", pt.PointerDevice.IsIntegrated);

        foreach (var prop in pt.Properties.GetType().GetRuntimeProperties())
        {
          var reflectedValue = prop.GetValue(pt.Properties);

          AddToDictionary(list, prop.Name, reflectedValue);
        }
        // This may be a pointless assignment, depends on the code above.
        this.Properties = list;
      }
      else
      {
        this.Properties = null;
      }
    }
    async void OnRecognise(object sender, RoutedEventArgs e)
    {
      var allResults = await this.inkManager.RecognizeAsync(InkRecognitionTarget.All);

      var topResults = allResults.Select(
        result => result.GetTextCandidates()?.FirstOrDefault() ?? string.Empty);

      var joined = string.Join(" ", topResults);

      this.txtRecognised.Text = joined;
    }
    bool SetProperty<T>(
      ref T storage,
      T value,
      [CallerMemberName] string propertyName = null)
    {
      if (object.Equals(storage, value)) return false;

      storage = value;

      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

      return true;
    }
    static void AddToDictionary(
      Dictionary<string, PropertyValue> dictionary,
      string name,
      object value)
    {
      PropertyValue storedValue = null;

      if (dictionary.ContainsKey(name))
      {
        storedValue = dictionary[name];
      }
      else
      {
        storedValue = new PropertyValue();
        dictionary[name] = storedValue;
      }
      storedValue.Value = value?.ToString() ?? "null";
    }

    // Note: deliberately simplified this example to only track a single
    // pointer rather than tracking multiple from (say) 2 fingers or
    // 2 pens or similar.
    Point? lastPoint;
    Brush lineBrush;
    Brush pathBrush;
    InkManager inkManager;
    Dictionary<string, PropertyValue> properties;
  }
}
