using UnityEngine;

public interface ISocialManager
{
    void PushOnWall();
    void PushOffWall();
}

public class SocialManager : MonoBehaviour, ISocialManager
{
    private void ShowLog(string content) => GlobalData.I.ShowLog(content);

    public void PushOffWall() => ShowLog($"PushOffWall");

    public void PushOnWall() => ShowLog($"PushOnWall");
}
