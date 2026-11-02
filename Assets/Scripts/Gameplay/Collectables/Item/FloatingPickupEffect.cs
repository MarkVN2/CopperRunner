using UnityEngine;

public class FloatingPickupEffect : MonoBehaviour
{
    [SerializeField]
    [Min(0f)]
    private float floatHeight = 0.12f;

    [SerializeField]
    [Min(0f)]
    private float floatFrequency = 2f;

    [SerializeField]
    [Min(0f)]
    private float wobbleAngle = 6f;

    [SerializeField]
    [Min(0f)]
    private float wobbleFrequency = 1.5f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float phaseOffset;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        phaseOffset = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        float time = Time.time;
        float floatWave = Mathf.Sin(time * floatFrequency + phaseOffset);
        float wobbleWave = Mathf.Sin(time * wobbleFrequency + phaseOffset);

        transform.localPosition =
            initialLocalPosition + Vector3.up * (floatWave * floatHeight);
        transform.localRotation =
            initialLocalRotation * Quaternion.Euler(0f, 0f, wobbleWave * wobbleAngle);
    }
}
