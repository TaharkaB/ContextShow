namespace SensorExampleMagnetometer
{
  using System;
  using Windows.Devices.Sensors;
  using Windows.UI.Core;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();

      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      // We use the compass here but the pattern is the same for other sensors
      // like;
      //
      // Accelerometer
      // Altimeter
      // Barometer
      // Gyrometer
      // Inclinometer
      // LightSensor
      // Magnetometer
      // Orientation
      // Pedometer
      // Proximity
      // SimpleOrientation
      // Basic pattern is get sensor (if present), ask it for reading (polling if you
      // like) and/or ask it to tell you when the reading changes.
      this.compass = Compass.GetDefault();

      if (this.compass != null)
      {
        // We could ask the compass to report more frequently but we don't
        // here
        this.compass.ReportInterval = compass.MinimumReportInterval;

        // Tell me when it changes.
        this.compass.ReadingChanged += OnCompassReadingChanged;

        var reading = this.compass.GetCurrentReading();

        this.DisplayReading(reading);
      }
    }
    async void DisplayReading(CompassReading reading)
    {
      await this.Dispatcher.RunAsync(
        CoreDispatcherPriority.Normal,
        () =>
        {
          // Taken from MSDN.
          // https://msdn.microsoft.com/en-us/library/dn440593.aspx

          // Calculate the compass heading offset based on
          // the current display orientation.
          var displayInfo = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
          var displayOffset = 0.0d;

          switch (displayInfo.CurrentOrientation)
          {
            case Windows.Graphics.Display.DisplayOrientations.Landscape:
              displayOffset = 0;
              break;
            case Windows.Graphics.Display.DisplayOrientations.Portrait:
              displayOffset = 270;
              break;
            case Windows.Graphics.Display.DisplayOrientations.LandscapeFlipped:
              displayOffset = 180;
              break;
            case Windows.Graphics.Display.DisplayOrientations.PortraitFlipped:
              displayOffset = 90;
              break;
          }
          if (reading.HeadingTrueNorth.HasValue)
          {
            this.rotateTrue.Angle =
              reading.HeadingTrueNorth.Value + displayOffset;

            this.gridTrueNorth.Visibility = Windows.UI.Xaml.Visibility.Visible;
          }
          else
          {
            this.gridTrueNorth.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
          }
          this.rotateMagnetic.Angle = reading.HeadingMagneticNorth + displayOffset;
        }
      );
    }
    void OnCompassReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
    {
      this.DisplayReading(args.Reading);
    }
    Compass compass;
  }
}
