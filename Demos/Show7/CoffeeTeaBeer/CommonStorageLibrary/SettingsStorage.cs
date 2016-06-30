namespace CommonStorageLibrary
{
  using System;
  using Windows.Storage;
  enum ItemType
  {
    Installed,
    TileId,
    Coffee,
    Tea,
    Beer
  }
  class SettingsStorage
  {
    public string this[ItemType itemType]
    {
      get
      {
        this.EnsureSettings();

        var data = ApplicationData.Current.LocalSettings.Values;

        var value = (string)data[itemType.ToString()];

        return (value);
      }
      set
      {
        this.EnsureSettings();

        var data = ApplicationData.Current.LocalSettings.Values;

        data[itemType.ToString()] = value;
      }
    }
    void EnsureSettings()
    {
      var data = ApplicationData.Current.LocalSettings.Values;

      var names = Enum.GetNames(typeof(ItemType));

      foreach (var name in names)
      {
        if (!data.ContainsKey(name))
        {
          data[name] = string.Empty;
        }
      }
    }
  }
}
