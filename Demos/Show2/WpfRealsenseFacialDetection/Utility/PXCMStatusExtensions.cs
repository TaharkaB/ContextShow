namespace WpfRealsenseFacialDetection
{
  using System;

  class PXCMStatusException : Exception
  {
    public PXCMStatusException(pxcmStatus status)
    {
      this.Status = status;
    }
    public pxcmStatus Status { get; private set; }
  }
  static class PXCMStatusExtensions
  {
    public static void ThrowOnFail(this pxcmStatus status)
    {
      if (!status.Succeeded())
      {
        throw new PXCMStatusException(status);
      }
    }
    public static bool Succeeded(this pxcmStatus status)
    {
      return (status == pxcmStatus.PXCM_STATUS_NO_ERROR);
    }
  }
}
