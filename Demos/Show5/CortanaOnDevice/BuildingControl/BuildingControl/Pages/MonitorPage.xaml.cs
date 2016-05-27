namespace BuildingControl.Pages
{
  using AllJoyn;
  using BuildingControlLibrary.Services;
  using com.taulty.LightControl;
  using Controls;
  using Model;
  using Newtonsoft.Json;
  using Services;
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;
  using Windows.Devices.AllJoyn;
  using Windows.Storage;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  using Windows.UI.Xaml.Navigation;

  public sealed partial class MonitorPage : Page, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    // Pretty horrible thing to do but makes things a lot simpler elsewhere
    // in our App class and its activation handler so we do it for the
    // demo.
    public static MonitorPage Instance { get; set; }

    public MonitorPage()
    {
      this.InitializeComponent();

      ApplicationData.Current.DataChanged += OnDataChanged;

      Instance = this;
    }
    public Building Building
    {
      get
      {
        return (this.currentBuilding);
      }
      set
      {
        this.SetProperty(ref this.currentBuilding, value);
      }
    }
    public ObservableCollection<string> RemoteBuildings
    {
      get; set;
    }
    public async Task SwitchLightsInBuildingAsync(bool onOff)
    {
      foreach (var room in this.currentBuilding.Rooms)
      {
        await this.SwitchLightsInRoomAsync(room, onOff);
      }
    }
    public async Task SwitchLightsInRoomAsync(RoomType roomType,
      bool onOff)
    {
      var room = this.Building.Rooms.SingleOrDefault(
        r => r.RoomType == roomType);

      if (room != null)
      {
        await this.SwitchLightsInRoomAsync(room, onOff);
      }
    }
    internal async Task SwitchToBuildingAsync(string building)
    {
      // This one is a bit tricky. We might just be starting up and waiting
      // for AllJoyn notifications that there are other buildings out there
      // and so there's a slew of race conditions here.
      if (building == BuildingPersistence.Instance.Name)
      {
        // NB: this doesn't switch if it doesn't need to.
        this.OnSwitchToLocalBuilding();
      }
      else
      {
        // We're in an interesting situation as we may have just spun the
        // page up and it's listening for remote buildings to come in and
        // so we give it a little time to see if that happens and, if not,
        // we leave well alone.
        await this.SwitchToRemoteBuildingAsync(building, 10);
      }
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      this.isLocalBuilding = true;

      this.RemoteBuildings = new ObservableCollection<string>();

      this.currentBuilding = BuildingPersistence.Instance;

      this.DataContext = this;

      this.StartAllJoynActivity();
    }
    void StartAllJoynActivity()
    {
      this.CreateBusAttachmentForAdvertisement();

      this.lightControlProducer = new LightControlProducer(
        this.advertisingBusAttachment)
      {
        Service = new LightControlService()
      };
      this.lightControlProducer.Start();

      RemoteLightControlManager.LightControlDiscovered += OnBuildingDiscovered;

      RemoteLightControlManager.Initialise(BuildingPersistence.Instance.Name);
    }

    async void OnBuildingDiscovered(object sender, LightControlDiscoveredEventArgs e)
    {
      await this.Dispatch(
        () =>
        {
          this.RemoteBuildings.Add(e.BuildingName);
        }
      );
    }
    void CreateBusAttachmentForAdvertisement()
    {
      this.advertisingBusAttachment = new AllJoynBusAttachment();
      this.advertisingBusAttachment.AboutData.DateOfManufacture = DateTime.Now;
      this.advertisingBusAttachment.AboutData.DefaultAppName = BuildingPersistence.Instance.Name;
      this.advertisingBusAttachment.AboutData.DefaultDescription = "Manages lighting";
      this.advertisingBusAttachment.AboutData.DefaultManufacturer = "Mike Taulty";
      this.advertisingBusAttachment.AboutData.ModelNumber = "Model 1";
      this.advertisingBusAttachment.AboutData.SoftwareVersion = "1.0";
    }
    async void OnUILightBulbPressed(object sender, PointerRoutedEventArgs e)
    {
      // Sorry about this, really should have bitten the bullet and written
      // viewmodels.
      var bulb = (LightBulb)sender;
      var light = (Light)bulb.Tag;

      light.IsOn = !light.IsOn;

      if (this.isLocalBuilding)
      {
        await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);
      }
      else
      {
        await this.currentConsumer?.ToggleRoomLightAsync(
          light.Room.ToString(), light.Id);
      }
      e.Handled = true;
    }
    async void OnUISwitchBuildingOn(object sender, RoutedEventArgs e)
    {
      await this.SwitchLightsInBuildingAsync(true);
    }
    async void OnUISwitchBuildingOff(object sender, RoutedEventArgs e)
    {
      await this.SwitchLightsInBuildingAsync(false);
    }
    async void OnUISwitchRoomOn(object sender, RoutedEventArgs e)
    {
      // code that only a mother could love, sorry!
      var room = (Room)(((Button)sender).Tag);
      await this.SwitchLightsInRoomAsync(room, true);
    }
    async void OnUISwitchRoomOff(object sender, RoutedEventArgs e)
    {
      var room = (Room)(((Button)sender).Tag);
      await this.SwitchLightsInRoomAsync(room, false);
    }
    async Task SwitchLightsInRoomAsync(Room room, bool onOff)
    {
      room.SwitchLights(onOff);

      if (this.isLocalBuilding)
      {
        await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);
      }
      else
      {
        await this.currentConsumer?.SwitchRoomAsync(
          room.RoomType.ToString(),
          onOff);
      }
    }
    async void OnUISwitchToRemoteBuilding(object sender, RoutedEventArgs e)
    {
      if (lstOtherBuildings.SelectedItem != null)
      {
        var otherBuilding = (string)lstOtherBuildings.SelectedItem;

        await SwitchToRemoteBuildingAsync(otherBuilding);
      }
    }
    async Task SwitchToRemoteBuildingAsync(string otherBuilding,
      int timeoutInSeconds = 0)
    {
      this.currentConsumer =
        await RemoteLightControlManager.GetConsumerForRemoteBuildingAsync(
          otherBuilding,
          timeoutInSeconds);

      var json = await this.currentConsumer.GetBuildingDefinitionJsonAsync();

      if (json.Status == AllJoynStatus.Ok)
      {
        var building = JsonConvert.DeserializeObject<Building>(json.Json);

        this.Building = building;

        this.isLocalBuilding = false;
      }
    }
    void OnSwitchToLocalBuilding()
    {
      if (!this.isLocalBuilding)
      {
        this.currentConsumer = null;
        this.isLocalBuilding = true;
        this.Building = BuildingPersistence.Instance;
      }
    }
    async void OnDataChanged(ApplicationData sender, object args)
    {
      await this.Dispatch(
        async () =>
        {
          // Something has written our data file so we should reread it.
          BuildingPersistence.Instance = await BuildingPersistence.LoadAsync();

          // If we are showing that building on the screen then we need to refresh.
          if (this.isLocalBuilding)
          {
            // to ensure change notification fires.
            this.Building = null;
            this.Building = BuildingPersistence.Instance;
          }
        }
      );
    }
    void OnUISwitchToLocalBuilding(object sender, RoutedEventArgs e)
    {
      this.OnSwitchToLocalBuilding();
    }
    bool SetProperty<T>(ref T storage, T value,
      [CallerMemberName] String propertyName = null)
    {
      if (object.Equals(storage, value)) return false;

      storage = value;
      this.OnPropertyChanged(propertyName);
      return true;
    }
    void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      this.PropertyChanged?.Invoke(
        this, new PropertyChangedEventArgs(propertyName));
    }
    async Task Dispatch(Action action)
    {
      await this.Dispatcher.RunAsync(
        Windows.UI.Core.CoreDispatcherPriority.Normal,
        () =>
        {
          action();
        }
      );
    }
    bool isLocalBuilding;
    Building currentBuilding;
    LightControlProducer lightControlProducer;
    AllJoynBusAttachment advertisingBusAttachment;
    LightControlConsumer currentConsumer;
  }
}