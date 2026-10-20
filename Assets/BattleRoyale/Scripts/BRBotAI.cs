using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BRHealth))]
public sealed class BRBotAI : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3.6f;
    public float detectionRange = 65f;
    public float fireRange = 42f;
    public float damagePerShot = 7f;
    public float secondsBetweenShots = 1.25f;
    private CharacterController controller;
    private float verticalVelocity;
    private float nextShot;
    private Vector3 wanderPoint;
    private float nextWander;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        PickWanderPoint();
    }

    private void Start()
    {
        if (target == null)
        {
            BRPlayerController player = Object.FindFirstObjectByType<BRPlayerController>();
            if (player != null) target = player.transform;
        }
    }

    private void Update()
    {
        if (BRGameManager.Instance != null && BRGameManager.Instance.matchEnded) return;
        if (target == null)
        {
            BRPlayerController player = Object.FindFirstObjectByType<BRPlayerController>();
            if (player != null) target = player.transform;
            else return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        Vector3 move = Vector3.zero;

        if (distance < detectionRange)
        {
            if (distance > 15f) move = toTarget.normalized;
            else if (distance < 8f) move = -toTarget.normalized;
            if (distance > 1.3f)
            {
                Quaternion facing = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, facing, 1f - Mathf.Exp(-7f * Time.deltaTime));
            }
            if (distance <= fireRange && Time.time >= nextShot) TryFireAtPlayer(distance);
        }
        else
        {
            if (Time.time >= nextWander || Vector3.Distance(transform.position, wanderPoint) < 2f) PickWanderPoint();
            Vector3 toWander = wanderPoint - transform.position; toWander.y = 0f;
            if (toWander.sqrMagnitude > 1f) move = toWander.normalized;
            if (move.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 1f - Mathf.Exp(-4f * Time.deltaTime));
        }

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity -= 22f * Time.deltaTime;
        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void TryFireAtPlayer(float distance)
    {
        nextShot = Time.time + secondsBetweenShots + Random.Range(-.2f, .35f);
        Vector3 origin = transform.position + Vector3.up * 1.3f;
        Vector3 destination = target.position + Vector3.up * 1.2f;
        Vector3 aim = (destination - origin).normalized;
        aim += new Vector3(Random.Range(-.025f, .025f), Random.Range(-.02f, .02f), Random.Range(-.025f, .025f));
        aim.Normalize();
        if (Physics.Raycast(origin, aim, out RaycastHit hit, Mathf.Min(fireRange, distance + 2f),
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            BRHealth hp = hit.collider.GetComponentInParent<BRHealth>();
            if (hp != null && hp.IsPlayer) hp.TakeDamage(damagePerShot, gameObject);
        }
    }

    private void PickWanderPoint()
    {
        wanderPoint = new Vector3(Random.Range(-95f, 95f), 0f, Random.Range(-95f, 95f));
        nextWander = Time.time + Random.Range(4f, 8f);
    }
}
