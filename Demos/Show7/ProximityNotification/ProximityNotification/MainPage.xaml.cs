namespace ProximityNotification
{
  using BackgroundComponents;
  using Microsoft.Band;
  using Microsoft.Band.Tiles;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.ApplicationModel.Background;
  using Windows.Devices.Bluetooth.Advertisement;
  using Windows.Storage.Streams;
  using Windows.UI.Popups;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
    }
    async Task RegisterTaskAsync(
      string taskName,
      IBackgroundTrigger trigger,
      Type taskType)
    {
      if (BackgroundTaskRegistration.AllTasks.Count > 0)
      {
        await DisplayMessageAsync("Error", "Task is already registered");
      }
      else
      {
        var allowed = await BackgroundExecutionManager.RequestAccessAsync();

        if ((allowed != BackgroundAccessStatus.Denied) &&
          (allowed != BackgroundAccessStatus.Unspecified))
        {
          foreach (var existingTask in BackgroundTaskRegistration.AllTasks.ToList())
          {
            existingTask.Value.Unregister(false);
          }
        }
        BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
        builder.Name = taskName;
        builder.TaskEntryPoint = taskType.FullName;
        builder.SetTrigger(trigger);
        builder.Register();

        await DisplayMessageAsync("Registered", "Task is now registered");
      }
    }
    BluetoothLEAdvertisementWatcherTrigger MakeWatcherTrigger()
    {
      var trigger = new BluetoothLEAdvertisementWatcherTrigger();

      trigger.AdvertisementFilter.Advertisement.ManufacturerData.Add(
        new BluetoothLEManufacturerData()
        {
          CompanyId = TheTask.MSCompanyId
        }
      );
      return (trigger);
    }
    void OnStartPublishing(object sender, RoutedEventArgs e)
    {
      // We do our publishing in the foreground to make it easier to control
      // when we are and when we are not publishing. No app running? Then 
      // we're not publishing.
      string data = "Hello";
      DataWriter writer = new DataWriter();
      writer.WriteInt32(data.Length);
      writer.WriteString(data);

      var manufacturerData = new BluetoothLEManufacturerData(
        TheTask.MSCompanyId, writer.DetachBuffer());

      this.publisher = new BluetoothLEAdvertisementPublisher();

      this.publisher.Advertisement.ManufacturerData.Add(manufacturerData);

      this.publisher.Start();

      this.beaconGrid.Visibility = Visibility.Visible;
    }
    async void OnRegisterWatcher(object sender, RoutedEventArgs e)
    {
      await this.RegisterTaskAsync(
        "Watcher Task",
        this.MakeWatcherTrigger(),
        typeof(BackgroundComponents.TheTask));
    }
    async void OnRegisterBandTile(object sender, RoutedEventArgs e)
    {
      bool alreadyRegistered = false;

      try
      {
        // this will throw if it's not there.
        Guid id = TheTask.TileIdentifier;
        alreadyRegistered = true;
      }
      catch (InvalidOperationException) { };

      if (!alreadyRegistered)
      {
        var bands = await BandClientManager.Instance.GetBandsAsync();

        var firstBand = bands.FirstOrDefault();

        if (firstBand != null)
        {
          using (var bandClient = await BandClientManager.Instance.ConnectAsync(
            firstBand))
          {
            await this.CreateTileAsync(bandClient);
          }
        }
        await DisplayMessageAsync("Registered", "Tile has been created");
      }
      else
      {
        await DisplayMessageAsync("Error", "Tile was already there");
      }
    }
    async Task<BandTile> CreateTileAsync(IBandClient client)
    {
      BandTile bandTile = null;

      var tileSpace = await client.TileManager.GetRemainingTileCapacityAsync();

      if (tileSpace > 0)
      {
        var smallIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/bandSmall.png"), 24);

        var largeIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/bandLarge.png"), 48);

        TheTask.TileIdentifier = Guid.NewGuid();

        bandTile = new BandTile(TheTask.TileIdentifier)
        {
          Name = "Beacon",
          TileIcon = largeIcon,
          SmallIcon = smallIcon
        };

        var added = await client.TileManager.AddTileAsync(bandTile);
      }
      return (bandTile);
    }
    static async Task DisplayMessageAsync(string title, string message)
    {
      var dialog = new MessageDialog(message, title);
      await dialog.ShowAsync();
    }
    BluetoothLEAdvertisementPublisher publisher;
  }
}