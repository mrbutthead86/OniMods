
namespace Internals
{
  public sealed class Logger
  {
    private readonly string scope;

    public Logger(string scope)
    {
      this.scope = scope;
    }

    public void Log(string message) => Debug.Log(CreateLogMessage(message));

    public void Warn(string message) => Debug.LogWarning(CreateLogMessage(message));

    private string CreateLogMessage(string message) => $"[{scope}] {message}";
  }
}
