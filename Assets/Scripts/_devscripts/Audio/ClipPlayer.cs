using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ClipPlayer : MonoBehaviour 
{
    [BoxGroup("Clips")]
    public AudioClip[] Clips;
    
    [BoxGroup("Audio Settings")]
    [Range(0f, 1f)]
    public float PitchRandomization = 0.3f;
    
    [BoxGroup("Audio Settings")]
    public bool NoOverlap = true;
    
    [BoxGroup("Timer Settings")]
    public bool PlayOnTimer;
    
    [BoxGroup("Timer Settings")]
    [ShowIf("PlayOnTimer")]
    public Vector2 MinMaxPlaytime = new Vector2(1f, 3f);

    [SerializeField, ReadOnly]
    private Queue<AudioClip> _clipQueue;
    
    [SerializeField, ReadOnly]
    private bool _isTimerActive;
    
    private AudioSource _audioSource;
    private Coroutine _timerCoroutine;

    void Awake()
    {
        _clipQueue = new Queue<AudioClip>();
        _audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (PlayOnTimer)
            StartTimerCoroutine();
    }

    void OnDisable()
    {
        StopTimerCoroutine();
    }

    void LoadClips()
    {
        if (Clips == null || Clips.Length == 0)
        {
            Debug.LogWarning($"ClipPlayer on {gameObject.name} has no clips assigned!", this);
            return;
        }

        var shuffledClips = Clips.ToArray();
        shuffledClips.ShuffleArray();
        
        _clipQueue.Clear();
        foreach (var clip in shuffledClips)
        {
            if (clip != null)
                _clipQueue.Enqueue(clip);
        }
    }

    void StartTimerCoroutine()
    {
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
        
        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    void StopTimerCoroutine()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
        _isTimerActive = false;
    }

    IEnumerator TimerCoroutine()
    {
        while (PlayOnTimer && enabled)
        {
            _isTimerActive = true;
            var timeToWait = Random.Range(MinMaxPlaytime.x, MinMaxPlaytime.y);
            yield return new WaitForSeconds(timeToWait);
            
            PlayClip();
        }
        _isTimerActive = false;
    }

    [Button("Play Random Clip")]
    public void PlayClip()
    {
        if (_audioSource.isPlaying && NoOverlap)
            return;

        if (!_clipQueue.Any())
            LoadClips();

        if (!_clipQueue.Any())
        {
            Debug.LogWarning($"ClipPlayer on {gameObject.name} has no valid clips to play!", this);
            return;
        }

        var clip = _clipQueue.Dequeue();
        PlayClip(clip);
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"Attempted to play null clip on {gameObject.name}", this);
            return;
        }

        if (_audioSource.isPlaying && NoOverlap)
            return;

        _audioSource.clip = clip;
        _audioSource.pitch = 1f + Random.Range(-PitchRandomization, PitchRandomization);
        _audioSource.Play();
    }

    public void PlayClip(int index)
    {
        if (Clips == null || index < 0 || index >= Clips.Length)
        {
            Debug.LogWarning($"Invalid clip index {index} on {gameObject.name}", this);
            return;
        }
        
        PlayClip(Clips[index]);
    }

    [Button("Stop Audio")]
    public void StopAudio()
    {
        if (_audioSource.isPlaying)
            _audioSource.Stop();
    }

    // Public properties for external access
    public bool IsPlaying => _audioSource.isPlaying;
    public bool IsTimerActive => _isTimerActive;
}
