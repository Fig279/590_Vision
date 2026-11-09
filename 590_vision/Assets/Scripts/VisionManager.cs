using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

/// <summary>
/// singleton 
/// tracks whether vision is on, runs the fade, and dimming
/// keeps the list of VisionTargets
/// </summary>
public class VisionManager : MonoBehaviour
{
    public static VisionManager Instance { get; private set; }

    [SerializeField] private VisionSettings settings;
    // Global Volume with Color Adjustments that dims the normal scene 
    [SerializeField] private Volume dimmingVolume;
    // Transparent Shader Graph material with _HighlightColor and _Intensity properties to determine the appearance
    [SerializeField] private Material highlightMaterial;

    [Header("Events")]
    public UnityEvent onVisionEnabled;
    public UnityEvent onVisionDisabled;

    //True when vision has been switched on (even while still fading/transitioning)
    public bool IsOn { get; private set; }
    // 0 = fully off, 1 = fully on
    public float Amount { get; private set; }

    public Material HighlightMaterial => highlightMaterial;
    // Layer index used by the highlight copies
    public int HighlightLayer { get; private set; } = -1;

    private readonly List<VisionTarget> targets = new List<VisionTarget>();
    private float nextToggleTime;

    private static readonly int ColorId = Shader.PropertyToID("_HighlightColor");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

    private void Awake()
    {
        // singleton check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // required assets check
        if (settings == null)
        {
            Debug.LogError("VisionManager: no VisionSettings assigned.", this);
            return;
        }

        // grab the correct layer for highlight objects
        HighlightLayer = LayerMask.NameToLayer(settings.highlightLayerName);

        if (HighlightLayer < 0)
            Debug.LogError($"VisionManager: layer '{settings.highlightLayerName}' does not exist. " +
                           "Add it in Project Settings > Tags and Layers.", this);

        ApplyVisuals();
    }

    private void Start()
    {
        // Pick up any targets that enabled before this existed
        foreach (var targets in FindObjectsByType<VisionTarget>(FindObjectsSortMode.None))
            Register(targets);
    }

    /// <summary>
    /// if this is removed  / destroyed ensure everything gets turned off
    /// </summary>
    private void OnDestroy()
    {
        if (Instance != this) return;

        foreach (var target in targets)
            if (target != null) target.SetHighlighted(false);

        Amount = 0f;
        ApplyVisuals();
        Instance = null;
    }

    private void Update()
    {
        // if we are fading, continue to fade
        float goal = IsOn ? 1f : 0f;
        if (Mathf.Approximately(Amount, goal)) return;

        Amount = Mathf.MoveTowards(Amount, goal, Time.unscaledDeltaTime / settings.fadeTime);
        ApplyVisuals();

        // Fully faded out, hide every target's highlight copy
        if (Amount <= 0f && !IsOn)
            foreach (var target in targets)
                if (target != null) target.SetHighlighted(false);
    }

    public void Toggle() => SetVision(!IsOn);

    public void SetVision(bool on)
    {
        if (settings == null || on == IsOn) return;
        if (Time.unscaledTime < nextToggleTime) return;
        nextToggleTime = Time.unscaledTime + settings.cooldown;

        IsOn = on;
        if (on)
        {
            foreach (var t in targets)
                if (t != null) t.SetHighlighted(true);
            onVisionEnabled.Invoke();
        }
        else
        {
            onVisionDisabled.Invoke();
        }
    }

    /// <summary>
    /// track a new highlight target
    /// </summary>
    /// <param name="target"> new highlight target to be tracked </param>
    public void Register(VisionTarget target)
    {
        if (target == null || targets.Contains(target)) return;
        targets.Add(target);
        if (IsOn || Amount > 0f)
            target.SetHighlighted(true);
    }

    public void Unregister(VisionTarget target)
    {
        targets.Remove(target);
    }

    /// <summary>
    /// checks if effects need to be applied to objects, applies them if so
    /// </summary>
    private void ApplyVisuals()
    {
        if (settings == null) return;

        if (dimmingVolume != null)
            dimmingVolume.weight = Amount * settings.dimAmount;

        if (highlightMaterial != null)
        {
            highlightMaterial.SetColor(ColorId, settings.highlightColor);
            highlightMaterial.SetFloat(IntensityId, Amount * settings.highlightIntensity);
        }
    }
}
