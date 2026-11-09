using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// One per scene. Tracks whether vision is on, runs the fade, drives the dimming
/// Volume weight and highlight material, and keeps the list of VisionTargets.
/// </summary>
public class VisionManager : MonoBehaviour
{
    public static VisionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private VisionSettings settings;
    //[Tooltip("Global Volume with Color Adjustments that dims the normal scene.")]
    //[SerializeField] private Volume dimmingVolume;
    [Tooltip("The material used by the Render Objects renderer feature.")]
    [SerializeField] private Material highlightMaterial;

    [Header("Input")]
    [Tooltip("Quick built-in key so you can test right away. Turn off if you call Toggle() yourself.")]
    [SerializeField] private bool useBuiltInToggleKey = true;

    [Header("Events")]
    public UnityEvent onVisionEnabled;
    public UnityEvent onVisionDisabled;

    /// <summary>True when vision has been switched on (even while still fading in/out).</summary>
    public bool IsOn { get; private set; }
    /// <summary>0 = fully off, 1 = fully on.</summary>
    public float Amount { get; private set; }
    public uint HighlightMask => 1u << settings.highlightRenderingLayer;

    private readonly List<VisionTarget> targets = new List<VisionTarget>();
    private float nextToggleTime;

    private static readonly int ColorId = Shader.PropertyToID("_HighlightColor");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ApplyVisuals();
    }

    private void Start()
    {
        // Pick up any targets that enabled before this manager existed.
        foreach (var t in FindObjectsByType<VisionTarget>(FindObjectsSortMode.None))
            Register(t);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        foreach (var t in targets)
            if (t != null) t.SetHighlighted(false);

        Amount = 0f;
        ApplyVisuals();
        Instance = null;
    }

    private void Update()
    {
        if (useBuiltInToggleKey && TogglePressed())
            Toggle();

        float goal = IsOn ? 1f : 0f;
        if (!Mathf.Approximately(Amount, goal))
        {
            Amount = Mathf.MoveTowards(Amount, goal, Time.unscaledDeltaTime / settings.fadeTime);
            ApplyVisuals();

            // Fully faded out: restore every target's original rendering layers.
            if (Amount <= 0f && !IsOn)
                foreach (var t in targets)
                    if (t != null) t.SetHighlighted(false);
        }
    }

    private static bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.V);
#endif
    }

    /// <summary>Call this from any input, UI button, trigger zone, etc.</summary>
    public void Toggle() => SetVision(!IsOn);

    public void SetVision(bool on)
    {
        if (on == IsOn) return;
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

    private void ApplyVisuals()
    {
        if (settings == null) return;

    //    if (dimmingVolume != null)
     //       dimmingVolume.weight = Amount * settings.dimAmount;

        if (highlightMaterial != null)
        {
            highlightMaterial.SetColor(ColorId, settings.highlightColor);
            highlightMaterial.SetFloat(IntensityId, Amount * settings.highlightIntensity);
        }
    }
}
