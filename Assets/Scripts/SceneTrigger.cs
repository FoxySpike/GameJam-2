using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        Debug.Log("Jugador entrando al trigger");

        SceneLoader.Instance.LoadScene(sceneToLoad);
    }
}