internal partial class UserDataRepository
{
  internal class RGSException : ApplicationException
  {
    public RGSException()
    {
    }

    public RGSException(string? message) : base(message)
    {
    }

    public RGSException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
  }
}