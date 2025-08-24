using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Creates a dynamic line between this transform and another transform.
/// Useful for connecting objects, showing relationships, or creating dynamic cables/ropes.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class DynamicLineRenderer : MonoBehaviour
{
    [BoxGroup("Line Settings")]
    [Tooltip("The target transform to draw a line to")]
    public Transform TargetTransform;
    
    [BoxGroup("Line Settings")]
    [Tooltip("Should the line update every frame?")]
    public bool UpdateContinuously = true;
    
    [BoxGroup("Line Settings")]
    [Tooltip("Offset from this transform's position")]
    public Vector3 StartOffset = Vector3.zero;
    
    [BoxGroup("Line Settings")]
    [Tooltip("Offset from the target transform's position")]
    public Vector3 EndOffset = Vector3.zero;

    [BoxGroup("Performance")]
    [Tooltip("How often to update the line (in seconds). 0 = every frame")]
    public float UpdateInterval = 0f;

    private LineRenderer _lineRenderer;
    private float _lastUpdateTime;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        
        if (_lineRenderer == null)
        {
            Debug.LogError($"DynamicLineRenderer on {gameObject.name}: No LineRenderer component found!", this);
            enabled = false;
            return;
        }

        // Ensure we have exactly 2 points
        _lineRenderer.positionCount = 2;
    }

    private void Start()
    {
        UpdateLine();
    }

    private void Update()
    {
        if (!UpdateContinuously) return;

        if (UpdateInterval <= 0f || Time.time - _lastUpdateTime >= UpdateInterval)
        {
            UpdateLine();
            _lastUpdateTime = Time.time;
        }
    }

    [Button("Update Line Now")]
    public void UpdateLine()
    {
        if (_lineRenderer == null || TargetTransform == null)
        {
            if (_lineRenderer != null && TargetTransform == null)
            {
                Debug.LogWarning($"DynamicLineRenderer on {gameObject.name}: Target transform is null!", this);
            }
            return;
        }

        Vector3 startPos = transform.position + StartOffset;
        Vector3 endPos = TargetTransform.position + EndOffset;

        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);
    }

    public void SetTarget(Transform newTarget)
    {
        TargetTransform = newTarget;
        if (newTarget != null)
        {
            UpdateLine();
        }
    }

    public void SetOffsets(Vector3 startOffset, Vector3 endOffset)
    {
        StartOffset = startOffset;
        EndOffset = endOffset;
        UpdateLine();
    }

    // Public properties
    public float LineLength => TargetTransform != null ? 
        Vector3.Distance(transform.position + StartOffset, TargetTransform.position + EndOffset) : 0f;
    
    public bool HasValidTarget => TargetTransform != null;
}
