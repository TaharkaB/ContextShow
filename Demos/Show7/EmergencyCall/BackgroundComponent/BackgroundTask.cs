namespace BackgroundComponent
{
  using CommonLibrary;
  using Microsoft.Band;
  using Microsoft.Band.Tiles;
  using NotificationsExtensions.Toasts;
  using System.Linq;
  using Windows.ApplicationModel.AppService;
  using Windows.ApplicationModel.Background;
  using Windows.Phone.Notification.Management;
  using Windows.UI.Notifications;
  public sealed class BackgroundTask
  {
    static readonly string BAND_OBSERVER_SERVICE_NAME = "com.microsoft.band.observer";

    public void Run(IBackgroundTaskInstance taskInstance)
    {
      this.deferral = taskInstance.GetDeferral();

      taskInstance.Canceled += OnCancelled;

      var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

      if (triggerDetails.Name == BAND_OBSERVER_SERVICE_NAME)
      {
        triggerDetails.AppServiceConnection.RequestReceived += this.OnRequestReceived;
      }
    }
    void OnRequestReceived(AppServiceConnection sender,
      AppServiceRequestReceivedEventArgs args)
    {
      if (!this.addedHandlers)
      {
        this.addedHandlers = true;
        BackgroundTileEventHandler.Instance.TileOpened += this.OnTileOpened;
      }
      // Ask this class to figure out the details of the message that's
      // coming in.
      BackgroundTileEventHandler.Instance.HandleTileEvent(args.Request.Message);
    }

    private void OnTileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
    {
      // most scenarios would need this, I don't.
      var tileId = e.TileEvent.TileId;

      // We make our call. This is hard-coded for now.
      var line = AccessoryManager.PhoneLineDetails.FirstOrDefault();

      if (line != null)
      {
        AccessoryManager.MakePhoneCall(line.LineId, Constants.PhoneNumber);
      }
    }
    void OnCancelled(IBackgroundTaskInstance sender,
      BackgroundTaskCancellationReason reason)
    {
      this.deferral?.Complete();
      this.deferral = null;
    }
    bool addedHandlers;
    BackgroundTaskDeferral deferral;
  }
}

