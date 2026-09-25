using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    [TextArea(2, 4)]
    public string[] lines; // Aquí escribes los cuadros de texto en el Inspector

    public string menuSceneName = "Menu"; // nombre exacto de tu escena de menú

    private int currentLine = 0;

    void Start()
    {
        ShowLine();
    }

    void Update()
    {
        // Cambia esto por el input que prefieras (click, tecla, etc.)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    void ShowLine()
    {
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
        SceneManager.LoadScene(menuSceneName);
    }
}