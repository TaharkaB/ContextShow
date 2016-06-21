namespace GeofencingBackgroundTasks
{
  namespace RollingGeofence
  {
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Background;
    using Windows.Devices.Geolocation;
    using Windows.Devices.Geolocation.Geofencing;

    public static partial class GeofenceHelper
    {
      public static bool IsRegistered
      {
        get
        {
          return (IsTaskRegistered || IsFenceRegistered);
        }
      }
      static bool IsTaskRegistered
      {
        get
        {
          var hasTask = BackgroundTaskRegistration.AllTasks.Any(reg =>
            reg.Value.Name == TASK_NAME);

          return (hasTask);
        }
      }
      static bool IsFenceRegistered
      {
        get
        {
          var hasFence = GeofenceMonitor.Current.Geofences.Any(
            fence => fence.Id == FENCE_ID);

          return (hasFence);
        }
      }
      static IBackgroundTaskRegistration TaskRegistration
      {
        get
        {
          if (!IsTaskRegistered)
          {
            throw new InvalidOperationException("background task not yet registered");
          }
          IBackgroundTaskRegistration registration =
            BackgroundTaskRegistration.AllTasks.First(t => t.Value.Name == TASK_NAME).Value;

          return (registration);
        }
      }
      public static void AddTaskCompletedHandler(
        BackgroundTaskCompletedEventHandler handler)
      {
        TaskRegistration.Completed += handler;
      }
      public static void RemoveTaskCompletedHandler(
        BackgroundTaskCompletedEventHandler handler)
      {
        TaskRegistration.Completed -= handler;
      }

      static async Task InternalRegisterAsync(double fenceRadius,
        MonitoredGeofenceStates monitoredStates,
        TimeSpan dwellTime)
      {
        try
        {
          if (IsRegistered)
          {
            throw new InvalidOperationException("Already registered");
          }
          var access = await BackgroundExecutionManager.RequestAccessAsync();

          if ((access != BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity) &&
            (access != BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity))
          {
            throw new InvalidOperationException("Bad exception but we've no access!");
          }
          await RegisterFenceAsync(fenceRadius, monitoredStates, dwellTime);
          await RegisterTaskAsync();
        }
        catch
        {
          Unregister();
          throw;
        }
      }
      static async Task RegisterFenceAsync(double fenceRadius,
        MonitoredGeofenceStates monitoredStates,
        TimeSpan dwellTime)
      {
        // Where are we?
        BasicGeoposition position = await GetCurrentLocationAsync();

        Geofence fence = null;

        // Get rid of any existing geofences. This is a bit of a TBD. The fence I'm
        // creating is meant to be a single-use fence so my understanding would be
        // that the system gets rid of it for me. Maybe the system will do that 
        // "later" but, right now, it's still there so if I try and add a fence
        // with the same ID I'll get an error so I remove the original fence
        // myself.
        if (IsFenceRegistered)
        {
          fence = GeofenceMonitor.Current.Geofences.First(
            f => f.Id == FENCE_ID);

          GeofenceMonitor.Current.Geofences.Remove(fence);
        }
        fence = new Geofence(
          FENCE_ID,
          new Geocircle(position, fenceRadius),
          monitoredStates,
          false, // means it's not a single-use event
          dwellTime);

        GeofenceMonitor.Current.Geofences.Add(fence);
      }
      static async Task<BasicGeoposition> GetCurrentLocationAsync()
      {
        Geolocator locator = new Geolocator();

        Geoposition position = await locator.GetGeopositionAsync(
          TimeSpan.FromSeconds(LOCATION_AGE_SEC),
          TimeSpan.FromSeconds(LOCATION_TIMEOUT_SEC));

        return (position.Coordinate.Point.Position);
      }
      static async Task RegisterTaskAsync()
      {
        await BackgroundExecutionManager.RequestAccessAsync();

        BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
        builder.Name = TASK_NAME;
        builder.SetTrigger(new LocationTrigger(LocationTriggerType.Geofence));
        builder.TaskEntryPoint = typeof(GeofenceTask).FullName;
        builder.Register();
      }

      static void UnregisterFence()
      {
        if (IsFenceRegistered)
        {
          Geofence fence = GeofenceMonitor.Current.Geofences.First(
            f => f.Id == FENCE_ID);

          GeofenceMonitor.Current.Geofences.Remove(fence);
        }
      }
      static void UnregisterTask()
      {
        if (IsTaskRegistered)
        {
          TaskRegistration.Unregister(false);
        }
      }
      static readonly int LOCATION_TIMEOUT_SEC = 10;
      static readonly int LOCATION_AGE_SEC = 60;
      static readonly string FENCE_ID = "demoFence";
      static readonly string TASK_NAME = FENCE_ID + "Task";
      static readonly string LOG_FILE = "positionLog.txt";
    }
  }
}
