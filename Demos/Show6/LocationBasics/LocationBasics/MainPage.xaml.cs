namespace LocationBasics
{
    using System.Threading.Tasks;
    using Windows.Devices.Geolocation;
    using Windows.UI.Xaml.Controls;
    using System;
    using Windows.UI.Xaml.Controls.Maps;
    using Windows.Services.Maps;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Popups;

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.mapControl.MapServiceToken = mapControlKey;
        }
        async Task<Geoposition> GetUsersPositionAsync()
        {
            Geoposition position = null;

            var status = await Geolocator.RequestAccessAsync();

            if (status == GeolocationAccessStatus.Allowed)
            {
                Geolocator locator = new Geolocator();

                // Where's the user?
                position = await locator.GetGeopositionAsync();
            }
            return (position);
        }
        async Task<MapLocation> GetStreetAddressDetails(Geoposition position)
        {
            MapLocation mapLocation = null;

            var locationResult = await MapLocationFinder.FindLocationsAtAsync(
              position.Coordinate.Point);

            if (locationResult.Status == MapLocationFinderStatus.Success)
            {
                // We'll take the first one we get back.
                mapLocation = locationResult.Locations.FirstOrDefault();
            }
            return (mapLocation);
        }
        async void OnLocateButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var position = await this.GetUsersPositionAsync();

            if (position != null)
            {
                // Update the UI.
                VisualStateManager.GoToState(this, "located", true);

                // Move the map.
                this.mapControl.Center = position.Coordinate.Point;

                // Add a circle at the location.
                var circle = new Circle();
                this.mapControl.Children.Add(circle);
                MapControl.SetLocation(circle, position.Coordinate.Point);

                // See if we can figure out some details around street address.
                this.stackAddress.DataContext = null;

                var mapLocation = await this.GetStreetAddressDetails(position);

                if (mapLocation != null)
                {
                    // Update the UI with details.
                    this.stackAddress.DataContext = mapLocation.Address;
                }
                else
                {
                    await DisplayMessageAsync("Found you but can't get a street address");
                }
            }
            else
            {
                await DisplayMessageAsync("Couldn't locate you, how disappointing");
            }
        }
        static async Task DisplayMessageAsync(string message)
        {
            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }
        static readonly string mapControlKey = "07nnZ2CDfipUmgUmJ4eD~7YH9TgCfVU9SwrbY2Z8z_Q~ApOigDmzgqkSdBX8eWKNN1tX8MKCbFeGKTWxP5GBtQwzKYUfFhtZaMJ9biQyoCW8";
    }
}
