namespace CoffeeTeaBeer
{
  using Microsoft.Band;
  using Microsoft.Band.Tiles;
  using Microsoft.Band.Tiles.Pages;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.UI.Core;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    SettingsStorage storage;

    public MainPage()
    {
      this.InitializeComponent();

      // We do this here on the main page, not because it's the right
      // place to do it but because it's cheap.
      this.Loaded += OnLoaded;
    }
    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.storage = new SettingsStorage();

      this.designedLayout = new DesignedBandTileLayout();

      await this.GetBandClientAsync();

      this.OneTimeInitialise();

      // XAML UI update from storage
      this.UpdateUI();
      // Band UI update from storage
      await this.UpdatePageDataAsync();

      await StartHandlingTileEventsAsync();
    }
    async Task GetBandClientAsync()
    {
      var bands = await BandClientManager.Instance.GetBandsAsync();

      var firstBand = bands.FirstOrDefault();

      if (firstBand != null)
      {
        this.bandClient = 
          await BandClientManager.Instance.ConnectAsync(firstBand);
      }
    }
    async void OneTimeInitialise()
    {
      if (string.IsNullOrEmpty(this.storage[ItemType.Installed]))
      {
        await CreateTileOnPhoneAsync();
        this.storage[ItemType.Installed] = true.ToString();
      }
    }
    async Task StartHandlingTileEventsAsync()
    {
      this.bandClient.TileManager.TileOpened += OnTileOpened;
      this.bandClient.TileManager.TileButtonPressed += OnTileButtonPressed;
      await this.bandClient.TileManager.StartReadingsAsync();
    }
    async Task CreateTileOnPhoneAsync()
    {
      BandTile bandTile = null;

      var tileSpace = await this.bandClient.TileManager.GetRemainingTileCapacityAsync();

      if (tileSpace > 0)
      {
        var smallIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/tileSmall.png"), 24);

        var largeIcon = await TileImageUtility.MakeTileIconFromFileAsync(
          new Uri("ms-appx:///Assets/tileLarge.png"), 48);

        var tileGuid = Guid.NewGuid();

        bandTile = new BandTile(tileGuid)
        {
          Name = "Coffee",
          TileIcon = largeIcon,
          SmallIcon = smallIcon
        };
        bandTile.PageLayouts.Add(this.designedLayout.Layout);

        var added = await this.bandClient.TileManager.AddTileAsync(bandTile);

        this.storage.SetGuid(ItemType.TileId, tileGuid);
        this.storage.SetGuid(ItemType.PageId, Guid.NewGuid());

        await UpdatePageDataAsync();
      }
    }
    async Task UpdatePageDataAsync()
    {
      if (this.storage.GetGuid(ItemType.PageId) != Guid.Empty)
      {
        if (this.pageData == null)
        {
          this.pageData = new PageData(
            this.storage.GetGuid(ItemType.PageId),
            0,
            this.designedLayout.Data.All);
        }
        this.designedLayout.txtBeerData.Text =
          this.storage.GetCounter(ItemType.Beer).ToString();

        this.designedLayout.txtCoffeeData.Text =
          this.storage.GetCounter(ItemType.Coffee).ToString();

        this.designedLayout.txtTeaData.Text =
          this.storage.GetCounter(ItemType.Tea).ToString();

        this.designedLayout.txtWineData.Text =
          this.storage.GetCounter(ItemType.Wine).ToString();

        await this.bandClient.TileManager.SetPagesAsync(
          this.storage.GetGuid(ItemType.TileId),
          this.pageData);
      }
    }
    async void OnTileButtonPressed(object sender,
      BandTileEventArgs<IBandTileButtonPressedEvent> e)
    {
      if (e.TileEvent.ElementId == this.designedLayout.btnBeer.ElementId)
      {
        this.storage.IncrementCounter(ItemType.Beer);
      }
      else if (e.TileEvent.ElementId == this.designedLayout.btnCoffee.ElementId)
      {
        this.storage.IncrementCounter(ItemType.Coffee);
      }
      else if (e.TileEvent.ElementId == this.designedLayout.btnTea.ElementId)
      {
        this.storage.IncrementCounter(ItemType.Tea);
      }
      else if (e.TileEvent.ElementId == this.designedLayout.btnWine.ElementId)
      {
        this.storage.IncrementCounter(ItemType.Wine);
      }
      await this.UpdatePageDataAsync();
      this.UpdateUI();
    }
    async void UpdateUI()
    {
      await this.Dispatcher.RunAsync(
        CoreDispatcherPriority.Normal,
        () =>
        {
          this.txtBeer.Text = this.storage.GetCounter(ItemType.Beer).ToString();
          this.txtWine.Text = this.storage.GetCounter(ItemType.Wine).ToString();
          this.txtCoffee.Text = this.storage.GetCounter(ItemType.Coffee).ToString();
          this.txtTea.Text = this.storage.GetCounter(ItemType.Tea).ToString();
        }
      );
    }
    void OnTileOpened(object sender,
      BandTileEventArgs<IBandTileOpenedEvent> e)
    {

    }
    async void OnUICoffeeClick(object sender, RoutedEventArgs e)
    {
      this.storage.IncrementCounter(ItemType.Coffee);
      this.UpdateUI();
      await this.UpdatePageDataAsync();
    }
    async void OnUITeaClick(object sender, RoutedEventArgs e)
    {
      this.storage.IncrementCounter(ItemType.Tea);
      this.UpdateUI();
      await this.UpdatePageDataAsync();
    }
    async void OnUIBeerClick(object sender, RoutedEventArgs e)
    {
      this.storage.IncrementCounter(ItemType.Beer);
      this.UpdateUI();
      await this.UpdatePageDataAsync();    
    }
    async void OnUIWineClick(object sender, RoutedEventArgs e)
    {
      this.storage.IncrementCounter(ItemType.Wine);
      this.UpdateUI();
      await this.UpdatePageDataAsync();
    }
    PageData pageData;
    DesignedBandTileLayout designedLayout;
    IBandClient bandClient;
  }
}