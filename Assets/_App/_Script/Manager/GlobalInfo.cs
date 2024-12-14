using UnityEngine;
using Zenject;

public class GlobalInfo : MonoBehaviour, ILogger
{
    public static GlobalInfo I { get; set; }
    private void Awake()
    {
        I = this;

        logger = new Logger();
    }

    #region Area logger
    [Inject] ILogger logger;

    public void ShowError(string content) => logger?.ShowError(content);

    public void ShowLog(string content) => logger?.ShowLog(content);

    public void ShowWarning(string content) => logger?.ShowWarning(content); 
    #endregion
}
