
using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    public Color flashColor = Color.white;
    public float flashDuration = 0.12f;

    private Renderer[] renderers;
    private MaterialPropertyBlock[] blocks;
    private int[] colorProperties;
    private Coroutine flashRoutine;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        blocks = new MaterialPropertyBlock[renderers.Length];
        colorProperties = new int[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].sharedMaterial;

            if (material == null)
                continue;

            if (material.HasProperty("_BaseColor"))
                colorProperties[i] =
                    Shader.PropertyToID("_BaseColor");
            else if (material.HasProperty("_Color"))
                colorProperties[i] =
                    Shader.PropertyToID("_Color");

            blocks[i] = new MaterialPropertyBlock();
        }
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (blocks[i] == null ||
                colorProperties[i] == 0)
                continue;

            renderers[i].GetPropertyBlock(blocks[i]);

            blocks[i].SetColor(
                colorProperties[i],
                flashColor
            );

            renderers[i].SetPropertyBlock(blocks[i]);
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (blocks[i] == null)
                continue;

            blocks[i].Clear();
            renderers[i].SetPropertyBlock(blocks[i]);
        }

        flashRoutine = null;
    }
}
