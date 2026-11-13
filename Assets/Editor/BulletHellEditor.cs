using Mono.Cecil;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletHellEditor : EditorWindow
{
    private PreviewRenderUtility preview;
    private PatternEditor patternEditor;
    private PhaseEditor phaseEditor;

    private BHEInputManager inputManager;
    private BHECollisionManager collisionManager;

    public BossEnemy bossEnemy;
    public Player player;
    Vector3 playerStart;
     
    private readonly float cameraZoom = 60f;

    float bulletSpeed;

    bool playingPreview;
    bool phasePreview;
    bool isPaused;

    Button playButton, pauseButton, selectPatternButton, selectPhaseButton;

    Texture playButtonTexture, stopButtonTexture, pauseButtonTexture;
    Image playButtonIcon, pauseButtonIcon;

    int phasesAmount;
    int previewingPhase;
    float phaseWaitTimer;
    double lastTime;

    int currentAttackIndex;

    [MenuItem("Window/UI Toolkit/BulletHellEditor")]
    public static void ShowExample()
    {
        BulletHellEditor wnd = GetWindow<BulletHellEditor>();
        wnd.titleContent = new GUIContent("BulletHellEditor");
    }

    public void CreateGUI()
    {
        playButtonTexture = EditorGUIUtility.IconContent("PlayButton").image;
        stopButtonTexture = EditorGUIUtility.IconContent("StopButton").image;
        pauseButtonTexture = EditorGUIUtility.IconContent("PauseButton").image;

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        VisualElement label = new Label("Bullet Hell Editor Window");
        label.style.marginBottom = 10;
        root.Add(label);

        TwoPaneSplitView splitView = new TwoPaneSplitView(
           0,      // Index of the fixed pane
           300,    // Initial size of the fixed pane
           TwoPaneSplitViewOrientation.Horizontal
       );

        // Left panel
        VisualElement leftPanel = new VisualElement();
        leftPanel.style.paddingLeft = 10;
        leftPanel.style.paddingRight = 10;
        leftPanel.style.paddingTop = 10;
        leftPanel.style.minWidth = 300;
        leftPanel.style.maxWidth = 300;

        // Right panel
        VisualElement rightPanel = new VisualElement();
        rightPanel.style.paddingLeft = 10;
        rightPanel.style.paddingTop = 10;

        splitView.Add(leftPanel);
        splitView.Add(rightPanel);

        rootVisualElement.Add(splitView);

        patternEditor = new(this);
        leftPanel.Add(patternEditor);

        phaseEditor = new(this);
        leftPanel.Add(phaseEditor);

        VisualElement controlRow = new VisualElement();
        controlRow.style.flexDirection = FlexDirection.Row;
        rightPanel.Add(controlRow);

        //Play label
        Label playLabel = new Label("Play/Pause:");
        playLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        playLabel.style.marginTop = 5;
        playLabel.style.marginBottom = 5;
        controlRow.Add(playLabel);

        //Play Button
        playButton = new Button(() =>
        {
            TogglePlayMode();
        });
        playButtonIcon = new Image();
        playButtonIcon.image = playButtonTexture;
        playButton.Add(playButtonIcon);
        controlRow.Add(playButton);

        //Pause Button
        pauseButton = new Button(() =>
        {
            TogglePause(!isPaused);
        });
        pauseButtonIcon = new Image();
        pauseButtonIcon.image = pauseButtonTexture;
        pauseButton.Add(pauseButtonIcon);
        controlRow.Add(pauseButton);

        //Mode label
        Label modeLabel = new Label("Mode:");
        modeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        modeLabel.style.marginTop = 5;
        modeLabel.style.marginBottom = 5;
        controlRow.Add(modeLabel);

        selectPatternButton = new Button(() =>
        {
            SwitchPreviewMode(false);
        })
        {
            text = "Pattern"
        };
        controlRow.Add(selectPatternButton);

        selectPhaseButton = new Button(() =>
        {
            SwitchPreviewMode(true);
        })
        {
            text = "Phase"
        };
        controlRow.Add(selectPhaseButton);

        // Preview box
        VisualElement previewBox = new VisualElement();
        previewBox.style.marginTop = 10; 
        previewBox.style.borderTopWidth = 1;
        previewBox.style.borderBottomWidth = 1;
        previewBox.style.borderLeftWidth = 1;
        previewBox.style.borderRightWidth = 1;

        previewBox.style.flexGrow = 1;

        previewBox.style.minHeight = 200;
        previewBox.style.maxHeight = 1000;

        //previewBox.style.height = 400;
        previewBox.style.backgroundColor = new Color(1, 0, 0, 0.2f);

        rightPanel.Add(previewBox);

        //Add input handler
        inputManager = new(root);

        // IMGUI preview
        IMGUIContainer previewGUI = new IMGUIContainer(PreviewGUIDraw);

        previewBox.Add(previewGUI);

        // Keep repainting so the preview can animate.
        previewGUI.schedule.Execute(() =>
        {
            PreviewUpdate();
            previewGUI.MarkDirtyRepaint();
        }).Every(16);

        bossEnemy.UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true, AddBulletsToPreview);
        SwitchPreviewMode(false);

        isPaused = false;
    }

    public void AddBulletsToPreview(List<Bullet> _bulletsToAdd)
    {
        foreach (Bullet _bullet in _bulletsToAdd)
        {
            preview.AddSingleGO(_bullet.gameObject);
        }
    }

    private void PreviewUpdate()
    {
        double currentTime = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(currentTime - lastTime);

        if(bossEnemy)
        {
            if(!isPaused && bossEnemy.shouldPauseTool)
            {
                TogglePause(true);
                
                if(phasePreview) phaseEditor.phaseRows[bossEnemy.hitAttackInPhaseIndex].style.backgroundColor = Color.red;
                    
                bossEnemy.shouldPauseTool = false;
                bossEnemy.hitAttackInPhaseIndex = -1;

                Debug.Log($"PAUSE FROM BOSS HIT");
            }
        }

        if (playingPreview && !isPaused)
        {
            bossEnemy.PreviewUpdate(deltaTime, AddBulletsToPreview);
            player.PreviewUpdate(deltaTime);
            player.playerMovement.PreviewUpdate(deltaTime, inputManager.keysPressed);

            UpdateRowHighlights();
        }

        lastTime = currentTime;
    }

    void SwitchPreviewMode(bool _newModeIsPhase)
    {
        if(_newModeIsPhase)
        {
            selectPhaseButton.enabledSelf = false;
            selectPatternButton.enabledSelf = true;

            selectPatternButton.style.backgroundColor = Color.grey;
            selectPhaseButton.style.backgroundColor = Color.blue;

            phasePreview = true;
            phaseEditor.enabledSelf = true;
            patternEditor.enabledSelf = false;
            bossEnemy.DestroyAllBullets(true);
        }
        else
        {
            selectPatternButton.enabledSelf = false;
            selectPhaseButton.enabledSelf = true;

            selectPatternButton.style.backgroundColor = Color.blue;
            selectPhaseButton.style.backgroundColor = Color.grey;

            phasePreview = false;
            phaseEditor.enabledSelf = false;
            patternEditor.enabledSelf = true;
            bossEnemy.UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true, AddBulletsToPreview);
        }
    }

    public void UpdateBulletCountFromPatternEditor(ChangeEvent<int> evt)
    {
        if(!phasePreview) bossEnemy.UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true, AddBulletsToPreview);
    }

    public void UpdateBulletCountFromPatternEditor(ChangeEvent<float> evt)
    {
        if (!phasePreview) bossEnemy.UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true, AddBulletsToPreview);
    }

    private void CreatePreview()
    {
        preview = new PreviewRenderUtility();

        // Camera
        preview.camera.orthographic = true;
        preview.camera.orthographicSize = 10;
        preview.camera.transform.position = new Vector3(0, 0, -10);

        // Lighting
        preview.lights[0].intensity = 1.4f;
        preview.lights[0].transform.rotation =
            Quaternion.Euler(30f, 30f, 0f);

        preview.lights[1].intensity = 1.0f;

        //Add room
        GameObject backgroundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BossRoom.prefab");
        GameObject backgroundGO = PrefabUtility.InstantiatePrefab(backgroundPrefab) as GameObject;

        preview.AddSingleGO(backgroundGO);

        //Add boss
        GameObject bossEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BossEnemy.prefab");
        GameObject bossEnemyGO = PrefabUtility.InstantiatePrefab(bossEnemyPrefab) as GameObject;

        preview.AddSingleGO(bossEnemyGO);

        //Set boss to in tool
        collisionManager = new();
        bossEnemy = bossEnemyGO.GetComponent<BossEnemy>();
        bossEnemy.InitializeBoss(collisionManager);

        Debug.Log($"Create Preview");


        //Add player
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        GameObject playerGO = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;

        player = playerGO.GetComponent<Player>();
        player.InitializePlayer(collisionManager, true);
        playerStart = player.transform.position;

        preview.AddSingleGO(playerGO);

        lastTime = EditorApplication.timeSinceStartup;
    }

    private void PreviewGUIDraw()
    {
        if (preview == null)
            return;


        Rect rect = GUILayoutUtility.GetRect(
            600,
            1000
        );

        // Camera looks directly at the sphere's center
        preview.camera.transform.position = new Vector3(0, 0, -10);
        preview.camera.transform.LookAt(Vector3.zero);

        preview.BeginPreview(rect, GUIStyle.none);

        preview.camera.clearFlags = CameraClearFlags.Color;
        preview.camera.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

        preview.camera.Render();

        Texture result = preview.EndPreview();

        GUI.DrawTexture(
            rect,
            result,
            ScaleMode.StretchToFill
        );

        Handles.BeginGUI();

        if (bossEnemy.bullets != null)
        {
            foreach(Bullet bullet in bossEnemy.bullets)
            {
                if(bullet) DrawBulletDirection(rect, bullet.transform.position, patternEditor.bulletAngleField.value, 100, bullet.transform.rotation, bullet);
            }

        }
        Handles.EndGUI();
    }

    private void TogglePlayMode()
    {
        phasesAmount = phaseEditor.phaseRows.Count;
        previewingPhase = 0;
        phaseWaitTimer = 0;
        playingPreview = !playingPreview;

        if(playingPreview)
        {
            playButton.style.backgroundColor = Color.blue;
            playButtonIcon.image = stopButtonTexture;
            patternEditor.enabledSelf = false;
            phaseEditor.enabledSelf = false;
            selectPatternButton.enabledSelf = false;
            selectPhaseButton.enabledSelf = false;

            if (phasePreview)
            {
                bossEnemy.StartPhase(phaseEditor.currentPhase);
            }
        }
        else
        {
            playButton.style.backgroundColor = Color.grey;
            playButtonIcon.image = playButtonTexture;

            if (phasePreview)
            {
                bossEnemy.DestroyAllBullets(true);
                selectPatternButton.enabledSelf = true;

                foreach(PhaseRow row in phaseEditor.phaseRows)
                {
                    row.HighlightRow(false);
                }
            }
            else
            {
                bossEnemy.UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true, AddBulletsToPreview);
                selectPhaseButton.enabledSelf = true;
            }

            TogglePause(false);
            patternEditor.enabledSelf = true;
            player.transform.position = playerStart;
        }
    }

    private void TogglePause(bool _pause)
    {
        if(isPaused && !_pause)
        {
            pauseButton.style.backgroundColor = Color.grey;
            pauseButtonIcon.image = pauseButtonTexture;
            isPaused = false;
        }
        else if (_pause)
        {
            pauseButton.style.backgroundColor = Color.blue;
            pauseButtonIcon.image = playButtonTexture;
            isPaused = true;
        }
    }

    private void DrawBulletDirection(Rect rect, Vector2 bulletPosition, float angle, float length, Quaternion bulletRotation, Bullet bullet)
    {
        Vector2 direction = bossEnemy.GetBulletDirection(angle, bulletRotation);

        Vector2 endPosition =
            bulletPosition + direction * length;

        float zoom = 100f;

        // Convert to GUI coordinates relative to the rect
        Vector2 start = new Vector2(
            rect.width / 2f + bulletPosition.x * zoom,
            rect.height / 2f - bulletPosition.y * zoom
        );

        Vector2 end = new Vector2(
            rect.width / 2f + endPosition.x * zoom,
            rect.height / 2f - endPosition.y * zoom
        );

        // Clip everything drawn inside this rectangle
        if(!phasePreview)
        {
            GUI.BeginClip(rect);

            Handles.DrawLine(start, end);

            GUI.EndClip();
        }
    }

    void UpdateRowHighlights()
    {
        if(currentAttackIndex != bossEnemy.attackIndex)
        {
            currentAttackIndex = bossEnemy.attackIndex;

            if ((phaseEditor.phaseRows.Count > currentAttackIndex - 1) && (currentAttackIndex - 1 >= 0))
            {
                phaseEditor.phaseRows[currentAttackIndex - 1].HighlightRow(false);
            }
            if (currentAttackIndex < phaseEditor.phaseRows.Count)
            {
                phaseEditor.phaseRows[currentAttackIndex].HighlightRow(true);
            }
        }
    }
    
    
    private void OnLostFocus()
    {
        Debug.Log("ON LOST FOCUS");
        if(inputManager != null) inputManager.ResetInputs();
    }

    private void OnEnable()
    {
        CreatePreview();
    }

    private void OnDisable()
    {
        if (preview != null)
        {
            preview.Cleanup();
            preview = null;
        }
    }
}
