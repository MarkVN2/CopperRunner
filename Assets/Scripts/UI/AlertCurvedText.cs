using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class AlertCurvedText : MonoBehaviour
{
    [Header("Curve")]
    [SerializeField]
    private AnimationCurve vertexCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 0.25f),
        new Keyframe(1f, 0f)
    );

    [SerializeField]
    private float curveScale = 30f;

    [Header("Middle Character")]
    [Tooltip("Scale multiplier applied to the middle character (1 = normal, 1.4 = 40% bigger).")]
    [SerializeField]
    private float middleCharScale = 1.35f;

    private TMP_Text tmpText;
    private bool updating;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        ApplyCurve();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        ResetMesh();
    }

    private void OnValidate()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
        ApplyCurve();
    }

    private void OnTextChanged(Object obj)
    {
        if (obj == tmpText && !updating)
            ApplyCurve();
    }

    public void ResetMesh()
    {
        if (tmpText == null || updating)
            return;
        updating = true;
        try
        {
            tmpText.ForceMeshUpdate();
        }
        finally
        {
            updating = false;
        }
    }

    public void ApplyCurve()
    {
        if (tmpText == null || updating)
            return;

        updating = true;
        try
        {
            tmpText.ForceMeshUpdate();
            TMP_TextInfo info = tmpText.textInfo;
            int charCount = info.characterCount;
            if (charCount == 0)
                return;

            // Collect visible character indices so spaces/hidden chars don't skew the middle
            int visibleCount = 0;
            for (int i = 0; i < charCount; i++)
            {
                if (info.characterInfo[i].isVisible)
                    visibleCount++;
            }
            if (visibleCount == 0)
                return;

            int middleVisibleIndex = visibleCount / 2;
            int currentVisibleIndex = 0;

            float minX = tmpText.bounds.min.x;
            float maxX = tmpText.bounds.max.x;
            float width = maxX - minX;
            bool hasWidth = width > 0.001f;

            for (int i = 0; i < charCount; i++)
            {
                TMP_CharacterInfo charInfo = info.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                bool isMiddle = (currentVisibleIndex == middleVisibleIndex);
                currentVisibleIndex++;

                int vIdx = charInfo.vertexIndex;
                int mIdx = charInfo.materialReferenceIndex;
                if (mIdx < 0 || mIdx >= info.meshInfo.Length)
                    continue;

                Vector3[] verts = info.meshInfo[mIdx].vertices;
                if (vIdx + 3 >= verts.Length)
                    continue;

                Vector3 pivot = new Vector3(
                    (verts[vIdx + 0].x + verts[vIdx + 2].x) * 0.5f,
                    charInfo.baseLine,
                    0f
                );

                float yOffset = 0f;
                float angleDeg = 0f;

                if (hasWidth)
                {
                    float u0 = Mathf.Clamp01((pivot.x - minX) / width);
                    float u1 = Mathf.Clamp01(u0 + 0.01f);
                    yOffset = vertexCurve.Evaluate(u0) * curveScale;
                    float yNext = vertexCurve.Evaluate(u1) * curveScale;

                    float dx = (u1 - u0) * width;
                    float dy = yNext - yOffset;
                    if (Mathf.Abs(dx) > 0.0001f || Mathf.Abs(dy) > 0.0001f)
                        angleDeg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                }

                float scale = isMiddle ? middleCharScale : 1f;
                Matrix4x4 trs = Matrix4x4.TRS(
                    new Vector3(0f, yOffset, 0f),
                    Quaternion.Euler(0f, 0f, angleDeg),
                    new Vector3(scale, scale, 1f)
                );

                for (int j = 0; j < 4; j++)
                {
                    Vector3 local = verts[vIdx + j] - pivot;
                    verts[vIdx + j] = trs.MultiplyPoint3x4(local) + pivot;
                }
            }

            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
        finally
        {
            updating = false;
        }
    }
}
