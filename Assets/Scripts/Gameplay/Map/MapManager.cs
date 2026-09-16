using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MapManager : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField]
    private List<GameObject> sectionPrefabs = new List<GameObject>();

    [SerializeField]
    [Min(1)]
    private int activeSectionCount = 4;

    [SerializeField]
    [Min(0.01f)]
    private float sectionLength = 20f;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private Transform despawnPoint;

    [Header("Movement")]
    [SerializeField]
    [Min(0f)]
    private float startingSpeed = 5f;

    [SerializeField]
    [Min(0f)]
    private float speedIncreasePerSecond = 0.1f;

    [SerializeField]
    [Min(0f)]
    private float maximumSpeed = 20f;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools =
        new Dictionary<GameObject, ObjectPool<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> prefabByInstance =
        new Dictionary<GameObject, GameObject>();
    private readonly List<GameObject> activeSections = new List<GameObject>();
    private GameObject previousPrefab;
    private float totalDistanceTravelled;
    private float speedMultiplier = 1f;
    private float runStartedAt;
    private bool hasLoggedMissingPrefabWarning;

    public List<GameObject> SectionsPrefabs => sectionPrefabs;
    public float TotalDistanceTravelled => totalDistanceTravelled;

    public float CurrentSpeed
    {
        get
        {
            float elapsedRunTime = Mathf.Max(0f, Time.time - runStartedAt);
            float acceleratedSpeed = startingSpeed + speedIncreasePerSecond * elapsedRunTime;
            return Mathf.Min(acceleratedSpeed, maximumSpeed) * speedMultiplier;
        }
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        if (multiplier <= 0f)
            multiplier = 0.01f;

        speedMultiplier = Mathf.Clamp(
            speedMultiplier * Mathf.Clamp(multiplier, 0.01f, 1f),
            0.01f,
            1f
        );
    }

    private void Start()
    {
        runStartedAt = Time.time;
        SpawnInitialSections();
    }

    private void OnDestroy()
    {
        ClearActiveSections();
        foreach (ObjectPool<GameObject> pool in pools.Values)
            pool.Clear();
        pools.Clear();
        prefabByInstance.Clear();
    }

    private void FixedUpdate()
    {
        if (runStartedAt <= 0f)
            runStartedAt = Time.time;

        if (activeSections.Count == 0)
            return;

        float distance = CurrentSpeed * Time.fixedDeltaTime;
        for (int i = 0; i < activeSections.Count; i++)
        {
            if (activeSections[i] != null)
                activeSections[i].transform.position += Vector3.left * distance;
        }

        totalDistanceTravelled += distance;
        RecycleExitedSections();
    }

    private void SpawnInitialSections()
    {
        ClearActiveSections();

        int sectionCount = Mathf.Max(1, activeSectionCount);
        for (int i = 0; i < sectionCount; i++)
        {
            Vector3 position = GetSpawnPosition();
            position.x += i * sectionLength;
            SpawnSection(position);
        }
    }

    private void RecycleExitedSections()
    {
        RemoveNullActiveSections();

        float exitX =
            despawnPoint != null ? despawnPoint.position.x : transform.position.x - sectionLength;

        while (
            activeSections.Count > 0
            && activeSections[0].transform.position.x + sectionLength <= exitX
        )
        {
            GameObject section = activeSections[0];
            activeSections.RemoveAt(0);
            ReleaseSection(section);

            Vector3 position = GetSpawnPosition();
            if (activeSections.Count > 0)
            {
                position.x =
                    activeSections[activeSections.Count - 1].transform.position.x + sectionLength;
            }

            SpawnSection(position);
        }
    }

    private void SpawnSection(Vector3 position)
    {
        GameObject prefab = ChoosePrefab();
        if (prefab == null)
        {
            if (!hasLoggedMissingPrefabWarning)
            {
                Debug.LogWarning("[MapManager] No valid section prefabs are configured.", this);
                hasLoggedMissingPrefabWarning = true;
            }

            return;
        }

        GameObject section = GetPool(prefab).Get();
        section.transform.SetPositionAndRotation(position, Quaternion.identity);
        RandomlyActivateObject[] randomActivators =
            section.GetComponentsInChildren<RandomlyActivateObject>(true);
        for (int i = 0; i < randomActivators.Length; i++)
            randomActivators[i].ActivateRandomObject();

        activeSections.Add(section);
        previousPrefab = prefab;
    }

    private ObjectPool<GameObject> GetPool(GameObject prefab)
    {
        ObjectPool<GameObject> pool;
        if (pools.TryGetValue(prefab, out pool))
            return pool;

        pool = new ObjectPool<GameObject>(
            createFunc: delegate
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.name = prefab.name + " (Pooled)";
                prefabByInstance[instance] = prefab;
                return instance;
            },
            actionOnGet: delegate(GameObject instance)
            {
                instance.SetActive(true);
            },
            actionOnRelease: delegate(GameObject instance)
            {
                instance.SetActive(false);
                instance.transform.SetParent(transform);
            },
            actionOnDestroy: delegate(GameObject instance)
            {
                prefabByInstance.Remove(instance);
                if (instance != null)
                    Destroy(instance);
            },
            collectionCheck: true,
            defaultCapacity: 1,
            maxSize: Mathf.Max(1, activeSectionCount)
        );
        pools.Add(prefab, pool);
        return pool;
    }

    private void ReleaseSection(GameObject section)
    {
        GameObject prefab;
        ObjectPool<GameObject> pool;
        if (
            section == null
            || !prefabByInstance.TryGetValue(section, out prefab)
            || !pools.TryGetValue(prefab, out pool)
        )
        {
            return;
        }

        pool.Release(section);
    }

    private GameObject ChoosePrefab()
    {
        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < sectionPrefabs.Count; i++)
        {
            if (sectionPrefabs[i] != null)
                validPrefabs.Add(sectionPrefabs[i]);
        }

        if (validPrefabs.Count == 0)
            return null;

        if (validPrefabs.Count == 1 || previousPrefab == null)
            return validPrefabs[UnityEngine.Random.Range(0, validPrefabs.Count)];

        int selectedIndex = UnityEngine.Random.Range(0, validPrefabs.Count - 1);
        if (validPrefabs[selectedIndex] == previousPrefab)
            selectedIndex++;

        return validPrefabs[selectedIndex];
    }

    private Vector3 GetSpawnPosition()
    {
        return spawnPoint != null ? spawnPoint.position : transform.position;
    }

    private void ClearActiveSections()
    {
        for (int i = 0; i < activeSections.Count; i++)
        {
            if (activeSections[i] != null)
                ReleaseSection(activeSections[i]);
        }

        activeSections.Clear();
        previousPrefab = null;
    }

    private void RemoveNullActiveSections()
    {
        for (int i = activeSections.Count - 1; i >= 0; i--)
        {
            if (activeSections[i] == null)
                activeSections.RemoveAt(i);
        }
    }

    public void SpawnSection()
    {
        Vector3 position = GetSpawnPosition();
        if (activeSections.Count > 0)
        {
            position.x =
                activeSections[activeSections.Count - 1].transform.position.x + sectionLength;
        }

        SpawnSection(position);
    }

    public void ResetDistance()
    {
        totalDistanceTravelled = 0f;
    }
}
