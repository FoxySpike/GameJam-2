using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias de Escena")]
    [SerializeField] private SleeperNPC sleeperNPC;
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Cinemática y Cámara")]
    [Tooltip("Objeto o Transform hacia donde la cámara enfocará al perder (ej. la cara del NPC)")]
    [SerializeField] private Transform npcFocusPoint;
    [Tooltip("Tiempo en segundos antes de mostrar la UI de GameOver tras despertar el NPC")]
    [SerializeField] private float delayBeforeShowUI = 2.5f;

    [Header("Opcional: Desactivar Input Jugador")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    private bool isGameOver = false;

    private void OnEnable()
    {
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnMaxNoiseReached += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnMaxNoiseReached -= HandleGameOver;
        }
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("🚨 [GameOverManager] Secuencia de Game Over iniciada.");

        // 1. Desactivamos el control del jugador
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // 2. Despertamos al NPC
        if (sleeperNPC != null)
        {
            sleeperNPC.WakeUp();
        }

        // 3. Enfocamos la cámara (si tienes tu GrillCameraController u otro script de cámara, llámalo aquí)
        // Ejemplo genérico haciendo que la cámara mire al NPC:
        if (Camera.main != null && npcFocusPoint != null)
        {
            Camera.main.transform.LookAt(npcFocusPoint);
        }

        // 4. Iniciamos la espera para mostrar la interfaz
        StartCoroutine(ShowGameOverSequence());
    }

    private IEnumerator ShowGameOverSequence()
    {
        yield return new WaitForSeconds(delayBeforeShowUI);

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);

            // Liberamos el cursor para que el jugador pueda hacer clic en Reintentar
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Este método se asigna directamente al evento OnClick del botón "Reintentar" en la UI.
    /// </summary>
    public void RetryLevel()
    {
        // Reinicia la escena actual desde cero
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}