using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlatformRider : MonoBehaviour
{
    CharacterController cc;
    MovingPlatform currentPlatform;
    bool touchedPlatformThisStep;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y < 0.5f) return;

        MovingPlatform mp = hit.collider.GetComponentInParent<MovingPlatform>();
        if (mp == null) return;

        currentPlatform = mp;
        touchedPlatformThisStep = true;
    }

    void LateUpdate()
    {
        if (touchedPlatformThisStep && currentPlatform != null)
        {
            cc.enabled = false;
            transform.position += currentPlatform.LastDelta;
            cc.enabled = true;
        }

        touchedPlatformThisStep = false;
        currentPlatform = null;
    }
}