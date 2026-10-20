using UnityEngine;

public sealed class BRPickup : MonoBehaviour
{
    public enum PickupKind { Ammo, Medkit, Rifle, Precision }
    public PickupKind kind = PickupKind.Ammo;
    public float bobHeight = .16f;
    public float bobSpeed = 2f;
    private Vector3 startPosition;
    private bool collected;

    private void Start() { startPosition = transform.position; }

    private void Update()
    {
        if (collected) return;
        Vector3 p = startPosition;
        p.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = p;
        transform.Rotate(Vector3.up, 35f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        BRPlayerController player = other.GetComponentInParent<BRPlayerController>();
        if (player == null) return;
        collected = true;
        player.ApplyPickup(kind);
        Destroy(gameObject);
    }
}
