using BuildingControl.Model;
using BuildingControl.Utility;

namespace BuildingControl.Pages
{
  public class RoomDataViewModel : ViewModelBase
  {
    public int Count
    {
      get
      {
        return (this.count);
      }
      set
      {
        base.SetProperty(ref this.count, value);
      }
    }
    public RoomType RoomType { get; set; }
    int count;
  }
}
