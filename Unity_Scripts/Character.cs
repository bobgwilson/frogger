using UnityEngine;

/// <summary>
/// Displays a single character using a sprite from a custom Font mapping.
/// </summary>
[ExecuteInEditMode]
public class Character : MonoBehaviour
{
    public char character;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        DisplayCharacter();
    }

    public void DisplayCharacter()
    {
        spriteRenderer.sprite = BitmapFont.Instance.charToSprite[character];
    }
}