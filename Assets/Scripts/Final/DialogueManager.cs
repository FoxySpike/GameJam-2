using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    [TextArea(2, 4)]
    public string[] lines;

    public string menuSceneName = "Menu";

    private int currentLine = 0;

    void OnEnable()
    {
        currentLine = 0;
        ShowLine();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    void ShowLine()
    {
        if (lines == null || lines.Length == 0) return;
        dialogueText.text = lines[currentLine];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndGame();
        }
    }

    void EndGame()
    {
        // Returning to the first level begins a new session, including drink counters and vision.
        if (PersistentPlayer.Instance != null)
        {
            GameObject oldPlayer = PersistentPlayer.Instance.gameObject;
            PersistentPlayer.Instance = null;
            oldPlayer.SetActive(false);
            Destroy(oldPlayer);
        }
        SceneManager.LoadScene(menuSceneName);
    }
}
