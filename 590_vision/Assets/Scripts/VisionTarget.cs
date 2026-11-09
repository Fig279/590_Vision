using UnityEngine;

/// <summary>
/// Add to any object that should be highlighted while vision is on.
/// Registers with the VisionManager and adds the vision rendering layer to
/// its renderers while vision is active, restoring the originals afterward.
/// </summary>
[DisallowMultipleComponent]
public class VisionTarget : MonoBehaviour
{
    private Renderer[] renderers;
    private uint[] originalMasks;
    private bool highlighted;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalMasks = new uint[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalMasks[i] = renderers[i].renderingLayerMask;
    }

    private void OnEnable()
    {
        if (VisionManager.Instance != null)
            VisionManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (VisionManager.Instance != null)
            VisionManager.Instance.Unregister(this);
        SetHighlighted(false);
    }

    public void SetHighlighted(bool on)
    {
        if (on == highlighted || renderers == null) return;
        if (on && VisionManager.Instance == null) return;

        uint mask = on ? VisionManager.Instance.HighlightMask : 0u;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].renderingLayerMask = on ? (originalMasks[i] | mask) : originalMasks[i];
        }
        highlighted = on;
    }
}
