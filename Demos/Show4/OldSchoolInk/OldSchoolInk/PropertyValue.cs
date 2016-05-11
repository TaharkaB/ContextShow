namespace OldSchoolInk
{
  public class PropertyValue : ViewModelBase
  {
    public string Value
    {
      get
      {
        return (this._value);
      }
      set
      {
        base.SetProperty(ref this._value, value);
      }
    }
    string _value;
  }
}
