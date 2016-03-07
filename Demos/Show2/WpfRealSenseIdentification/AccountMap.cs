namespace UWPOxfordFaceIdentification
{
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  static class AccountMap
  {
    static AccountMap()
    {
      accountMap = new Dictionary<int, string>();
    }
    static void LoadAccountMap()
    {
      if (!loaded)
      {
        try
        {
          var contents = File.ReadAllText(FILENAME);

          var deserialized = JsonConvert.DeserializeObject<
            Dictionary<int, string>>(contents);

          accountMap = deserialized;

          loaded = true;
        }
        catch (FileNotFoundException)
        {
        }
      }
    }
    public static string GetNameForId(int id)
    {
      string name = string.Empty;

      LoadAccountMap();

      if (accountMap.ContainsKey(id))
      {
        name = accountMap[id];
      }
      return (name);
    }
    public static void SetNameForIdAsync(int id, string userName)
    {
      LoadAccountMap();

      accountMap[id] = userName;

      var serialized = JsonConvert.SerializeObject(accountMap);

      File.WriteAllText(FILENAME, serialized);
    }
    static bool loaded;
    static readonly string FILENAME = "map.json";
    static Dictionary<int, string> accountMap;
  }
}