using System.Collections.Generic;
using UnityEngine;

public class RandomlyActivateObject : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objects = new List<GameObject>();

    [SerializeField]
    private bool activateOnStart = true;

    [SerializeField]
    [Range(0f, 1f)]
    private float activationChance = 0.5f;

    private void Start()
    {
        if (activateOnStart)
            ActivateRandomObject();
    }

    public GameObject ActivateRandomObject()
    {
        bool hasValidObject = false;
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
                hasValidObject = true;
                break;
            }
        }

        if (!hasValidObject)
        {
            Debug.LogWarning("[RandomlyActivateObject] No valid objects are configured.", this);
            return null;
        }

        GameObject firstActivatedObject = null;
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject candidate = objects[i];
            if (candidate == null)
                continue;

            bool shouldActivate = Random.value <= activationChance;
            candidate.SetActive(shouldActivate);
            if (shouldActivate && firstActivatedObject == null)
                firstActivatedObject = candidate;
        }

        return firstActivatedObject;
    }

    public void DeactivateAllObjects()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(false);
        }
    }
}