using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TrafficLane : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject vehicleTemplate;
    [SerializeField] private Vector2 speedRange = new Vector2(7f, 11f);
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(1.8f, 3.4f);
    [SerializeField, Min(1)] private int maxActiveVehicles = 4;
    [SerializeField, Min(0f)] private float initialDelay;

    private int activeVehicles;
    private Coroutine spawning;

    public void Configure(
        Transform start,
        Transform end,
        GameObject template,
        Vector2 speeds,
        Vector2 intervals,
        int maximumVehicles,
        float delay)
    {
        spawnPoint = start;
        exitPoint = end;
        vehicleTemplate = template;
        speedRange = speeds;
        spawnIntervalRange = intervals;
        maxActiveVehicles = Mathf.Max(1, maximumVehicles);
        initialDelay = Mathf.Max(0f, delay);
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            spawning = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawning != null) StopCoroutine(spawning);
        spawning = null;
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (enabled)
        {
            if (activeVehicles < maxActiveVehicles)
                SpawnVehicle();

            float minimum = Mathf.Max(0.25f, Mathf.Min(spawnIntervalRange.x, spawnIntervalRange.y));
            float maximum = Mathf.Max(minimum, Mathf.Max(spawnIntervalRange.x, spawnIntervalRange.y));
            yield return new WaitForSeconds(Random.Range(minimum, maximum));
        }
    }

    private void SpawnVehicle()
    {
        if (spawnPoint == null || exitPoint == null || vehicleTemplate == null) return;

        GameObject instance = Instantiate(vehicleTemplate, spawnPoint.position, Quaternion.identity, transform);
        instance.name = "Vehicle";
        instance.SetActive(true);

        TrafficVehicle vehicle = instance.GetComponent<TrafficVehicle>();
        if (vehicle == null)
        {
            Debug.LogError("TrafficLane requires a vehicle template with TrafficVehicle.", this);
            Destroy(instance);
            enabled = false;
            return;
        }

        float minimum = Mathf.Max(0.1f, Mathf.Min(speedRange.x, speedRange.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(speedRange.x, speedRange.y));
        activeVehicles++;
        vehicle.Initialize(this, exitPoint.position, Random.Range(minimum, maximum));
    }

    public void NotifyVehicleReleased()
    {
        activeVehicles = Mathf.Max(0, activeVehicles - 1);
    }
}
