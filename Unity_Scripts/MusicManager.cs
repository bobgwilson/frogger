using UnityEngine;

/// <summary>
/// Manages background music playback throughout the game lifecycle with global access through a singleton instance.
/// Handles intro, main theme, death, respawn, and victory music sequences.
/// </summary>
public sealed class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    #region References
    [SerializeField] private AudioClip songIntro;
    [SerializeField] private AudioClip songGameOver;
    [SerializeField] private AudioClip songLevelComplete;
    [SerializeField] private AudioClip songMainTheme;
    [SerializeField] private AudioClip songRespawnAfterDeath;
    [SerializeField] private AudioClip[] songNumberOfOpenHomes;
    private AudioSource musicSource;
    #endregion

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple MusicManager instances detected! There should only be one in the scene.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        musicSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        PlayIntro();
    }
    
    private void Update()
    {
        // After any song finishes, play the main theme.
        if (GameManager.Instance.gameState == GameState.Playing && !musicSource.isPlaying) PlayMainTheme();
    }

    private void PlayIntro()
    {
        PlayMusic(songIntro);
    }
    
    private void PlayMainTheme()
    {
        PlayMusic(songMainTheme);
    }
    
    private void PlayMusic(AudioClip clip)
    {
        StopMusic();
        musicSource.clip = clip;
        if (clip == null)
        {
            Debug.LogWarning("MusicManager: PlayMusic called with null clip");
            return;
        }
        musicSource.Play();
    }
    
    public void PlayMusicDependingOnHomesFilled()
    {
        PlayMusic(songNumberOfOpenHomes[GameManager.Instance.HomesFilled]);
    }

    public void PlayGameOver()
    {
        PlayMusic(songGameOver);
    }
    
    public void PlayRespawnAfterDeath()
    {
        PlayMusic(songRespawnAfterDeath);
    }
    
    public void PlayLevelComplete()
    {
        PlayMusic(songLevelComplete);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}