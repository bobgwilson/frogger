using UnityEngine;

/// <summary>
/// Manages the countdown timer, updates its visual representation, and triggers events when time is running out or has expired.
/// </summary>
public class Timer : MonoBehaviour
{
    [SerializeField] private float startTime;
    [SerializeField] private float timeLeft;
    [SerializeField] private SpriteRenderer[] timeLabelSpriteRenderers;
    [SerializeField] private bool timeRunningOutSoundHasPlayed;
    private SpriteRenderer spriteRenderer;

    private readonly Color32 white = new(195, 195, 217, 255);
    private readonly Color32 red = new(224, 0, 0, 255);
    private readonly Color32 yellow = new(224, 224, 0, 255);
    private readonly Color32 green = new(29, 195, 0, 255);
    
    private const int TimeSlicesPerSecond = 2;
    public int TimeSlicesLeft => (int)(timeLeft * TimeSlicesPerSecond);

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ResetTimer();
    }

    /// <summary>
    /// Resets the timer to its initial state, size, and color.
    /// </summary>
    public void ResetTimer()
    {
        startTime = Time.time;
        timeRunningOutSoundHasPlayed = false;
        SetTimeLabelColor(yellow);
        spriteRenderer.color = green;
        UpdateTimer();
    }
    
    /// <summary>
    /// Updates and displays the timer, and triggers events when time is running out or over.
    /// </summary>
    public void UpdateTimer()
    {
        const float timerDuration = 30;
        const float pixelsPerUnit = 16f;
        const float timerHeight = 0.5f;
        const float timeRunningOutThreshold = 5;
        Vector2 startPosition = new(1.75f, -7.25f);

        float timeElapsed = Time.time - startTime;
        timeLeft = timerDuration - timeElapsed;

        // Update timer position and scale. It scales from the right side from full length to zero over 30 seconds.
        transform.localScale = new Vector2(TimeSlicesLeft * 2 / pixelsPerUnit, timerHeight);
        float xOffset = (timerDuration * TimeSlicesPerSecond - TimeSlicesLeft) / pixelsPerUnit;
        transform.position = startPosition + Vector2.right * xOffset;
        
        if (timeLeft < 0) GameManager.Instance.OnTimeOver();
        else if (timeLeft <= timeRunningOutThreshold && !timeRunningOutSoundHasPlayed) OnTimeRunningOut();
    }

    /// <summary>
    /// Plays a warning sound and changes the timer's colors when time is running out.
    /// </summary>
    private void OnTimeRunningOut()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.timeRunningOut);
        timeRunningOutSoundHasPlayed = true;
        SetTimeLabelColor(white);
        spriteRenderer.color = red;
    }

    /// <summary>
    /// Sets the color of the time label letters.
    /// </summary>
    /// <param name="color">The color to apply</param>
    private void SetTimeLabelColor(Color32 color)
    {
        foreach (SpriteRenderer letterSpriteRenderer in timeLabelSpriteRenderers)
        {
            letterSpriteRenderer.color = color;
        }
    }
}
