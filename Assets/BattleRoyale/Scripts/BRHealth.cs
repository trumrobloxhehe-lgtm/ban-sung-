using UnityEngine;

public sealed class BRHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool isPlayer;
    private float currentHealth;
    private bool dead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => dead;
    public bool IsPlayer => isPlayer;

    private void Awake() { currentHealth = maxHealth; }

    public void Configure(bool playerEntity, float health = 100f)
    {
        isPlayer = playerEntity;
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        dead = false;
    }

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (dead || damage <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (isPlayer) BRGameManager.Instance?.ShowMessage("Under fire!", .7f);
        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (!dead) currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0f, amount));
    }

    private void Die()
    {
        dead = true;
        BRBotAI bot = GetComponent<BRBotAI>();
        if (bot != null) bot.enabled = false;
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            if (collider != null && !collider.isTrigger) collider.enabled = false;

        BRGameManager manager = BRGameManager.Instance;
        if (manager != null)
        {
            if (isPlayer) manager.NotifyPlayerEliminated();
            else manager.NotifyBotEliminated(this);
        }
        if (!isPlayer) Destroy(gameObject, 2.5f);
    }
}
