using UnityEngine;

public class TipDetector : MonoBehaviour
{
    private LegsTool parentTool;

    void Start()
    {
        parentTool = GetComponentInParent<LegsTool>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (parentTool != null) parentTool.AttemptStick(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (parentTool != null) parentTool.AttemptStick(other);
    }
}