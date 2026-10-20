using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class BRGameManager : MonoBehaviour
{
    public static BRGameManager Instance { get; private set; }
    public BRPlayerController player;
    public BRSafeZone safeZone;
    public int eliminatedBots;
    public int initialBotCount;
    public bool matchEnded;
    public bool playerWon;
    public string statusMessage = "Find gear and stay inside the safe zone.";
    private float messageUntil;
    private GUIStyle titleStyle, hudStyle, warningStyle, centerStyle, overlayStyle;

    public int AliveBots
    {
        get
        {
            int alive = 0;
            foreach (BRBotAI bot in Object.FindObjectsByType<BRBotAI>(FindObjectsSortMode.None))
            {
                BRHealth hp = bot != null ? bot.GetComponent<BRHealth>() : null;
                if (bot != null && bot.isActiveAndEnabled && hp != null && !hp.IsDead) alive++;
            }
            return alive;
        }
    }

    private void Awake() { Instance = this; }

    private void Start()
    {
        if (player == null) player = Object.FindFirstObjectByType<BRPlayerController>();
        if (safeZone == null) safeZone = Object.FindFirstObjectByType<BRSafeZone>();
        initialBotCount = Object.FindObjectsByType<BRBotAI>(FindObjectsSortMode.None).Length;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (matchEnded && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RestartMatch();
    }

    public void ShowMessage(string message, float seconds = 2.5f)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        statusMessage = message;
        messageUntil = Time.time + seconds;
    }

    public void NotifyBotEliminated(BRHealth bot)
    {
        eliminatedBots++;
        if (matchEnded) return;
        int alive = AliveBots;
        ShowMessage("Opponent eliminated • " + alive + " remaining", 2.5f);
        if (alive <= 0) EndMatch(true);
    }

    public void NotifyPlayerEliminated()
    {
        if (!matchEnded) EndMatch(false);
    }

    private void EndMatch(bool won)
    {
        matchEnded = true;
        playerWon = won;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartMatch()
    {
        Time.timeScale = 1f;
        Scene active = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(active.name)) SceneManager.LoadScene(active.name);
    }

    private void EnsureStyles()
    {
        if (hudStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;
        hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = 15;
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.normal.textColor = Color.white;
        warningStyle = new GUIStyle(hudStyle);
        warningStyle.fontSize = 18;
        warningStyle.alignment = TextAnchor.MiddleCenter;
        warningStyle.normal.textColor = new Color(1f, .65f, .28f);
        centerStyle = new GUIStyle(hudStyle);
        centerStyle.fontSize = 25;
        centerStyle.alignment = TextAnchor.MiddleCenter;
        overlayStyle = new GUIStyle(GUI.skin.label);
        overlayStyle.fontSize = 36;
        overlayStyle.fontStyle = FontStyle.Bold;
        overlayStyle.alignment = TextAnchor.MiddleCenter;
        overlayStyle.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        EnsureStyles();
        float w = Screen.width, h = Screen.height;
        GUI.color = new Color(.025f, .045f, .055f, .84f);
        GUI.Box(new Rect(16, 16, 275, 144), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(30, 22, 245, 32), "LAST LIGHT", titleStyle);

        BRHealth hp = player != null ? player.GetComponent<BRHealth>() : null;
        float health = hp != null ? hp.CurrentHealth : 100f;
        GUI.Label(new Rect(30, 58, 235, 23), "HEALTH   " + Mathf.CeilToInt(health) + " / 100", hudStyle);
        Rect back = new Rect(30, 84, 235, 12);
        GUI.color = new Color(.12f, .14f, .15f, 1f); GUI.DrawTexture(back, Texture2D.whiteTexture);
        GUI.color = health > 35f ? new Color(.22f, .78f, .42f) : new Color(.94f, .25f, .18f);
        GUI.DrawTexture(new Rect(back.x, back.y, back.width * Mathf.Clamp01(health / 100f), back.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(30, 105, 245, 26),
            (player != null ? player.WeaponName : "No weapon") + "   " +
            (player != null ? player.Magazine + " / " + player.ReserveAmmo : "--"), hudStyle);

        GUI.color = new Color(.025f, .045f, .055f, .84f);
        GUI.Box(new Rect(w - 250, 16, 234, 106), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(w - 234, 24, 205, 26), "MATCH STATUS", hudStyle);
        GUI.Label(new Rect(w - 234, 52, 205, 25), "OPPONENTS  " + AliveBots, hudStyle);
        GUI.Label(new Rect(w - 234, 78, 205, 25), "ELIMINATIONS  " + eliminatedBots, hudStyle);

        if (safeZone != null)
        {
            string z = safeZone.IsShrinking ? "ZONE IS CLOSING" : "ZONE SHRINKS IN " + Mathf.CeilToInt(safeZone.SecondsUntilNextShrink) + "s";
            GUI.Label(new Rect(w * .5f - 180, 16, 360, 32), z, warningStyle);
            GUI.Label(new Rect(w * .5f - 150, 46, 300, 25), "SAFE RADIUS  " + Mathf.CeilToInt(safeZone.CurrentRadius) + "m", hudStyle);
        }

        GUI.Label(new Rect(w * .5f - 10, h * .5f - 17, 20, 34), "+", centerStyle);
        GUI.color = new Color(.025f, .045f, .055f, .8f);
        GUI.Box(new Rect(16, h - 66, 620, 48), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(28, h - 61, 600, 35), "WASD Move   Shift Sprint   Space Jump   Mouse Fire   R Reload   Esc Cursor", hudStyle);

        if (Time.time < messageUntil && !matchEnded)
        {
            GUI.color = new Color(.025f, .035f, .045f, .78f);
            GUI.Box(new Rect(w * .5f - 260, h - 122, 520, 38), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(w * .5f - 250, h - 118, 500, 30), statusMessage, warningStyle);
        }

        if (matchEnded)
        {
            GUI.color = new Color(.015f, .02f, .03f, .87f);
            GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(w * .5f - 300, h * .5f - 85, 600, 70),
                playerWon ? "CHAMPION" : "MATCH OVER", overlayStyle);
            GUI.Label(new Rect(w * .5f - 270, h * .5f + 5, 540, 40),
                "Press R to play another match", warningStyle);
        }
        GUI.color = Color.white;
    }
}
