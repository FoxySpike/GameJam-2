using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, 5f); // hasta dónde se mueve
    public float moveTime = 3f;   // segundos que tarda en ir de un extremo al otro
    public float pauseTime = 1f;  // pausa en cada extremo
    public float phase = 0f;      // desfase para variar entre excavadoras

    Vector3 startPos;

    void Start() => startPos = transform.position;

    void FixedUpdate()
    {
        // PingPong va de 0 a L y vuelve: los tramos donde t > moveTime son la pausa
        float t = Mathf.PingPong(Time.time + phase, moveTime + pauseTime);
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / moveTime));
        transform.position = startPos + offset * k;
    }

    // Para que el jugador se mueva con la plataforma
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player")) c.transform.SetParent(transform);
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Player")) c.transform.SetParent(null);
    }
}
