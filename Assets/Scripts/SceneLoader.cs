using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    public void LoadScene(string sceneName)
    {
        Debug.Log("Cargando escena: " + sceneName);

        SceneManager.LoadScene(sceneName);
    }
}