namespace GeofenceBasics
{
  using GeofencingBackgroundTasks.RollingGeofence;
  using System.Threading.Tasks;
  using Windows.UI.Xaml.Controls;
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using GeofencingBackgroundTasks;
  using System.Collections.Generic;
  using System.Linq;
  using Windows.UI.Xaml;
  using Windows.Devices.Geolocation.Geofencing;
  using Windows.UI.Core;
  using Windows.ApplicationModel.Background;

  public sealed partial class MainPage : Page, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    async void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      this.DataContext = this;

      await this.ReloadLogFileEntriesAsync();

      this.UpdateStatusDependentProperties();

      if (GeofenceHelper.IsRegistered)
      {
        // If we're already registered then we want to know whenever our background
        // task completes so that we can update the UI.
        GeofenceHelper.AddTaskCompletedHandler(this.BackgroundTaskCompletedHandler);
      }
    }
    async void OnRegisterAtCurrentLocation(object s, RoutedEventArgs e)
    {
      // We register a 1km fence where the user has to dwell for 30 seconds before
      // we expect to get notified that they have entered/exited the fence.
      await GeofenceHelper.RegisterAsync(
        1000.0d,
        MonitoredGeofenceStates.Entered | MonitoredGeofenceStates.Exited,
        TimeSpan.FromSeconds(30));

      this.UpdateStatusDependentProperties();

      GeofenceHelper.AddTaskCompletedHandler(
        this.BackgroundTaskCompletedHandler);
    }
    async void BackgroundTaskCompletedHandler(IBackgroundTaskRegistration reg,
      BackgroundTaskCompletedEventArgs args)
    {
      await this.Dispatcher.RunAsync(
        CoreDispatcherPriority.Normal,
        async () =>
        {
          await this.ReloadLogFileEntriesAsync();
        }
      );
    }
    async void OnReReadLogs(object sender, RoutedEventArgs e)
    {
      await this.ReloadLogFileEntriesAsync();
    }
    async void OnClearLogs(object sender, RoutedEventArgs e)
    {
      await GeofenceHelper.ClearLogAsync();
      await this.ReloadLogFileEntriesAsync();
    }
    async void OnUnregister(object sender, RoutedEventArgs e)
    {
      if (GeofenceHelper.IsRegistered)
      {
        GeofenceHelper.RemoveTaskCompletedHandler(
          this.BackgroundTaskCompletedHandler);

        GeofenceHelper.Unregister();

        this.UpdateStatusDependentProperties();
      }
    }
    async Task ReloadLogFileEntriesAsync()
    {
      this.logEntries = (await GeofenceHelper.GetLogFileEntriesAsync()).ToList();
      this.OnPropertyChanged("LogEntries");
    }
    List<PositionLogEntry> logEntries;
    string currentStatus;
    bool isRegistered;
  }
}