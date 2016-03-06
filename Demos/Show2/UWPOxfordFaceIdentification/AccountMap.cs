namespace UWPOxfordFaceIdentification
{
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using Windows.Storage;

  static class AccountMap
  {
    static AccountMap()
    {
      accountMap = new Dictionary<Guid, string>();
    }
    static async Task LoadAccountMapAsync()
    {
      if (!loaded)
      {
        try
        {
          var file = await ApplicationData.Current.LocalFolder.GetFileAsync(
            FILENAME);

          var text = await FileIO.ReadTextAsync(file);

          var deserialized = JsonConvert.DeserializeObject<
            Dictionary<Guid, string>>(text);

          accountMap = deserialized;

          loaded = true;
        }
        catch (FileNotFoundException)
        {
        }
      }
    }
    public static async Task<string> GetNameForGuidAsync(Guid guid)
    {
      string name = string.Empty;

      await LoadAccountMapAsync();

      if (accountMap.ContainsKey(guid))
      {
        name = accountMap[guid];
      }
      return (name);
    }
    public static async Task SetNameForGuidAsync(Guid guid, string userName)
    {
      await LoadAccountMapAsync();

      accountMap[guid] = userName;

      var serialized = JsonConvert.SerializeObject(accountMap);

      var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
        FILENAME, CreationCollisionOption.ReplaceExisting);

      await FileIO.WriteTextAsync(file, serialized);
    }
    static bool loaded;
    static readonly string FILENAME = "map.json";
    static Dictionary<Guid, string> accountMap;
  }
}