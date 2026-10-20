using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class BRSafeZone : MonoBehaviour
{
    public Vector3 center = Vector3.zero;
    public float startingRadius = 108f;
    public float minimumRadius = 10f;
    public float waitBeforeFirstShrink = 35f;
    public float secondsPerPhase = 32f;
    public float damagePerSecond = 5f;

    public float CurrentRadius { get; private set; }
    public float SecondsUntilNextShrink { get; private set; }
    public bool IsShrinking { get; private set; }
    public int Phase { get; private set; }

    private float phaseTimer;
    private float damageTimer;
    private float phaseStartRadius;
    private float phaseTargetRadius;
    private LineRenderer ring;
    private const int SegmentCount = 128;

    private void Awake()
    {
        ring = GetComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = SegmentCount;
        ring.startWidth = .28f;
        ring.endWidth = .28f;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) ring.material = new Material(shader);
        ring.startColor = new Color(.15f, .78f, 1f, .9f);
        ring.endColor = ring.startColor;
        ring.numCapVertices = 2;

        CurrentRadius = startingRadius;
        phaseStartRadius = startingRadius;
        SetNextTarget();
        UpdateRing();
    }

    private void Update()
    {
        phaseTimer += Time.deltaTime;
        SecondsUntilNextShrink = Mathf.Max(0f, waitBeforeFirstShrink - phaseTimer);

        if (Phase == 0 && phaseTimer >= waitBeforeFirstShrink)
        {
            Phase = 1; phaseTimer = 0f; phaseStartRadius = CurrentRadius; SetNextTarget();
        }
        else if (Phase > 0 && CurrentRadius > minimumRadius + .01f)
        {
            IsShrinking = phaseTimer < secondsPerPhase;
            CurrentRadius = Mathf.Lerp(phaseStartRadius, phaseTargetRadius, Mathf.Clamp01(phaseTimer / secondsPerPhase));
            SecondsUntilNextShrink = Mathf.Max(0f, secondsPerPhase - phaseTimer);
            if (phaseTimer >= secondsPerPhase)
            {
                CurrentRadius = phaseTargetRadius;
                phaseTimer = 0f;
                phaseStartRadius = CurrentRadius;
                if (CurrentRadius > minimumRadius + .1f) Phase++;
                SetNextTarget();
            }
        }

        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0f) { damageTimer = 1f; DamageOutsideZone(); }
        UpdateRing();
    }

    private void SetNextTarget()
    {
        phaseTargetRadius = Mathf.Max(minimumRadius, CurrentRadius * .72f);
        IsShrinking = false;
    }

    private void DamageOutsideZone()
    {
        foreach (BRHealth hp in Object.FindObjectsByType<BRHealth>(FindObjectsSortMode.None))
        {
            if (hp == null || hp.IsDead) continue;
            Vector3 p = hp.transform.position;
            float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(center.x, center.z));
            if (d > CurrentRadius) hp.TakeDamage(damagePerSecond, gameObject);
        }
    }

    private void UpdateRing()
    {
        if (ring == null) return;
        for (int i = 0; i < SegmentCount; i++)
        {
            float a = i * Mathf.PI * 2f / SegmentCount;
            ring.SetPosition(i, new Vector3(center.x + Mathf.Cos(a) * CurrentRadius, .16f,
                center.z + Mathf.Sin(a) * CurrentRadius));
        }
    }
}
