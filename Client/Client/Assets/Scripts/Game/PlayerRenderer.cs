using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    public string id;

    private Vector3 lastPos;
    private Vector3 targetPos;
    private float interpTime = 0f;

    public float interpolationTime = 0.2f;
    public float rotationSpeed = 10f;

    private Quaternion targetRotation;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        lastPos = transform.position;
        targetPos = transform.position;
        targetRotation = transform.rotation;
    }

    void FixedUpdate()   // <-- Physics update instead of Update()
    {
        interpTime += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(interpTime / interpolationTime);

        Vector3 newPos = Vector3.Lerp(lastPos, targetPos, t);
        rb.MovePosition(newPos);   // <-- Physics safe movement

        rb.MoveRotation(
            Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime)
        );
    }

    public void SetTargetPosition(float x, float y)
    {
        lastPos = transform.position;
        targetPos = new Vector3(x, y, 0f);
        interpTime = 0f;

        Vector3 dir = targetPos - lastPos;

        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            targetRotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
