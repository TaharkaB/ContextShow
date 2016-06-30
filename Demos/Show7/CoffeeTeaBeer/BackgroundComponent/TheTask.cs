namespace BackgroundComponent
{
  using System;
  using Windows.ApplicationModel.Background;

  public sealed class TheTask : IBackgroundTask
  {
    public async void Run(IBackgroundTaskInstance taskInstance)
    {
      var deferral = taskInstance.GetDeferral();
      taskInstance.Canceled += OnCancelled;

      try
      {
        // We want to send a message to the band and get something back.
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