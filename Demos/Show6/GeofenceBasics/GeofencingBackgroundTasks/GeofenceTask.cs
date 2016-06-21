namespace GeofencingBackgroundTasks
{
  using GeofencingBackgroundTasks.RollingGeofence;
  using System.Collections.Generic;
  using Windows.ApplicationModel.Background;
  using Windows.Devices.Geolocation.Geofencing;

  public sealed class GeofenceTask : IBackgroundTask
  {
    public async void Run(IBackgroundTaskInstance taskInstance)
    {
      var deferral = taskInstance.GetDeferral();

      try
      {
        // Get all the reports from the system for geofence activity.
        IReadOnlyList<GeofenceStateChangeReport> reports =
          GeofenceMonitor.Current.ReadReports();

        // Filter down to the reports that we recognise.
        var filteredReports = GeofenceHelper.FilterReports(reports);

        // Log them to our file for future reading and so that any UI
        // can pick them up if it wants to.
        foreach (var report in filteredReports)
        {
          await GeofenceHelper.WritePositionToLogFileAsync(
            report.NewState,
            report.Geoposition);
        }
      }
      finally
      {
        deferral.Complete();
      }
    }
  }
}