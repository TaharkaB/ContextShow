namespace BuildingControl.Services
{
  using BuildingControl.Model;
  using System.IO;
  using System.Threading.Tasks;
  using Windows.Storage;
  using System;
  using Newtonsoft.Json;

  // This could/should be migrated into a proper service which is injected but,
  // for demo purposes, we just make it static here and use it where we need
  // it.
  static class BuildingPersistence
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
      }
      catch (FileNotFoundException)
      {

      }
      return (building);
    }
    public static async Task SaveAsync(Building building)
    {
      if (building != null)
      {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(STORAGE_FILE,
          CreationCollisionOption.ReplaceExisting);

        var json = JsonConvert.SerializeObject(building);

        await FileIO.WriteTextAsync(file, json);
      }
    }
    static readonly string STORAGE_FILE = "building.dat";
  }
}
