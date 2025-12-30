using UnityEngine;

public enum PlayerState
{
    Ready,
    Hopping,
    Dead
}

/// <summary>
/// Handles the player character's movement, input, collision detection, and lifecycle events.
/// Manages player state including death, respawn, and interactions with game objects.
/// </summary>
public class Player : MonoBehaviour
{
    private Animator animator;
    private float hopStartTime;
    private Vector2 hopStartPosition;
    private Vector2 hopEndPosition;
    
    public PlayerState State { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        State = PlayerState.Ready;
    }

    private void Update()
    {
        HandleInput();
        if (State == PlayerState.Hopping) UpdateHopMovement();
        CheckIfOffscreen();
    }

    private void HandleInput()
    {
        // Rotation angles for each direction
        const float up = 0;
        const float left = 90;
        const float down = 180;
        const float right = 270;
        
        // Screen boundaries for player movement
        const float topBound = 5.001f;
        const float bottomBound = -6.001f;
        const float leftBound = -5.001f;
        const float rightBound = 6.001f;

        if (State != PlayerState.Ready) return;
        if (Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y <= topBound)
        {
            if (IsHomeBlocked()) return;
            transform.parent = null;
            StartHop(up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && transform.position.y >= bottomBound)
        {
            transform.parent = null;
            StartHop(down);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && transform.position.x <= rightBound) StartHop(right);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.x >= leftBound) StartHop(left);
    }

    /// <summary>
    /// Is the player trying to jump into an occupied home?
    /// </summary>
    /// <returns>true if the home is blocked, otherwise false</returns>
    private bool IsHomeBlocked()
    {
        const float homeRowYThreshold = 4.9f;
        if (!(transform.position.y >= homeRowYThreshold)) return false;
        const float homeXtolerance = 0.5f;
        const float firstHomeXPosition = -5.5f;
        const float homeSpacing = 3f;
        for (int homeNumber = 0; homeNumber < 5; homeNumber++)
        {
            float homeX = firstHomeXPosition + homeSpacing * homeNumber;
            if (transform.position.x >= homeX - homeXtolerance &&
                transform.position.x <= homeX + homeXtolerance &&
                GameManager.Instance.IsHomeOccupied(homeNumber))
            {
                Debug.Log("Player:IsHomeBlocked() - cannot jump up into occupied home #" + homeNumber);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Start a hop in the given direction.
    /// </summary>
    /// <param name="direction">The direction of the hop.</param>
    private void StartHop(float direction)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.hop);
        transform.rotation = Quaternion.Euler(0, 0, direction);
        hopStartPosition = transform.localPosition;
        hopEndPosition = transform.localPosition + transform.up;
        animator.SetTrigger("Hop");
        State = PlayerState.Hopping;
        hopStartTime = Time.time;
    }

    /// <summary>
    /// Move the player during a hop.
    /// </summary>
    private void UpdateHopMovement()
    {
        const float hopDuration = 8f / 60;
        float tValue = Mathf.Clamp01((Time.time - hopStartTime) / hopDuration);
        transform.localPosition = Vector2.Lerp(hopStartPosition, hopEndPosition, tValue);
        if (tValue >= 1) Land();
    }
    
    /// <summary>
    /// Handle landing after a hop completes.
    /// </summary>
    private void Land()
    {
        // Did the player land on a moving platform? If so, parent to it.
        Collider2D hit = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("MovingPlatform"));
        if (hit != null) transform.parent = hit.transform;

        // Did the player land in water?
        else if (Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Water")) != null)
        {
            transform.parent = null;
            DieDrown();
            return;
        }

        // The player landed safely (on ground or a moving platform).
        State = PlayerState.Ready;
        animator.SetTrigger("Stop");
        GameManager.Instance.EvaluatePlayerProgressY(transform.position.y);
    }

    /// <summary>
    /// The player has hit a collider, so find out if it hit something hazardous.
    /// </summary>
    /// <param name="other">The other GameObject's Collider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance.gameState == GameState.Playing && other.CompareTag("Hazard"))
        {
            // Don't die if overlapping a Home.
            if (Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Home")) != null) return;

            transform.parent = null;
            DieHazard();
        }
    }

    /// <summary>
    /// The player has drowned, so play the drown animation and sound effect.
    /// </summary>
    public void DieDrown()
    {
        Die();
        SoundManager.Instance.PlaySound(SoundManager.Instance.drown);
        animator.SetTrigger("Die_Drown");
    }

    /// <summary>
    /// The player has died from a hazard, so play the hazard death animation and sound effect.
    /// </summary>
    public void DieHazard()
    {
        Die();
        SoundManager.Instance.PlaySound(SoundManager.Instance.dieHazard);
        animator.SetTrigger("Die_Hazard");
    }

    /// <summary>
    /// The player has died, so update state, reset rotation, and stop the music.
    /// </summary>
    private void Die()
    {
        State = PlayerState.Dead;
        GameManager.Instance.gameState = GameState.Dead;
        transform.rotation = Quaternion.identity;
        MusicManager.Instance.StopMusic();
    }

    /// <summary>
    /// This is called as an animation event at the end of the death animations.
    /// </summary>
    private void OnDeathAnimationComplete()
    {
        GameManager.Instance.OnDeathAnimationComplete();
    }

    /// <summary>
    /// If player rides a platform offscreen, they die and reappear on the opposite side.
    /// </summary>
    private void CheckIfOffscreen()
    {
        const float offscreenLeftBoundary = -7f;
        const float offscreenRightBoundary = 8f;
        const float screenWrapDistance = 16f;

        float posX = transform.position.x;
        if ((State != PlayerState.Dead) && (posX < offscreenLeftBoundary || posX > offscreenRightBoundary))
        {
            DieHazard();
            // Wrap around to other side of screen
            if (posX < offscreenLeftBoundary) transform.position += Vector3.right * screenWrapDistance;
            else transform.position += Vector3.left * screenWrapDistance;
        }
    }

    /// <summary>
    /// Respawns the player at the starting position and resets state.
    /// </summary>
    public void Respawn()
    {
        transform.parent = null;
        transform.position = new Vector2(1, -6);

        GameManager.Instance.gameState = GameState.Playing;
        if (State == PlayerState.Hopping)
        {
            // The player hopped into home, so reset the animation to stop hopping.
            animator.SetTrigger("Stop");
        }

        State = PlayerState.Ready;
    }
}