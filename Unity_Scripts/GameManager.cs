using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Dead,
    GameOver,
    LevelComplete
}

/// <summary>
/// Singleton that manages the game state, score, player lives, UI updates, and scene transitions.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region References
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private NumberDisplay scoreDisplay;
    [SerializeField] private NumberDisplay highScoreDisplay;
    [SerializeField] private NumberDisplay bonusTimeDisplay;
    [SerializeField] private LivesDisplay livesDisplay;
    [SerializeField] private Timer timer;
    [SerializeField] private GameObject timeOverDisplay;
    [SerializeField] private GameObject gameOverDisplay;
    [SerializeField] private GameObject gameplayObjects;
    [SerializeField] private Home[] homes;
    #endregion
    
    #region Runtime State
    [Header("Runtime State")]
    public GameState gameState;
    public int HomesFilled { get; private set; }
    private int extraLives;
    private int playerMaxY;
    private float timeSinceGameOver;
    
    private int score;
    /// <summary>
    /// The current player score. Setting this value updates the UI and high score if exceeded.
    /// </summary>
    private int Score
    {
        get => score;
        set
        {
            score = value;
            scoreDisplay.DisplayNumber(score);
            if (score > highScore) HighScore = score;
        }
    }
    
    private int highScore;
    /// <summary>
    /// The player's current score. Automatically updates UI and high score when set.
    /// </summary>
    private int HighScore
    {
        set
        {
            highScore = value;
            highScoreDisplay.DisplayNumber(highScore);
            PlayerPrefs.SetInt("highscore", highScore);
        }
    }
    #endregion
    
    /// <summary>
    /// Ensures only one GameManager singleton exists and assigns the player reference.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameManager instances detected! There should only be one in the scene.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (player == null) player = FindFirstObjectByType<Player>();
    }
    
    /// <summary>
    /// Initializes the game state, UI elements, player lives, and high score at the start of the scene.
    /// </summary>
    private void Start()
    {
        gameState = GameState.Playing;
        timeOverDisplay.SetActive(false);
        gameOverDisplay.SetActive(false);
        bonusTimeDisplay.gameObject.SetActive(false);
        gameplayObjects.SetActive(true);
        playerMaxY = Mathf.RoundToInt(player.transform.position.y);
        extraLives = 2;
        livesDisplay.DisplayLifeIcons(extraLives);
        if (PlayerPrefs.HasKey("highscore")) HighScore = PlayerPrefs.GetInt("highscore");
    }
    
    /// <summary>
    /// Updates the timer during gameplay and handles input for restarting the game after gameover.
    /// </summary>
    private void Update()
    {
        if (gameState == GameState.Playing) timer.UpdateTimer();
        else if (gameState == GameState.GameOver)
        {
            const float restartDelay = 1; // Seconds before restart input is accepted
            timeSinceGameOver += Time.unscaledDeltaTime;
            if (timeSinceGameOver >= restartDelay && Input.anyKeyDown) RestartGame();
        }
    }
    
    /// <summary>
    /// Add points to the score if making vertical progress, unless jumping to the median
    /// </summary>
    /// <param name="y">the player's current vertical position</param>
    public void EvaluatePlayerProgressY(float y)
    {
        if (y - playerMaxY >= 0.999)
        {
            playerMaxY = Mathf.RoundToInt(y);
            const int playerProgressYPoints = 10;
            if (playerMaxY != 0) Score += playerProgressYPoints; 
        }
    }
    
    /// <summary>
    /// Checks whether the specified home is currently occupied by a frog.
    /// </summary>
    /// <param name="homeNumber">The index of the home to check.</param>
    /// <returns>True if the home is occupied; otherwise, false.</returns>
    public bool IsHomeOccupied(int homeNumber)
    {
        return homes[homeNumber].IsOccupied;
    }
    
    /// <summary>
    /// Handles logic when the player reaches a home, including scoring, UI updates, and level progression.
    /// </summary>
    public void OnPlayerReachedHome()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.reachedHome);
        const int timeSlicePoints = 10;
        const int reachedHomePoints = 50;
        Score += reachedHomePoints + timeSlicePoints * timer.TimeSlicesLeft;
        ShowBonusTimeDisplay();
        HomesFilled += 1;
        if (HomesFilled < 5)
        {
            RespawnPlayer();
            MusicManager.Instance.PlayMusicDependingOnHomesFilled();
        }
        else OnLevelComplete();
    }
    
    /// <summary>
    /// Handles logic when the player completes a level: plays music, awards bonus, updates state, and schedules restart.
    /// </summary>
    private void OnLevelComplete()
    {
        MusicManager.Instance.PlayLevelComplete();
        const int levelCompletePoints = 1000;
        Score += levelCompletePoints;
        player.gameObject.SetActive(false);
        gameState = GameState.LevelComplete;
        const float restartDelayAfterLevelComplete = 7;
        Invoke(nameof(RestartGame), restartDelayAfterLevelComplete);
    }
    
    /// <summary>
    /// Shows the bonus time display UI element and schedules it to hide after a set duration.
    /// </summary>
    private void ShowBonusTimeDisplay()
    {
        const float bonusTimeDisplayDuration = 4.25f;

        bonusTimeDisplay.gameObject.SetActive(true);
        bonusTimeDisplay.DisplayNumber(timer.TimeSlicesLeft);
        CancelInvoke(nameof(HideBonusTimeDisplay));
        Invoke(nameof(HideBonusTimeDisplay), bonusTimeDisplayDuration);
    }
    
    /// <summary>
    /// Hides the bonus time display UI element.
    /// </summary>
    private void HideBonusTimeDisplay()
    {
        bonusTimeDisplay.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Handles logic when time runs out, showing the time over display and killing the player.
    /// </summary>
    public void OnTimeOver()
    {
        timeOverDisplay.SetActive(true);
        player.DieHazard();
    }
    
    /// <summary>
    /// After the death animation completes, reduce lives and either respawn or end the game.
    /// </summary>
    public void OnDeathAnimationComplete()
    {
        extraLives -= 1;
        if (extraLives >= 0)
        {
            livesDisplay.RemoveLifeIcon();
            RespawnPlayer();
            MusicManager.Instance.PlayRespawnAfterDeath();
        }
        else GameOver();
    }

    /// <summary>
    /// Respawns the player, resets the timer, and updates the UI.
    /// </summary>
    private void RespawnPlayer()
    {
        player.Respawn();
        timer.ResetTimer();
        timeOverDisplay.SetActive(false);
        playerMaxY = Mathf.RoundToInt(player.transform.position.y);
    }
    
    /// <summary>
    /// Handles game over logic: hides gameplay UI, stops the game, displays the game over screen, and plays the game over music.
    /// </summary>
    private void GameOver()
    {
        HideBonusTimeDisplay();
        gameplayObjects.SetActive(false);
        player.gameObject.SetActive(false);
        gameState = GameState.GameOver;
        gameOverDisplay.SetActive(true);
        timeOverDisplay.SetActive(false);
        Time.timeScale = 0;
        timeSinceGameOver = 0;
        MusicManager.Instance.PlayGameOver();
    }
    
    /// <summary>
    /// Restarts the game by reloading the current scene.
    /// </summary>
    private void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}