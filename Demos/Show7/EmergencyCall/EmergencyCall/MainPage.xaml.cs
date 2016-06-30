namespace EmergencyCall
{
  using CommonLibrary;
  using Microsoft.Band;
  using Microsoft.Band.Tiles;
  using Microsoft.Band.Tiles.Pages;
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

      // We always do this part.
      AccessoryManager.RegisterAccessoryApp();

      this.SetButtonContent();
    }
    void SetButtonContent()
    {
      this.btnRegister.Content =
        (TileGuidSetting == null) ? "Register" : "Unregister";
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
    async void OnRegisterButtonClick(object sender, RoutedEventArgs e)
    {
      if (TileGuidSetting == null)
      {
        await this.RegisterAsync();
      }
      else
      {
        await this.UnregisterAsync();
      }
      this.SetButtonContent();
    }
    async Task RegisterAsync()
    {
      using (var client = await this.GetClientForFirstBandAsync())
      {
        var tileGuid = await CreateTileAsync(client);

        // Note: we can also subscribe to tile events in the foreground
        // but this scenario makes more sense in the background.
        await client.SubscribeToBackgroundTileEventsAsync(
          tileGuid);

        TileGuidSetting = tileGuid;
      }
    }
    async Task UnregisterAsync()
    {
      using (var client = await this.GetClientForFirstBandAsync())
      {
        await RemoveTileAsync(client);

        await client.UnsubscribeFromBackgroundTileEventsAsync(
          TileGuidSetting.Value);

        TileGuidSetting = null;
      }
    }
    async Task<IBandClient> GetClientForFirstBandAsync()
    {
      var bands = await BandClientManager.Instance.GetBandsAsync();
      IBandClient client = null;

      if (bands?.Count() > 0)
      {
        client = await BandClientManager.Instance.ConnectAsync(bands.First());
      }
      return (client);
    }
    async Task<Guid> CreateTileAsync(IBandClient client)
    {
      BandTile bandTile = null;
      Guid tileGuid = Guid.NewGuid();

      var tileSpace = await client.TileManager.GetRemainingTileCapacityAsync();

      if (tileSpace > 0)
      {
        var smallIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/tileSmall.png"), 24);

        var largeIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/tileLarge.png"), 48);

        var layout = new TileLayout();

        bandTile = new BandTile(tileGuid)
        {
          Name = "Emergency",
          TileIcon = largeIcon,
          SmallIcon = smallIcon
        };
        bandTile.PageLayouts.Add(layout.Layout);

        await layout.LoadIconsAsync(bandTile);

        var added = await client.TileManager.AddTileAsync(bandTile);

        PageData pageData = new PageData(
          Guid.NewGuid(),
          0,
          layout.Data.All);

        await client.TileManager.SetPagesAsync(
          bandTile.TileId,
          pageData);
      }
      return (tileGuid);
    }
    async Task RemoveTileAsync(IBandClient client)
    {
      var bandTiles = await client.TileManager.GetTilesAsync();
      var bandTile = bandTiles.Single(b => b.TileId == TileGuidSetting);
      await client.TileManager.RemoveTileAsync(bandTile);
    }
    static readonly string SETTING_KEY = "id";
  }
}
