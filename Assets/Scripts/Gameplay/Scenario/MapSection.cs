using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapSection : MonoBehaviour
{
    [Header("Compatible Sections")]
    [SerializeField]
    private List<GameObject> compatibleSections = new();

    public List<GameObject> CompatibleSections => compatibleSections;

    [Header("Size Gizmos")]
    [SerializeField]
    private bool showSizeGizmos = true;

    [SerializeField]
    private bool showLabels = true;

    [SerializeField]
    [Min(0.01f)]
    private float expectedSectionLength = 25f;

    [SerializeField]
    [Min(0.01f)]
    private float pivotMarkerSize = 0.5f;

    [Header("Gizmo Colors")]
    [SerializeField]
    private Color rendererColor = Color.green;

    [SerializeField]
    private Color colliderColor = Color.magenta;

    [SerializeField]
    private Color pivotExpectedColor = Color.red;

    [SerializeField]
    private Color geometryExpectedColor = Color.blue;

    [SerializeField]
    private Color pivotColor = Color.yellow;

    private bool TryGetCombinedRendererBounds(out Bounds bounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        bounds = new Bounds();
        bool initialized = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer is ParticleSystemRenderer)
                continue;

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return initialized;
    }

    private bool TryGetCombinedColliderBounds(out Bounds bounds)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);

        bounds = new Bounds();
        bool initialized = false;

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            if (!initialized)
            {
                bounds = col.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        return initialized;
    }

    private void OnDrawGizmos()
    {
        if (!showSizeGizmos)
            return;

        bool hasRenderers = TryGetCombinedRendererBounds(out Bounds rendererBounds);

        bool hasColliders = TryGetCombinedColliderBounds(out Bounds colliderBounds);

        if (!hasRenderers && !hasColliders)
            return;

        Bounds referenceBounds = hasRenderers ? rendererBounds : colliderBounds;

        // ACTUAL RENDERER BOUNDS
        if (hasRenderers)
        {
            Gizmos.color = rendererColor;

            Gizmos.DrawWireCube(rendererBounds.center, rendererBounds.size);
        }

        // ACTUAL COLLIDER BOUNDS
        if (hasColliders)
        {
            Gizmos.color = colliderColor;

            Gizmos.DrawWireCube(colliderBounds.center, colliderBounds.size);
        }

        // EXPECTED SECTION DIMENSIONS
        Vector3 expectedSize = new Vector3(
            expectedSectionLength,
            referenceBounds.size.y,
            referenceBounds.size.z
        );

        // RED: Expected bounds centered on prefab pivot
        Vector3 pivotExpectedCenter = new Vector3(
            transform.position.x,
            referenceBounds.center.y,
            referenceBounds.center.z
        );

        Gizmos.color = pivotExpectedColor;

        Gizmos.DrawWireCube(pivotExpectedCenter, expectedSize);

        // BLUE: Expected bounds centered on actual geometry
        Vector3 geometryExpectedCenter = referenceBounds.center;

        Gizmos.color = geometryExpectedColor;

        Gizmos.DrawWireCube(geometryExpectedCenter, expectedSize);

        // PIVOT MARKER
        Gizmos.color = pivotColor;

        Vector3 pivot = transform.position;

        Gizmos.DrawSphere(pivot, pivotMarkerSize * 0.15f);

        Gizmos.DrawLine(
            pivot - Vector3.right * pivotMarkerSize,
            pivot + Vector3.right * pivotMarkerSize
        );

        Gizmos.DrawLine(pivot - Vector3.up * pivotMarkerSize, pivot + Vector3.up * pivotMarkerSize);

        Gizmos.DrawLine(
            pivot - Vector3.forward * pivotMarkerSize,
            pivot + Vector3.forward * pivotMarkerSize
        );

        // LABELS
#if UNITY_EDITOR
        if (showLabels)
        {
            DrawLabels(referenceBounds, hasRenderers, rendererBounds, hasColliders, colliderBounds);
        }
#endif
    }

#if UNITY_EDITOR

    private void DrawLabels(
        Bounds referenceBounds,
        bool hasRenderers,
        Bounds rendererBounds,
        bool hasColliders,
        Bounds colliderBounds
    )
    {
        string label = "";

        label += $"{gameObject.name}\n";
        label += $"Expected Width: {expectedSectionLength:F2} units\n";

        if (hasRenderers)
        {
            Vector3 offset = rendererBounds.center - transform.position;

            float difference = rendererBounds.size.x - expectedSectionLength;

            label += "\n--- RENDERER ---\n";
            label += $"Width: {rendererBounds.size.x:F2}\n";
            label += $"Height: {rendererBounds.size.y:F2}\n";
            label += $"Depth: {rendererBounds.size.z:F2}\n";
            label += $"Pivot Offset X: {offset.x:+0.00;-0.00;0.00}\n";
            label += $"Width Difference: {difference:+0.00;-0.00;0.00}\n";
        }

        if (hasColliders)
        {
            Vector3 offset = colliderBounds.center - transform.position;

            float difference = colliderBounds.size.x - expectedSectionLength;

            label += "\n--- COLLIDERS ---\n";
            label += $"Width: {colliderBounds.size.x:F2}\n";
            label += $"Height: {colliderBounds.size.y:F2}\n";
            label += $"Depth: {colliderBounds.size.z:F2}\n";
            label += $"Pivot Offset X: {offset.x:+0.00;-0.00;0.00}\n";
            label += $"Width Difference: {difference:+0.00;-0.00;0.00}\n";
        }

        Handles.Label(
            referenceBounds.center + Vector3.up * (referenceBounds.extents.y + 1f),
            label
        );
    }

#endif
}
