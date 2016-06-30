namespace HelloBandWorld
{
  using Microsoft.Band;
  using Microsoft.Band.Sensors;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
    }
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      var bands = await BandClientManager.Instance.GetBandsAsync();
      this.gridConnect.DataContext = bands;
    }
    async void OnConnectToSelectedBand(object sender, RoutedEventArgs e)
    {
      var selectedBand = this.listViewBands.SelectedItem as IBandInfo;

      if (selectedBand != null)
      {
        this.bandClient = await BandClientManager.Instance.ConnectAsync(selectedBand);
        await this.InitialiseSensorsAsync();
        this.gridDisplay.Visibility = Visibility.Visible;

        // doesn't really matter as it's at the back in the z-order anyway.
        this.gridConnect.Visibility = Visibility.Collapsed;
      }
    }
    async Task InitialiseSensorsAsync()
    {
      // The band has a lot of sensors. They follow a pattern and so I've
      // used that pattern to make a generic list of them so we can handle
      // them as a list of similar-shaped data sources.
      var sensorManager = this.bandClient.SensorManager;

      // One sensor has to behave differently :-)
      var currentContact = await sensorManager.Contact.GetCurrentStateAsync();

      var sensorReadings = new List<ISensorReadingHelper>()
      {
        new SensorReadingHelper<IBandAccelerometerReading>(
          "Accelerometer",
          sensorManager.Accelerometer,
          this.FormatAccelerometer),
        new SensorReadingHelper<IBandAltimeterReading>(
          "Altimeter",
          sensorManager.Altimeter,
          this.FormatAltimeter),
        new SensorReadingHelper<IBandAmbientLightReading>(
          "Ambient Light",
          sensorManager.AmbientLight,
          this.FormatAmbientLight),
        new SensorReadingHelper<IBandBarometerReading>(
          "Barometer",
          sensorManager.Barometer,
          this.FormatBarometer),
        new SensorReadingHelper<IBandCaloriesReading>(
          "Calories",
          sensorManager.Calories,
          this.FormatCalories),
        new SensorReadingHelper<IBandContactReading>(
          "Contact",
          sensorManager.Contact, 
          this.FormatContact),
        new SensorReadingHelper<IBandDistanceReading>(
          "Distance",
          sensorManager.Distance,
          this.FormatDistance),
        new SensorReadingHelper<IBandGsrReading>(
          "Skin Resistance",
          sensorManager.Gsr,
          this.FormatGsr),
        new SensorReadingHelper<IBandGyroscopeReading>(
          "Gyroscope",
          sensorManager.Gyroscope,
          this.FormatGyroscope),
        new SensorReadingHelper<IBandHeartRateReading>(
          "Heart Rate",
          sensorManager.HeartRate,
          this.FormatHeartRate),
        new SensorReadingHelper<IBandPedometerReading>(
          "Pedometer",
          sensorManager.Pedometer,
          this.FormatPedometer),
        new SensorReadingHelper<IBandRRIntervalReading>(
          "Recovery",
          sensorManager.RRInterval,
          this.FormatRR),
        new SensorReadingHelper<IBandSkinTemperatureReading>(
          "Skin Temperature",
          sensorManager.SkinTemperature,
          this.FormatSkin),
        new SensorReadingHelper<IBandUVReading>(
          "UV Exposure",
          sensorManager.UV,
          this.FormatUV)
      };
      foreach (var sensor in sensorReadings)
      {
        await sensor.InitialiseAsync();
      }
      this.gridDisplay.DataContext = sensorReadings;
    }

    string FormatAccelerometer(IBandAccelerometerReading reading) =>
      $"X:{reading.AccelerationX:F2},Y:{reading.AccelerationY:F2},Z:{reading.AccelerationZ:F2}";

    string FormatAmbientLight(IBandAmbientLightReading reading) =>
      $"Brightness: {reading.Brightness}";

    string FormatAltimeter(IBandAltimeterReading reading) =>
      $"Rate: {reading.Rate}, Up: {reading.FlightsAscended}, Down: {reading.FlightsDescended}";

    string FormatBarometer(IBandBarometerReading reading) =>
      $"Pressure: {reading.AirPressure:F2}, Temp:{reading.Temperature:F2}";

    string FormatCalories(IBandCaloriesReading reading) =>
      $"Calories: {reading.Calories}, Today: {reading.CaloriesToday}";

    string FormatContact(IBandContactReading reading) =>
      $"State: {reading.State}";

    string FormatDistance(IBandDistanceReading reading) =>
      $"Current:{reading.CurrentMotion},Pace:{reading.Pace:F2},Speed:{reading.Speed:F2},Today:{reading.DistanceToday}";

    string FormatGsr(IBandGsrReading reading) =>
      $"Resistance: {reading.Resistance:F2}";

    string FormatGyroscope(IBandGyroscopeReading reading) =>
      $"XYZ:{reading.AccelerationX:F2},{reading.AccelerationY:F2},{reading.AccelerationZ:F2}. AVXYZ:{reading.AngularVelocityX:F2}, {reading.AngularVelocityY:F2}, {reading.AngularVelocityZ:F2}";

    string FormatHeartRate(IBandHeartRateReading reading) =>
      $"Rate:{reading.HeartRate}, Quality:{reading.Quality}";

    string FormatPedometer(IBandPedometerReading reading) =>
      $"Today:{reading.StepsToday}, Total:{reading.TotalSteps}";

    string FormatRR(IBandRRIntervalReading reading) =>
      $"Interval: {reading.Interval}";

    string FormatSkin(IBandSkinTemperatureReading reading) =>
      $"Temperature: {reading.Temperature:F2}";

    string FormatUV(IBandUVReading reading) =>
      $"UV: {reading.IndexLevel}, Today: {reading.ExposureToday}";

    IBandClient bandClient;
  }
}
