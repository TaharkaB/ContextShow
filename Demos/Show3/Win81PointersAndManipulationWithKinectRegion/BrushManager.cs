namespace Win81PointersAndManipulation
{
  using Windows.UI;
  using Windows.UI.Xaml.Media;

  class BrushManager
  {
    public BrushManager()
    {
      this.ToggleRed();
    }
    public Brush DrawBrush
    {
      get
      {
        return (this.brush);
      }
    }
    public bool ToggleRed()
    {
      byte newValue =
        this.brushColour.R == 0 ? (byte)0xFF : (byte)0x00;

      this.brushColour = Color.FromArgb(0xFF, newValue,
        this.brushColour.G, this.brushColour.B);

      this.RecreateBrush();

      return (newValue == 0xFF);
    }
    public bool ToggleGreen()
    {
      byte newValue =
        this.brushColour.G == 0 ? (byte)0xFF : (byte)0x00;

      this.brushColour = Color.FromArgb(0xFF, this.brushColour.R,
        newValue, this.brushColour.B);

      this.RecreateBrush();

      return (newValue == 0xFF);
    }
    public bool ToggleBlue()
    {
      byte newValue =
        this.brushColour.B == 0 ? (byte)0xFF : (byte)0x00;

      this.brushColour = Color.FromArgb(0xFF, this.brushColour.R,
        this.brushColour.G, newValue);

      this.RecreateBrush();

      return (newValue == 0xFF);
    }
    void RecreateBrush()
    {
      this.brush = new SolidColorBrush(this.brushColour);
    }
    SolidColorBrush brush;
    Color brushColour;

  }
}
