using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BRHealth))]
public sealed class BRPlayerController : MonoBehaviour
{
    public float walkSpeed = 5.5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.25f;
    public float gravity = 22f;
    public float damage = 18f;
    public float fireRate = 5.5f;
    public float range = 180f;

    public int Magazine { get; private set; } = 24;
    public int ReserveAmmo { get; private set; } = 96;
    public string WeaponName { get; private set; } = "Ranger Rifle";
    public bool IsAiming => Mouse.current != null && Mouse.current.rightButton.isPressed;

    private CharacterController controller;
    private Camera gameCamera;
    private float verticalVelocity;
    private float nextShot;
    private bool reloading;
    private float reloadEnd;
    private bool cursorUnlocked;
    private int magazineCapacity = 24;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        gameCamera = Camera.main;
    }

    private void Update()
    {
        if (BRGameManager.Instance != null && BRGameManager.Instance.matchEnded) return;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            cursorUnlocked = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (mouse.leftButton.wasPressedThisFrame && cursorUnlocked)
        {
            cursorUnlocked = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        if (gameCamera == null) gameCamera = Camera.main;
        MoveCharacter(keyboard);
        if (keyboard.rKey.wasPressedThisFrame) BeginReload();
        if (reloading && Time.time >= reloadEnd) FinishReload();
        if (mouse.leftButton.isPressed && !cursorUnlocked) TryShoot();
    }

    private void MoveCharacter(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
        input = Vector2.ClampMagnitude(input, 1f);

        Transform cam = gameCamera != null ? gameCamera.transform : transform;
        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cam.right; right.y = 0f; right.Normalize();
        Vector3 move = (forward * input.y + right * input.x).normalized;
        bool sprint = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        Vector3 velocity = move * (sprint ? sprintSpeed : walkSpeed);
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame)
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        else verticalVelocity -= gravity * Time.deltaTime;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (move.sqrMagnitude > .01f)
        {
            Quaternion facing = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }
    }

    private void TryShoot()
    {
        if (Time.time < nextShot || reloading) return;
        if (Magazine <= 0) { BeginReload(); return; }
        if (gameCamera == null) return;

        nextShot = Time.time + 1f / Mathf.Max(.1f, fireRate);
        Magazine--;
        Vector3 direction = gameCamera.transform.forward;
        float spread = IsAiming ? .0015f : .006f;
        direction += gameCamera.transform.right * Random.Range(-spread, spread);
        direction += gameCamera.transform.up * Random.Range(-spread, spread);
        RaycastHit[] hits = Physics.RaycastAll(gameCamera.transform.position, direction.normalized, range,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root == transform.root) continue;
            BRHealth targetHealth = hit.collider.GetComponentInParent<BRHealth>();
            if (targetHealth != null && !targetHealth.IsPlayer)
                targetHealth.TakeDamage(damage, gameObject);
            break;
        }
        if (Magazine <= 4) BRGameManager.Instance?.ShowMessage("Low ammo — press R to reload.", .8f);
    }

    private void BeginReload()
    {
        if (reloading || Magazine >= magazineCapacity || ReserveAmmo <= 0) return;
        reloading = true;
        reloadEnd = Time.time + 1.65f;
        BRGameManager.Instance?.ShowMessage("Reloading...", 1.7f);
    }

    private void FinishReload()
    {
        int loaded = Mathf.Min(magazineCapacity - Magazine, ReserveAmmo);
        Magazine += loaded;
        ReserveAmmo -= loaded;
        reloading = false;
    }

    public void ApplyPickup(BRPickup.PickupKind kind)
    {
        switch (kind)
        {
            case BRPickup.PickupKind.Ammo:
                ReserveAmmo += 36;
                BRGameManager.Instance?.ShowMessage("+36 ammo");
                break;
            case BRPickup.PickupKind.Medkit:
                GetComponent<BRHealth>().Heal(35f);
                BRGameManager.Instance?.ShowMessage("Health restored");
                break;
            case BRPickup.PickupKind.Rifle:
                WeaponName = "Ranger Rifle"; damage = 22f; fireRate = 7.5f;
                magazineCapacity = 30; Magazine = Mathf.Max(Magazine, 30); ReserveAmmo += 45;
                BRGameManager.Instance?.ShowMessage("Ranger Rifle equipped");
                break;
            case BRPickup.PickupKind.Precision:
                WeaponName = "Longshot"; damage = 48f; fireRate = 1.25f;
                magazineCapacity = 8; Magazine = 8; ReserveAmmo += 16;
                BRGameManager.Instance?.ShowMessage("Longshot equipped");
                break;
        }
    }
}
