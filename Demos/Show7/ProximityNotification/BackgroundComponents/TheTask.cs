namespace BackgroundComponents
{
  using Microsoft.Band;
  using Microsoft.Band.Notifications;
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.ApplicationModel.Background;
  using Windows.Devices.Bluetooth.Background;
  using Windows.Storage;
  using Windows.Storage.Streams;
  public sealed class TheTask : IBackgroundTask
  {
    public async void Run(IBackgroundTaskInstance taskInstance)
    {
      var deferral = taskInstance.GetDeferral();
      taskInstance.Canceled += OnCancelled;

      try
      {
        var watcherDetails = taskInstance.TriggerDetails as
          BluetoothLEAdvertisementWatcherTriggerDetails;

        if (watcherDetails != null)
        {
          // hoping only one
          foreach (var item in watcherDetails.Advertisements)
          {
            // hoping only one
            foreach (var data in item.Advertisement.GetManufacturerDataByCompanyId(MSCompanyId))
            {
              using (var dataReader = DataReader.FromBuffer(data.Data))
              {
                var length = dataReader.ReadInt32();
                var detail = dataReader.ReadString((uint)length);
                await this.PostNotificationToBandAsync(detail);
              }
            }
          }
        }
      }
      finally
      {
        taskInstance.Canceled -= OnCancelled;
        deferral.Complete();
      }
    }
    async Task PostNotificationToBandAsync(string message)
    {
      // NB: note the background flag here to let the system know what
      // context we are running in.
      var bands = await BandClientManager.Instance.GetBandsAsync(true);

      var firstBand = bands.FirstOrDefault();

      if (firstBand != null)
      {
        using (var bandClient = await BandClientManager.Instance.ConnectAsync(firstBand))
        {
          // just to show that we can, the notification will do this anywya.
          await bandClient.NotificationManager.VibrateAsync(
            VibrationType.NotificationOneTone);

          // display the data from the beacon (or as much as we can)
          await bandClient.NotificationManager.SendMessageAsync(
            TileIdentifier, "Spotted a Beacon", message, DateTimeOffset.Now);
        }
      }
    }
    public static Guid TileIdentifier
    {
      get
      {
        object oValue = null;
        Guid guid;

        if (ApplicationData.Current.LocalSettings.Values.TryGetValue("id", out oValue))
        {
          guid = (Guid)oValue;
        }
        else
        {
          throw new InvalidOperationException("No value set");
        }
        return (guid);
      }
      set
      {
        ApplicationData.Current.LocalSettings.Values["id"] = value;
      }
    }
    void OnCancelled(IBackgroundTaskInstance sender,
      BackgroundTaskCancellationReason reason)
    {
    }
    public static ushort MSCompanyId
    {
      get
      {
        return (0x0006);
      }
    }
  }
}
