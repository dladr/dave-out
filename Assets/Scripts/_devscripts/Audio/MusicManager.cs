using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Simple priority-based music manager that handles multiple music sources
/// and ensures only the highest priority one plays.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [BoxGroup("Settings")]
    [Tooltip("Priority of this music manager. Higher values take precedence.")]
    public int Priority = 0;
    
    [BoxGroup("Settings")]
    [Tooltip("Should this music manager persist across scene loads?")]
    public bool KeepAlive = true;

    [BoxGroup("Debug")]
    [SerializeField, ReadOnly]
    private bool _isActiveMusicManager;

    private void Awake()
    {
        HandlePersistence();
        HandlePriority();
    }

    private void HandlePersistence()
    {
        if (KeepAlive)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void HandlePriority()
    {
        var allMusicManagers = FindObjectsByType<MusicManager>(FindObjectsSortMode.None);
        MusicManager managerToDestroy = null;
        MusicManager highestPriorityManager = this;

        foreach (var musicManager in allMusicManagers)
        {
            if (musicManager == this) continue;

            if (musicManager.Priority < Priority)
            {
                // This manager has lower priority, mark for destruction
                if (managerToDestroy == null || musicManager.Priority < managerToDestroy.Priority)
                {
                    managerToDestroy = musicManager;
                }
            }
            else if (musicManager.Priority > Priority)
            {
                // Found a manager with higher priority
                highestPriorityManager = musicManager;
                managerToDestroy = this;
                break;
            }
            else if (musicManager.Priority == Priority)
            {
                // Same priority - destroy the newer one (this one)
                managerToDestroy = this;
                break;
            }
        }

        _isActiveMusicManager = (managerToDestroy != this);

        if (managerToDestroy != null)
        {
            Debug.Log($"MusicManager: Destroying {managerToDestroy.gameObject.name} (Priority: {managerToDestroy.Priority}). Active manager: {highestPriorityManager.gameObject.name} (Priority: {highestPriorityManager.Priority})");
            Destroy(managerToDestroy.gameObject);
        }
        else
        {
            Debug.Log($"MusicManager: {gameObject.name} is now the active music manager (Priority: {Priority})");
        }
    }

    [Button("Force Check Priority")]
    private void ForceCheckPriority()
    {
        HandlePriority();
    }

    // Public properties for external access
    public bool IsActiveMusicManager => _isActiveMusicManager;
}
