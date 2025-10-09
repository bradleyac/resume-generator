namespace RGS.Frontend;

public class RGSException : ApplicationException
{
  public RGSException()
  {
  }

  public RGSException(string message)
      : base(message)
  {
  }

  public RGSException(string message, Exception inner)
      : base(message, inner)
  {
  }
}