using System.Collections;
using UnityEngine;
using TMPro;

public class Level2IntroSequence : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject subtitleContainer;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private string newObjective = "Find a way to get a chicken";

    [Header("Referencias de Cámara/Tutorial")]
    [Tooltip("La cámara (o Virtual Camera) que apunta al empleado durmiendo")]
    [SerializeField] private GameObject npcTutorialCamera;
    [Tooltip("El texto que dice 'No hagas ruido'")]
    [SerializeField] private GameObject quietWarningUI;

    [Header("Referencias de Audio")]
    [SerializeField] private AudioClip introClip;

    [Header("Tiempos de Sincronización")]
    [SerializeField] private float initialDelay = 1.0f;
    [SerializeField] private float timeUntilSecondLine = 5f;
    // NUEVO: Variable para controlar el tiempo antes de pasar a la cámara
    [SerializeField] private float timeUntilCameraTransition = 3.0f;
    [SerializeField] private float tutorialCameraDuration = 5.0f;

    [Header("Textos")]
    [TextArea(2, 3)]
    [SerializeField] private string firstLine = "[excited] Alright... one beautiful, hot roasted chicken, please! [worried] I need to save my marriage.";
    [TextArea(2, 3)]
    [SerializeField] private string secondLine = "[confused, mumbling] Wait... where's my wallet? [frustrated] Oh, you've gotta be kidding me. [defeated, sighing] I drank the chicken money..";

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. BLOQUEAR JUGADOR Y APAGAR SU UI
        if (PersistentPlayer.Instance != null)
        {
            if (PersistentPlayer.Instance.InputReader != null)
                PersistentPlayer.Instance.InputReader.SetGameplayBlocked(this, true);

            PersistentPlayer.Instance.SetUIVisibility(false);
        }

        // Asegurarnos de que las cámaras y UI extra estén apagadas
        if (npcTutorialCamera != null) npcTutorialCamera.SetActive(false);
        if (quietWarningUI != null) quietWarningUI.SetActive(false);

        // 2. MOSTRAR SECUENCIA DE SUBTÍTULOS
        if (subtitleContainer != null) subtitleContainer.SetActive(true);
        subtitleText.text = "";

        yield return new WaitForSeconds(initialDelay);

        AudioSource playerAudio = PersistentPlayer.Instance.GetComponent<AudioSource>();
        if (playerAudio != null) playerAudio.PlayOneShot(introClip);

        subtitleText.text = firstLine;
        yield return new WaitForSeconds(timeUntilSecondLine);

        subtitleText.text = secondLine;

        // CORRECCIÓN: Usamos la variable en lugar del "número mágico" 4.0f
        yield return new WaitForSeconds(timeUntilCameraTransition);

        // 3. TUTORIAL: MOSTRAR AL NPC DURMIENDO
        // Apagamos los subtítulos
        if (subtitleContainer != null) subtitleContainer.SetActive(false);

        // Prendemos la cámara que apunta al NPC y la advertencia
        if (npcTutorialCamera != null) npcTutorialCamera.SetActive(true);
        if (quietWarningUI != null) quietWarningUI.SetActive(true);

        yield return new WaitForSeconds(tutorialCameraDuration);

        // Apagamos la cámara del NPC (volviendo a la del jugador) y la advertencia
        if (npcTutorialCamera != null) npcTutorialCamera.SetActive(false);
        if (quietWarningUI != null) quietWarningUI.SetActive(false);

        // 4. LIMPIAR CINEMÁTICA Y RESTAURAR AL JUGADOR
        if (PersistentPlayer.Instance != null)
        {
            if (PersistentPlayer.Instance.PlayerObjective != null)
                PersistentPlayer.Instance.PlayerObjective.SetObjective(newObjective);

            PersistentPlayer.Instance.SetUIVisibility(true);

            if (PersistentPlayer.Instance.InputReader != null)
                PersistentPlayer.Instance.InputReader.SetGameplayBlocked(this, false);
        }

        // NUEVO: Asegurarnos de que el jugador recupera el control del ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}