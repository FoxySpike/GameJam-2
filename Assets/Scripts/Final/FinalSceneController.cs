using UnityEngine;
using System.Collections;

public class FinalSceneController : MonoBehaviour
{
    [Header("Cámara")]
    public Transform senoraLookTarget;

    [Header("Diálogo")]
    public GameObject dialogManagerObject;

    [Header("Config")]
    public string playerTag = "Player";
    public string cameraPivotChildName = "Camera Pivot";

    void Start()
    {
        StartCoroutine(WaitForPlayerAndTrigger());
    }

    private IEnumerator WaitForPlayerAndTrigger()
    {
        GameObject player = null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
            yield return null;
        }

        PlayerMovementV3 movementScript = player.GetComponent<PlayerMovementV3>();
        PlayerInteraction interactionScript = player.GetComponent<PlayerInteraction>();
        PlayerLookV2 cameraScript = player.GetComponent<PlayerLookV2>();

        if (movementScript != null) movementScript.enabled = false;
        if (interactionScript != null) interactionScript.enabled = false;
        if (cameraScript != null) cameraScript.enabled = false;

        Transform camaraTransform = player.transform.Find(cameraPivotChildName);

        if (camaraTransform != null && senoraLookTarget != null)
        {
            camaraTransform.LookAt(senoraLookTarget);
        }

        if (dialogManagerObject != null)
        {
            dialogManagerObject.SetActive(true);
        }
    }
}