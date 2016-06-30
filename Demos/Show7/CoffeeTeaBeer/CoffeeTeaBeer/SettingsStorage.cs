namespace CoffeeTeaBeer
{
  using System;
  using Windows.Storage;
  enum ItemType
  {
    Installed,
    TileId,
    PageId,
    Coffee,
    Tea,
    Beer,
    Wine
  }
  class SettingsStorage
  {
    public int GetCounter(ItemType itemType)
    {
      var value = this[itemType];
      var intValue = string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
      return (intValue);
    }
    public void IncrementCounter(ItemType itemType)
    {
      var storedValue = this.GetCounter(itemType);
      storedValue++;
      this[itemType] = storedValue.ToString();
    }
    public Guid GetGuid(ItemType itemType)
    {
      var storedValue = this[itemType];

      Guid guid =
        string.IsNullOrEmpty(storedValue) ? Guid.Empty : Guid.Parse(storedValue);

      return (guid);
    }
    public void SetGuid(ItemType itemType, Guid guid)
    {
      this[itemType] = guid.ToString();
    }
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
