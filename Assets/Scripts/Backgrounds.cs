using UnityEngine;

public class Backgrounds : MonoBehaviour
{
    public Transform cameraTransform;

    [Range(0f, 1f)]
    public float parallaxAmount = 0.1f;

    private Vector3 startingPosition;
    private Vector3 startingCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("Drag the Main Camera into Camera Transform.");
            enabled = false;
            return;
        }

        startingPosition = transform.position;
        startingCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 cameraMovement =
            cameraTransform.position - startingCameraPosition;

        transform.position = startingPosition + new Vector3(
            cameraMovement.x * parallaxAmount,
            cameraMovement.y * parallaxAmount,
            0f
        );
    }
}