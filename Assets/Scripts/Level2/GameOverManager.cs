using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias de Cinemática")]
    [Tooltip("El NPC que se va a despertar")]
    [SerializeField] private SleeperNPC sleepingNPC;

    [Tooltip("La nueva cámara que enfoca al NPC despertando")]
    [SerializeField] private GameObject gameOverCamera;

    [Tooltip("El panel de UI de Game Over (donde estará el botón de Reintentar)")]
    [SerializeField] private GameObject gameOverUIPanel;

    [Header("Tiempos")]
    [Tooltip("Cuánto tiempo vemos la animación antes de que salga el panel de Game Over")]
    [SerializeField] private float delayBeforeUIPanel = 3f;

    private NoiseManager noiseManager;
    private PlayerInputReader blockedInput;
    private string levelSceneName;
    private bool isGameOver;

    private void Start()
    {
        levelSceneName = gameObject.scene.name;
        noiseManager = NoiseManager.Instance;

        // Asegurarnos de que las cosas de Game Over estén apagadas al inicio
        if (gameOverCamera != null) gameOverCamera.SetActive(false);
        if (gameOverUIPanel != null) gameOverUIPanel.SetActive(false);

        if (noiseManager != null)
            noiseManager.OnMaxNoiseReached += HandleGameOver;
        else
            Debug.LogError("[GameOverManager] No se encontró el NoiseManager.", this);
    }

    private void OnDestroy()
    {
        if (noiseManager != null)
            noiseManager.OnMaxNoiseReached -= HandleGameOver;

        ReleasePlayerInput();
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("🚨 [GameOverManager] Secuencia de Game Over iniciada.");

        // En lugar de hacer las cosas de golpe, iniciamos la "cinemática"
        StartCoroutine(PlayGameOverSequence());
    }

    private IEnumerator PlayGameOverSequence()
    {
        // 1. BLOQUEAR JUGADOR Y APAGAR SU UI
        BlockPlayerInput();
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetUIVisibility(false);
        }

        // 2. CAMBIAR A LA CÁMARA DE GAME OVER
        if (gameOverCamera != null) gameOverCamera.SetActive(true);

        // 3. DESPERTAR AL NPC
        if (sleepingNPC != null)
        {
            sleepingNPC.WakeUp();
        }
        else
        {
            Debug.LogWarning("[GameOverManager] Falta asignar el SleeperNPC en el inspector.");
        }

        // 4. ESPERAR A QUE TERMINE LA ANIMACIÓN
        yield return new WaitForSeconds(delayBeforeUIPanel);

        // 5. MOSTRAR EL PANEL DE GAME OVER Y LIBERAR EL CURSOR
        if (gameOverUIPanel != null)
            gameOverUIPanel.SetActive(true);

        // NUEVO: Liberamos el cursor de Unity para poder hacer clic
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void BlockPlayerInput()
    {
        if (PersistentPlayer.Instance != null)
            blockedInput = PersistentPlayer.Instance.InputReader;
        else
            blockedInput = FindFirstObjectByType<PlayerInputReader>();

        if (blockedInput != null)
            blockedInput.SetGameplayBlocked(this, true);
    }

    private void ReleasePlayerInput()
    {
        if (blockedInput == null) return;

        blockedInput.SetGameplayBlocked(this, false);
        blockedInput = null;
    }

    /// <summary>
    /// Este método se asigna directamente al evento OnClick del botón "Reintentar" en tu nuevo panel de UI.
    /// </summary>
    public void RetryLevel()
    {
        ReleasePlayerInput();

        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.ResetToDefaultState();
            // NUEVO: Le decimos al jugador persistente que esto es un reintento
            PersistentPlayer.Instance.isRetryingLevel = true;
        }

        SceneManager.LoadScene(levelSceneName);
    }
}