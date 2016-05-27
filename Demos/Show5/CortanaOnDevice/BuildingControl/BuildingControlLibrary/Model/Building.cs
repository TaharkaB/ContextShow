using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingControl.Model
{
  public class Building : NamedObject
  {
    public List<Room> Rooms { get; set; }
    public void SwitchAllLights(bool onOff)
    {
      foreach (var room in this.Rooms)
      {
        room.SwitchLights(onOff);
      }
    }
    public Room GetRoomByType(RoomType roomType)
    {
      return (
        this.Rooms.SingleOrDefault(r => r.RoomType == roomType));
    }
    public Room GetRoomByName(string roomType)
    {
      var actualType = (RoomType)Enum.Parse(typeof(RoomType), roomType);
      return (this.GetRoomByType(actualType));
    }
  }
}
