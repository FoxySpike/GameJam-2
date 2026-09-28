using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Derrota")]
    [TextArea]
    [SerializeField] private string defeatMessage = "¡DESPERTASTE AL EMPLEADO!\nReintentando nivel...";
    [SerializeField] private float delayBeforeRestart = 3f;

    private NoiseManager noiseManager;
    private PlayerInputReader blockedInput;
    private string levelSceneName;
    private bool isGameOver;

    private void Start()
    {
        levelSceneName = gameObject.scene.name;
        noiseManager = NoiseManager.Instance;

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

        BlockPlayerInput();
        ShowDefeatMessage();
        StartCoroutine(RestartAfterDelay());
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

    private void ShowDefeatMessage()
    {
        ObjectiveUI objectiveUI = null;

        if (PersistentPlayer.Instance != null)
            objectiveUI = PersistentPlayer.Instance.GetComponentInChildren<ObjectiveUI>(true);

        if (objectiveUI == null)
            objectiveUI = FindFirstObjectByType<ObjectiveUI>();

        if (objectiveUI != null)
            objectiveUI.SetObjective(defeatMessage);
        else
            Debug.LogWarning("[GameOverManager] No se encontró el ObjectiveUI.", this);
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeRestart);
        RetryLevel();
    }

    private void ReleasePlayerInput()
    {
        if (blockedInput == null) return;

        blockedInput.SetGameplayBlocked(this, false);
        blockedInput = null;
    }

    /// <summary>
    /// Este método se asigna directamente al evento OnClick del botón "Reintentar" en la UI.
    /// </summary>
    public void RetryLevel()
    {
        ReleasePlayerInput();
        SceneManager.LoadScene(levelSceneName);
    }
}
