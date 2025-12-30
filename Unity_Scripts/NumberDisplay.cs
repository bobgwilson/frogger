using UnityEngine;

/// <summary>
/// Displays a multi-digit number using an array of Character objects.
/// Used for Score and Hi-Score (five digits), and BonusTimeDisplay (two digits).
/// </summary>
public class NumberDisplay : MonoBehaviour
{
    [Tooltip("Array of Character objects, ordered from rightmost (ones) to leftmost digit.")]
    [SerializeField] private Character[] digits;
    
    /// <summary>
    /// Displays the specified number by distributing its digits across the Character array.
    /// </summary>
    /// <param name="number">The number to display.</param>
    public void DisplayNumber(int number)
    {
        foreach (Character digit in digits)
        {
            digit.character = (char)('0' + (number % 10));
            digit.DisplayCharacter();
            number /= 10;
        }
    }
}
