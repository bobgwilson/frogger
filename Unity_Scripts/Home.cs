using UnityEngine;

/// <summary>
/// Represents one of the five goal positions at the top of the level that the player jumps into.
/// </summary>
public class Home : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsOccupied) return;
        if (other.gameObject.CompareTag("Player") && GameManager.Instance.gameState == GameState.Playing)
        {
            IsOccupied = true;
            spriteRenderer.enabled = true;
            GameManager.Instance.OnPlayerReachedHome();
        }
    }
}
