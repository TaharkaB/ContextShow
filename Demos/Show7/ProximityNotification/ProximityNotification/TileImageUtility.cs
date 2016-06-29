using Microsoft.Band;
using Microsoft.Band.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace ProximityNotification
{
  static class TileImageUtility
  {
    public static async Task<BandIcon> MakeTileIconFromFileAsync(
      Uri packageFileUri, int size)
    {
      BandIcon icon = null;

      var file = await StorageFile.GetFileFromApplicationUriAsync(
        packageFileUri);

      using (var stream = await file.OpenReadAsync())
      {
        var bitmap = new WriteableBitmap(size, size);
        bitmap.SetSource(stream);
        icon = bitmap.ToBandIcon();
      }
      return (icon);
    }
  }
}
