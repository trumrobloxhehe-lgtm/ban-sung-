using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public sealed class BRCameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 focusOffset = new Vector3(0f, 1.35f, 0f);
    public float distance = 4.8f;
    public float sensitivity = .12f;
    public float minPitch = -25f;
    public float maxPitch = 65f;
    public float followSmooth = 18f;

    private float yaw;
    private float pitch = 12f;

    private void Start()
    {
        if (target == null)
        {
            BRPlayerController player = Object.FindFirstObjectByType<BRPlayerController>();
            if (player != null) target = player.transform;
        }
        if (target != null) yaw = target.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (target == null || (BRGameManager.Instance != null && BRGameManager.Instance.matchEnded)) return;
        Mouse mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, minPitch, maxPitch);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + focusOffset;
        Vector3 offset = rotation * (Vector3.back * distance);
        Vector3 desired = focus + offset;
        if (Physics.SphereCast(focus, .22f, offset.normalized, out RaycastHit hit, offset.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            desired = focus + offset.normalized * Mathf.Max(.35f, hit.distance - .1f);

        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = rotation;
    }
}
