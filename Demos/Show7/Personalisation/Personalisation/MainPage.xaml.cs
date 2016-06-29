namespace Personalisation
{
  using Microsoft.Band;
  using Microsoft.Band.Personalization;
  using System;
  using System.Threading.Tasks;
  using Windows.Foundation;
  using Windows.Storage.Pickers;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media.Imaging;
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

        // doesn't really matter as it's at the back in the z-order anyway.
        this.gridConnect.Visibility = Visibility.Collapsed;
        this.gridCustomise.Visibility = Visibility.Visible;

        await this.LoadBandPersonalisedDetailsAsync();
      }
    }
    async Task LoadBandPersonalisedDetailsAsync()
    {
      this.image = await this.bandClient.PersonalizationManager.GetMeTileImageAsync();
      this.theme = await this.bandClient.PersonalizationManager.GetThemeAsync();

      this.writeableBitmap = image.ToWriteableBitmap();
      this.bandImage.Source = writeableBitmap;

      var colourProperties = new[]
      {
        new
        {
          Name = nameof(this.theme.Base),
          Colour = this.theme.Base.ToColor()
        },
        new
        {
          Name = nameof(this.theme.Highlight),
          Colour = this.theme.Highlight.ToColor()
        },
        new
        {
          Name = nameof(this.theme.Lowlight),
          Colour = this.theme.Lowlight.ToColor()
        },
        new
        {
          Name = nameof(this.theme.Muted),
          Colour = this.theme.Muted.ToColor()
        },
        new
        {
          Name = nameof(this.theme.SecondaryText),
          Colour = this.theme.SecondaryText.ToColor()
        },
        new
        {
          Name = nameof(this.theme.HighContrast),
          Colour = this.theme.HighContrast.ToColor()
        }
      };
      this.lstColours.DataContext = colourProperties;
    }
    async void OnReplaceImage(object sender, RoutedEventArgs e)
    {
      var dialog = new FileOpenPicker();
      dialog.CommitButtonText = "Replace";
      dialog.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
      dialog.FileTypeFilter.Add(".jpg");
      dialog.FileTypeFilter.Add(".png");

      var file = await dialog.PickSingleFileAsync();

      if (file != null)
      {
        using (var stream = await file.OpenReadAsync())
        {
          var bitmap = new WriteableBitmap(
            (int)bandImageSize.Width, (int)bandImageSize.Height);

          bitmap.SetSource(stream);

          await this.bandClient.PersonalizationManager.SetMeTileImageAsync(
            bitmap.ToBandImage());

          this.writeableBitmap = bitmap;

          this.bandImage.Source = this.writeableBitmap;
        }
      }
    }
    IBandClient bandClient;
    WriteableBitmap writeableBitmap;
    BandTheme theme;
    BandImage image;
    static readonly Size bandImageSize = new Size(310, 102);
  }
}
