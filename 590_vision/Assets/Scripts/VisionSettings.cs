using UnityEngine;

/// <summary>
///  values for the vision effect
///  could potentially create multiple setting assets and use different tags to have different effects applied
/// </summary>
[CreateAssetMenu(fileName = "VisionSettings", menuName = "Vision/Vision Settings")]
public class VisionSettings : ScriptableObject
{

    public Color highlightColor = new Color(1f, 0.8f, 0.1f, 1f);
    [Min(0f)] public float highlightIntensity = 1f;

    // looks for things on this layer to highlight
    public string highlightLayerName = "VisionHighlight";

    //How strongly the dim is applied while vision is on (0 = none, 1 = full)
    [Range(0f, 1f)] public float dimAmount = 1f;

    [Min(0.01f)] public float fadeTime = 0.4f;

    // unusued, it currently works even if the player spams(I think)
    [Min(0f)] public float cooldown = 0f;
}
