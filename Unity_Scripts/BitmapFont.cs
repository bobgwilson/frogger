using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that maps characters to sprites for custom font rendering.
/// </summary>
[ExecuteInEditMode]
public class BitmapFont : MonoBehaviour
{
    public static BitmapFont Instance { get; private set; }
    
    public readonly Dictionary<char, Sprite> charToSprite = new();
    [SerializeField] private Sprite[] sprites;
    private const string CharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-©#";

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple BitmapFont instances detected! There should only be one in the scene.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        for (int i = 0; i < sprites.Length; i++)
        {
            charToSprite[CharacterSet[i]] = sprites[i];
        }
    }
}