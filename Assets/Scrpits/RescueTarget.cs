
using System;
using UnityEngine;
using TMPro;

public class RescueTarget : MonoBehaviour
{
    [Header("Wave")]
    public int waveNumber = 1;

    [Header("References")]
    public Transform player;
    public GameObject visualRoot;
    public TMP_Text rescuePrompt;

    [Header("Rescue Settings")]
    public float rescueRadius = 2.5f;
    public float rescueDuration = 1.5f;

    public bool IsRescued { get; private set; }

    public event Action<RescueTarget> OnRescued;

    private PlayerHealth playerHealth;
    private float rescueTimer;
    private bool isAvailable;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        SetAvailable(false);
    }

    void Update()
    {
        if (!isAvailable || IsRescued || player == null)
            return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            HidePrompt();
            return;
        }

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;

        bool isNearby =
            offset.sqrMagnitude <= rescueRadius * rescueRadius;

        if (!isNearby)
        {
            rescueTimer = 0f;
            HidePrompt();
            return;
        }

        rescueTimer += Time.deltaTime;

        float duration = Mathf.Max(0.1f, rescueDuration);

        float progress = Mathf.Clamp01(
            rescueTimer / duration
        );

        if (rescuePrompt != null)
        {
            rescuePrompt.gameObject.SetActive(true);
            rescuePrompt.text =
                $"Rescuing Horse... {progress * 100f:F0}%";
        }

        if (rescueTimer >= duration)
            CompleteRescue();
    }

    public void SetAvailable(bool available)
    {
        isAvailable = available && !IsRescued;
        rescueTimer = 0f;

        if (visualRoot != null)
            visualRoot.SetActive(isAvailable);

        HidePrompt();
    }

    void CompleteRescue()
    {
        if (IsRescued)
            return;

        IsRescued = true;
        isAvailable = false;

        if (visualRoot != null)
            visualRoot.SetActive(false);

        HidePrompt();

        Debug.Log(
            $"Horse Rescued! Wave {waveNumber}"
        );

        OnRescued?.Invoke(this);
    }

    void HidePrompt()
    {
        if (rescuePrompt != null)
            rescuePrompt.gameObject.SetActive(false);
    }
}
