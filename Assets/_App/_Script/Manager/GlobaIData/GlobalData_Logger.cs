using Zenject;

public partial class GlobalData : ILogger
{
    [Inject] ILogger logger;

    public void ShowError(string content) => logger?.ShowError(content);

    public void ShowLog(string content) => logger?.ShowLog(content);

    public void ShowWarning(string content) => logger?.ShowWarning(content);
}
