namespace GeofenceBasics
{
  using GeofencingBackgroundTasks.RollingGeofence;
  using System.Threading.Tasks;
  using Windows.UI.Xaml.Controls;
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using GeofencingBackgroundTasks;
  using System.Collections.Generic;
  using System.Linq;
  using Windows.UI.Xaml;
  using Windows.Devices.Geolocation.Geofencing;
  using Windows.UI.Core;
  using Windows.ApplicationModel.Background;

  public sealed partial class MainPage : Page, INotifyPropertyChanged
  {
    public IEnumerable<PositionLogEntry> LogEntries
    {
      get
      {
        return (this.logEntries);
      }
    }

    public string CurrentStatus
    {
      get
      {
        return (this.currentStatus);
      }
      set
      {
        this.SetProperty(ref this.currentStatus, value);
      }
    }
    public bool IsRegistered
    {
      get
      {
        return (this.isRegistered);
      }
      set
      {
        this.SetProperty(ref this.isRegistered, value);
      }
    }
    void UpdateStatusDependentProperties()
    {
      this.CurrentStatus = GeofenceHelper.IsRegistered ? "registered" : "not registered";
      this.IsRegistered = GeofenceHelper.IsRegistered;
    }
    void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetProperty<T>(ref T storage, T value,
      [CallerMemberName] String propertyName = null)
    {
      if (object.Equals(storage, value)) return false;

      storage = value;
      this.OnPropertyChanged(propertyName);
      return true;
    }
  }
}
