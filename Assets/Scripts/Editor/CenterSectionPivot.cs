using UnityEditor;
using UnityEngine;

public class CenterSectionPivot : Editor
{
    [MenuItem("Tools/Map Sections/Center Selected Section")]
    private static void CenterSelectedSection()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("Select a section prefab first.");
            return;
        }

        Renderer[] renderers = selected.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found.");
            return;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Calculate offset relative to the root.
        Vector3 offset = bounds.center - selected.transform.position;

        // Ignore vertical and depth offsets if you only want
        // to center sections along the X axis.
        offset.y = 0f;
        offset.z = 0f;

        if (offset.sqrMagnitude < 0.0001f)
        {
            Debug.Log("Section is already centered.");
            return;
        }

        Undo.SetCurrentGroupName("Center Section Geometry");
        int undoGroup = Undo.GetCurrentGroup();

        // Move all root children in the opposite direction.
        foreach (Transform child in selected.transform)
        {
            Undo.RecordObject(child, "Center Section Geometry");

            child.position -= offset;
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"Centered section '{selected.name}'. " + $"Applied offset: {-offset}", selected);

        EditorUtility.SetDirty(selected);
    }
}
