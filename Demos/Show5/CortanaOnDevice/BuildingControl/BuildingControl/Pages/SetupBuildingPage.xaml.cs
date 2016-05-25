namespace BuildingControl.Pages
{
  using Services;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class SetupBuildingPage : Page
  {
    public SetupBuildingPage()
    {
      this.InitializeComponent();
    }

    async void OnNameBuildingAndContinue(object sender, RoutedEventArgs e)
    {
      if (!string.IsNullOrEmpty(this.txtBuildingName.Text))
      {
        BuildingPersistence.Instance = new Model.Building();
        BuildingPersistence.Instance.Name = this.txtBuildingName.Text;
        await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);
        Navigator.Navigate(typeof(SetupRoomsPage), null);
      }
    }
  }
}
