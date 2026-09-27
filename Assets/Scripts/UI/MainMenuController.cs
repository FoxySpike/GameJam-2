using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Connects the controls already saved in the main menu scene.</summary>
public class MainMenuController : MonoBehaviour
{
    private const string FirstLevel = "Nivel 1 - La parranda MVP";

    private void Awake()
    {
        EnsureEventSystem();

        var canvas = transform.Find("Main Menu Canvas");
        if (canvas == null)
        {
            Debug.LogError("Main Menu Canvas is missing from the Menu Inicial scene.", this);
            return;
        }

        var optionsPanel = canvas.Find("Background/Options panel")?.gameObject;
        foreach (var button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "JUGAR button")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Play);
            }
            else if (button.name == "OPCIONES button" && optionsPanel != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => optionsPanel.SetActive(true));
            }
            else if (button.name == "VOLVER button" && optionsPanel != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => optionsPanel.SetActive(false));
            }
        }

        var slider = canvas.GetComponentInChildren<Slider>(true);
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(value => AudioListener.volume = value);
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        var input = eventSystem.AddComponent<InputSystemUIInputModule>();
        input.AssignDefaultActions();
    }

    private static void Play()
    {
        SceneManager.LoadScene(FirstLevel);
    }
}
