namespace BuildingControlLibrary.Services
{
  using com.taulty.LightControl;
  using System;
  using System.Collections.Concurrent;
  using System.Threading.Tasks;
  using Windows.Devices.AllJoyn;

  public class LightControlDiscoveredEventArgs : EventArgs
  {
    public string BuildingName { get; set; }
  }
  public static class RemoteLightControlManager
  {
    public static event EventHandler<LightControlDiscoveredEventArgs> LightControlDiscovered;

    static RemoteLightControlManager()
    {
      cachedConsumers = new ConcurrentDictionary<string, LightControlConsumer>();
      discoveredServices = new ConcurrentDictionary<string, AllJoynServiceInfo>();
    }
    public static void Initialise(string buildingNameToIgnore)
    {
      if (!initialised)
      {
        initialised = true;

        localBuildingName = buildingNameToIgnore;

        watchingBusAttachment = new AllJoynBusAttachment();

        lightControlWatcher = new LightControlWatcher(watchingBusAttachment);
        lightControlWatcher.Added += OnLightControlAdded;
        lightControlWatcher.Start();
      }
    }
    static async void OnLightControlAdded(
      LightControlWatcher sender, AllJoynServiceInfo args)
    {
      AllJoynAboutDataView advertisementMetadata =
        await AllJoynAboutDataView.GetDataBySessionPortAsync(
          args.UniqueName, watchingBusAttachment, args.SessionPort);

      var buildingName = advertisementMetadata.AppName;

      if (buildingName != localBuildingName)
      {
        discoveredServices[buildingName] = args;

        LightControlDiscovered?.Invoke(null,
          new LightControlDiscoveredEventArgs()
          {
            BuildingName = buildingName
          });
      }
    }
    public static async Task<LightControlConsumer> GetConsumerForRemoteBuildingAsync(
      string buildingName,
      int timeoutInSeconds)
    {
      LightControlConsumer consumer = null;

      int timeoutCount = 0;

      while (!discoveredServices.ContainsKey(buildingName))
      {
        // We can wait and see if it shows up. It may not and this is very
        // simplistic but we'll wait.
        await Task.Delay(TimeSpan.FromSeconds(1));

        if (++timeoutCount >= timeoutInSeconds)
        {
          break;
        }
      }
      if (discoveredServices.ContainsKey(buildingName))
      {
        if (!cachedConsumers.ContainsKey(buildingName))
        {
          var allJoynInfo = discoveredServices[buildingName];

          var result = await LightControlConsumer.JoinSessionAsync(
            allJoynInfo,
            lightControlWatcher);

          if (result.Status == AllJoynStatus.Ok)
          {
            consumer = result.Consumer;
            cachedConsumers[buildingName] = consumer;
          }
        }
        else
        {
          consumer = cachedConsumers[buildingName];
        }
      }
      return (consumer);
    }
    static bool initialised;
    static string localBuildingName;
    static LightControlWatcher lightControlWatcher;
    static AllJoynBusAttachment watchingBusAttachment;
    static ConcurrentDictionary<string, AllJoynServiceInfo> discoveredServices;
    static ConcurrentDictionary<string, LightControlConsumer> cachedConsumers;
  }
}
