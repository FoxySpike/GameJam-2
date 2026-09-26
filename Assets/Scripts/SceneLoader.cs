using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Panel de objetivo")]
    [SerializeField] private TMP_Text panelText;

    [Header("Textos por escena")]
    [TextArea] [SerializeField] private string level1Text = "GET DRUNK";
    [TextArea] [SerializeField] private string level2Text = "MAKE THE CHICKEN";
    [TextArea] [SerializeField] private string level3Text = "CROSS THE STREET";
    [TextArea] [SerializeField] private string constructionText = "GO THROUGH THE BUILDING WITH THE CHICKEN";
    [TextArea] [SerializeField] private string level4Text = "FIND THE CHICKEN AND GO OUTSIDE";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(SetObjectiveAfterSceneStarts(SceneManager.GetActiveScene()));
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetObjectiveAfterSceneStarts(scene));
    }

    private IEnumerator SetObjectiveAfterSceneStarts(Scene scene)
    {
        // Los controladores de cada nivel inicializan su UI en Start().
        // Esperamos un frame para que la instruccion de esta escena sea la final.
        yield return null;

        string objective = GetObjectiveForScene(scene.name);
        if (string.IsNullOrEmpty(objective))
            yield break;

        if (panelText != null)
            panelText.text = objective;
        else
            Debug.LogWarning("Asigna el TMP_Text del panel en el SceneLoader.", this);
    }

    private string GetObjectiveForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Nivel 1 - La parranda MVP":
                return level1Text;
            case "Nivel-2-Asadero":
                return level2Text;
            case "Nivel 3 - Cruzar la calle":
                return level3Text;
            case "Construccion":
                return constructionText;
            case "Nivel 4 - El parque":
                return level4Text;
            default:
                return string.Empty;
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("Cargando escena: " + sceneName);

        SceneManager.LoadScene(sceneName);
    }
}
