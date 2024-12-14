using UnityEngine;

public interface ILogger
{
    void ShowError(string content);
    void ShowLog(string content);
    void ShowWarning(string content);
}

public class Logger : ILogger
{
    public void ShowError(string content) => Debug.LogError(content);

    public void ShowLog(string content) => Debug.Log(content);

    public void ShowWarning(string content) => Debug.LogWarning(content);
}
