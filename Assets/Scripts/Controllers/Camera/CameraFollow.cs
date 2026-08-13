using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Targeting")]
	[SerializeField]
	private Transform currentTarget;

	[SerializeField]
	private Vector3 offset = new Vector3(0f, 0f, -10f);

	[Header("Movement Settings")]
	[Tooltip("Approximately the time it takes to reach the target. Smaller = faster.")]
	[SerializeField]
	private float smoothTime = 0.3f;

	private Vector3 currentVelocity = Vector3.zero;

	void LateUpdate()
	{
		if (currentTarget == null)
			return;

		Vector3 targetPosition = currentTarget.position + offset;

		transform.position = Vector3.SmoothDamp(
			transform.position,
			targetPosition,
			ref currentVelocity,
			smoothTime
		);
	}

	/// <summary>
	/// Call this from any script to dynamically change who the camera is looking at.
	/// </summary>
	public void SetTarget(Transform newTarget)
	{
		currentTarget = newTarget;
	}

	/// <summary>
	/// Dynamically change target AND adjust how fast the camera snaps to them.
	/// </summary>
	public void SetTarget(Transform newTarget, float temporarySmoothTime)
	{
		currentTarget = newTarget;
		smoothTime = temporarySmoothTime;
	}
}