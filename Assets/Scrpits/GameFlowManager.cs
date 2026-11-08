
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    [Header("Game Systems")]
    public PlayerHealth playerHealth;
    public EnemySpawner enemySpawner;
    public RescueManager rescueManager;

    [Header("Result Panels")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    [Header("Result Text")]
    public TMP_Text victorySummaryText;
    public TMP_Text gameOverSummaryText;

    private bool gameEnded;

    void Awake()
    {
        Time.timeScale = 1f;
        gameEnded = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (playerHealth == null ||
            enemySpawner == null ||
            rescueManager == null)
        {
            Debug.LogError(
                "GameFlowManager: Assign all game systems."
            );
        }
    }

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;

        if (enemySpawner != null)
            enemySpawner.OnAllWavesCleared +=
                HandleAllWavesCleared;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;

        if (enemySpawner != null)
            enemySpawner.OnAllWavesCleared -=
                HandleAllWavesCleared;
    }

    void HandlePlayerDied()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        Debug.Log("GAME OVER");

        if (gameOverSummaryText != null)
        {
            int wave = enemySpawner != null
                ? enemySpawner.CurrentWave : 0;

            int totalWaves = enemySpawner != null
                ? enemySpawner.waves.Length : 3;

            int rescued = rescueManager != null
                ? rescueManager.RescuedCount : 0;

            gameOverSummaryText.text =
                $"Wave Reached: {wave}/{totalWaves}\n" +
                $"Horses Rescued: {rescued}/2";
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void HandleAllWavesCleared()
    {
        if (gameEnded)
            return;

        if (playerHealth == null || playerHealth.IsDead)
            return;

        // Both rescue objectives are required.
        if (rescueManager == null ||
            !rescueManager.IsRescueCompleteForWave(1) ||
            !rescueManager.IsRescueCompleteForWave(3))
        {
            Debug.LogError(
                "Cannot win: rescue objectives incomplete."
            );
            return;
        }

        gameEnded = true;

        Debug.Log("VICTORY!");

        if (victorySummaryText != null)
        {
            victorySummaryText.text =
                $"All Waves Cleared!\n" +
                $"Horses Rescued: " +
                $"{rescueManager.RescuedCount}/2";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
