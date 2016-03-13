namespace WpfRealSenseHands
{
  interface ISampleProcessor
  {
    void Initialise(PXCMSenseManager senseManager);
    void ProcessFrame(PXCMCapture.Sample sample);
    void DrawUI(PXCMCapture.Sample sample);
  }
}
