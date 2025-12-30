using UnityEngine;

/// <summary>
/// Manages sound effects with global access through a singleton instance.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public sealed class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioClip hop;
    public AudioClip drown;
    public AudioClip dieHazard;
    public AudioClip timeRunningOut;
    public AudioClip raceCar;
    public AudioClip reachedHome;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple SoundManager instances detected! There should only be one in the scene.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
