using UnityEngine;

public interface IAudioManager
{
    void Play();
    void Stop();
    void Pause();
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    private void ShowLog(string content) => GlobalInfo.I.ShowLog(content);

    public void Pause() => ShowLog($"Pause");

    public void Play() => ShowLog($"Play");

    public void Stop() => ShowLog($"Stop");
}
