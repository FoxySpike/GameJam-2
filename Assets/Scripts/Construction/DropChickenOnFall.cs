using UnityEngine;

[DisallowMultipleComponent]
public sealed class DropChickenOnFall : MonoBehaviour
{
    [SerializeField] private FallImpactDetector fallDetector;
    [SerializeField] private ChickenCarryController chicken;

    private void Awake()
    {
        if (chicken == null) chicken = GetComponent<ChickenCarryController>();
    }

    private void OnEnable()
    {
        if (fallDetector != null)
            fallDetector.OnFallImpact += DropChicken;
    }

    private void OnDisable()
    {
        if (fallDetector != null)
            fallDetector.OnFallImpact -= DropChicken;
    }

    private void DropChicken(Vector3 direction, float force)
    {
        if (chicken != null)
            chicken.Drop(direction, force);
    }
}