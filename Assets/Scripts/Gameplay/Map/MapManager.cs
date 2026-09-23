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
    private float minimumSpeed = 1f;

    [SerializeField]
    [Min(0f)]
    private float speedIncreasePerSecond = 1f;

    [SerializeField]
    [Min(0f)]
    private float maximumSpeed = 20f;

    // The actual current speed.
    private float currentSpeed;

    // Time remaining before speed starts increasing again.
    private float accelerationDelay;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools =
        new Dictionary<GameObject, ObjectPool<GameObject>>();

    private readonly Dictionary<GameObject, GameObject> prefabByInstance =
        new Dictionary<GameObject, GameObject>();

    private readonly List<GameObject> activeSections = new List<GameObject>();

    private GameObject previousPrefab;

    private float totalDistanceTravelled;

    private bool hasLoggedMissingPrefabWarning;

    public List<GameObject> SectionsPrefabs => sectionPrefabs;

    public float TotalDistanceTravelled => totalDistanceTravelled;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        minimumSpeed = Mathf.Min(minimumSpeed, maximumSpeed);

        currentSpeed = Mathf.Max(startingSpeed, minimumSpeed);
        currentSpeed = Mathf.Min(currentSpeed, maximumSpeed);

        accelerationDelay = 0f;
    }

    private void Start()
    {
        SpawnInitialSections();
    }

    private void FixedUpdate()
    {
        if (activeSections.Count == 0)
            return;

        UpdateSpeed();

        float distance = currentSpeed * Time.fixedDeltaTime;

        Debug.Log(currentSpeed + "| | " + Time.fixedDeltaTime);
        for (int i = 0; i < activeSections.Count; i++)
        {
            GameObject section = activeSections[i];

            if (section != null)
            {
                section.transform.position += Vector3.left * distance;
            }
        }

        totalDistanceTravelled += distance;

        RecycleExitedSections();
    }

    private void UpdateSpeed()
    {
        // A slowdown has happened.
        // Keep the current speed unchanged until the delay expires.
        if (accelerationDelay > 0f)
        {
            accelerationDelay -= Time.fixedDeltaTime;

            if (accelerationDelay < 0f)
                accelerationDelay = 0f;

            return;
        }

        // The slowdown duration has expired.
        // Start increasing the current speed again.
        currentSpeed += speedIncreasePerSecond * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, minimumSpeed, maximumSpeed);
    }

    /// <summary>
    /// Permanently reduces the current speed.
    ///
    /// Example:
    /// Current speed = 10
    /// multiplier = 0.5
    /// duration = 3
    ///
    /// Speed becomes 5 permanently.
    /// It stays at 5 for 3 seconds.
    /// Then it starts increasing again:
    /// 5 -> 5.1 -> 5.2 -> etc.
    /// </summary>
    public void ApplySpeedModifier(float multiplier, float duration)
    {
        if (multiplier <= 0f)
            multiplier = 0.01f;

        multiplier = Mathf.Clamp(multiplier, 0.01f, 1f);

        // Permanently reduce the current speed.
        currentSpeed *= multiplier;

        // Prevent the speed from becoming zero.

        currentSpeed = Mathf.Clamp(currentSpeed, minimumSpeed, maximumSpeed);
        // Reset the acceleration delay.
        accelerationDelay = Mathf.Max(0f, duration);
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
        {
            randomActivators[i].ActivateRandomObject();
        }

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
        if (section == null)
            return;

        GameObject prefab;
        ObjectPool<GameObject> pool;

        if (!prefabByInstance.TryGetValue(section, out prefab))
        {
            return;
        }

        if (!pools.TryGetValue(prefab, out pool))
        {
            return;
        }

        pool.Release(section);
    }

    private GameObject ChoosePrefab()
    {
        int validCount = 0;

        for (int i = 0; i < sectionPrefabs.Count; i++)
        {
            if (sectionPrefabs[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        if (validCount == 1 || previousPrefab == null)
        {
            int selectedIndex = Random.Range(0, validCount);

            for (int i = 0; i < sectionPrefabs.Count; i++)
            {
                GameObject prefab = sectionPrefabs[i];

                if (prefab == null)
                    continue;

                if (selectedIndex == 0)
                    return prefab;

                selectedIndex--;
            }

            return null;
        }

        // Pick a random prefab that isn't the previous prefab.
        int randomIndex = Random.Range(0, validCount - 1);

        for (int i = 0; i < sectionPrefabs.Count; i++)
        {
            GameObject prefab = sectionPrefabs[i];

            if (prefab == null || prefab == previousPrefab)
                continue;

            if (randomIndex == 0)
                return prefab;

            randomIndex--;
        }

        return null;
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
            {
                ReleaseSection(activeSections[i]);
            }
        }

        activeSections.Clear();

        previousPrefab = null;
    }

    private void RemoveNullActiveSections()
    {
        for (int i = activeSections.Count - 1; i >= 0; i--)
        {
            if (activeSections[i] == null)
            {
                activeSections.RemoveAt(i);
            }
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
