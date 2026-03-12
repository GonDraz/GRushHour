# AGENTS.md
## Project Snapshot
- Unity version: `6000.3.8f1` (see `ProjectSettings/ProjectVersion.txt`).
- Build scenes in `ProjectSettings/EditorBuildSettings.asset`: `Assets/Scenes/Splash.unity` -> `Assets/Scenes/Application.unity`.
- Main gameplay code: `Assets/Scripts`; reusable framework: `Assets/GonDraz`.
- Puzzle module: `Assets/Scripts/Puzzle/` — see `Assets/Scripts/Puzzle/README.md` for full module docs.
## Runtime Architecture (Read This First)
- Boot path: `Managers.Bootstrap` (`Assets/GonDraz/Managers/Bootstrap.cs`) runs in startup scene, sets target FPS, configures PrimeTween capacity, and async-loads the configured scene.
- Global bootstrap: `ApplicationManager` (`Assets/Scripts/Managers/ApplicationManager.cs`) is created with `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` and instantiates core managers (currently `GlobalStateMachine`).
- State orchestration: `GlobalStateMachine` is a partial class split per state file in `Assets/Scripts/GlobalState/` and inherits `BaseGlobalStateMachine<T>`. Current states: `PreLoaderState`, `InGameState`, `InGamePauseState`, `GameOver`, `GameWon`.
- UI routing is decoupled: states call `RouteManager.Go(...)` to show `Presentation` screens (`Assets/Scripts/UI/Screens/*`).
- Route registration happens from `Route` (`Assets/GonDraz/UI/Route/Route.cs`) by scanning child `Presentation` components once on Awake.
- Puzzle module: `PuzzleGameController` (`Assets/Scripts/Puzzle/Core/PuzzleGameController.cs`) is a `BaseBehaviour` placed in scene; it owns `PuzzleBoardState` and publishes events via `Managers.EventManager`.
## Core Data / Event Flows
- Startup transition: `PreLoaderState` waits for `BaseEventManager.ApplicationLoadFinished`; then immediately transitions to `InGameState` (no score check; there is no `MenuState`).
- Pause flow: `InGameState` listens to `BaseEventManager.GamePause` and `ApplicationPause`, then changes to `InGamePauseState`.
- Score flow: `ScoreManager.Score` is a `GObservableValue<int>`; OnChanged updates `HighScore` automatically; `HighScore` and `PreviousHighScore` persist via `GObservablePlayerPrefs`. **Note:** `ScoreManager.OnSetupGamePlay` (which resets `Score` to 0) is currently commented out in the static constructor — score does **not** auto-reset when `SetupGameplay` fires.
- Audio flow: `VolumeManager` loads/saves per-`BroAudioType` volume from prefs and applies to BroAudio on `ApplicationLoadFinished`.
- Tutorial flow: `TutorialManager` loads `TutorialSequenceSO` from `TutorialRegistry.Instance`, persists step index with key prefix `tutorial.progress.`, emits `StepChanged` / `TutorialEnded`, and resolves targets by `targetId`.
- Puzzle event flow: `PuzzleGameController` fires `Managers.EventManager` events (`SetupGameplay`, `PuzzleInitialized`, `PuzzleBlockMoved(string blockId, Vector2Int newOrigin)`, `PuzzleInvalidMove(string blockId)`, `PuzzleSolved`) and C# `Action` delegates (`BoardInitialized`, `BlockMoved`, `Solved`). `EventManager.SetupGameplay` triggers `PuzzleGameController.SetupLevel()`. `PuzzleGameController` also has `setupOnStart = true` (default) that auto-calls `SetupLevel()` in `Start()` — be aware of double-init if both `setupOnStart` and `SetupGameplay` fire.
- Puzzle win flow: `InGameState` subscribes to `EventManager.PuzzleSolved` and calls `Host.ChangeState<GameWon>()` directly — no score check.
## Project-Specific Conventions
- Keep global states as nested classes inside `GlobalStateMachine` partials; naming pattern: `GlobalStateMachine.<StateName>.cs`.
- In state classes, subscribe in `OnEnter` and unsubscribe in `OnExit` (see `GlobalStateMachine.InGameState.cs`).
- For `BaseBehaviour` derivatives, prefer event wiring via `Subscribe()` / `Unsubscribe()` and opt-in with `SubscribeUsingOnEnable()` and `UnsubscribeUsingOnDisable()`.
- UI screens derive from `Presentation`; use `Show/Hide` lifecycle instead of manual active toggles for routed screens.
- `TutorialTarget.targetId` must match `TutorialStepSO.targetId`; empty IDs auto-fill in editor from object name.
- From UI, trigger state changes via the public static `GlobalStateMachine.Change<T>()` (e.g., `GlobalStateMachine.Change<GlobalStateMachine.InGamePauseState>()`); use `Host.ChangeState<T>()` only inside state class bodies.
- Puzzle levels are `PuzzleLevelSO` assets (`Create > Puzzle > Puzzle Level`); store under `Assets/Scripts/Puzzle/Levels/`. Exactly one block must have `isTarget = true` and its `id` must match `targetBlockId`.
- `Managers.EventManager` (in `Assets/Scripts/Managers/EventManager.cs`) holds puzzle-specific `GEvent` instances; use it for puzzle cross-system signals, not `BaseEventManager`.
- `TutorialType` enum values: `GameplayIntro = 0`, `AchievementsIntro = 1`, `PowerUpIntro = 2` (defined in `TutorialManager.cs`).
- Resume without resetting the puzzle: `GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false)` alone (see `InGamePauseScreen.OnBackGameButtonClick`). Restart/next level: `Change<InGameState>(false)` + `EventManager.SetupGameplay.Invoke()` (see `GameOverScreen`, `GameWonScreen`, `InGamePauseScreen.OnRestartButtonClick`).
- `InGamePauseScreen.homeButton` is shown only when `TutorialManager.IsCompleted(TutorialType.GameplayIntro)` returns `true`.
## Editing Guidance For Agents
- If you add a new screen state, update both: a state partial in `Assets/Scripts/GlobalState/` and a `Presentation` in `Assets/Scripts/UI/Screens/`, then route via `RouteManager`.
- If you add tutorial content, create/update `TutorialSequenceSO` and `TutorialStepSO` assets and ensure the active scene has a `TutorialRegistry` with those assets assigned.
- Use `BaseEventManager`/`GEvent` for cross-system signaling rather than direct scene object references.
- Preserve existing bilingual comments (English + Vietnamese) where touched.
- If you add a puzzle level: create a `PuzzleLevelSO`, assign it to `PuzzleGameController.level`, and verify via Play Mode that the board initializes without errors in the Console before entering Play Mode. Use `Tools/Puzzle/Level Editor` to visually drag blocks and validate.
- If you add a new puzzle event, declare it in `Managers.EventManager` and subscribe/unsubscribe in the relevant `BaseBehaviour.Subscribe()` / `Unsubscribe()` pair.
- `PuzzleBoardPresenter` requires `exitIndicatorPrefab` (`PuzzleExitView` component) for exit gate visuals; `cellBackgroundPrefab` (`PuzzleImageView`) and `borderWallPrefab` (`PuzzleImageView`) are optional — the presenter creates plain `Image` rects at runtime if they are unassigned. Block sprites: assign `normalBlockSprite` / `targetBlockSprite` (supports 9-slice auto-detection); `normalBlockOverlaySprite` / `targetBlockOverlaySprite` + `overlayColor` drive an optional second Image layer on the block prefab. Key layout fields: `cellSize` (default 120×120 px), `cellSpacing` (default 8×8 px), `invertY` (default `true` — board row 0 is visual top). Grid display: `showGrid` / `checkerboard` / `cellColorA` / `cellColorB`. Border display: `showBorder` / `borderThickness` / `borderColor`. Exit gate display: `exitIndicatorColor` / `exitIndicatorThickness` / `exitIndicatorGap`. Movement arrows: `showMovementArrows` (default `true`) renders per-block directional arrows updated after every move; configure with `arrowSprite`, `arrowColor`, `arrowSize`, `arrowOffset`.
- `PuzzleBlockView` prefab: assign `overlayImage` (optional second Image for shine/border); for `Corner2x2MissingOne` shapes the presenter calls `SetupCellImages` — add an optional `cellImagesRoot` (`RectTransform`) child in the prefab (stretch-fill, pivot `(0,1)`) to parent the per-cell images, otherwise they are parented directly on the block root.
- To switch puzzle level at runtime without scene reload, call `PuzzleGameController.SetLevel(PuzzleLevelSO)` then `EventManager.SetupGameplay.Invoke()` (or call `SetupLevel()` directly).
## Build, Test, and Debug Workflows
- Open and run from Unity Editor `6000.3.8f1` (verified from project settings). Prefer validating behavior in Play Mode because no automated tests were discovered in `Assets`.
- Windows open command:
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -projectPath "C:\Users\vungo\OneDrive\Desktop\code\unity\GRushHour"
```
- Tutorial debug helpers: `Tools/Tutorial/*` from `Assets/Scripts/Tutorial/Editor/TutorialEditorTools.cs` and `Tools/Tutorial/Tools Window` from `Assets/Scripts/Tutorial/Editor/TutorialToolsWindow.cs`.
- Puzzle debug helpers: `Tools/Puzzle/Level Editor` (`Assets/Scripts/Puzzle/Editor/PuzzleLevelEditorWindow.cs`) — visual IMGUI board canvas with drag-to-move blocks, right-click context menu, quick-add presets, text preview, and validation report; compiled only when `ODIN_INSPECTOR` is defined (left panel uses Odin `PropertyTree`, right panel is pure IMGUI). `Tools/Puzzle/Create Sample Level` + `Tools/Puzzle/Validate Selected Level` logic both available via `PuzzleEditorTools.cs` and `PuzzleLevelValidationUtility.cs`. Custom inspector buttons on `PuzzleLevelSO` added by `PuzzleLevelSOEditor.cs`. **Note:** `PuzzleDebugInput` (keyboard runtime testing) is referenced in the puzzle README but **does not yet exist**.
- If behavior seems stuck, verify a `Route` root with child `Presentation`s and a live `TutorialRegistry` in scene.
## Key Dependencies / Integration Points
- Tweening/UI animation: PrimeTween (`Packages/manifest.json`, `Presentation`, `TutorialOverlay`, `TutorialHand`).
- Audio: BroAudio (`Ami.BroAudio`) via `VolumeManager` and screen SFX (for example `GameOverScreen`).
- Inspector tooling: Odin Inspector attributes used on `PuzzleLevelSO` and `Presentation`; `PuzzleLevelEditorWindow` and `PuzzleLevelSOEditor` compile only when `ODIN_INSPECTOR` scripting define is present — Odin's `PropertyTree` / `InspectorUtilities.DrawPropertiesInTree` drives the left panel of the level editor window.
- Input System package is enabled (`com.unity.inputsystem`), with actions asset at `Assets/InputSystem_Actions.inputactions`.
