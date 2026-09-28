using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FallImpactDetector))]
public sealed class DropChickenOnFall : MonoBehaviour
{
    [SerializeField] private ChickenCarryController chicken;

    private FallImpactDetector fallDetector;

    private void Awake()
    {
        fallDetector = GetComponent<FallImpactDetector>();
    }

    private void OnEnable()
    {
        fallDetector.OnFallImpact += DropChicken;

        if (chicken == null)
            StartCoroutine(FindChickenWhenReady());
    }

    private void OnDisable()
    {
        fallDetector.OnFallImpact -= DropChicken;
    }

    private IEnumerator FindChickenWhenReady()
    {
        while (chicken == null)
        {
            chicken = FindAnyObjectByType<ChickenCarryController>();
            yield return null;
        }
    }

    private void DropChicken(Vector3 direction, float force)
    {
        if (chicken != null)
            chicken.Drop(direction, force);
    }
}