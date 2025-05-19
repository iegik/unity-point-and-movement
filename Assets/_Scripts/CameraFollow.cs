using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float smoothSpeed = 0.125f; // Smoothing speed for the camera movement
    public Vector3 offset; // Offset from the player's position
    public float swayAmount = 0.5f; // Amount of sway
    public float swaySpeed = 0.3f; // Speed of sway

    private Vector3 initialOffset;
    private Vector3 lastPlayerPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialOffset = offset;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player.position != lastPlayerPosition)
        {
            Vector3 desiredPosition = player.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
            lastPlayerPosition = player.position;
        }
    }
}
