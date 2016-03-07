namespace WpfRealsenseFacialDetection.Utility
{
  using System;
  using System.Linq;

  static class PXCMFaceExpressionExtensions
  {
    public static string GetName(this PXCMFaceData.ExpressionsData.FaceExpression expression)
    {
      var pieces = expression.ToString().Split('_');

      return (string.Join(" ", pieces.Skip(1).Select(p => p.ToLower())));
    }
  }
}
