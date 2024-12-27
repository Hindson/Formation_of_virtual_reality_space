using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    readonly float mainSpeed = 20.0f;
    public float shiftSpeedMultiplier = 2.0f;
    readonly float camSens = 1f;
    public float maxSpeed = 50f;
    private float totalRun = 1.0f;

    void Update()
    {
        HandleMouseRotation();
        HandleMovement();
    }

    private void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * camSens;
        float mouseY = Input.GetAxis("Mouse Y") * camSens;

        transform.eulerAngles += new Vector3(-mouseY, mouseX, 0);
    }

    private void HandleMovement()
    {
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                totalRun += Time.deltaTime * shiftSpeedMultiplier;
                totalRun = Mathf.Clamp(totalRun, 1f, maxSpeed);
                p *= totalRun;
            }
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, maxSpeed);
                p *= mainSpeed;
            }

            p = mainSpeed * Time.deltaTime * p.normalized;

            transform.Translate(p);
        }
    }

    private Vector3 GetBaseInput()
    {
        Vector3 p_Velocity = new();

        if (Input.GetKey(KeyCode.W))
            p_Velocity += new Vector3(0, 0, 1);
        if (Input.GetKey(KeyCode.S))
            p_Velocity += new Vector3(0, 0, -1);

        if (Input.GetKey(KeyCode.A))
            p_Velocity += new Vector3(-1, 0, 0);
        if (Input.GetKey(KeyCode.D))
            p_Velocity += new Vector3(1, 0, 0);

        if (Input.GetKey(KeyCode.Q))
            p_Velocity += new Vector3(0, -1, 0);
        if (Input.GetKey(KeyCode.E))
            p_Velocity += new Vector3(0, 1, 0);

        return p_Velocity;
    }
}
