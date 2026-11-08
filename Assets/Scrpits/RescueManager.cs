
using UnityEngine;
using TMPro;

public class RescueManager : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public RescueTarget[] rescueTargets;

    [Header("UI")]
    public TMP_Text rescueCountText;

    public int RescuedCount { get; private set; }

    void Awake()
    {
        if (enemySpawner == null)
        {
            Debug.LogError(
                "RescueManager: EnemySpawner not assigned."
            );
            return;
        }

        bool hasWave1 = false;
        bool hasWave3 = false;

        foreach (RescueTarget target in rescueTargets)
        {
            if (target == null)
                continue;

            target.SetAvailable(false);
            target.OnRescued += HandleRescued;

            if (target.waveNumber == 1)
                hasWave1 = true;

            if (target.waveNumber == 3)
                hasWave3 = true;
        }

        if (!hasWave1 || !hasWave3)
        {
            Debug.LogWarning(
                "RescueManager: Assign rescue targets " +
                "for both Wave 1 and Wave 3."
            );
        }

        // Subscribe in Awake so Wave 1 is not missed.
        enemySpawner.OnWaveStarted += HandleWaveStarted;

        UpdateUI();
    }

    void HandleWaveStarted(int waveNumber)
    {
        Debug.Log(
            $"RescueManager: Wave {waveNumber} started."
        );

        foreach (RescueTarget target in rescueTargets)
        {
            if (target == null)
                continue;

            bool shouldAppear =
                target.waveNumber == waveNumber &&
                !target.IsRescued;

            target.SetAvailable(shouldAppear);
        }

        UpdateUI();
    }

    void HandleRescued(RescueTarget target)
    {
        RescuedCount++;

        Debug.Log(
            $"Rescued Horses: {RescuedCount}/{rescueTargets.Length}"
        );

        UpdateUI();
    }

    public bool IsRescueCompleteForWave(int waveNumber)
    {
        // Only Wave 1 and 3 require rescue.
        if (waveNumber != 1 && waveNumber != 3)
            return true;

        bool hasTarget = false;

        foreach (RescueTarget target in rescueTargets)
        {
            if (target == null ||
                target.waveNumber != waveNumber)
                continue;

            hasTarget = true;

            if (!target.IsRescued)
                return false;
        }

        // Do not silently skip a missing rescue objective.
        return hasTarget;
    }

    void UpdateUI()
    {
        if (rescueCountText != null)
        {
            rescueCountText.text =
                $"Horses Rescued: {RescuedCount}/" +
                $"{rescueTargets.Length}";
        }
    }

    void OnDestroy()
    {
        if (enemySpawner != null)
        {
            enemySpawner.OnWaveStarted -= HandleWaveStarted;
        }

        foreach (RescueTarget target in rescueTargets)
        {
            if (target != null)
                target.OnRescued -= HandleRescued;
        }
    }
}
