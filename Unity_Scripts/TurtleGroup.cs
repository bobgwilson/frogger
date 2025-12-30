using UnityEngine;
using NaughtyAttributes; // This is used to hide diving attributes in the Editor for turtles that don't dive.

/// <summary>
/// Turtle group class. Handles swimming and diving animations for a group of turtles that all swim and dive in sync.
/// </summary>
public class TurtleGroup : MonoBehaviour
{
    [Header("Swimming")]
    public Sprite[] swimSprites;
    
    [Header("Diving")]
    public bool doesDive;
    [ShowIf(nameof(doesDive))] public int framesBeforeFirstDive = 24;
    [ShowIf(nameof(doesDive))] public Sprite[] diveSprites;
    [ShowIf(nameof(doesDive))] public int[] diveSpriteDurations;
    
    private SpriteRenderer[] spriteRenderers;
    private float levelStartTime;
    private float timeSinceLevelStart;
    private float nextDiveStartTime;
    
    // I am manually swapping the turtle sprites for the animation instead of using an Animator component,
    // to guarantee that all turtles on screen always swim in sync (even after diving and resurfacing).
    // I also did this to learn how to manually control animation of multiple sprites with different durations.
    private const float FramesPerSecond = 60f;
    private const float TimeBetweenDives = 48 / FramesPerSecond;
    private const float SwimSpriteDuration = 16 / FramesPerSecond;
    private const int Underwater = 2; // dive sprite index when turtles are completely underwater
    private const int DiveFinished = -1; // sentinel dive sprite index indicating the dive is finished
    
    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        levelStartTime = Time.time;
        nextDiveStartTime = framesBeforeFirstDive / FramesPerSecond;
    }
    
    /// <summary>
    /// Handles the per-frame logic of whether to dive or swim.
    /// </summary>
    void Update()
    {
        timeSinceLevelStart = Time.time - levelStartTime;
        if (doesDive && (timeSinceLevelStart >= nextDiveStartTime)) Dive();
        else Swim();
    }

    /// <summary>
    /// Updates all turtle sprites to the current swimming frame.
    /// </summary>
    private void Swim()
    {
        int currentSwimFrame = (int)(timeSinceLevelStart / SwimSpriteDuration) % swimSprites.Length;
        UpdateSpriteRenderers(swimSprites[currentSwimFrame]);
    }
    
    /// <summary>
    /// Handles dive animation, scheduling the next dive, and player drowning when the turtles are completely underwater.
    /// </summary>
    private void Dive()
    {
        int diveSpriteIndex = GetDiveSpriteIndex();
        if (diveSpriteIndex == DiveFinished)
        {
            nextDiveStartTime = timeSinceLevelStart + TimeBetweenDives;
            Swim();
            return;
        }
        UpdateSpriteRenderers(diveSprites[diveSpriteIndex]);
        if (diveSpriteIndex == Underwater) TryToDrownPlayer();
    }
    
    /// <summary>
    /// Calculates the current dive animation sprite index.
    /// </summary>
    /// <returns>
    /// The dive sprite index, or the sentinel value DiveFinished when the dive animation is complete.
    /// </returns>
    private int GetDiveSpriteIndex()
    {
        int diveFrames = (int)(FramesPerSecond * (timeSinceLevelStart - nextDiveStartTime));
        for (int i = 0; i < diveSpriteDurations.Length; i++)
        {
            if (diveFrames < diveSpriteDurations[i]) return i;
            diveFrames -= diveSpriteDurations[i];
        }
        return DiveFinished;
    }
    
    /// <summary>
    /// Updates all turtle sprite renderers in the group to display the specified sprite.
    /// </summary>
    /// <param name="sprite">The sprite to display on all renderers.</param>
    private void UpdateSpriteRenderers(Sprite sprite)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sprite = sprite;
        }
    }
    
    /// <summary>
    /// Drowns the player if they are alive and currently riding this turtle group while it is fully underwater.
    /// </summary>
    private void TryToDrownPlayer()
    {
        Player player = GetComponentInChildren<Player>();
        if ((player != null) && (player.State != PlayerState.Dead))
        {
            player.DieDrown();
        }
    }
}
