using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject noiseBarContainer;
    [SerializeField] private Image noiseBarFill;

    private NoiseManager connectedManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ConnectToNoiseManager();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        DisconnectFromNoiseManager();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConnectToNoiseManager();
    }

    private void ConnectToNoiseManager()
    {
        DisconnectFromNoiseManager();

        connectedManager = NoiseManager.Instance;
        bool noiseIsAvailable = connectedManager != null;

        if (noiseBarContainer != null)
            noiseBarContainer.SetActive(noiseIsAvailable);

        if (!noiseIsAvailable)
            return;

        connectedManager.OnNoiseChanged += UpdateBar;
        UpdateBar(connectedManager.CurrentNoise, connectedManager.MaxNoise);
    }

    private void DisconnectFromNoiseManager()
    {
        if (connectedManager != null)
            connectedManager.OnNoiseChanged -= UpdateBar;

        connectedManager = null;
    }

    private void UpdateBar(float current, float max)
    {
        if (noiseBarFill != null && max > 0f)
            noiseBarFill.fillAmount = current / max;
    }
}