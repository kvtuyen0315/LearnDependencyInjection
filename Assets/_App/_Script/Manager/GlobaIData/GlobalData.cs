using UnityEngine;

public partial class GlobalData : MonoBehaviour
{
    public static GlobalData I { get; set; }
    private void Awake() => I = this;

    #region Area string const
    public const string PATH_MANAGER = "Manager/";
    public static string GetClassInManager<T>() where T : class => $"{PATH_MANAGER}{typeof(T).Name}";
    #endregion
}
