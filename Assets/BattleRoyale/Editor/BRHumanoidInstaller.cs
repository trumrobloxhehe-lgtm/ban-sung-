#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BRHumanoidInstaller : EditorWindow
{
    private GameObject humanoidModel;
    private RuntimeAnimatorController animatorController;
    private AnimationClip idleClip;
    private AnimationClip walkClip;
    private AnimationClip runClip;
    private bool replacePlayer = true;
    private bool replaceBots = true;
    private float targetHeight = 1.82f;
    private float yawCorrection;
    private string status = "Import a licensed Humanoid FBX, then select it below.";

    [MenuItem("Battle Royale/Install Humanoid Model")]
    public static void Open()
    {
        BRHumanoidInstaller window = GetWindow<BRHumanoidInstaller>("Humanoid Installer");
        window.minSize = new Vector2(440f, 490f);
        window.FindModel(false);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Last Light — Human Model Installer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("The starter project does not include proprietary PUBG models. Import a human FBX you are allowed to use. " +
            "This tool replaces placeholder visuals while preserving movement and health components.", MessageType.Info);
        if (GUILayout.Button("Find imported Humanoid FBX")) FindModel(true);

        humanoidModel = (GameObject)EditorGUILayout.ObjectField("Human Model (FBX/Prefab)", humanoidModel, typeof(GameObject), false);
        animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller (optional)", animatorController, typeof(RuntimeAnimatorController), false);
        EditorGUILayout.LabelField("Or create a basic controller from Humanoid clips", EditorStyles.boldLabel);
        idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle clip", idleClip, typeof(AnimationClip), false);
        walkClip = (AnimationClip)EditorGUILayout.ObjectField("Walk clip", walkClip, typeof(AnimationClip), false);
        runClip = (AnimationClip)EditorGUILayout.ObjectField("Run clip (optional)", runClip, typeof(AnimationClip), false);
        replacePlayer = EditorGUILayout.Toggle("Replace player", replacePlayer);
        replaceBots = EditorGUILayout.Toggle("Replace opponents", replaceBots);
        targetHeight = EditorGUILayout.Slider("Target height (m)", targetHeight, 1.35f, 2.15f);
        yawCorrection = EditorGUILayout.Slider("Yaw correction", yawCorrection, -180f, 180f);

        EditorGUILayout.Space(10f);
        using (new EditorGUI.DisabledScope(humanoidModel == null))
        {
            if (GUILayout.Button("INSTALL MODEL ON CURRENT SCENE", GUILayout.Height(38f)))
                Install();
        }
        EditorGUILayout.HelpBox(status, MessageType.None);
    }

    private void FindModel(bool showStatus)
    {
        GameObject best = null;
        int bestScore = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null || importer.animationType != ModelImporterAnimationType.Human) continue;
            int score = 100;
            string lower = path.ToLowerInvariant();
            if (lower.Contains("mixamo")) score += 20;
            if (lower.Contains("character") || lower.Contains("human")) score += 10;
            if (lower.Contains("idle") || lower.Contains("walk") || lower.Contains("run")) score -= 40;
            if (score > bestScore) { bestScore = score; best = AssetDatabase.LoadAssetAtPath<GameObject>(path); }
        }
        if (humanoidModel == null) humanoidModel = best;
        if (showStatus) status = humanoidModel != null ? "Selected: " + AssetDatabase.GetAssetPath(humanoidModel) : "No Humanoid model found. Drag it from Project.";
        Repaint();
    }

    private void Install()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorUtility.DisplayDialog("Install human model", "Save or back up this scene first. Replace placeholder visuals now?", "Install", "Cancel") == false)
            return;
        RuntimeAnimatorController controllerToUse = animatorController;
        if (controllerToUse == null && (idleClip != null || walkClip != null || runClip != null))
        {
            if (idleClip == null || walkClip == null)
            {
                status = "Provide both Idle and Walk clips, or assign an existing Animator Controller.";
                return;
            }
            controllerToUse = CreateAnimatorController();
            if (controllerToUse == null) { status = "Could not create Animator Controller; check Console."; return; }
        }

        int count = 0;
        foreach (BRPlayerController player in FindObjectsByType<BRPlayerController>(FindObjectsSortMode.None))
            if (replacePlayer && InstallOnRoot(player.gameObject, controllerToUse)) count++;
        foreach (BRBotAI bot in FindObjectsByType<BRBotAI>(FindObjectsSortMode.None))
            if (replaceBots && InstallOnRoot(bot.gameObject, controllerToUse)) count++;

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        status = count > 0
            ? "Installed model on " + count + " character(s). Check Humanoid Avatar, clips/controller, feet, scale and facing. Save the scene."
            : "No Player/Opponent found. Run Battle Royale > Build Last Light Demo first.";
    }

    private bool InstallOnRoot(GameObject root, RuntimeAnimatorController controllerToUse)
    {
        Transform oldImported = root.transform.Find("ImportedHumanoidVisual");
        if (oldImported != null) Undo.DestroyObjectImmediate(oldImported.gameObject);
        Transform placeholder = root.transform.Find("CharacterVisual");
        if (placeholder != null)
        {
            Undo.RecordObject(placeholder.gameObject, "Hide placeholder visual");
            placeholder.gameObject.SetActive(false);
        }

        GameObject instance;
        try { instance = PrefabUtility.InstantiatePrefab(humanoidModel, root.scene) as GameObject; }
        catch { instance = null; }
        if (instance == null)
        {
            instance = Instantiate(humanoidModel);
            SceneManager.MoveGameObjectToScene(instance, root.scene);
        }
        if (instance == null) return false;

        Undo.RegisterCreatedObjectUndo(instance, "Install Humanoid Visual");
        instance.name = "ImportedHumanoidVisual";
        instance.transform.SetParent(root.transform, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0f, yawCorrection, 0f);
        instance.transform.localScale = Vector3.one;

        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;
        foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y > .01f)
            {
                instance.transform.localScale *= targetHeight / bounds.size.y;
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                instance.transform.position += new Vector3(root.transform.position.x - bounds.center.x,
                    root.transform.position.y - bounds.min.y, root.transform.position.z - bounds.center.z);
            }
        }

        Animator animator = instance.GetComponentInChildren<Animator>(true);
        if (animator != null)
        {
            if (controllerToUse != null) animator.runtimeAnimatorController = controllerToUse;
            animator.applyRootMotion = false;
        }
        BRHumanoidAnimatorDriver driver = root.GetComponent<BRHumanoidAnimatorDriver>();
        if (driver == null) driver = Undo.AddComponent<BRHumanoidAnimatorDriver>(root);
        if (animator != null) driver.targetAnimator = animator;
        return true;
    }

    private AnimatorController CreateAnimatorController()
    {
        const string folder = "Assets/BattleRoyale/Generated";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/BattleRoyale"))
                AssetDatabase.CreateFolder("Assets", "BattleRoyale");
            AssetDatabase.CreateFolder("Assets/BattleRoyale", "Generated");
        }

        string path = folder + "/LastLightHumanoid.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        if (controller == null) return null;
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        AnimatorState idle = machine.AddState("Idle");
        AnimatorState walk = machine.AddState("Walk");
        idle.motion = idleClip;
        walk.motion = walkClip;
        machine.defaultState = idle;
        AnimatorState run = null;
        if (runClip != null)
        {
            run = machine.AddState("Run");
            run.motion = runClip;
        }
        AddTransition(idle, walk, AnimatorConditionMode.Greater, .12f);
        AddTransition(walk, idle, AnimatorConditionMode.Less, .08f);
        if (run != null)
        {
            AddTransition(walk, run, AnimatorConditionMode.Greater, .82f);
            AddTransition(run, walk, AnimatorConditionMode.Less, .72f);
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold)
    {
        AnimatorStateTransition t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = .12f;
        t.AddCondition(mode, threshold, "Speed");
    }
}
#endif
