namespace BuildingControl.Services
{
  using BuildingControl.Model;
  using System.IO;
  using System.Threading.Tasks;
  using Windows.Storage;
  using System;
  using Newtonsoft.Json;
  using System.Linq;

  // This could/should be migrated into a proper service which is injected but,
  // for demo purposes, we just make it static here and use it where we need
  // it.
  public static class BuildingPersistence
  {
    public static Building Instance
    {
      get; set;
    }
    public static async Task<Building> LoadAsync()
    {
      Building building = null;

      try
      {
        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(STORAGE_FILE);

        var json = await FileIO.ReadTextAsync(file);

        building = JsonConvert.DeserializeObject<Building>(json);

        // We run through the lights to make sure that if they are connected
        // to GPIO switches then we turn them on/off the right way.
        var lights = building.Rooms.SelectMany(r => r.Lights);

        foreach (var light in lights)
        {
          light.SwitchPin();
        }
      }
      catch (FileNotFoundException)
      {

      }
      return (building);
    }
    public static async Task SaveAsync(Building building, bool fireDataChanged = false)
    {
      if (building != null)
      {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(STORAGE_FILE,
          CreationCollisionOption.ReplaceExisting);

        var json = JsonConvert.SerializeObject(building);

        await FileIO.WriteTextAsync(file, json);

        if (fireDataChanged)
        {
          ApplicationData.Current.SignalDataChanged();
        }
      }
    }
    static readonly string STORAGE_FILE = "building.dat";
  }
}
