using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, 5f);
    public float moveTime = 3f;
    public float pauseTime = 1f;
    public float phase = 0f;

    Vector3 startPos;
    Vector3 lastPos;

    public Vector3 LastDelta { get; private set; }

    void Start()
    {
        startPos = transform.position;
        lastPos = startPos;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time + phase, moveTime + pauseTime);
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / moveTime));
        transform.position = startPos + offset * k;

        LastDelta = transform.position - lastPos;
        lastPos = transform.position;
    }
}