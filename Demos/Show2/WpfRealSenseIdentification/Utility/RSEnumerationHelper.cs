namespace WpfRealSenseIdentification
{
  using System.Collections.Generic;

  delegate pxcmStatus RSQueryWithDescriptionAndReturnTypeIteratorFunction<D,T>(
    D descriptionType, int index, out T returnType);

  delegate pxcmStatus RSQueryWithDescriptionIteratorFunction<T>(
    T descriptionType, int index, out T returnType);

  delegate pxcmStatus RSQueryIteratorFunction<T>(
    int index, out T returnType);

  static class RSEnumerationHelper
  {
    public static IEnumerable<T> QueryValuesWithDescription<D,T>(D description,
      RSQueryWithDescriptionAndReturnTypeIteratorFunction<D,T> queryIterator)
    {
      int i = 0;
      T current;

      while (queryIterator(description, i++, out current) == pxcmStatus.PXCM_STATUS_NO_ERROR)
      {
        yield return current;
      }
    }
    public static IEnumerable<T> QueryValuesWithDescription<T>(T description,
      RSQueryWithDescriptionIteratorFunction<T> queryIterator)
    {
      int i = 0;
      T current;

      while (queryIterator(description, i++, out current) == pxcmStatus.PXCM_STATUS_NO_ERROR)
      {
        yield return current;
      }
    }
    public static IEnumerable<T> QueryValues<T>(RSQueryIteratorFunction<T> queryIterator)
    {
      int i = 0;
      T current;

      while (queryIterator(i++, out current) == pxcmStatus.PXCM_STATUS_NO_ERROR)
      {
        yield return current;
      }
    }
  }
}
