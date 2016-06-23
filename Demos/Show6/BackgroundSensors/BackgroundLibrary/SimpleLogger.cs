namespace BackgroundLibrary
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using Windows.Foundation;
  using Windows.Storage;

  public static class SimpleLogger
  {
    public static IAsyncAction LogAsync(string message)
    {
      return(InternalLogAsync(message).AsAsyncAction());
    }
    public static IAsyncOperation<IEnumerable<string>> ReadEntriesAsync()
    {
      return (InternalReadEntriesAsync().AsAsyncOperation());
    }
    public static IAsyncAction TruncateLogAsync()
    {
      return (InternalTruncateLogAsync().AsAsyncAction());
    }
    internal static async Task InternalLogAsync(string message)
    {
      StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
        logFileName, CreationCollisionOption.OpenIfExists);

      await FileIO.AppendLinesAsync(file, new string[] { message });
    }
    internal static async Task<IEnumerable<string>> InternalReadEntriesAsync()
    {
      List<string> list = null;

      try
      {
        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(
          logFileName);

        var lines = await FileIO.ReadLinesAsync(file);
        list = new List<string>(lines);
      }
      catch (FileNotFoundException)
      {

      }
      return (list);
    }
    internal static async Task InternalTruncateLogAsync()
    {
      try
      {
        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(
          logFileName);

        await file.DeleteAsync();
      }
      catch (FileNotFoundException)
      {

      }
    }
    static readonly string logFileName = "log.txt";
  }
}
