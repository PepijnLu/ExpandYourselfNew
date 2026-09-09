using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletHellEditor : EditorWindow
{
    private PreviewRenderUtility preview;

    private readonly float cameraZoom = 60f;

    private IntegerField bulletAmountField, bulletAngleField;
    List<GameObject> bullets;

    [MenuItem("Window/UI Toolkit/BulletHellEditor")]
    public static void ShowExample()
    {
        BulletHellEditor wnd = GetWindow<BulletHellEditor>();
        wnd.titleContent = new GUIContent("BulletHellEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Bullet Hell Editor Window");
        label.style.marginBottom = 10;
        root.Add(label);

        // Text input
        //nameField = new TextField("Player Name");
        //rootVisualElement.Add(nameField);

        // Bullet Amount
        bulletAmountField = new IntegerField("Bullet Amount");
            rootVisualElement.Add(bulletAmountField);

        // Bullet Angle
        bulletAngleField = new IntegerField("Bullet Angle");
            rootVisualElement.Add(bulletAngleField);

        Button createButton = new Button(() =>
            {
                Debug.Log($"Bullet Amount: {bulletAmountField.value}");
            })
            {
                text = "Create"
            };
            rootVisualElement.Add(createButton);

        // Preview box label
        Label previewLabel = new Label("Preview");
            previewLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            previewLabel.style.marginTop = 10;
            previewLabel.style.marginBottom = 4;

            rootVisualElement.Add(previewLabel);

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

            rootVisualElement.Add(previewBox);


        // IMGUI preview
        IMGUIContainer previewGUI = new IMGUIContainer(DrawPreview);

        previewBox.Add(previewGUI);

        bulletAmountField.RegisterValueChangedCallback(UpdateBulletCount);

        // Keep repainting so the preview can animate.
        previewGUI.schedule.Execute(() =>
        {
            previewGUI.MarkDirtyRepaint();
        }).Every(16);

    }

    private void UpdateBulletCount(ChangeEvent<int> evt)
    {
        float bulletAmount = evt.newValue;
        float bulletSpread = 360;
        float bulletStartAngle = 0;

        if (bullets != null)
        {
            foreach(GameObject bullet in bullets)
            {
                DestroyImmediate(bullet);
            }
            bullets.Clear();
        }
        else
        {
            bullets = new();
        }

        float angleStep = 0;

        if(bulletAmount == 1)
        {
            angleStep = 0;
        }
        else
        {
            angleStep = bulletSpread / (bulletAmount);
        }
        float startAngle = bulletSpread / 2f;
        startAngle += bulletStartAngle;

        //Create New Bullets
        for (int i = 0; i < bulletAmount; i++)
        {
            float bulletAngle = startAngle + i * angleStep;
            Vector2 bulletDirection = Quaternion.Euler(0, 0, bulletAngle) * new Vector3(0, 0, 1);

            Vector2 direction = new Vector2(
                Mathf.Cos(bulletAngle * Mathf.Deg2Rad),
                Mathf.Sin(bulletAngle * Mathf.Deg2Rad)
            );

            Quaternion bulletRotation =
                 Quaternion.Euler(0f, 0f, bulletAngle);

            GameObject newBullet = CreateCircle(direction, bulletRotation, .3f, Color.red);
            bullets.Add(newBullet);
            preview.AddSingleGO(newBullet);
        }

    }

    private void CreatePreview()
    {
        preview = new PreviewRenderUtility();

        // Camera
        preview.camera.fieldOfView = cameraZoom;

        // Lighting
        preview.lights[0].intensity = 1.4f;
        preview.lights[0].transform.rotation =
            Quaternion.Euler(30f, 30f, 0f);

        preview.lights[1].intensity = 1.0f;

        // Add starting circle to the preview scene
        GameObject startCircle = CreateCircle(new Vector2(0, 0), Quaternion.identity, 1, Color.grey);
        preview.AddSingleGO(startCircle);

        Debug.Log($"Created preview: Location {startCircle.transform.position}, {startCircle.transform.localScale}");
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
        preview.camera.transform.position =
            new Vector3(0, 0, -10);

        preview.camera.transform.LookAt(Vector3.zero);

        preview.BeginPreview(rect, GUIStyle.none);

        preview.camera.clearFlags = CameraClearFlags.Color;
        preview.camera.backgroundColor =
            new Color(0.18f, 0.18f, 0.18f);

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
            foreach(GameObject bullet in  bullets)
            {
                if(bullet) DrawBulletDirection(rect, bullet.transform.position, bulletAngleField.value, 100, bullet.transform.rotation);
            }

        }
        Handles.EndGUI();
    }

    private GameObject CreateCircle(Vector2 circlePosition, Quaternion circleRotation, float circleSize, Color color)
    {
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet.prefab");

        if (bulletPrefab == null) return null;

        GameObject newBullet =
            PrefabUtility.InstantiatePrefab(bulletPrefab) as GameObject;

        if (newBullet == null)
        {
            Debug.LogError($"Could not load prefab at: {"Assets/Prefabs/Bullet.prefab"}");
            return null;
        }

        newBullet.transform.localScale = new Vector2(circleSize, circleSize);
        newBullet.transform.position = circlePosition;
        newBullet.transform.rotation = circleRotation;  
        newBullet.GetComponent<SpriteRenderer>().color = color;
        return newBullet;
    }

    private void DrawBulletDirection(
     Rect rect,
     Vector2 bulletPosition,
     float angle,
     float length,
     Quaternion bulletRotation)
    {
        Quaternion directionRotation =
            bulletRotation * Quaternion.Euler(0, 0, angle);

        Vector2 direction =
            directionRotation * Vector2.right;

        Vector2 endPosition =
            bulletPosition + direction * length;

        float zoom = 87f;

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
        GUI.BeginClip(rect);

        Handles.DrawLine(start, end);

        GUI.EndClip();
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
