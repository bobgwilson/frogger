using UnityEngine;

/// <summary>
/// Displays and manages the player's remaining lives visually in the UI.
/// The game starts with 3 lives, and will display the 2 extra lives as icons.
/// </summary>
public class LivesDisplay : MonoBehaviour
{
    [SerializeField] private GameObject lifeIconPrefab;
    
    /// <summary>
    /// Displays the specified number of extra life icons in the UI.
    /// </summary>
    /// <param name="extraLives">The number of extra lives to display.</param>
    public void DisplayLifeIcons(int extraLives)
    {
        const float startXPosition = -6.3125f;
        const float yPosition = -6.75f;
        const float frogSpacing = 9.0f / 16.0f;
        for (int i = 0; i < extraLives; i++)
        {
            GameObject extraLifeIcon = Instantiate(lifeIconPrefab, transform);
            extraLifeIcon.transform.localPosition = new Vector3(startXPosition + i * frogSpacing, yPosition, 0);
        }
    }
    
    /// <summary>
    /// Removes the right-most (most recently added) extra life icon from the display.
    /// </summary>
    public void RemoveLifeIcon()
    {
        Transform extraLifeIcon = transform.GetChild(transform.childCount - 1);
        Destroy(extraLifeIcon.gameObject);
    }
}
