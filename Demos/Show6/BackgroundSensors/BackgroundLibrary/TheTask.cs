namespace BackgroundLibrary
{
  using System;
  using Windows.ApplicationModel.Background;
  using Windows.Devices.Sensors;
  public sealed class TheTask : IBackgroundTask
  {
    public async void Run(IBackgroundTaskInstance taskInstance)
    {
      var deferral = taskInstance.GetDeferral();
      taskInstance.Canceled += OnCancelled;

      try
      {
        if (taskInstance.TriggerDetails is ActivitySensorTriggerDetails)
        {
          var details = (ActivitySensorTriggerDetails)taskInstance.TriggerDetails;
          var reports = details.ReadReports();
          foreach (var report in reports)
          {
            await SimpleLogger.InternalLogAsync(
              $"{DateTime.Now:hh.mm}:{report.Reading.Confidence} confidence that {report.Reading.Activity} happened");
          }
        }
        else if (taskInstance.TriggerDetails is SensorDataThresholdTriggerDetails)
        {
          var details = (SensorDataThresholdTriggerDetails)taskInstance.TriggerDetails;
          // There's no data here, we just have to figure it out.
          await SimpleLogger.InternalLogAsync(
            $"{DateTime.Now:hh.mm}:{details.SensorType} saw activity");
        }
      }
      finally
      {
        deferral.Complete();
      }
    }
    void OnCancelled(IBackgroundTaskInstance sender,
      BackgroundTaskCancellationReason reason)
    {
    }
  }
}
