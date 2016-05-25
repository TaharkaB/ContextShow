namespace BuildingControl.Pages
{
  using Model;
  using PI;
  using Services;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class SetupRoomsPage : Page
  {
    public SetupRoomsPage()
    {
      this.InitializeComponent();

      this.Rooms = new ObservableCollection<RoomDataViewModel>();

      this.Loaded += (s, e) =>
      {
        this.DataContext = this;
      };
    }
    public ObservableCollection<RoomDataViewModel> Rooms
    {
      get;
      set;
    }
    public RoomDataViewModel SelectedRoom
    {
      get;
      set;
    }
    public RoomType SelectedRoomType
    {
      get;
      set;
    }
    public RoomType[] RoomTypes
    {
      get
      {
        return ((RoomType[])Enum.GetValues(typeof(RoomType)));
      }
    }
    void OnAddRoom(object sender, RoutedEventArgs e)
    {
      this.Rooms.Add(new RoomDataViewModel()
      {
        Count = 0,
        RoomType = this.SelectedRoomType
      });
    }
    void OnIncrementRooms(object sender, RoutedEventArgs e)
    {
      if (this.SelectedRoom != null)
      {
        this.SelectedRoom.Count++;
      }
    }
    void OnDecrementRooms(object sender, RoutedEventArgs e)
    {
      if (this.SelectedRoom != null)
      {
        this.SelectedRoom.Count--;
      }
    }
    async void OnDone(object sender, RoutedEventArgs e)
    {
      BuildingPersistence.Instance.Rooms = new List<Room>();
      int gpioPin = 0;

      foreach (var room in this.Rooms)
      {
        BuildingPersistence.Instance.Rooms.Add(
          new Room()
          {
            RoomType = room.RoomType,
            Lights =
              Enumerable
                .Range(1, room.Count)
                .Select(
                  (c, i) => new Light()
                  {
                    Id = i,
                    GpioPinNumber = GpioPinLookup.GetGpioPinIndexForOrdinal(gpioPin++),
                    Room = room.RoomType
                  }
                ).ToList()
          }
        );
      }
      await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);

      Navigator.Navigate(typeof(MonitorPage), null);
    }
  }
}