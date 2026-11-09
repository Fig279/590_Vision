using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Add to an object that should be highlighted while vision is on
/// On first use it creates render-only "highlight copies" as children of each
/// The copies sit on the VisionHighlight layer use the highlight material, and have no colliders
/// the objects rea layer and physics are never touched
/// </summary>
[DisallowMultipleComponent]
public class VisionTarget : MonoBehaviour
{
    private readonly List<GameObject> proxies = new List<GameObject>();
    private bool built;
    private bool highlighted;

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

    /// <summary>
    /// turn highlighted on or off
    /// </summary>
    /// <param name="on">true if turning on, false if turning off</param>
    public void SetHighlighted(bool on)
    {
        // check if these settings have already been applied
        if (on == highlighted) return;

        if (on)
        {
            var manager = VisionManager.Instance;
            if (manager == null || manager.HighlightMaterial == null || manager.HighlightLayer < 0)
                return;
            if (!built)
                BuildProxies(manager);
        }

        foreach (var proxy in proxies)
            // check the highlighted object exists before acting on it
            if (proxy != null) proxy.SetActive(on);

        highlighted = on;
    }

    /// <summary>
    /// creates the objects to be highlighted for their corresponding targets
    /// </summary>
    /// <param name="manager"></param>
    private void BuildProxies(VisionManager manager)
    {
        built = true;

        // Collect first so newly created copies are never picked up
        var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        var skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        // create a new mesh renderer
        foreach (var mr in meshRenderers)
        {
            var mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var go = CreateProxyObject(mr.transform, manager.HighlightLayer);
            go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var proxyRenderer = go.AddComponent<MeshRenderer>();
            ConfigureRenderer(proxyRenderer, mf.sharedMesh.subMeshCount, manager.HighlightMaterial);
        }

        // create a new skin renderer
        foreach (var smr in skinnedRenderers)
        {
            if (smr.sharedMesh == null) continue;

            var go = CreateProxyObject(smr.transform, manager.HighlightLayer);
            var proxyRenderer = go.AddComponent<SkinnedMeshRenderer>();
            proxyRenderer.sharedMesh = smr.sharedMesh;
            proxyRenderer.bones = smr.bones;
            proxyRenderer.rootBone = smr.rootBone;
            proxyRenderer.localBounds = smr.localBounds;
            ConfigureRenderer(proxyRenderer, smr.sharedMesh.subMeshCount, manager.HighlightMaterial);
        }
    }

    private GameObject CreateProxyObject(Transform parent, int layer)
    {
        var gameObject = new GameObject("VisionHighlightCopy");
        gameObject.SetActive(false);
        gameObject.layer = layer;

        // same pose as the original
        gameObject.transform.SetParent(parent, false);
        proxies.Add(gameObject);
        return gameObject;
    }

    private static void ConfigureRenderer(Renderer renderer, int subMeshCount, Material highlight)
    {
        var mats = new Material[Mathf.Max(1, subMeshCount)];
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = highlight;
        }

        // set settings 
        renderer.sharedMaterials = mats;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }
}
