using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace UnityExplorer.ObjectExplorer;

public static class SceneHandler
{
    /// <summary>The currently inspected Scene.</summary>
    public static Scene? SelectedScene
    {
        get => selectedScene;
        internal set
        {
            if (selectedScene.HasValue && selectedScene == value)
            {
                return;
            }
            if (!value.HasValue)
            {
                return;
            }
            selectedScene = value;
            OnInspectedSceneChanged?.Invoke(selectedScene.Value);
        }
    }
    private static Scene? selectedScene;

    /// <summary>The GameObjects in the currently inspected scene.</summary>
    public static IEnumerable<GameObject> CurrentRootObjects { get; private set; } = new GameObject[0];

    /// <summary>All currently loaded Scenes.</summary>
    public static List<Scene> LoadedScenes { get; private set; } = new();
    //private static HashSet<Scene> previousLoadedScenes;

    /// <summary>The names of all scenes in the build settings, if they could be retrieved.</summary>
    public static List<string> AllSceneNames { get; private set; } = new();

    /// <summary>Invoked when the currently inspected Scene changes. The argument is the new scene.</summary>
    public static event Action<Scene>? OnInspectedSceneChanged;

    /// <summary>Invoked whenever the list of currently loaded Scenes changes. The argument contains all loaded scenes after the change.</summary>
    public static event Action<List<Scene>>? OnLoadedScenesUpdated;

    /// <summary>Generally will be 2, unless DontDestroyExists == false, then this will be 1.</summary>
    internal static int DefaultSceneCount => 1 + (DontDestroyExists ? 1 : 0);

    /// <summary>Whether or not we are currently inspecting the "HideAndDontSave" asset scene.</summary>
#if UNITY_6000_3_OR_NEWER
    public static bool InspectingAssetScene => SelectedScene.HasValue && SelectedScene.Value.handle == (SceneHandle)(-1);
#else
    public static bool InspectingAssetScene => SelectedScene.HasValue && SelectedScene.Value.handle == -1;
#endif

    /// <summary>Whether or not we successfuly retrieved the names of the scenes in the build settings.</summary>
    public static bool WasAbleToGetScenesInBuild { get; private set; }

    /// <summary>Whether or not the "DontDestroyOnLoad" scene exists in this game.</summary>
    public static bool DontDestroyExists { get; private set; }

    private const string dontDestroyName = "DontDestroyOnLoad";

    internal static void Init()
    {
        // Check if the game has "DontDestroyOnLoad"
        try
        {
            Type? sceneType = ReflectionUtility.GetTypeByName("UnityEngine.SceneManagement.Scene");
            if (sceneType == null) throw new Exception("This version of Unity does not ship with the 'Scene' class.");
            
            MethodInfo? method = sceneType.GetMethod("GetNameInternal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            
            object[] args = new object[1];
#if UNITY_6000_3_OR_NEWER
            // get runtime type of SceneHandle to prevent reflection identity mismatches
            Type handleType = method!.GetParameters()[0].ParameterType;
            MethodInfo? implicitCast = handleType.GetMethod("op_Implicit", new Type[] { typeof(int) });
            args[0] = implicitCast!.Invoke(null, new object[] { -12 });
#else
            args[0] = -12;
#endif
                
            string? sceneName = (string?)method?.Invoke(null, args);
            
            if (string.IsNullOrEmpty(sceneName)) throw new Exception("Scene.GetNameInternal returned null for DontDestroyOnLoad scene.");
            DontDestroyExists = sceneName == dontDestroyName;
        }
        catch (Exception ex)
        {
            ExplorerCore.LogWarning($"Unable to check for existence of DontDestroyOnLoad scene via Scene.GetNameInternal: {ex.Message}");
#pragma warning disable CS0618
            DontDestroyExists = SceneManager.GetAllScenes().Any(s => s.name == dontDestroyName);
#pragma warning restore CS0618
        }

        // Try to get all scenes in the build settings. This may not work.
        try
        {
            Type sceneUtil = ReflectionUtility.GetTypeByName("UnityEngine.SceneManagement.SceneUtility");
            if (sceneUtil == null)
            {
                throw new Exception("This version of Unity does not ship with the 'SceneUtility' class, or it was not unstripped.");
            }

            MethodInfo? method = sceneUtil.GetMethod("GetScenePathByBuildIndex", ReflectionUtility.FLAGS);
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string? scenePath = (string?)method?.Invoke(null, [ i ]);
                if (string.IsNullOrEmpty(scenePath))
                {
                    continue;
                }
                AllSceneNames.Add(scenePath!);
            }

            WasAbleToGetScenesInBuild = true;
        }
        catch (Exception ex)
        {
            WasAbleToGetScenesInBuild = false;
            ExplorerCore.LogWarning($"Unable to generate list of all Scenes in the build: {ex}");
        }
    }

    // bypass private m_Handle protection level via Reflection
    private static Scene CreateSceneWithHandle(int id)
    {
        object sceneObj = new Scene();
#if UNITY_6000_3_OR_NEWER
        object handle = (SceneHandle)id;
#else
        object handle = id;
#endif
        typeof(Scene).GetField("m_Handle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sceneObj, handle);
        return (Scene)sceneObj;
    }

    internal static void Update()
    {
        // Inspected scene will exist if it's DontDestroyOnLoad or HideAndDontSave
#if UNITY_6000_3_OR_NEWER
        bool inspectedExists = SelectedScene.HasValue && ((DontDestroyExists && SelectedScene.Value.handle == (SceneHandle)(-12)) || SelectedScene.Value.handle == (SceneHandle)(-1));
#else
        bool inspectedExists = SelectedScene.HasValue && ((DontDestroyExists && SelectedScene.Value.handle == -12) || SelectedScene.Value.handle == -1);
#endif

        LoadedScenes.Clear();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene == default || !scene.isLoaded || !scene.IsValid())
            {
                continue;
            }

            // If we have not yet confirmed inspectedExists, check if this scene is our currently inspected one.
            if (!inspectedExists && scene == SelectedScene)
            {
                inspectedExists = true;
            }

            LoadedScenes.Add(scene);
        }

        if (DontDestroyExists)
        {
            LoadedScenes.Add(CreateSceneWithHandle(-12));
        }
        LoadedScenes.Add(CreateSceneWithHandle(-1));

        // Default to first scene if none selected or previous selection no longer exists.
        if (!inspectedExists)
        {
            SelectedScene = LoadedScenes.First();
        }

        // Notify on the list changing at all
        OnLoadedScenesUpdated?.Invoke(LoadedScenes);

        // Finally, update the root objects list.
        if (SelectedScene.HasValue && SelectedScene.Value.IsValid())
        {
            CurrentRootObjects = RuntimeHelper.GetRootGameObjects(SelectedScene.Value);
        }
        else
        {
            UnityEngine.Object[] allObjects = RuntimeHelper.FindObjectsOfTypeAll(typeof(GameObject));
            List<GameObject> objects = new();
            foreach (UnityEngine.Object obj in allObjects)
            {
                GameObject? go = obj.TryCast<GameObject>();
                if (go != null &&
                    go.transform != null &&
                    go.transform.parent == null && 
                    !go.scene.IsValid())
                {
                    objects.Add(go);
                }
            }
            CurrentRootObjects = objects;
        }
    }
}
