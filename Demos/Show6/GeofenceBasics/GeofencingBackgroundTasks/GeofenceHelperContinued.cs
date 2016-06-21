namespace GeofencingBackgroundTasks
{
  namespace RollingGeofence
  {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Background;
    using Windows.Devices.Geolocation;
    using Windows.Devices.Geolocation.Geofencing;
    using Windows.Foundation;

    public static partial class GeofenceHelper
    {
      static PositionLogger logger;

      internal static async Task WritePositionToLogFileAsync(GeofenceState newState,
        Geoposition position)
      {
        await logger.Log(newState, position);
      }
      public static IAsyncAction ClearLogAsync()
      {
        return (logger.TruncateLogAsync().AsAsyncAction());
      }

      static async Task<IEnumerable<PositionLogEntry>> InternalGetLogFileEntriesAsync()
      {
        var entries = await logger.ReadEntriesAsync();
        return (entries);
      }
      internal static IEnumerable<GeofenceStateChangeReport> FilterReports(
        IEnumerable<GeofenceStateChangeReport> reports)
      {
        IEnumerable<GeofenceStateChangeReport> filteredReports = null;

        filteredReports = reports?.Where(r => (r.Geofence.Id == FENCE_ID));

        return (filteredReports);
      }
      static GeofenceHelper()
      {
        logger = new PositionLogger(LOG_FILE);
      }
      public static IAsyncAction RegisterAsync(double fenceRadius,
        MonitoredGeofenceStates monitoredStates, TimeSpan dwellTime)
      {
        return (InternalRegisterAsync(fenceRadius, monitoredStates, dwellTime).AsAsyncAction());
      }
      public static void Unregister()
      {
        UnregisterFence();
        UnregisterTask();
      }
      public static IAsyncOperation<IEnumerable<PositionLogEntry>> GetLogFileEntriesAsync()
      {
        return (InternalGetLogFileEntriesAsync().AsAsyncOperation());
      }
    }
  }
}