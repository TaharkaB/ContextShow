namespace WpfRealsenseFacialDetection
{
  interface IFrameProcessor
  {
    void Initialise(PXCMSenseManager senseManager);
    void ProcessFrame(PXCMCapture.Sample sample);
    void DrawUI(PXCMCapture.Sample sample);
    int RealSenseModuleId { get;  }
  }
}
