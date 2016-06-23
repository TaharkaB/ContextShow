namespace BackgroundSensors
{
  using BackgroundLibrary;
  using System;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.ApplicationModel.Background;
  using Windows.Devices.Enumeration;
  using Windows.Devices.Sensors;
  using Windows.UI.Core;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.LogFileEntries = new ObservableCollection<string>();
      this.Loaded += OnLoaded;
    }
    public ObservableCollection<string> LogFileEntries
    {
      get;
      set;
    }
    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.DataContext = this;

      var access = await BackgroundExecutionManager.RequestAccessAsync();

      if ((access == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity) ||
        (access == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity))
      {
        await this.GetOrRegisterProximityTaskAsync();

        this.proximityTaskRegistration.Completed += this.OnBackgroundTaskCompleted;

        this.GetOrRegisterActivityTask();

        this.activityTaskRegistration.Completed += this.OnBackgroundTaskCompleted;
      }
    }
    async void OnBackgroundTaskCompleted(
      BackgroundTaskRegistration sender, 
      BackgroundTaskCompletedEventArgs args)
    {
      await this.Dispatcher.RunAsync(
        CoreDispatcherPriority.Normal,
        async () =>
        {
          await this.ReReadLogFileEntriesAsync();
        }
      );
    }
    async Task GetOrRegisterProximityTaskAsync()
    {
      // This technique also valid for the Pedometer where you can specify
      // a particular step count.
      var deviceInfoList = await DeviceInformation.FindAllAsync(
      ProximitySensor.GetDeviceSelector());

      var deviceInfo = deviceInfoList.FirstOrDefault();

      if (deviceInfo != null)
      {
        var proximtySensor = ProximitySensor.FromId(deviceInfo.Id);
        var threshold = new ProximitySensorDataThreshold(proximtySensor);
        var trigger = new SensorDataThresholdTrigger(threshold);

        this.proximityTaskRegistration = FindOrRegisterBackgroundTaskFromLibrary(
          "Proximity Trigger",
          trigger);
      }
    }
    void GetOrRegisterActivityTask()
    {
      var activityTrigger = new ActivitySensorTrigger(1000);

      var activities = new ActivityType[] { ActivityType.Walking, ActivityType.Idle };

      foreach (var activity in activities)
      {
        if (activityTrigger.SupportedActivities.Contains(activity))
        {
          activityTrigger.SubscribedActivities.Add(activity);
        }
      }
      this.activityTaskRegistration = FindOrRegisterBackgroundTaskFromLibrary(
        "Activity Trigger",
        activityTrigger);
    }
    static IBackgroundTaskRegistration FindOrRegisterBackgroundTaskFromLibrary(
      string name,
      IBackgroundTrigger trigger)
    {
      var registration = BackgroundTaskRegistration.AllTasks
        .Select(entry => entry.Value)
        .FirstOrDefault(entry => entry.Name == name);

      if (registration == null)
      {
        BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
        builder.Name = name;
        builder.TaskEntryPoint = typeof(BackgroundLibrary.TheTask).FullName;
        builder.SetTrigger(trigger);
        registration = builder.Register();
      }
      return (registration);
    }
    async Task ReReadLogFileEntriesAsync()
    {
      var entries = await SimpleLogger.ReadEntriesAsync();

      // Reversing these as they are logged at the end of the log
      // file.
      this.LogFileEntries.Clear();

      if (entries != null)
      {
        foreach (var entry in entries.Reverse())
        {
          this.LogFileEntries.Add(entry);
        }
      }
    }
    async void OnClearLogFile(object sender, RoutedEventArgs e)
    {
      await SimpleLogger.TruncateLogAsync();
      await this.ReReadLogFileEntriesAsync();      
    }
    IBackgroundTaskRegistration proximityTaskRegistration;
    IBackgroundTaskRegistration activityTaskRegistration;
  }
}
