using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class AFCacheWindow : EditorWindow
{
    public List<CacheTabInfo> tabs = new List<CacheTabInfo> { new CacheTabInfo { id = 0, tabName = "Common" } };
    public List<CacheObjectInfo> Objects = new List<CacheObjectInfo>();
    public List<CacheObjectInfo> FilterList = new List<CacheObjectInfo>();
    public string SearchText = string.Empty;
    private string oldSearchText;
    private Vector2 scrollPosition;
    private AFCacheStyle s;
    public int oldTabIndex = -1;
    public int tabIndex;
    public bool isEditMode;
    public ReorderableList reorderableList;

    private void OnEnable()
    {
        oldSearchText = "old";
    }

    [MenuItem("FunniiTool/AF Cache")]
    public static void Init()
    {
        var w = GetWindow<AFCacheWindow>();
        w.titleContent.text = "AF Cache";
        w.Read();
        w.Show();
    }

    private void OnGUI()
    {
        if (s == null) s = new AFCacheStyle();
        if (oldTabIndex != tabIndex)
        {
            oldTabIndex = tabIndex;
            if (tabs[tabIndex].list == null)
            {
                tabs[tabIndex].list = new List<CacheObjectInfo>();
            }

            Objects = tabs[tabIndex].list;
            Filter();
        }

        DisplayTabs();
        if (Objects == null)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        SearchText = EditorGUILayout.TextField(SearchText);
        GUILayout.Label($"{FilterList.Count.ToString()}/{Objects.Count.ToString()}", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            Objects.Clear();
            FilterList.Clear();
            Focus();
        }

        GUILayout.EndHorizontal();

        if (oldSearchText != SearchText)
        {
            oldSearchText = SearchText;
            Filter();
        }

        GUILayout.BeginHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (isEditMode)
        {
            DisplayReorderList();
        }
        else
        {
            DisplayListObjects();
        }

        GUILayout.EndScrollView();

        DisplayDetail();
        GUILayout.EndHorizontal();
        UpdateDragAndDrop();
    }

    private void DisplayDetail()
    {
    }

    private void DisplayReorderList()
    {
        GUILayout.BeginVertical();
        reorderableList.DoLayoutList();
        GUILayout.EndVertical();
    }

    private void DisplayListObjects()
    {
        GUILayout.BeginVertical();
        if (FilterList.Count == 0)
        {
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Label("Empty", s.textEmpty);
            EditorGUI.EndDisabledGroup();
        }

        foreach (var o in FilterList)
        {
            GUILayout.BeginHorizontal();
            if (o.previewTexture == null)
            {
                o.previewTexture = AssetPreview.GetAssetPreview(o.Obj);
                if (o.previewTexture == null)
                {
                    o.previewTexture = AssetPreview.GetMiniThumbnail(o.Obj);
                }
            }

            GUILayout.Label(o.previewTexture, s.previewTexture);
            var lastPreviewRect = GUILayoutUtility.GetLastRect();
            if (lastPreviewRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    GUIUtility.hotControl = 0;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { o.Obj };
                    DragAndDrop.SetGenericData("DRAG_ID", o.Obj);
                    DragAndDrop.StartDrag("A");
                }
            }

            if (GUILayout.Button(new GUIContent(o.GetDisplayName()), s.buttonStyle, null))
            {
                tabs[tabIndex].selected = o;
                tabs[tabIndex].editor = Editor.CreateEditor(o.Obj);
                if (o.Location == CacheObjectLocation.Scene || o.Location == CacheObjectLocation.Prefab)
                {
                    o.Ping();
                }
                else if (o.Obj is DefaultAsset)
                {
                    AFCacheHelper.ShowFolderContents(o.Obj.GetInstanceID());
                }
                else
                {
                    AssetDatabase.OpenAsset(o.Obj);
                }
            }

            if (GUILayout.Button("P", s.expandWidthFalse))
            {
                o.Ping();
            }

            if (GUILayout.Button("-", s.expandWidthFalse))
            {
                Objects.Remove(o);
                FilterList.Remove(o);
                Save();
                Focus();
                break;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void DisplayTabs()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginHorizontal();

        for (int i = 0; i < tabs.Count; i++)
        {
            GUIStyle buttonStyle = i == tabIndex ? s.buttonLeftSelected : s.buttonLeft;
            if (i == tabs.Count - 1)
                buttonStyle = i == tabIndex ? s.buttonRightSelected : s.buttonRight;
            else if (i > 0)
                buttonStyle = i == tabIndex ? s.buttonMidSelected : s.buttonMid;

            if (isEditMode && i == tabIndex)
            {
                GUILayout.BeginVertical();
                tabs[i].tabName = EditorGUILayout.TextField(tabs[i].tabName);
                GUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button(tabs[i].tabName, buttonStyle))
                {
                    tabIndex = i;
                }
            }
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("-", s.buttonLeft, s.expandWidth30))
        {
            tabs.RemoveAt(tabIndex);
            if (tabIndex >= tabs.Count)
                tabIndex = tabs.Count - 1;
            Save();
        }

        if (GUILayout.Button("+", s.buttonRight, s.expandWidth30))
        {
            if (tabs.Count == 0)
            {
                tabs.Add(new CacheTabInfo { id = 0, tabName = "New tab", list = new List<CacheObjectInfo>() });
            }
            else
            {
                tabs.Add(new CacheTabInfo
                { id = tabs.Max(s => s.id) + 1, tabName = "New tab", list = new List<CacheObjectInfo>() });
            }

            Save();
        }

        if (GUILayout.Button("<", s.buttonLeft, s.expandWidth30))
        {
            tabs.Swap(tabIndex, tabIndex - 1);
            tabIndex--;
            tabIndex = Mathf.Clamp(tabIndex, 0, tabs.Count);
            Save();
        }

        if (GUILayout.Button(">", s.buttonRight, s.expandWidth30))
        {
            tabs.Swap(tabIndex, tabIndex + 1);
            tabIndex++;
            tabIndex = Mathf.Clamp(tabIndex, 0, tabs.Count);
            Save();
        }

        if (!isEditMode)
        {
            if (GUILayout.Button("Edit", s.buttonStyle, s.expandWidthFalse))
            {
                isEditMode = true;
            }
        }
        else
        {
            if (GUILayout.Button("Ok", s.buttonStyle, s.expandWidthFalse))
            {
                isEditMode = false;
            }
        }

        GUILayout.EndHorizontal();
    }


    private void Filter()
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            FilterList = Objects;
        }
        else
        {
            var temp = SearchText.ToLower();
            FilterList = Objects.Where(s => s.Name.ToLower().Contains(temp)).ToList();
        }

        reorderableList = new ReorderableList(FilterList, typeof(CacheObjectInfo), true, false, false, false);
        reorderableList.drawElementCallback += DrawElementCallback;
        reorderableList.onChangedCallback += list => { Save(); };
    }

    private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
    {
        var data = FilterList[index];
        var avatarRect = new Rect(rect.x, rect.y, rect.height, rect.height);
        GUI.Label(avatarRect, data.previewTexture);
        var nameRect = new Rect(rect.x + rect.height + 3, rect.y, rect.width - rect.height - 3, rect.height);
        GUI.Label(nameRect, data.GetDisplayName());
    }

    void UpdateDragAndDrop()
    {
        if (Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            if (DragAndDrop.paths.Length == 0 && DragAndDrop.objectReferences.Length > 0)
            {
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    var currentPrefab = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                    Debug.Log("- " + obj);
                    string assetPath = obj.name;
                    GetScenePath((obj as GameObject).transform, ref assetPath);
                    var item = new CacheObjectInfo()
                    {
                        Name = obj.name,
                        Path = assetPath,
                    };

                    CacheObjectLocation location = CacheObjectLocation.Assets;
                    if (currentPrefab != null)
                    {
                        item.PrefabPath = currentPrefab.prefabAssetPath;
                        item.Location = CacheObjectLocation.Prefab;
                    }
                    else
                    {
                        item.Location = CacheObjectLocation.Scene;
                    }

                    if (Objects.Find(s => s.Obj == item.Obj) == null)
                    {
                        Objects.Add(item);
                        Save();
                    }
                    else if (Objects.Find(s => s.Path == item.Path) == null)
                    {
                        Objects.Add(item);
                        Save();
                    }
                }

                Filter();
            }
            else if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
            {
                Debug.Log("File");
                foreach (string path in DragAndDrop.paths)
                {
                    Debug.Log("- " + path);
                }
            }
            else if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    Object obj = DragAndDrop.objectReferences[i];
                    if (Objects.Find(s => s.Obj == obj) == null)
                    {
                        Objects.Add(new CacheObjectInfo()
                        {
                            Name = obj.name,
                            Location = CacheObjectLocation.Assets,
                            Obj = obj,
                        });
                        Save();
                    }
                }

                Filter();
            }
        }
    }

    public void GetScenePath(Transform obj, ref string path)
    {
        var parent = obj.parent;
        if (parent != null)
        {
            path = parent.name + "/" + path;
            GetScenePath(parent, ref path);
        }
    }

    [Serializable]
    public class CacheObjectInfo
    {
        public Object Obj;
        public string Path;
        public string PrefabPath;
        public string Name;
        public CacheObjectLocation Location;
        public Texture2D previewTexture;

        public string GetDisplayName()
        {
            return $"{GetPrefix()} {Name}";
        }

        public string GetPrefix()
        {
            if (Location == CacheObjectLocation.Assets)
                return "A:";
            if (Location == CacheObjectLocation.Scene)
                return "S:";
            return "P:";
        }

        public void Ping()
        {
            if (Location == CacheObjectLocation.Assets)
            {
                Selection.activeObject = Obj;
                EditorGUIUtility.PingObject(Obj);
            }
            else if (Location == CacheObjectLocation.Prefab)
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(GameObject)));
                var rootGameObjects = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

                var arrayPath = Path.Split('/').ToList();
                arrayPath.RemoveAt(0);
                arrayPath.RemoveAt(0);
                var newPath = string.Join("/", arrayPath);
                var obj = rootGameObjects.transform.Find(newPath);
                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }
            else if (Location == CacheObjectLocation.Scene)
            {
                if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    PrefabUtility.UnloadPrefabContents(UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot);
                }

                var arrayPath = Path.Split('/').ToList();
                arrayPath.RemoveAt(0);
                var newPath = string.Join("/", arrayPath);

                if (arrayPath.Count == 0)
                    newPath = Path;

                Transform obj = null;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    bool check = false;
                    var scene = SceneManager.GetSceneAt(i);
                    var rootGameObjects = scene.GetRootGameObjects().ToList();
                    foreach (var gameObject in rootGameObjects)
                    {
                        obj = gameObject.transform.Find(newPath);
                        if (obj != null)
                        {
                            check = true;
                            break;
                        }
                    }

                    if (check)
                        break;

                    var tempObj = rootGameObjects.Find(s => s.name == newPath);
                    if (tempObj != null)
                    {
                        obj = tempObj.transform;
                        break;
                    }
                }

                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }
        }
    }

    private void Save()
    {
        string data = JsonUtility.ToJson(new AFCacheSave { tabs = tabs });
        EditorPrefs.SetString(Application.dataPath + "AFCacheSave", data);
    }

    private void Read()
    {
        var data = EditorPrefs.GetString(Application.dataPath + "AFCacheSave");
        var afCacheSave = JsonUtility.FromJson<AFCacheSave>(data);
        if (afCacheSave == null)
        {
            afCacheSave = new AFCacheSave();
        }
        tabs = afCacheSave.tabs;
    }

    [Serializable]
    public class AFCacheSave
    {
        public List<CacheTabInfo> tabs = new List<CacheTabInfo>();
    }

    public enum CacheObjectLocation
    {
        Assets,
        Scene,
        Prefab,
    }

    [Serializable]
    public class CacheTabInfo
    {
        public int id;
        public string tabName;
        public List<CacheObjectInfo> list;
        public CacheObjectInfo selected;
        public Editor editor;
    }
}

public class AFCacheStyle
{
    public GUILayoutOption expandWidthFalse = GUILayout.ExpandWidth(false);
    public GUILayoutOption expandWidth30 = GUILayout.Width(20);
    public GUIStyle previewTexture;
    public GUIStyle buttonStyle;
    public GUIStyle textEmpty;
    public GUIStyle toolBar;

    public GUIStyle buttonLeft;
    public GUIStyle buttonMid;
    public GUIStyle buttonRight;

    public GUIStyle buttonLeftSelected;
    public GUIStyle buttonMidSelected;
    public GUIStyle buttonRightSelected;

    public AFCacheStyle()
    {
        previewTexture = new GUIStyle() { fixedWidth = 20, fixedHeight = 20 };
        buttonLeft = new GUIStyle(EditorStyles.miniButtonLeft);
        buttonMid = new GUIStyle(EditorStyles.miniButtonMid);
        buttonRight = new GUIStyle(EditorStyles.miniButtonRight);

        buttonLeftSelected = new GUIStyle(EditorStyles.miniButtonLeft);
        buttonMidSelected = new GUIStyle(EditorStyles.miniButtonMid);
        buttonRightSelected = new GUIStyle(EditorStyles.miniButtonRight);
        buttonLeftSelected.normal.textColor = buttonMidSelected.normal.textColor =
            buttonRightSelected.normal.textColor = Color.yellow;
        buttonLeftSelected.onHover.textColor = buttonMidSelected.onHover.textColor =
            buttonRightSelected.onHover.textColor = Color.yellow;

        buttonLeftSelected.focused.textColor = buttonMidSelected.focused.textColor =
            buttonRightSelected.focused.textColor = Color.yellow;

        buttonLeftSelected.fontStyle = buttonMidSelected.fontStyle =
            buttonRightSelected.fontStyle = FontStyle.Bold;

        ColorUtility.TryParseHtmlString("#363636", out var bgColor);
        buttonStyle = new GUIStyle("Button");
        buttonStyle.alignment = TextAnchor.MiddleLeft;
        textEmpty = new GUIStyle(EditorStyles.label);
        textEmpty.alignment = TextAnchor.MiddleCenter;
        toolBar = new GUIStyle(EditorStyles.toolbar);
        toolBar.fixedHeight = 60;
    }

    private Texture2D MakeTexDark(Texture2D texture, float delta = .1f)
    {
        if (texture == null)
        {
            return MakeTex(10, 10, Color.blue);
        }

        Color[] pix = new Color[texture.width * texture.height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = new Color(pix[i].r - delta, pix[i].g - delta, pix[i].b - delta);

        Texture2D result = new Texture2D(texture.width, texture.height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}

public static class AFCacheHelper
{
    public static void Swap<T>(this List<T> list, int index1, int index2)
    {
        index1 = Mathf.Clamp(index1, 0, list.Count);
        index2 = Mathf.Clamp(index2, 0, list.Count);
        if (index1 == index2)
            return;
        T temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }

    #region Folder

    public static void ShowFolderContents(int folderInstanceID)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
        MethodInfo showFolderContents = projectBrowserType.GetMethod(
            "ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);
        Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(projectBrowserType);
        if (projectBrowserInstances.Length > 0)
        {
            for (int i = 0; i < projectBrowserInstances.Length; i++)
                ShowFolderContentsInternal(projectBrowserInstances[i], showFolderContents, folderInstanceID);
        }
        else
        {
            EditorWindow projectBrowser = OpenNewProjectBrowser(projectBrowserType);
            ShowFolderContentsInternal(projectBrowser, showFolderContents, folderInstanceID);
        }
    }

    public static void ShowFolderContentsInternal(Object projectBrowser, MethodInfo showFolderContents,
        int folderInstanceID)
    {
        SerializedObject serializedObject = new SerializedObject(projectBrowser);
        bool inTwoColumnMode = serializedObject.FindProperty("m_ViewMode").enumValueIndex == 1;

        if (!inTwoColumnMode)
        {
            MethodInfo setTwoColumns = projectBrowser.GetType().GetMethod(
                "SetTwoColumns", BindingFlags.Instance | BindingFlags.NonPublic);
            setTwoColumns.Invoke(projectBrowser, null);
        }

        bool revealAndFrameInFolderTree = true;
        showFolderContents.Invoke(projectBrowser, new object[] { folderInstanceID, revealAndFrameInFolderTree });
    }

    public static EditorWindow OpenNewProjectBrowser(Type projectBrowserType)
    {
        EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType);
        projectBrowser.Show();
        MethodInfo init = projectBrowserType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
        init.Invoke(projectBrowser, null);
        return projectBrowser;
    }

    #endregion
}