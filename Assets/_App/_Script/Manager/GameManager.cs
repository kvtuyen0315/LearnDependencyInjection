using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; set; }

    public void Awake() => I = this;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            
        }
    }
}
