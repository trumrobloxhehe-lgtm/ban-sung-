# LAST LIGHT - Original Unity Battle Royale Prototype

A new Unity 6.6 third-person battle-royale prototype created for this repository. It is inspired by the battle-royale genre, but does not copy PUBG's maps, character models, branding, audio, UI artwork, or other proprietary assets.

## Included in this prototype

- Third-person movement, mouse-look camera, sprint and jump.
- Fictional game shooting, reload, ammo counts, health and eliminations.
- AI opponents that roam, pursue the player and shoot within range.
- Loot pickups for ammo, healing and fictional game weapon types.
- A closing safe zone that damages entities outside the boundary.
- An original pine-country map with village and warehouse layouts, roads, rocks, trees, loot and late-afternoon lighting.
- An in-game HUD for health, ammo, opponents remaining, eliminations and safe-zone status.
- Unity Editor utility to install a locally imported Humanoid FBX on the player and opponents.

This is a playable prototype foundation, not a finished production game. The generated environment and placeholder humans are simple geometry; detailed realistic characters and high-quality environment assets must be imported separately with permission to use them.

## Open it in Unity

1. GitHub: choose **Code -> Download ZIP**, extract the repository, or clone/pull with GitHub Desktop.
2. Unity Hub -> **Add project from disk** -> select the extracted repository folder (it contains Assets, Packages, and ProjectSettings).
3. Open using **Unity 6.6 (6000.6.6f1)**. Let Package Manager install Input System and wait for compilation.
4. On the Unity menu select **Battle Royale -> Build Last Light Demo**.
5. The scene is generated and saved as Assets/BattleRoyale/Scenes/LastLightDemo.unity.
6. Press Play.

## Controls

- **W/A/S/D** or arrow keys - move
- **Mouse** - aim and fire
- **Right mouse button** - aim more precisely
- **Shift** - sprint
- **Space** - jump
- **R** - reload
- **Esc** - unlock cursor; click game view to lock it again
- Collect floating loot to restore ammo, heal, or switch fictional game weapon types.

## Add realistic human characters

The code repository does not include third-party model files. For realistic people, import a rigged Humanoid model and animations that you have permission to use.

1. Visit Mixamo or use a licensed human asset you already own.
2. Download the character as FBX for Unity with its skin. For animation, download Idle, Walk and optionally Run clips.
3. Put the FBX files under the local project's Assets folder. Select the character FBX -> Inspector -> Rig -> set **Animation Type: Humanoid** -> Apply. Confirm the Avatar is valid.
4. Configure separate animation FBX files as Humanoid too.
5. Open **Battle Royale -> Install Humanoid Model**, select the human FBX/Prefab and optionally assign an Animator Controller. Or drag Idle and Walk clips to the clip fields to create a basic controller.
6. Choose whether to replace the player and opponents, then click **INSTALL MODEL ON CURRENT SCENE**.
7. Check height, foot alignment, facing and animations in Scene view. Save the scene after checking.

A model may remain in a T-pose if it has no compatible Animator Controller and clips. Adjust Yaw correction if the model faces backward. Imported assets remain local to your Unity project and are intentionally not committed here; only upload third-party asset files if their license allows redistribution.

## Not yet implemented

- Real multiplayer server and matchmaking.
- Mobile touch controls.
- A parachute/drop phase, inventory screen and vehicle system.
- Advanced pathfinding, cover behavior and polished animation blending.
- Final quality materials, detailed terrain and licensed character/weapon models.
- Performance and device QA.

The safe-zone and AI systems are prototypes and need testing and balance work. This project does not claim to reproduce PUBG's full gameplay or visual quality.
