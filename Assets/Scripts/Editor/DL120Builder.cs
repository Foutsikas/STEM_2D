using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using STEM.Experiments.Resistance;

namespace STEM.EditorTools
{
    // Replaces the old Screen Space DL120 UI panel with a world-space device:
    // a SpriteRenderer body + a World Space Canvas over the green LCD.
    public static class DL120Builder
    {
        // Measured off DL_120RS.png
        static readonly Vector2 LcdCenter = new Vector2(0.377f, 0.511f);
        static readonly Vector2 LcdSize   = new Vector2(0.331f, 0.293f);
        static readonly Vector2 OkButton  = new Vector2(0.830f, 0.865f);

        const float CanvasScale = 0.03125f;      // per the earlier DL120 work
        static readonly Vector2 DevicePos = new Vector2(6.7f, -3.2f);
        const float DeviceHeight = 3.2f;

        [MenuItem("STEM/Build DL120 Device")]
        public static void Build()
        {
            Sprite spr = Load("DL_120RS");
            if (spr == null) { Debug.LogError("DL_120RS sprite not found."); return; }

            DeleteObject("DL120Panel");   // old UI panel
            DeleteObject("DL120Device");  // previous run

            float w = DeviceHeight * (spr.bounds.size.x / spr.bounds.size.y);
            float h = DeviceHeight;

            var root = new GameObject("DL120Device");
            root.transform.position = new Vector3(DevicePos.x, DevicePos.y, 0f);
            var panel = root.AddComponent<DL120Panel>();
            panel.noise = 0.01f;

            // body sprite
            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = 0;
            body.transform.localScale = Vector3.one * (DeviceHeight / spr.bounds.size.y);

            // screen canvas over the LCD
            Vector3 lcdWorld = new Vector3(
                DevicePos.x + (LcdCenter.x - 0.5f) * w,
                DevicePos.y + (0.5f - LcdCenter.y) * h, 0f);
            Vector2 lcdWorldSize = new Vector2(LcdSize.x * w, LcdSize.y * h);

            var canvasGo = new GameObject("Screen");
            canvasGo.transform.SetParent(root.transform, false);
            canvasGo.transform.position = lcdWorld;
            canvasGo.transform.localScale = Vector3.one * CanvasScale;

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var crt = canvas.GetComponent<RectTransform>();
            crt.sizeDelta = lcdWorldSize / CanvasScale;

            var bright = new Color(0.78f, 1f, 0.48f);
            var dim = new Color(0.55f, 0.78f, 0.35f);

            float W = crt.sizeDelta.x;
            float H = crt.sizeDelta.y;
            Vector2 labelSize = new Vector2(W * 0.22f, H * 0.34f);
            Vector2 valueSize = new Vector2(W * 0.66f, H * 0.34f);
            float rowY = H * 0.26f;

            Text(crt, "VoltLabel", new Vector2(-W * 0.34f, rowY), labelSize, 6f, TextAlignmentOptions.Right, dim).text = "V";
            var vVal = Text(crt, "VoltageText", new Vector2(W * 0.06f, rowY), valueSize, 8f, TextAlignmentOptions.Left, bright);
            vVal.text = "---";

            Text(crt, "CurrLabel", new Vector2(-W * 0.34f, -rowY), labelSize, 6f, TextAlignmentOptions.Right, dim).text = "A";
            var cVal = Text(crt, "CurrentText", new Vector2(W * 0.06f, -rowY), valueSize, 8f, TextAlignmentOptions.Left, bright);
            cVal.text = "---";

            panel.voltageText = vVal;
            panel.currentText = cVal;

            // OK button collider + glow
            var ok = new GameObject("OK_Button");
            ok.transform.SetParent(root.transform, false);
            ok.transform.position = new Vector3(
                DevicePos.x + (OkButton.x - 0.5f) * w,
                DevicePos.y + (0.5f - OkButton.y) * h, 0f);
            var col = ok.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w * 0.12f, h * 0.13f);
            var okBtn = ok.AddComponent<DL120Button>();
            okBtn.panel = panel;

            Sprite ring = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var glow = new GameObject("OK_Glow");
            glow.transform.SetParent(ok.transform, false);
            glow.transform.localScale = Vector3.one * (h * 0.18f);
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = ring;
            gsr.color = new Color(1f, 0.85f, 0.2f, 0.6f);
            gsr.sortingOrder = 3;
            gsr.enabled = false;
            panel.okGlow = gsr;

            // rewire the scene manager
            var sm = Object.FindAnyObjectByType<ResistanceSceneManager>();
            if (sm != null)
            {
                sm.dl120 = panel;
                EditorUtility.SetDirty(sm);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("DL120 device built. Trigger a reading with F6 or the OK button.");
        }

        static TMP_Text Text(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize,
                             TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return t;
        }

        static void DeleteObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        static Sprite Load(string fileName)
        {
            foreach (string guid in AssetDatabase.FindAssets(fileName + " t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName) continue;

                foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (o is Sprite s) return s;
            }
            return null;
        }
    }
}