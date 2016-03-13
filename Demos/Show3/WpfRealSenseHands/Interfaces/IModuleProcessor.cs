namespace WpfRealSenseHands
{
  interface IModuleProcessor : ISampleProcessor
  {
    int RealSenseModuleId { get; }
  }
}
