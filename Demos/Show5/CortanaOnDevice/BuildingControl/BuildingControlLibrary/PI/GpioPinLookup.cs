namespace BuildingControl.PI
{
  public static class GpioPinLookup
  {
    public static int GetGpioPinIndexForOrdinal(int ordinal)
    {
      int pin = -1;

      if ((ordinal >= 0) && (ordinal < gpiopins.Length))
      {
        pin = gpiopins[ordinal];
      }
      return (pin);
    }
    static int[] gpiopins =
    {
      4,5,6,12,13,16,17,18,19,20,21,22,23,24,25,26,27
    };
  }
}
