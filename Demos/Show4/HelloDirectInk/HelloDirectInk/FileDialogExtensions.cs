namespace HelloDirectInk
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Windows.Storage;
  using Windows.Storage.Pickers;
  using Windows.Storage.Streams;

  static class FileDialogExtensions
  {
    public static async Task<IRandomAccessStream> PickFileForReadAsync(
      string fileExtension)
    {
      IRandomAccessStream stream = null;
      var picker = new FileOpenPicker();
      picker.SuggestedStartLocation = PickerLocationId.Desktop;
      picker.FileTypeFilter.Add(fileExtension);

      var file = await picker.PickSingleFileAsync();

      if (file != null)
      {
        stream = await file.OpenReadAsync();
      }
      return (stream);
    }
    public static async Task<IOutputStream> PickFileForSaveAsync(
      string typeOfFile, string typeOfFileExtension, string suggestedName)
    {
      IOutputStream stream = null;
      var picker = new FileSavePicker();

      picker.FileTypeChoices.Add(
        typeOfFile, new string[] { typeOfFileExtension });

      picker.SuggestedFileName = suggestedName;
      picker.SuggestedStartLocation = PickerLocationId.Desktop;

      var file = await picker.PickSaveFileAsync();

      if (file != null)
      {
        stream = await file.OpenAsync(FileAccessMode.ReadWrite);
      }
      return (stream);
    }
  }
}
