#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static class BRDemoBuilder
{
    private const string Root = "Assets/BattleRoyale";
    private const string ScenePath = Root + "/Scenes/LastLightDemo.unity";

    [MenuItem("Battle Royale/Build Last Light Demo")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EnsureFolder(Root);
        EnsureFolder(Root + "/Scenes");
        EnsureFolder(Root + "/Materials");
        Random.InitState(20261020);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material ground = Mat("Ground_PineField", new Color(.22f, .32f, .22f), .1f);
        Material road = Mat("Road_Asphalt", new Color(.16f, .18f, .19f), .18f);
        Material concrete = Mat("Concrete", new Color(.42f, .44f, .43f), .25f);
        Material roof = Mat("Roof_DarkMetal", new Color(.12f, .15f, .17f), .55f);
        Material wood = Mat("Weathered_Wood", new Color(.34f, .24f, .16f), .2f);
        Material glass = Mat("Window_Glass", new Color(.10f, .28f, .34f), .8f);
        Material treeTrunk = Mat("Tree_Bark", new Color(.24f, .15f, .09f), .1f);
        Material leaves = Mat("PineNeedles", new Color(.08f, .23f, .13f), .1f);
        Material pickupGold = Mat("Loot_Glow", new Color(.94f, .62f, .13f), .6f);
        Material playerCloth = Mat("Player_FieldGear", new Color(.13f, .25f, .20f), .25f);
        Material botCloth = Mat("Opponent_Gear", new Color(.31f, .24f, .18f), .2f);
        Material skin = Mat("Human_Skin", new Color(.68f, .48f, .35f), .2f);
        Material hair = Mat("Human_Hair", new Color(.12f, .09f, .07f), .1f);
        Material dark = Mat("Gear_Dark", new Color(.055f, .065f, .06f), .3f);
        Material blue = Mat("Gear_BlueAccent", new Color(.08f, .30f, .45f), .45f);
        Material red = Mat("Loot_Medkit_Red", new Color(.85f, .12f, .13f), .25f);
        Material roadLine = Mat("Road_Marking", new Color(.82f, .80f, .69f), .2f);
        Material shoulder = Mat("Road_Gravel_Shoulder", new Color(.25f, .27f, .25f), .05f);
        Material shrubGreen = Mat("Shrub_Green", new Color(.14f, .25f, .12f), .05f);
        Material grassPatch = Mat("Grass_Patch", new Color(.30f, .36f, .22f), .05f);
        Material stoneAccent = Mat("Stone_Accent", new Color(.29f, .31f, .29f), .08f);
        Material water = Mat("Reservoir_Water", new Color(.08f, .22f, .27f), .78f);

        GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrain.name = "Last Light - Terrain";
        terrain.transform.localScale = new Vector3(24f, 1f, 24f);
        terrain.GetComponent<Renderer>().sharedMaterial = ground;
        // Raised gravel shoulders and painted lane dashes make the central roads read more clearly.
        CreateRoad(new Vector3(0f, .008f, 0f), new Vector3(240f, .025f, 10f), shoulder);
        CreateRoad(new Vector3(0f, .009f, 0f), new Vector3(10f, .025f, 240f), shoulder);
        CreateRoad(new Vector3(0f, .025f, 0f), new Vector3(240f, .03f, 7f), road);
        CreateRoad(new Vector3(0f, .028f, 0f), new Vector3(7f, .03f, 240f), road);
        for (int mark = -108; mark <= 108; mark += 14)
        {
            if (Mathf.Abs(mark) < 8) continue;
            CreateRoad(new Vector3(0f, .052f, mark), new Vector3(.18f, .018f, 4.5f), roadLine);
            CreateRoad(new Vector3(mark, .052f, 0f), new Vector3(4.5f, .018f, .18f), roadLine);
        }

        // Rounded, overlapping hills create a softer natural horizon instead of a wall of box-shaped blocks.
        Material[] ridgeMaterials = {
            Mat("Ridge_Olive", new Color(.20f,.25f,.22f), .05f),
            Mat("Ridge_Stone", new Color(.35f,.36f,.32f), .05f),
            Mat("Ridge_Pine", new Color(.13f,.20f,.17f), .05f)
        };
        for (int i = 0; i < 42; i++)
        {
            float angle = i * Mathf.PI * 2f / 42f;
            float radius = Random.Range(119f, 138f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Random.Range(-8f, -3f), Mathf.Sin(angle) * radius);
            GameObject ridge = CreatePrimitive("Distant Rolling Hill", PrimitiveType.Sphere, pos,
                new Vector3(Random.Range(14f, 28f), Random.Range(20f, 34f), Random.Range(13f, 25f)),
                ridgeMaterials[Random.Range(0, ridgeMaterials.Length)]);
            ridge.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-3f, 3f));
        }

        CreateTown(new Vector3(-53f, 0f, -45f), concrete, roof, wood, glass, blue);
        CreateTown(new Vector3(54f, 0f, -35f), concrete, roof, wood, glass, blue);
        CreateTown(new Vector3(-42f, 0f, 51f), concrete, roof, wood, glass, blue);
        CreateTown(new Vector3(55f, 0f, 57f), concrete, roof, wood, glass, blue);
        CreateWarehouse(new Vector3(0f, 0f, 52f), concrete, roof, glass);

        for (int i = 0; i < 160; i++)
        {
            Vector3 p = new Vector3(Random.Range(-112f, 112f), 0f, Random.Range(-112f, 112f));
            if (NearAnyTown(p, 22f) || Mathf.Abs(p.x) < 8f || Mathf.Abs(p.z) < 8f) continue;
            CreateTree(p, treeTrunk, leaves);
        }
        for (int i = 0; i < 45; i++)
        {
            Vector3 p = new Vector3(Random.Range(-105f, 105f), .25f, Random.Range(-105f, 105f));
            if (NearAnyTown(p, 13f)) continue;
            GameObject rock = CreatePrimitive("Field Rock", PrimitiveType.Sphere, p,
                new Vector3(Random.Range(.5f, 1.6f), Random.Range(.35f, .9f), Random.Range(.5f, 1.5f)), stoneAccent);
            rock.transform.rotation = Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f));
        }

        // Add smaller foliage clusters, roadside lights, village yard clutter and a small reservoir.
        for (int i = 0; i < 88; i++)
        {
            Vector3 p = new Vector3(Random.Range(-108f, 108f), .12f, Random.Range(-108f, 108f));
            if (NearAnyTown(p, 18f) || Mathf.Abs(p.x) < 11f || Mathf.Abs(p.z) < 11f) continue;
            CreateShrub(p, shrubGreen);
        }
        for (int z = -100; z <= 100; z += 40)
        {
            if (Mathf.Abs(z) < 20) continue;
            CreateStreetLight(new Vector3(7f, 0f, z), dark, roadLine);
            CreateStreetLight(new Vector3(-7f, 0f, z), dark, roadLine);
        }
        for (int i = 0; i < 20; i++)
        {
            Vector3 p = new Vector3(Random.Range(-10f, 10f), .2f, Random.Range(45f, 65f));
            CreatePrimitive("Warehouse Supply Crate", PrimitiveType.Cube, p,
                new Vector3(Random.Range(.7f, 1.3f), Random.Range(.5f, .9f), Random.Range(.7f, 1.2f)), wood)
                .transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
        CreateReservoir(new Vector3(80f, .025f, -56f), water, stoneAccent);

        GameObject player = CreateCharacter("Player", new Vector3(-4f, .05f, -5f), playerCloth, skin, hair, dark, blue);
        player.AddComponent<BRPlayerController>();
        player.AddComponent<BRHealth>().Configure(true, 100f);

        GameObject cameraRoot = new GameObject("Battle Royale Camera");
        Camera cam = cameraRoot.AddComponent<Camera>();
        cam.nearClipPlane = .08f;
        cam.farClipPlane = 420f;
        cam.fieldOfView = 72f;
        cameraRoot.tag = "MainCamera";
        cameraRoot.AddComponent<AudioListener>();
        cameraRoot.AddComponent<BRCameraController>().target = player.transform;

        GameObject managerRoot = new GameObject("Battle Royale Game Manager");
        BRGameManager manager = managerRoot.AddComponent<BRGameManager>();
        manager.player = player.GetComponent<BRPlayerController>();

        GameObject zoneRoot = new GameObject("Safe Zone Boundary");
        LineRenderer ring = zoneRoot.AddComponent<LineRenderer>();
        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null) ring.material = new Material(lineShader);
        BRSafeZone zone = zoneRoot.AddComponent<BRSafeZone>();
        zone.startingRadius = 108f;
        zone.minimumRadius = 10f;
        zone.waitBeforeFirstShrink = 35f;
        zone.secondsPerPhase = 32f;
        manager.safeZone = zone;

        int botCount = 12;
        for (int i = 0; i < botCount; i++)
        {
            float angle = Mathf.PI * 2f * i / botCount;
            float radius = Random.Range(48f, 88f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, .05f, Mathf.Sin(angle) * radius);
            GameObject bot = CreateCharacter("Opponent_" + (i + 1).ToString("00"), pos, botCloth, skin, hair, dark, blue);
            bot.AddComponent<BRHealth>().Configure(false, 100f);
            BRBotAI ai = bot.AddComponent<BRBotAI>();
            ai.target = player.transform;
            ai.moveSpeed = Random.Range(2.7f, 3.7f);
            ai.secondsBetweenShots = Random.Range(1.1f, 1.6f);
        }

        for (int i = 0; i < 58; i++)
        {
            Vector3 p = new Vector3(Random.Range(-104f, 104f), .55f, Random.Range(-104f, 104f));
            if (NearAnyTown(p, 13f) || Mathf.Abs(p.x) < 7f || Mathf.Abs(p.z) < 7f) continue;
            float roll = Random.value;
            BRPickup.PickupKind kind = roll < .43f ? BRPickup.PickupKind.Ammo :
                roll < .67f ? BRPickup.PickupKind.Medkit :
                roll < .90f ? BRPickup.PickupKind.Rifle : BRPickup.PickupKind.Precision;
            CreatePickup(p, kind, pickupGold, blue, red);
        }

        CreateLighting();
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.60f, .67f, .63f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = .0014f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.52f, .56f, .52f);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Last Light", "Original battle royale demo generated.\\n\\nWASD Move • Shift Sprint • Space Jump\\nMouse Fire • R Reload • Esc Unlock Cursor\\nFind loot, eliminate opponents and survive the shrinking safe zone.\\n\\nFor realistic humans, import a licensed Humanoid FBX and use Battle Royale > Install Humanoid Model.", "OK");
        Debug.Log("Last Light battle royale demo created at " + ScenePath);
    }

    private static bool NearAnyTown(Vector3 p, float radius)
    {
        Vector2 v = new Vector2(p.x, p.z);
        Vector2[] centers = { new Vector2(-53f,-45f), new Vector2(54f,-35f), new Vector2(-42f,51f), new Vector2(55f,57f), new Vector2(0f,52f) };
        foreach (Vector2 c in centers) if (Vector2.Distance(v, c) < radius) return true;
        return false;
    }

    private static void CreateTown(Vector3 center, Material wall, Material roof, Material wood, Material glass, Material trim)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3((i % 3 - 1) * 13f + Random.Range(-2f, 2f), 0f, (i / 3 - .5f) * 15f + Random.Range(-2f, 2f));
            CreateHouse(center + offset, wall, roof, wood, glass, trim, Random.Range(.9f, 1.25f));
        }
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = center + new Vector3(-20f + i * 5.5f, .5f, 14f);
            CreatePrimitive("Yard Fence", PrimitiveType.Cube, p, new Vector3(4.8f, 1f, .22f), wood);
        }
    }

    private static void CreateHouse(Vector3 position, Material wall, Material roof, Material wood, Material glass, Material trim, float scale)
    {
        float width = 8f * scale, height = 4f * scale, depth = 7f * scale, thick = .28f * scale;
        // Four wall sides with a real doorway opening, so characters can enter the simple buildings.
        CreatePrimitive("House Back Wall", PrimitiveType.Cube, position + new Vector3(0f, height * .5f, -depth * .5f),
            new Vector3(width, height, thick), wall);
        CreatePrimitive("House Left Wall", PrimitiveType.Cube, position + new Vector3(-width * .5f, height * .5f, 0f),
            new Vector3(thick, height, depth), wall);
        CreatePrimitive("House Right Wall", PrimitiveType.Cube, position + new Vector3(width * .5f, height * .5f, 0f),
            new Vector3(thick, height, depth), wall);
        float frontZ = depth * .5f;
        CreatePrimitive("Front Left Wall", PrimitiveType.Cube, position + new Vector3(-width * .31f, height * .5f, frontZ),
            new Vector3(width * .38f, height, thick), wall);
        CreatePrimitive("Front Right Wall", PrimitiveType.Cube, position + new Vector3(width * .31f, height * .5f, frontZ),
            new Vector3(width * .38f, height, thick), wall);
        CreatePrimitive("Door Lintel", PrimitiveType.Cube, position + new Vector3(0f, height * .82f, frontZ),
            new Vector3(width * .25f, height * .36f, thick), wall);
        CreatePrimitive("Metal Roof", PrimitiveType.Cube, position + Vector3.up * (height + .18f * scale),
            new Vector3(width + .7f * scale, .28f * scale, depth + .7f * scale), roof);
        GameObject door = CreatePrimitive("Front Door", PrimitiveType.Cube, position + new Vector3(0f, 1f * scale, frontZ + .12f),
            new Vector3(1.05f * scale, 2f * scale, .12f), wood);
        RemoveColliders(door); // Visual door panel; the opening remains passable.
        GameObject windowLeft = CreatePrimitive("Window Left", PrimitiveType.Cube, position + new Vector3(-width * .29f, height * .62f, frontZ + .18f),
            new Vector3(1.2f * scale, .85f * scale, .12f), glass);
        RemoveColliders(windowLeft);
        GameObject windowRight = CreatePrimitive("Window Right", PrimitiveType.Cube, position + new Vector3(width * .29f, height * .62f, frontZ + .18f),
            new Vector3(1.2f * scale, .85f * scale, .12f), glass);
        RemoveColliders(windowRight);
        GameObject foundation = CreatePrimitive("House Foundation", PrimitiveType.Cube, position + Vector3.up * .12f,
            new Vector3(width + .2f, .24f, depth + .2f), trim);
        RemoveColliders(foundation);
        GameObject porch = CreatePrimitive("Front Porch Step", PrimitiveType.Cube,
            position + new Vector3(0f, .10f, frontZ + .48f), new Vector3(2.25f * scale, .20f, .95f * scale), wall);
        RemoveColliders(porch);
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject shutter = CreatePrimitive("Window Shutter", PrimitiveType.Cube,
                position + new Vector3(side * width * .29f + side * .85f * scale, height * .62f, frontZ + .17f),
                new Vector3(.28f * scale, 1.02f * scale, .13f), wood);
            RemoveColliders(shutter);
        }
        GameObject chimney = CreatePrimitive("Roof Vent", PrimitiveType.Cube,
            position + new Vector3(width * .30f, height + .62f * scale, -depth * .20f),
            new Vector3(.45f * scale, .9f * scale, .45f * scale), trim);
        RemoveColliders(chimney);
    }

    private static void CreateWarehouse(Vector3 center, Material wall, Material roof, Material glass)
    {
        CreatePrimitive("Warehouse Left Wall", PrimitiveType.Cube, center + new Vector3(-12f, 3.5f, 0f), new Vector3(.45f, 7f, 15f), wall);
        CreatePrimitive("Warehouse Right Wall", PrimitiveType.Cube, center + new Vector3(12f, 3.5f, 0f), new Vector3(.45f, 7f, 15f), wall);
        CreatePrimitive("Warehouse Back Wall", PrimitiveType.Cube, center + new Vector3(0f, 3.5f, -7.25f), new Vector3(24f, 7f, .45f), wall);
        CreatePrimitive("Warehouse Front Beam Left", PrimitiveType.Cube, center + new Vector3(-9f, 3.5f, 7.25f), new Vector3(6f, 7f, .45f), wall);
        CreatePrimitive("Warehouse Front Beam Right", PrimitiveType.Cube, center + new Vector3(9f, 3.5f, 7.25f), new Vector3(6f, 7f, .45f), wall);
        CreatePrimitive("Warehouse Roof", PrimitiveType.Cube, center + new Vector3(0f, 7.2f, 0f), new Vector3(25f, .35f, 16f), roof);
        CreatePrimitive("Warehouse Door Frame", PrimitiveType.Cube, center + new Vector3(0f, 2.2f, 7.38f), new Vector3(5f, 4.4f, .12f), glass);
    }

    private static void CreateTree(Vector3 pos, Material trunk, Material leaves)
    {
        float h = Random.Range(3.5f, 6.5f);
        CreatePrimitive("Pine Trunk", PrimitiveType.Cylinder, pos + Vector3.up * h * .35f,
            new Vector3(.35f, h * .38f, .35f), trunk);
        for (int k = 0; k < 3; k++)
        {
            float layerH = h * (.30f + k * .20f);
            float layerSize = h * (.60f - k * .14f);
            GameObject canopy = CreatePineCone("Pine Canopy", pos + Vector3.up * layerH,
                new Vector3(layerSize, h * .48f, layerSize), leaves);
            RemoveColliders(canopy); // Leaves should not block player movement; trunks still have colliders.
        }
    }

    private static GameObject CreatePineCone(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        go.transform.localScale = scale;
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = GetPineConeMesh();
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return go;
    }

    private static Mesh pineConeMesh;
    private static Mesh GetPineConeMesh()
    {
        if (pineConeMesh != null) return pineConeMesh;
        const int sides = 9;
        Vector3[] vertices = new Vector3[sides + 2];
        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2f / sides;
            vertices[i] = new Vector3(Mathf.Cos(angle) * .5f, 0f, Mathf.Sin(angle) * .5f);
        }
        vertices[sides] = new Vector3(0f, 1f, 0f);
        vertices[sides + 1] = Vector3.zero;
        int[] triangles = new int[sides * 6];
        int t = 0;
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            triangles[t++] = i;
            triangles[t++] = sides;
            triangles[t++] = next;
            triangles[t++] = sides + 1;
            triangles[t++] = next;
            triangles[t++] = i;
        }
        pineConeMesh = new Mesh { name = "Shared Low Poly Pine Cone" };
        pineConeMesh.vertices = vertices;
        pineConeMesh.triangles = triangles;
        pineConeMesh.RecalculateNormals();
        pineConeMesh.RecalculateBounds();
        return pineConeMesh;
    }

    private static void CreateShrub(Vector3 p, Material material)
    {
        GameObject root = new GameObject("Field Shrub");
        root.transform.position = p;
        for (int i = 0; i < 3; i++)
        {
            GameObject leaf = CreatePrimitive("Shrub Cluster", PrimitiveType.Sphere,
                p + new Vector3(Random.Range(-.35f, .35f), Random.Range(.05f, .25f), Random.Range(-.35f, .35f)),
                new Vector3(Random.Range(.45f, .8f), Random.Range(.3f, .55f), Random.Range(.45f, .8f)), material);
            leaf.transform.SetParent(root.transform, true);
            RemoveColliders(leaf);
        }
    }

    private static void CreateStreetLight(Vector3 p, Material metal, Material lamp)
    {
        CreatePrimitive("Street Light Pole", PrimitiveType.Cylinder, p + Vector3.up * 2.8f,
            new Vector3(.09f, 2.8f, .09f), metal);
        CreatePrimitive("Street Light Arm", PrimitiveType.Cube, p + new Vector3(.45f, 5.4f, 0f),
            new Vector3(1f, .09f, .09f), metal);
        GameObject head = CreatePrimitive("Street Light Head", PrimitiveType.Cube, p + new Vector3(.9f, 5.32f, 0f),
            new Vector3(.42f, .14f, .26f), lamp);
        RemoveColliders(head);
    }

    private static void CreateReservoir(Vector3 p, Material water, Material bank)
    {
        GameObject pond = CreatePrimitive("Small Reservoir", PrimitiveType.Sphere, p,
            new Vector3(26f, .08f, 18f), water);
        RemoveColliders(pond);
        for (int i = 0; i < 24; i++)
        {
            float a = i * Mathf.PI * 2f / 24f;
            Vector3 rockPos = p + new Vector3(Mathf.Cos(a) * Random.Range(12f, 15f), .16f, Mathf.Sin(a) * Random.Range(8f, 10f));
            GameObject rock = CreatePrimitive("Reservoir Shore Rock", PrimitiveType.Sphere, rockPos,
                new Vector3(Random.Range(.6f, 1.4f), Random.Range(.35f, .65f), Random.Range(.6f, 1.2f)), bank);
            rock.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
        }
    }

    private static GameObject CreateCharacter(string name, Vector3 position, Material clothing, Material skin, Material hair, Material gear, Material trim)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 1.82f;
        controller.radius = .32f;
        controller.center = new Vector3(0f, .91f, 0f);
        controller.stepOffset = .28f;

        Transform visual = new GameObject("CharacterVisual").transform;
        visual.SetParent(root.transform, false);

        // Layered gear and clearer human proportions: still a placeholder, but no longer bare capsules.
        MakePart(visual, "Neck", PrimitiveType.Cylinder, new Vector3(0f, 1.39f, 0f), new Vector3(.12f, .10f, .12f), skin);
        MakePart(visual, "Torso", PrimitiveType.Capsule, new Vector3(0f, 1.04f, 0f), new Vector3(.43f, .43f, .27f), clothing);
        MakePart(visual, "Body Armor Vest", PrimitiveType.Cube, new Vector3(0f, 1.08f, .16f), new Vector3(.38f, .39f, .16f), gear);
        MakePart(visual, "Vest Chest Panel", PrimitiveType.Cube, new Vector3(0f, 1.18f, .245f), new Vector3(.25f, .13f, .035f), trim);
        MakePart(visual, "Left Vest Pouch", PrimitiveType.Cube, new Vector3(-.12f, .93f, .255f), new Vector3(.09f, .12f, .05f), blue);
        MakePart(visual, "Right Vest Pouch", PrimitiveType.Cube, new Vector3(.12f, .93f, .255f), new Vector3(.09f, .12f, .05f), blue);
        MakePart(visual, "Belt", PrimitiveType.Cube, new Vector3(0f, .76f, .015f), new Vector3(.42f, .10f, .29f), dark);
        MakePart(visual, "Pelvis", PrimitiveType.Cube, new Vector3(0f, .65f, 0f), new Vector3(.40f, .22f, .27f), gear);

        MakePart(visual, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.63f, .005f),
            new Vector3(.31f, .35f, .30f), skin);
        MakePart(visual, "Hair Cap", PrimitiveType.Sphere, new Vector3(0f, 1.80f, -.015f),
            new Vector3(.33f, .16f, .32f), hair);
        MakePart(visual, "Left Ear", PrimitiveType.Sphere, new Vector3(-.16f, 1.63f, 0f),
            new Vector3(.055f, .09f, .06f), skin);
        MakePart(visual, "Right Ear", PrimitiveType.Sphere, new Vector3(.16f, 1.63f, 0f),
            new Vector3(.055f, .09f, .06f), skin);
        MakePart(visual, "Left Eye", PrimitiveType.Sphere, new Vector3(-.075f, 1.67f, .145f),
            new Vector3(.035f, .035f, .018f), dark);
        MakePart(visual, "Right Eye", PrimitiveType.Sphere, new Vector3(.075f, 1.67f, .145f),
            new Vector3(.035f, .035f, .018f), dark);
        MakePart(visual, "Nose", PrimitiveType.Sphere, new Vector3(0f, 1.61f, .165f),
            new Vector3(.045f, .07f, .045f), skin);

        Transform leftArm = MakePart(visual, "Left Arm", PrimitiveType.Capsule,
            new Vector3(-.34f, 1.05f, .015f), new Vector3(.135f, .37f, .135f), clothing);
        leftArm.localRotation = Quaternion.Euler(0f, 0f, -10f);
        Transform rightArm = MakePart(visual, "Right Arm", PrimitiveType.Capsule,
            new Vector3(.34f, 1.05f, .015f), new Vector3(.135f, .37f, .135f), clothing);
        rightArm.localRotation = Quaternion.Euler(0f, 0f, 10f);
        MakePart(visual, "Left Glove", PrimitiveType.Sphere, new Vector3(-.39f, .72f, .05f),
            new Vector3(.12f, .10f, .12f), dark);
        MakePart(visual, "Right Glove", PrimitiveType.Sphere, new Vector3(.39f, .72f, .05f),
            new Vector3(.12f, .10f, .12f), dark);
        Transform leftLeg = MakePart(visual, "Left Leg", PrimitiveType.Capsule,
            new Vector3(-.14f, .35f, 0f), new Vector3(.16f, .36f, .17f), gear);
        leftLeg.localRotation = Quaternion.Euler(0f, 0f, -2f);
        Transform rightLeg = MakePart(visual, "Right Leg", PrimitiveType.Capsule,
            new Vector3(.14f, .35f, 0f), new Vector3(.16f, .36f, .17f), gear);
        rightLeg.localRotation = Quaternion.Euler(0f, 0f, 2f);
        MakePart(visual, "Left Knee Pad", PrimitiveType.Cube, new Vector3(-.14f, .31f, .15f),
            new Vector3(.17f, .13f, .08f), trim);
        MakePart(visual, "Right Knee Pad", PrimitiveType.Cube, new Vector3(.14f, .31f, .15f),
            new Vector3(.17f, .13f, .08f), trim);
        MakePart(visual, "Left Boot", PrimitiveType.Cube, new Vector3(-.14f, .105f, .07f),
            new Vector3(.18f, .16f, .32f), dark);
        MakePart(visual, "Right Boot", PrimitiveType.Cube, new Vector3(.14f, .105f, .07f),
            new Vector3(.18f, .16f, .32f), dark);
        MakePart(visual, "Backpack", PrimitiveType.Cube, new Vector3(0f, 1.04f, -.22f),
            new Vector3(.32f, .42f, .18f), trim);
        MakePart(visual, "Backpack Top Pocket", PrimitiveType.Cube, new Vector3(0f, 1.19f, -.325f),
            new Vector3(.22f, .14f, .04f), clothing);
        MakePart(visual, "Backpack Side Pocket L", PrimitiveType.Cube, new Vector3(-.18f, .99f, -.22f),
            new Vector3(.08f, .19f, .16f), dark);
        MakePart(visual, "Backpack Side Pocket R", PrimitiveType.Cube, new Vector3(.18f, .99f, -.22f),
            new Vector3(.08f, .19f, .16f), dark);
        return root;
    }

    private static Transform MakePart(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 scale, Material mat)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = mat;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return part.transform;
    }

    private static GameObject CreatePickup(Vector3 pos, BRPickup.PickupKind kind, Material gold, Material blue, Material red)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.name = "Loot_" + kind;
        pickup.transform.position = pos;
        pickup.transform.localScale = kind == BRPickup.PickupKind.Precision ? new Vector3(.8f,.28f,.28f) : new Vector3(.55f,.55f,.55f);
        pickup.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        pickup.GetComponent<Renderer>().sharedMaterial = kind == BRPickup.PickupKind.Medkit ? red : kind == BRPickup.PickupKind.Rifle ? blue : gold;
        BoxCollider box = pickup.GetComponent<BoxCollider>();
        box.isTrigger = true;
        Rigidbody body = pickup.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        pickup.AddComponent<BRPickup>().kind = kind;

        GameObject beacon = CreatePrimitive("Loot Beacon", PrimitiveType.Cylinder, pos + Vector3.down * .26f,
            new Vector3(.28f, .04f, .28f), gold);
        Collider bc = beacon.GetComponent<Collider>();
        if (bc != null) Object.DestroyImmediate(bc);
        return pickup;
    }

    private static void CreateRoad(Vector3 pos, Vector3 scale, Material mat)
    {
        CreatePrimitive("Road", PrimitiveType.Cube, pos, scale, mat);
    }

    private static void RemoveColliders(GameObject go)
    {
        foreach (Collider collider in go.GetComponentsInChildren<Collider>(true))
            if (collider != null) Object.DestroyImmediate(collider);
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && material != null) renderer.sharedMaterial = material;
        return go;
    }

    private static Material Mat(string name, Color color, float smoothness)
    {
        string path = Root + "/Materials/" + name.Replace(" ", "_") + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.name = name;
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", .03f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateLighting()
    {
        GameObject sunObject = new GameObject("Late Afternoon Sun");
        Light sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.color = new Color(1f, .86f, .70f);
        sun.shadows = LightShadows.Soft;
        sunObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

        GameObject fillObject = new GameObject("Cool Sky Fill");
        Light fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = .28f;
        fill.color = new Color(.52f, .68f, 1f);
        fill.shadows = LightShadows.None;
        fillObject.transform.rotation = Quaternion.Euler(25f, 145f, 0f);
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = Path.GetFileName(folder);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
