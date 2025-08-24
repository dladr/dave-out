using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Assets.Scripts.Helpers;

/// <summary>
/// Generic scene loader with optional fade effects and events.
/// Can be triggered manually or automatically with a delay.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [BoxGroup("Scene Settings")]
    [Tooltip("Name of the scene to load")]
    public string SceneName;

    [BoxGroup("Scene Settings")]
    [Tooltip("How to load the scene")]
    public LoadSceneMode LoadMode = LoadSceneMode.Single;

    [BoxGroup("Auto Load")]
    [Tooltip("Should this scene load automatically after a delay?")]
    public bool AutoLoad = false;

    [BoxGroup("Auto Load")]
    [ShowIf("AutoLoad")]
    [Tooltip("Delay in seconds before auto-loading the scene")]
    public float AutoLoadDelay = 3f;

    [BoxGroup("Fade Settings")]
    [Tooltip("Should we fade out before loading?")]
    public bool UseFadeOut = false;

    [BoxGroup("Fade Settings")]
    [ShowIf("UseFadeOut")]
    [Tooltip("Component that handles fading (must be assigned in inspector)")]
    public SimpleFader FadeComponent;

    [BoxGroup("Fade Settings")]
    [ShowIf("UseFadeOut")]
    [Tooltip("How long to wait for fade to complete (in seconds)")]
    public float FadeOutTime = 1f;

    [BoxGroup("Fade Settings")]
    [ShowIf("UseFadeOut")]
    [Tooltip("Additional time to wait after fade before loading scene (0 = no wait)")]
    public float TimeToWaitBeforeLoading = 0f;

    [BoxGroup("Events")]
    [Tooltip("Called when starting to load the scene")]
    public UnityEvent OnLoadStart;

    [BoxGroup("Events")]
    [ShowIf("@TimeToWaitBeforeLoading > 0")]
    [Tooltip("Called when beginning wait period before loading")]
    public UnityEvent OnBeginWait;

    [BoxGroup("Events")]
    [Tooltip("Called if the scene load fails")]
    public UnityEvent OnLoadFailed;

    private bool _isLoading = false;
    private Coroutine _autoLoadCoroutine;

    void Start()
    {
        if (AutoLoad && !string.IsNullOrEmpty(SceneName))
        {
            _autoLoadCoroutine = StartCoroutine(AutoLoadCoroutine());
        }
    }

    void OnDestroy()
    {
        if (_autoLoadCoroutine != null)
        {
            StopCoroutine(_autoLoadCoroutine);
        }
    }

    IEnumerator AutoLoadCoroutine()
    {
        yield return new WaitForSeconds(AutoLoadDelay);
        LoadScene();
    }

    [Button("Load Scene Now")]
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError($"SceneLoader on {gameObject.name}: Scene name is empty!", this);
            OnLoadFailed?.Invoke();
            return;
        }

        if (_isLoading)
        {
            Debug.LogWarning($"SceneLoader on {gameObject.name}: Already loading a scene", this);
            return;
        }

        StartCoroutine(LoadSceneCoroutine());
    }

    [Button("Load Scene by Name")]
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"SceneLoader on {gameObject.name}: Provided scene name is empty!", this);
            OnLoadFailed?.Invoke();
            return;
        }

        var originalSceneName = SceneName;
        SceneName = sceneName;
        LoadScene();
        SceneName = originalSceneName; // Restore original for potential reuse
    }

    private IEnumerator LoadSceneCoroutine()
    {
        _isLoading = true;
        OnLoadStart?.Invoke();

        Debug.Log($"SceneLoader: Loading scene '{SceneName}' with mode {LoadMode}");

        // Handle fade out if enabled
        if (UseFadeOut)
        {
            yield return StartCoroutine(HandleFadeOut());
        }

        // Handle additional wait time if specified
        if (TimeToWaitBeforeLoading > 0)
        {
            OnBeginWait?.Invoke();
            Debug.Log($"SceneLoader: Waiting {TimeToWaitBeforeLoading} seconds before loading");
            yield return new WaitForSeconds(TimeToWaitBeforeLoading);
        }

        // Load the scene asynchronously for better performance
        var asyncOperation = SceneManager.LoadSceneAsync(SceneName, LoadMode);
        
        if (asyncOperation == null)
        {
            Debug.LogError($"SceneLoader: Failed to start loading scene '{SceneName}' - scene may not exist in build settings", this);
            OnLoadFailed?.Invoke();
            _isLoading = false;
            yield break;
        }

        // Wait for the scene to load
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        Debug.Log($"SceneLoader: Successfully loaded scene '{SceneName}'");
        _isLoading = false;
    }

    private IEnumerator HandleFadeOut()
    {
        if (FadeComponent == null)
        {
            Debug.LogError($"SceneLoader: FadeComponent is not assigned! Please assign it in the inspector.", this);
            yield break;
        }

        Debug.Log($"SceneLoader: Starting fade out with duration {FadeOutTime}");
        FadeComponent.FadeOut(FadeOutTime);

        // Wait for the specified fade time
        yield return new WaitForSeconds(FadeOutTime);
        Debug.Log($"SceneLoader: Fade out completed");
    }

    private bool TryInvokeFadeMethod(string methodName, params object[] parameters)
    {
        if (FadeComponent == null) return false;

        var methodInfo = FadeComponent.GetType().GetMethod(methodName);
        if (methodInfo != null)
        {
            try
            {
                methodInfo.Invoke(FadeComponent, parameters);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SceneLoader: Failed to invoke {methodName}: {e.Message}");
            }
        }

        return false;
    }

    // Public utility methods
    public bool IsLoading => _isLoading;
    
    public void StopAutoLoad()
    {
        if (_autoLoadCoroutine != null)
        {
            StopCoroutine(_autoLoadCoroutine);
            _autoLoadCoroutine = null;
        }
    }

    public void RestartAutoLoad()
    {
        StopAutoLoad();
        if (AutoLoad && !string.IsNullOrEmpty(SceneName))
        {
            _autoLoadCoroutine = StartCoroutine(AutoLoadCoroutine());
        }
    }

    [Button("Validate Scene")]
    private void ValidateScene()
    {
        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogWarning("Scene name is empty");
            return;
        }

        // Check if scene exists in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneName == SceneName)
            {
                Debug.Log($"✓ Scene '{SceneName}' found in build settings at index {i}");
                return;
            }
        }

        Debug.LogError($"✗ Scene '{SceneName}' NOT found in build settings! Add it to File > Build Settings > Scenes In Build");
    }
}
