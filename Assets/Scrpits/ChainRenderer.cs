
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ChainRenderer : MonoBehaviour
{
    [Header("References")]
    public Transform chainAnchor;
    public Transform ironBall;

    [Header("Visual Settings")]
    public float chainWidth = 0.12f;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

        line.startWidth = chainWidth;
        line.endWidth = chainWidth;
    }

    void LateUpdate()
    {
        if (chainAnchor == null || ironBall == null)
            return;

        line.SetPosition(0, chainAnchor.position);
        line.SetPosition(1, ironBall.position);
    }
}
