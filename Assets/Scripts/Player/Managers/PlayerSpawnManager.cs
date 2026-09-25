using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");

        if(spawn == null)
        {
            Debug.LogWarning("No existe SpawnPoint en la escena");
            return;
        }


        CharacterController controller =
            GetComponent<CharacterController>();

        if(controller != null)
            controller.enabled = false;


        transform.position = spawn.transform.position;
        transform.rotation = spawn.transform.rotation;


        if(controller != null)
            controller.enabled = true;
    }
}