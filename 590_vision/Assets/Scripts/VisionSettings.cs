using UnityEngine;

/// <summary>
/// All tunable values for the vision effect. Create one via
/// Assets > Create > Vision > Vision Settings and assign it to the VisionManager.
/// </summary>
[CreateAssetMenu(fileName = "VisionSettings", menuName = "Vision/Vision Settings")]
public class VisionSettings : ScriptableObject
{
    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 0.8f, 0.1f, 1f);
    [Min(0f)] public float highlightIntensity = 1f;

    [Tooltip("Index of the Rendering Layer used to mark special items (see Project Settings > Tags and Layers).")]
    [Range(1, 31)] public int highlightRenderingLayer = 1;

    [Header("Dimming")]
    [Tooltip("How strongly the Dimming Volume is applied while vision is on (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float dimAmount = 1f;

    [Header("Timing")]
    [Min(0.01f)] public float fadeTime = 0.4f;
    [Tooltip("Minimum seconds between toggles. 0 = no cooldown.")]
    [Min(0f)] public float cooldown = 0f;
}
