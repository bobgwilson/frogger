using UnityEngine;

/// <summary>
/// Base class for game objects that move horizontally (logs, vehicles, turtle groups).
/// </summary>
public class MovingObject : MonoBehaviour
{
    [Header("MovingObject")]
    
    [Tooltip("Speed in pixels per second.")]
    [SerializeField] protected float speed = 10;

    [Tooltip("Left boundary in Unity units for wrapping.")]
    [SerializeField] private float xMin = -8;
    
    [Tooltip("Right boundary in Unity units for wrapping.")]
    [SerializeField] private float xMax = 8;

    // Precise floating-point position before pixel snapping
    private Vector2 pos;

    private const int PixelsPerUnit = 16;

    private void Awake()
    {
        pos = transform.position;
    }

    /// <summary>
    /// Moves the object horizontally and wraps it around screen boundaries with pixel-perfect positioning.
    /// </summary>
    void Update()
    {
        pos.x += speed / PixelsPerUnit * Time.deltaTime;
        if (pos.x <= xMin)
        {
            FinishedLap();
            pos.x = xMax;
        }
        else if (pos.x >= xMax)
        {
            FinishedLap();
            pos.x = xMin;
        }
        
        // Snap to pixel grid. (Pixel Perfect with pixel snapping does this in Mac/PC builds, but doesn't work in WebGL.)
        transform.position = new Vector2(Mathf.Round(pos.x * PixelsPerUnit) / PixelsPerUnit, pos.y);
    }

    /// <summary>
    /// Called when the object wraps around the screen. Override to add custom behavior.
    /// </summary>
    protected virtual void FinishedLap() {}
}
