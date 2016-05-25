namespace BuildingControl.Pages
{
  using System;
  using Services;
  using Windows.UI.Xaml.Controls;
  using System.Threading.Tasks;
  using Windows.Devices.AllJoyn;
  using com.taulty.LightControl;
  using AllJoyn;
  using Windows.UI.Xaml.Navigation;
  using Controls;
  using Model;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Input;
  using System.Collections.Concurrent;
  using System.Collections.ObjectModel;
  using Newtonsoft.Json;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Linq;
  public sealed partial class MonitorPage : Page, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public MonitorPage()
    {
      this.InitializeComponent();
    }
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      this.isLocalBuilding = true;

      this.RemoteBuildings = new ObservableCollection<AllJoynRemoteBuilding>();

      this.currentBuilding = BuildingPersistence.Instance;

      this.DataContext = this;

      await this.StartAllJoynActivityAsync();
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
    public ObservableCollection<AllJoynRemoteBuilding> RemoteBuildings
    {
      get; set;
    }
    async Task StartAllJoynActivityAsync()
    {
      this.CreateBusAttachments();

      this.lightControlWatcher = new LightControlWatcher(this.watchingBusAttachment);
      this.lightControlWatcher.Added += OnLightControlAdded;
      this.lightControlWatcher.Start();

      this.lightService = new LightControlService();

      this.lightControlProducer = new LightControlProducer(this.advertisingBusAttachment)
      {
        Service = this.lightService
      };
      this.lightControlProducer.Start();
    }
    async void OnLightControlAdded(LightControlWatcher sender, AllJoynServiceInfo args)
    {
      // TBD: my way of trying to avoid connecting back to ourselves.
      if (this.advertisingBusAttachment.UniqueName != args.UniqueName)
      {
        AllJoynAboutDataView advertisementMetadata =
          await AllJoynAboutDataView.GetDataBySessionPortAsync(
            args.UniqueName, this.watchingBusAttachment, args.SessionPort);

        var buildingName = advertisementMetadata.AppName;

        await this.Dispatch(
          () =>
          {
            if (!this.RemoteBuildings.Any(b => b.Name == buildingName))
            {
              this.RemoteBuildings.Add(new AllJoynRemoteBuilding()
              {
                Name = buildingName,
                AllJoynInfo = args
              });
            }
          }
        );
      }
    }
    void CreateBusAttachments()
    {
      this.advertisingBusAttachment = new AllJoynBusAttachment();
      this.watchingBusAttachment = new AllJoynBusAttachment();

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
        await this.lightControlConsumer?.ToggleRoomLightAsync(
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
    async Task SwitchLightsInBuildingAsync(bool onOff)
    {
      foreach (var room in this.currentBuilding.Rooms)
      {
        await this.SwitchLightsInRoomAsync(room, onOff);
      }
    }
    async void OnUISwitchRoomOn(object sender, RoutedEventArgs e)
    {
      // code that only a mother could love.
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
        await this.lightControlConsumer?.SwitchRoomAsync(
          room.RoomType.ToString(),
          onOff);
      }
    }
    async void OnUISwitchToRemoteBuilding(object sender, RoutedEventArgs e)
    {
      if (lstOtherBuildings.SelectedItem != null)
      {
        var otherBuilding = (AllJoynRemoteBuilding)lstOtherBuildings.SelectedItem;

        var result = await LightControlConsumer.JoinSessionAsync(
          otherBuilding.AllJoynInfo,
          this.lightControlWatcher);

        if (result.Status == AllJoynStatus.Ok)
        {
          this.lightControlConsumer = result.Consumer;

          var json = await this.lightControlConsumer.GetBuildingDefinitionJsonAsync();

          if (json.Status == AllJoynStatus.Ok)
          {
            var building = JsonConvert.DeserializeObject<Building>(json.Json);
            this.Building = building;

            this.isLocalBuilding = false;
          }
        }
      }
    }
    void OnUISwitchToLocalBuilding(object sender, RoutedEventArgs e)
    {
      if (!this.isLocalBuilding)
      {
        this.lightControlConsumer?.Dispose();
        this.lightControlConsumer = null;
        this.isLocalBuilding = true;
        this.Building = BuildingPersistence.Instance;
      }
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
      var eventHandler = this.PropertyChanged;
      if (eventHandler != null)
      {
        eventHandler(this, new PropertyChangedEventArgs(propertyName));
      }
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
    LightControlService lightService;
    LightControlProducer lightControlProducer;
    LightControlWatcher lightControlWatcher;
    AllJoynBusAttachment advertisingBusAttachment;
    AllJoynBusAttachment watchingBusAttachment;
    LightControlConsumer lightControlConsumer;
  }
}
