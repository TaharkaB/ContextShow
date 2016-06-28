namespace EmergencyCall
{
  using CommonLibrary;
  using Microsoft.Band;
  using Microsoft.Band.Tiles;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.Phone.Notification.Management;
  using Windows.Storage;
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

      AccessoryManager.RegisterAccessoryApp();
      await this.GetOrCreateTileAsync();
    }
    static Guid? TileGuidSetting
    {
      get
      {
        object oValue = null;
        Guid? value = null;

        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(
          SETTING_KEY,
          out oValue))
        {
          value = (Guid)oValue;
        }
        return (value);
      }
      set
      {
        if (value.HasValue)
        {
          ApplicationData.Current.LocalSettings.Values[SETTING_KEY] = value.Value;
        }
        else
        {
          ApplicationData.Current.LocalSettings.Values.Remove(SETTING_KEY);
        }
      }
    }
    async Task GetOrCreateTileAsync()
    {
      var bands = await BandClientManager.Instance.GetBandsAsync();

      if (bands?.Count() > 0)
      {
        this.client = await BandClientManager.Instance.ConnectAsync(bands.First());
      }

      if (TileGuidSetting.HasValue)
      {
        var allTiles = await this.client.TileManager.GetTilesAsync();
        this.bandTile = allTiles.Single(t => t.TileId == TileGuidSetting.Value);
      }
      else
      {
        this.bandTile = await CreateTileAsync();
      }
      this.client.TileManager.TileOpened += OnTileOpened;
      await this.client.TileManager.StartReadingsAsync();
    }
    async void OnTileOpened(
      object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
    {
      await this.Dispatcher.RunAsync(
        Windows.UI.Core.CoreDispatcherPriority.Normal,
        async () =>
        {
          if (e.TileEvent.TileId == this.bandTile.TileId)
          {
            // Display a message on the band.
            await this.client.NotificationManager.ShowDialogAsync(
              this.bandTile.TileId,
              "Ringing",
              "Ringing now");

            // We make our call. This is hard-coded for now.
            var line = AccessoryManager.PhoneLineDetails.FirstOrDefault();

            if (line != null)
            {
              this.okGrid.Visibility = Visibility.Collapsed;
              this.ringingGrid.Visibility = Visibility.Visible;
              AccessoryManager.MakePhoneCall(line.LineId, Constants.PhoneNumber);
            }
          }
        }
      );
    }
    async Task<BandTile> CreateTileAsync()
    {
      BandTile bandTile = null;

      var tileSpace = await this.client.TileManager.GetRemainingTileCapacityAsync();

      if (tileSpace > 0)
      {
        var smallIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/tileSmall.png"), 24);

        var largeIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/tileLarge.png"), 48);

        var tileGuid = Guid.NewGuid();        

        bandTile = new BandTile(tileGuid)
        {
          Name = "Emergency",
          TileIcon = largeIcon,
          SmallIcon = smallIcon
        };
        var added = await this.client.TileManager.AddTileAsync(bandTile);

        // TBD: I've had this working in the past but on the current bits
        // my background events are not firing.
        await this.client.SubscribeToBackgroundTileEventsAsync(tileGuid);

        TileGuidSetting = tileGuid;
      }
      return (bandTile);
    }
    static readonly string SETTING_KEY = "id";
    IBandClient client;
    BandTile bandTile;
  }
}

// If we later want to remove things, we'd need to be able to
// get to this function
/*
async void OnRemove(object sender, RoutedEventArgs e)
{
  var tileManager = await this.GetTileManagerForFirstBandAsync();

  await tileManager.RemoveTileAsync(TileGuidSetting.Value);

  TileGuidSetting = null;
}
*/
