using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MapManager : MonoBehaviour
{
    [Header("Seed")]
    [SerializeField] private bool randomizeSeed = true;
    [SerializeField] private int seed = 12345;
    public int RunSeed => seed;
    private System.Random rng;

    [Header("Difficult Settings")]
    [SerializeField]
    private float distanceUntilShop = 100f;

    [Header("Sections")]
    [SerializeField]
    private List<GameObject> sectionPrefabs = new List<GameObject>();
    [SerializeField]
    private GameObject shopPrefab;

    [SerializeField]
    private GameObject startingSectionPrefab;

    [SerializeField]
    [Min(1)]
    private int activeSectionCount = 4;

    [SerializeField]
    [Min(0.01f)]
    private float sectionLength = 25f;

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
    private float minimumSpeed = 3f;

    [SerializeField]
    [Min(0f)]
    private float speedIncreasePerSecond = 1f;

    [SerializeField]
    [Min(0f)]
    private float maximumSpeed = 30f;

    private float currentSpeed;

    private float accelerationDelay;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools =
        new Dictionary<GameObject, ObjectPool<GameObject>>();

    private readonly Dictionary<GameObject, GameObject> prefabByInstance =
        new Dictionary<GameObject, GameObject>();

    private readonly List<GameObject> activeSections = new List<GameObject>();

    private GameObject previousPrefab;

    [SerializeField]
    private float totalDistanceTravelled;

    private bool hasLoggedMissingPrefabWarning;

    public List<GameObject> SectionsPrefabs => sectionPrefabs;

    public float TotalDistanceTravelled => totalDistanceTravelled;
    private float distanceTraveledSinceLastShop = 0f;
    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        if (randomizeSeed)
            seed = UnityEngine.Random.Range(10000, 100000);
        rng = new System.Random(seed);

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

        for (int i = 0; i < activeSections.Count; i++)
        {
            GameObject section = activeSections[i];

            if (section != null)
            {
                section.transform.position += Vector3.left * distance;
            }
        }
        distanceTraveledSinceLastShop += distance;
        totalDistanceTravelled += distance;

        RecycleExitedSections();
    }

    private void UpdateSpeed()
    {
        if (accelerationDelay > 0f)
        {
            accelerationDelay -= Time.fixedDeltaTime;

            if (accelerationDelay < 0f)
                accelerationDelay = 0f;

            return;
        }

        currentSpeed += speedIncreasePerSecond * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, minimumSpeed, maximumSpeed);
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        if (multiplier <= 0f)
            multiplier = 0.01f;

        multiplier = Mathf.Clamp(multiplier, 0.01f, 1f);

        currentSpeed *= multiplier;
        currentSpeed = Mathf.Clamp(currentSpeed, minimumSpeed, maximumSpeed);
        accelerationDelay = Mathf.Max(0f, duration);
    }

    private void SpawnInitialSections()
    {
        ClearActiveSections();

        int sectionCount = Mathf.Max(1, activeSectionCount);

        for (int i = 0; i < sectionCount; i++)
        {
            Vector3 position = GetSpawnPosition();

            bool isStartingSection = i == 0 && startingSectionPrefab != null;

            if (startingSectionPrefab != null)
                position.x += (i - 1) * sectionLength;
            else
                position.x += i * sectionLength;

            SpawnSection(position, isStartingSection ? startingSectionPrefab : null);
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

    private void SpawnSection(Vector3 position, GameObject prefabOverride = null)
    {
        GameObject prefab;

        if (prefabOverride != null)
        {
            prefab = prefabOverride;
        }
        else if (distanceTraveledSinceLastShop >= distanceUntilShop)
        {
            prefab = shopPrefab;
            distanceTraveledSinceLastShop = 0f;
        }
        else
        {
            prefab = ChoosePrefab();
        }

        if (prefab == null)
        {
            if (!hasLoggedMissingPrefabWarning)
            {
                Debug.LogWarning("[MapManager] No valid section prefab configured.", this);
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
            actionOnGet: delegate (GameObject instance)
            {
                instance.SetActive(true);
            },
            actionOnRelease: delegate (GameObject instance)
            {
                instance.SetActive(false);
                instance.transform.SetParent(transform);
            },
            actionOnDestroy: delegate (GameObject instance)
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

        if (!prefabByInstance.TryGetValue(section, out GameObject prefab))
        {
            return;
        }

        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            return;
        }

        pool.Release(section);
    }

    private GameObject ChoosePrefab()
    {
        List<GameObject> poolToUse = null;
        bool usingCompatibleList = false;

        // Check if the previous prefab has a MapSection component with defined compatible sections
        if (previousPrefab != null)
        {
            MapSection mapSection = previousPrefab.GetComponent<MapSection>();
            if (mapSection != null)
            {
                if (mapSection.CompatibleSections != null && mapSection.CompatibleSections.Count > 0)
                {
                    poolToUse = mapSection.CompatibleSections;
                    usingCompatibleList = true;
                }
                else
                {
                    Debug.LogWarning($"[MapManager] '{previousPrefab.name}' has a MapSection component, but its 'CompatibleSections' list is empty. Falling back to general section prefabs.", this);
                }
            }
            else
            {
                Debug.LogWarning($"[MapManager] '{previousPrefab.name}' is missing a MapSection component. Falling back to general section prefabs.", this);
            }
        }

        if (poolToUse == null || poolToUse.Count == 0)
        {
            poolToUse = sectionPrefabs;
        }

        List<GameObject> validCandidates = new List<GameObject>();
        for (int i = 0; i < poolToUse.Count; i++)
        {
            GameObject prefab = poolToUse[i];
            if (prefab != null)
            {
                if (!usingCompatibleList && poolToUse == sectionPrefabs && prefab == previousPrefab && poolToUse.Count > 1)
                    continue;

                validCandidates.Add(prefab);
            }
        }

        if (validCandidates.Count == 0)
        {
            for (int i = 0; i < poolToUse.Count; i++)
            {
                if (poolToUse[i] != null)
                    validCandidates.Add(poolToUse[i]);
            }
        }

        if (validCandidates.Count == 0)
            return null;

        int randomIndex = rng.Next(validCandidates.Count);
        GameObject selectedPrefab = validCandidates[randomIndex];

        Debug.Log($"[MapManager] Successfully chose next section prefab: '{selectedPrefab.name}' (Source: {(usingCompatibleList ? "Compatible List" : "General/Fallback List")})", this);

        return selectedPrefab;
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