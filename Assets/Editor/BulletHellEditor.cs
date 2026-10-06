using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletHellEditor : EditorWindow
{
    private PreviewRenderUtility preview;
    private PatternEditor patternEditor;
    private PhaseEditor phaseEditor;

    private BossEnemy bossEnemy;
     
    private readonly float cameraZoom = 60f;

    List<Bullet> bullets;

    float bulletSpeed;

    bool playingPreview;
    bool phasePreview;

    Button playButton, pauseButton, selectPatternButton, selectPhaseButton;

    Texture playButtonTexture, stopButtonTexture, pauseButtonTexture;
    Image playButtonIcon;

    int phasesAmount;
    int previewingPhase;
    float phaseWaitTimer;
    float lastTime;

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
        Label playLabel = new Label("Play:");
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


        //

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


        // IMGUI preview
        IMGUIContainer previewGUI = new IMGUIContainer(DrawPreview);

        previewBox.Add(previewGUI);


        bullets = new();

        // Keep repainting so the preview can animate.
        previewGUI.schedule.Execute(() =>
        {
            PreviewUpdate();
            previewGUI.MarkDirtyRepaint();
        }).Every(16);

        UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true);
        SwitchPreviewMode(false);
    }

    private void PreviewUpdate()
    {
        float deltaTime = Time.time - lastTime;
        if(playingPreview)
        {
            HandlePhase(deltaTime);

            foreach (Bullet bullet in bullets)
            {
                bullet.PreviewUpdate(deltaTime);
            }
        }

        lastTime = Time.time;
    }

    void HandlePhase(float _deltaTime)
    {
        if (phasePreview && !(previewingPhase > phasesAmount - 1))
        {
            switch ((AttackType)phaseEditor.phaseRows[previewingPhase].phaseTypeField.value)
            {
                case AttackType.Wait:
                    phaseWaitTimer += _deltaTime;
                    Debug.Log($"Phase Wait Timer: {phaseWaitTimer}");
                    if (phaseWaitTimer >= phaseEditor.phaseRows[previewingPhase].waitTimeField.value)
                    {
                        previewingPhase++;
                        phaseWaitTimer = 0;
                    }
                    break;
                case AttackType.Attack:
                    BulletPattern attack = phaseEditor.phaseRows[previewingPhase].patternField.value as BulletPattern;
                    UpdateBulletCount(attack.bulletAmount, attack.bulletSpread, attack.bulletAngle, false);
                    previewingPhase++;
                    break;
            }

            //Skip no wait time frames
            if (previewingPhase < phaseEditor.phaseRows.Count)
            {
                if ((phaseEditor.phaseRows[previewingPhase].waitTimeField.value == 0) || ((AttackType)phaseEditor.phaseRows[previewingPhase].phaseTypeField.value == AttackType.Attack))
                {
                    HandlePhase(_deltaTime);
                }
            }
        }
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
            DestroyAllBullets();
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
            UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true);
        }
    }

    public void UpdateBulletCountFromPatternEditor(ChangeEvent<int> evt)
    {
        if(!phasePreview) UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true);
    }

    public void UpdateBulletCountFromPatternEditor(ChangeEvent<float> evt)
    {
        if (!phasePreview) UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true);
    }

    public void UpdateBulletCount(float _bulletAmount, float _bulletSpread, float _bulletStartAngle, bool _destroyOld)
    {
        Debug.Log($"New Bullets: {_bulletAmount} , {_bulletSpread} , {_bulletStartAngle}");
        if(_destroyOld)
        {
            DestroyAllBullets();
        }

        float angleStep = 0;

        if (_bulletAmount == 1)
        {
            angleStep = 0;
        }
        else
        {
            angleStep = _bulletSpread / (_bulletAmount);
        }
        float startAngle = _bulletSpread / 2f;
        startAngle += _bulletStartAngle;

        //Create New Bullets
        for (int i = 0; i < _bulletAmount; i++)
        {
            float bulletAngle = startAngle + i * angleStep;
            Vector2 bulletDirection = Quaternion.Euler(0, 0, bulletAngle) * new Vector3(0, 0, 1);

            Vector2 direction = new Vector2(
                Mathf.Cos(bulletAngle * Mathf.Deg2Rad),
                Mathf.Sin(bulletAngle * Mathf.Deg2Rad)
            );

            Quaternion bulletRotation =
                 Quaternion.Euler(0f, 0f, bulletAngle);

            Bullet newBullet = CreateBullet(new Vector3(direction.x * 0.2f, direction.y * 0.2f, -1), bulletRotation, .3f, Color.red);
            bullets.Add(newBullet);
            preview.AddSingleGO(newBullet.gameObject);
        }
    }

    void DestroyAllBullets()
    {
        if (bullets != null)
        {
            foreach (Bullet bullet in bullets)
            {
                DestroyImmediate(bullet.gameObject);
            }
            bullets.Clear();
        }
        else
        {
            bullets = new();
        }
    }

    private void CreatePreview()
    {
        preview = new PreviewRenderUtility();

        // Camera
        preview.camera.fieldOfView = cameraZoom;
        preview.camera.orthographic = true;

        // Lighting
        preview.lights[0].intensity = 1.4f;
        preview.lights[0].transform.rotation =
            Quaternion.Euler(30f, 30f, 0f);

        preview.lights[1].intensity = 1.0f;

        // Add starting circle to the preview scene
       

        GameObject bossEnemy = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BossEnemy.prefab");

        if (bossEnemy != null)
        {
            GameObject bossEnemyObject = PrefabUtility.InstantiatePrefab(bossEnemy) as GameObject;
            preview.AddSingleGO(bossEnemyObject);
        }

    }

    private void DrawPreview()
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

        if (bullets != null)
        {
            foreach(Bullet bullet in  bullets)
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
        }
        else
        {
            playButton.style.backgroundColor = Color.grey;
            playButtonIcon.image = playButtonTexture;

            if (phasePreview)
            {
                DestroyAllBullets();
                phaseEditor.enabledSelf = true;
            }
            else
            {
                UpdateBulletCount(patternEditor.bulletAmountField.value, patternEditor.bulletSpreadField.value, patternEditor.bulletAngleField.value, true);
                patternEditor.enabledSelf = true;
            }
        }
    }

    private Bullet CreateBullet(Vector3 circlePosition, Quaternion circleRotation, float circleSize, Color color)
    {
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet.prefab");

        if (bulletPrefab == null) return null;

        GameObject newBulletObject =
            PrefabUtility.InstantiatePrefab(bulletPrefab) as GameObject;

        if (newBulletObject == null)
        {
            Debug.LogError($"Could not load prefab at: {"Assets/Prefabs/Bullet.prefab"}");
            return null;
        }

        if(newBulletObject.TryGetComponent<Bullet>(out Bullet newBullet))

        newBullet.transform.localScale = new Vector2(circleSize, circleSize);
        newBullet.transform.position = circlePosition;
        newBullet.transform.rotation = circleRotation;  
        newBullet.GetComponent<SpriteRenderer>().color = color;

        bulletSpeed = 5;
        newBullet.InitializeBullet(bulletSpeed, Vector2.up, circlePosition);
        return newBullet;
    }

    private void DrawBulletDirection(
     Rect rect,
     Vector2 bulletPosition,
     float angle,
     float length,
     Quaternion bulletRotation,
     Bullet bullet)
    {
        Quaternion directionRotation =
            bulletRotation * Quaternion.Euler(0, 0, angle);

        Vector2 direction =
            directionRotation * Vector2.right;

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

        bullet.bulletDirection = direction;
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
