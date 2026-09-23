using UnityEngine;

[DisallowMultipleComponent]
public sealed class DropChickenOnPlayerHit : MonoBehaviour
{
    [SerializeField] private PlayerHitReaction hitReaction;
    [SerializeField] private ChickenCarryController chicken;

    private void Awake()
    {
        if (chicken == null) chicken = GetComponent<ChickenCarryController>();
    }

    private void OnEnable()
    {
        if (hitReaction != null)
            hitReaction.OnHit += DropChicken;
    }

    private void OnDisable()
    {
        if (hitReaction != null)
            hitReaction.OnHit -= DropChicken;
    }

    private void DropChicken(Vector3 direction, float force)
    {
        if (chicken != null)
            chicken.Drop(direction, force);
    }
}
