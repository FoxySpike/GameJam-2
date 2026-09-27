using UnityEngine;

public class RotarObjeto : MonoBehaviour
{
    public Vector3 velocidadRotacion = new Vector3(0, 100f, 0); // grados por segundo

    void Update()
    {
        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }
}
