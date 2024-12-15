using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; set; }

    public void Awake() => I = this;

    #region Area audio
    [Inject] IAudioManager audioManager;
    [Inject] ISocialManager socialManager;
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            audioManager.Play();

        if (Input.GetKeyDown(KeyCode.DownArrow))
            socialManager.PushOnWall();
    }
}
