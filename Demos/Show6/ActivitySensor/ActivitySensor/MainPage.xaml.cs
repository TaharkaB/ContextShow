namespace ActivitySensorCode
{
  using Windows.Devices.Sensors;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using System;
  using Windows.UI.Core;
  using Windows.UI.Xaml.Media.Imaging;
  using System.Linq;
  using System.Collections.ObjectModel;
  using System.ComponentModel;

  public sealed partial class MainPage : Page, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public MainPage()
    {
      this.InitializeComponent();

      this.Loaded += OnLoaded;
    }
    public ObservableCollection<ActivitySensorReading> ActivityHistory
    {
      get
      {
        return (this.activityHistory);
      }
      private set
      {
        this.activityHistory = value;
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActivityHistory"));
      }
    }
    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.DataContext = this;

      // We should check that we have access to this sensor really rather
      // than just going for it as we do here.
      this.activitySensor = await ActivitySensor.GetDefaultAsync();

      if (this.activitySensor != null)
      {
        var reading = await this.activitySensor.GetCurrentReadingAsync();

        if (reading != null)
        {
          this.DisplayReading(reading);
        }
        // Subscribe to all the activities that the sensor supports.
        foreach (var activityType in Enum.GetValues(typeof(ActivityType)))
        {
          if (this.activitySensor.SupportedActivities.Contains((ActivityType)activityType))
          {
            this.activitySensor.SubscribedActivities.Add((ActivityType)activityType);
          }
        }
        // Wait for new values to come through.
        this.activitySensor.ReadingChanged += OnReadingChanged;

        // Get the history for the past week
        var history = await ActivitySensor.GetSystemHistoryAsync(
          DateTimeOffset.Now - TimeSpan.FromDays(7));

        this.ActivityHistory = new ObservableCollection<ActivitySensorReading>(
          history);
      }
    }
    void DisplayReading(ActivitySensorReading reading)
    {
      var bitmapImage = new BitmapImage(
        new Uri($"ms-appx:///Assets/{reading.Activity}.png"));

      this.activityImage.Source = bitmapImage;

      this.txtActivity.Text = reading.Activity.ToString();

      var confidenceLabel =
        reading.Confidence == ActivitySensorReadingConfidence.High ? "certain" : "unsure";

      this.txtActivityDetails.Text =
        $"{confidenceLabel} of this at {reading.Timestamp:hh.mm}";

    }
    async void OnReadingChanged(ActivitySensor sender, 
      ActivitySensorReadingChangedEventArgs args)
    {
      await this.Dispatcher.RunAsync(
        CoreDispatcherPriority.Normal,
        () =>
        {
          if (args.Reading != null)
          {
            this.DisplayReading(args.Reading);
          }
        }
      );
    }
    ObservableCollection<ActivitySensorReading> activityHistory;
    ActivitySensor activitySensor;
  }
}