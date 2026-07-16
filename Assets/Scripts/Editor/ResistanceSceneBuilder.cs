using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using STEM.Experiments.Resistance;

namespace STEM.EditorTools
{
    public static class ResistanceSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/ResistanceScene.unity";
        const string DataFolder = "Assets/Data/Resistance";
        const string MatFolder = "Assets/Materials";
        const string CableMatPath = MatFolder + "/CableLine.mat";

        // Camera: orthographic size 5.4 at 16:9 -> world x -9.6..9.6, y -5.4..5.4
        //
        // UI keep-out zones (do not place anything clickable inside):
        //   InstructionPanel  screen x   40..1020, y 840..1040  -> world x -9.6..0.6,  y  3.0..5.4
        //   DL120Panel        screen x 1460..1880, y  40.. 460  -> world x  5.0..9.6,  y -5.4..-0.8

        static Sprite knob;
        static Sprite panel;
        static Material cableMat;

        static readonly Dictionary<NodeId, Vector2> NodePos = new Dictionary<NodeId, Vector2>
        {
            { NodeId.BAT_POS, new Vector2(-5.0f, -3.4f) },
            { NodeId.BAT_NEG, new Vector2(-3.0f, -3.4f) },
            { NodeId.SW_A,    new Vector2(-0.5f, -3.4f) },
            { NodeId.SW_B,    new Vector2( 1.5f, -3.4f) },
            { NodeId.AMM_A,   new Vector2(-6.2f, -0.9f) },
            { NodeId.AMM_B,   new Vector2(-6.2f,  0.9f) },
            { NodeId.R1_A,    new Vector2(-3.0f,  2.1f) },
            { NodeId.R1_B,    new Vector2(-1.0f,  2.1f) },
            { NodeId.R2_A,    new Vector2( 1.0f,  2.1f) },
            { NodeId.R2_B,    new Vector2( 3.0f,  2.1f) },
            { NodeId.VOLT_A,  new Vector2( 5.3f,  3.6f) },
            { NodeId.VOLT_B,  new Vector2( 6.7f,  3.6f) },
        };

        [MenuItem("STEM/Build Resistance Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolders();
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            panel = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cableMat = LoadOrCreateCableMaterial();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildCamera();
            BuildEquipment();

            Dictionary<NodeId, CircuitNode> nodes = BuildNodes();
            Transform restRoot = new GameObject("RestPoints").transform;
            List<Cable> cables = BuildCables(restRoot);
            CircuitSwitch sw = BuildSwitch();

            DL120Panel dl120;
            TMP_Text instruction, status, result;
            GameObject resultPanel;
            Button next;
            BuildUI(out dl120, out instruction, out status, out resultPanel, out result, out next);

            ResistancePhase[] phases = BuildPhaseAssets();

            var managers = new GameObject("Managers");

            var cm = managers.AddComponent<ConnectionManager>();
            cm.cables = cables;
            cm.nodes = new List<CircuitNode>(nodes.Values);
            cm.circuitSwitch = sw;

            var sm = managers.AddComponent<ResistanceSceneManager>();
            sm.phases = phases;
            sm.instructionText = instruction;
            sm.statusText = status;
            sm.resultPanel = resultPanel;
            sm.resultText = result;
            sm.nextButton = next;
            sm.dl120 = dl120;
            sm.quizSceneName = "ResistanceQuizScene";

            managers.AddComponent<CircuitInputController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Resistance scene built at " + ScenePath);
        }

        // ---------------- scene pieces ----------------

        static void BuildCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);

            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.15f, 0.19f);
            cam.nearClipPlane = 0.1f;

            go.AddComponent<AudioListener>();
        }

        static void BuildEquipment()
        {
            var root = new GameObject("Equipment").transform;

            Board(root, "BatteryHolder_PT2013.3", new Vector2(-4.0f, -3.4f), new Vector2(3.2f, 1.1f), new Color(0.30f, 0.33f, 0.38f));
            Board(root, "Board1_PS2031.1", new Vector2(0.5f, -3.2f), new Vector2(3.4f, 1.6f), new Color(0.16f, 0.28f, 0.46f));
            Board(root, "Board2_PT2013.2", new Vector2(0.0f, 2.1f), new Vector2(8.0f, 1.4f), new Color(0.16f, 0.28f, 0.46f));
            Board(root, "CurrentSensor_RS102", new Vector2(-6.2f, 0.0f), new Vector2(1.2f, 2.6f), new Color(0.85f, 0.86f, 0.88f));
            Board(root, "VoltageSensor_RS101", new Vector2(6.0f, 3.6f), new Vector2(2.6f, 1.1f), new Color(0.85f, 0.86f, 0.88f));

            Label(root, "R1_Label", new Vector2(-2.0f, 2.95f), "R1", 1.3f, Color.white);
            Label(root, "R2_Label", new Vector2(2.0f, 2.95f), "R2", 1.3f, Color.white);
            Label(root, "Amm_Label", new Vector2(-6.2f, 0.0f), "A", 2.0f, Color.black);
            Label(root, "Volt_Label", new Vector2(6.0f, 4.5f), "V", 1.6f, Color.white);
            Label(root, "Bat_Label", new Vector2(-4.0f, -2.5f), "4 x 1.5 V", 1.0f, Color.white);
        }

        static void Board(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = panel;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = color;
            sr.sortingOrder = 0;
        }

        static void Label(Transform parent, string name, Vector2 pos, string text, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var t = go.AddComponent<TextMeshPro>();
            t.text = text;
            t.fontSize = size * 4f;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color;
            t.rectTransform.sizeDelta = new Vector2(3f, 1f);

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 2;
        }

        static Dictionary<NodeId, CircuitNode> BuildNodes()
        {
            var root = new GameObject("Nodes").transform;
            var map = new Dictionary<NodeId, CircuitNode>();

            foreach (var kv in NodePos)
            {
                var go = new GameObject(kv.Key.ToString());
                go.transform.SetParent(root);
                go.transform.position = kv.Value;
                go.transform.localScale = Vector3.one * 0.9f;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = knob;
                sr.color = new Color(0.85f, 0.85f, 0.85f);
                sr.sortingOrder = 3;

                var hi = new GameObject("Highlight");
                hi.transform.SetParent(go.transform);
                hi.transform.localPosition = Vector3.zero;
                hi.transform.localScale = Vector3.one * 1.7f;

                var hsr = hi.AddComponent<SpriteRenderer>();
                hsr.sprite = knob;
                hsr.color = new Color(1f, 0.85f, 0.2f, 0.55f);
                hsr.sortingOrder = 2;
                hsr.enabled = false;

                var node = go.AddComponent<CircuitNode>();
                node.nodeId = kv.Key;
                node.highlight = hsr;

                map.Add(kv.Key, node);
            }
            return map;
        }

        static List<Cable> BuildCables(Transform restRoot)
        {
            var root = new GameObject("Cables").transform;
            var list = new List<Cable>();

            Color[] colors =
            {
                new Color(0.85f, 0.20f, 0.20f),
                new Color(0.25f, 0.25f, 0.28f),
                new Color(0.85f, 0.20f, 0.20f),
                new Color(0.25f, 0.25f, 0.28f),
                new Color(0.90f, 0.60f, 0.15f),
                new Color(0.25f, 0.65f, 0.35f),
                new Color(0.80f, 0.30f, 0.65f),
                new Color(0.30f, 0.55f, 0.85f),
            };

            for (int i = 0; i < 8; i++)
            {
                var go = new GameObject("Cable_" + i);
                go.transform.SetParent(root);

                var line = go.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.material = cableMat;
                line.startColor = colors[i];
                line.endColor = colors[i];
                line.startWidth = 0.09f;
                line.endWidth = 0.09f;
                line.numCapVertices = 4;
                line.sortingOrder = 5;
                line.positionCount = 2;

                var cable = go.AddComponent<Cable>();
                cable.line = line;
                cable.endA = MakeEnd(go.transform, "EndA", colors[i], RestPoint(restRoot, i * 2));
                cable.endB = MakeEnd(go.transform, "EndB", colors[i], RestPoint(restRoot, i * 2 + 1));

                list.Add(cable);
            }
            return list;
        }

        static CableEnd MakeEnd(Transform parent, string name, Color color, Transform rest)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = rest.position;
            go.transform.localScale = Vector3.one * 1.3f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = knob;
            sr.color = color;
            sr.sortingOrder = 8;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.22f;

            var end = go.AddComponent<CableEnd>();
            end.restPoint = rest;
            end.snapRadius = 0.55f;
            return end;
        }

        // Two rows along the bottom left. Kept clear of the DL120 panel.
        static Transform RestPoint(Transform root, int index)
        {
            int col = index % 8;
            int row = index / 8;

            var go = new GameObject("Rest_" + index);
            go.transform.SetParent(root);
            go.transform.position = new Vector3(-8.4f + col * 1.35f, row == 0 ? -4.2f : -4.9f, 0f);
            return go.transform;
        }

        static CircuitSwitch BuildSwitch()
        {
            var go = new GameObject("Switch");
            go.transform.position = new Vector3(0.5f, -2.5f, 0f);
            go.transform.localScale = Vector3.one * 1.8f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = knob;
            sr.color = new Color(0.75f, 0.2f, 0.2f);
            sr.sortingOrder = 6;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.4f, 0.4f);

            var sw = go.AddComponent<CircuitSwitch>();
            sw.graphic = sr;
            sw.closed = false;

            return sw;
        }

        // ---------------- UI ----------------

        static void BuildUI(out DL120Panel dl120, out TMP_Text instruction, out TMP_Text status,
                            out GameObject resultPanel, out TMP_Text result, out Button next)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // instruction, top left, 980 x 200
            RectTransform instrPanel = Panel(canvasGo.transform, "InstructionPanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -40f), new Vector2(980f, 200f), new Color(0f, 0f, 0f, 0.6f));

            instruction = Text(instrPanel, "InstructionText", new Vector2(20f, -16f),
                new Vector2(940f, 120f), 28f, TextAlignmentOptions.TopLeft, Color.white);

            status = Text(instrPanel, "StatusText", new Vector2(20f, -140f),
                new Vector2(940f, 50f), 24f, TextAlignmentOptions.TopLeft, new Color(1f, 0.8f, 0.3f));

            // DL120, bottom right, 420 x 420
            RectTransform dlPanel = Panel(canvasGo.transform, "DL120Panel",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-40f, 40f), new Vector2(420f, 420f), new Color(0.92f, 0.93f, 0.95f, 1f));

            TMP_Text title = Text(dlPanel, "Title", Vector2.zero, Vector2.zero, 28f,
                TextAlignmentOptions.Center, new Color(0.15f, 0.18f, 0.25f));
            title.text = "DL120RS";
            CenterTop(title.rectTransform, new Vector2(0f, -14f), new Vector2(380f, 40f));

            TMP_Text vLabel = Text(dlPanel, "VoltageLabel", Vector2.zero, Vector2.zero, 22f,
                TextAlignmentOptions.Center, new Color(0.35f, 0.38f, 0.45f));
            vLabel.text = "Voltage (V)";
            CenterTop(vLabel.rectTransform, new Vector2(0f, -70f), new Vector2(380f, 30f));

            TMP_Text vValue = Text(dlPanel, "VoltageText", Vector2.zero, Vector2.zero, 60f,
                TextAlignmentOptions.Center, new Color(0.20f, 0.45f, 0.70f));
            vValue.text = "---";
            CenterTop(vValue.rectTransform, new Vector2(0f, -102f), new Vector2(380f, 80f));

            TMP_Text cLabel = Text(dlPanel, "CurrentLabel", Vector2.zero, Vector2.zero, 22f,
                TextAlignmentOptions.Center, new Color(0.35f, 0.38f, 0.45f));
            cLabel.text = "Current (A)";
            CenterTop(cLabel.rectTransform, new Vector2(0f, -196f), new Vector2(380f, 30f));

            TMP_Text cValue = Text(dlPanel, "CurrentText", Vector2.zero, Vector2.zero, 60f,
                TextAlignmentOptions.Center, new Color(0.20f, 0.45f, 0.70f));
            cValue.text = "---";
            CenterTop(cValue.rectTransform, new Vector2(0f, -228f), new Vector2(380f, 80f));

            Button start = MakeButton(dlPanel, "StartButton", "Start (F6)", new Vector2(240f, 60f));
            CenterTop(start.GetComponent<RectTransform>(), new Vector2(0f, -330f), new Vector2(240f, 60f));

            dl120 = dlPanel.gameObject.AddComponent<DL120Panel>();
            dl120.voltageText = vValue;
            dl120.currentText = cValue;
            dl120.startButton = start;
            dl120.noise = 0.01f;

            // result, centered modal
            RectTransform rp = Panel(canvasGo.transform, "ResultPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(900f, 520f), new Color(0.08f, 0.10f, 0.14f, 0.96f));

            result = Text(rp, "ResultText", Vector2.zero, Vector2.zero, 28f,
                TextAlignmentOptions.TopLeft, Color.white);
            CenterTop(result.rectTransform, new Vector2(0f, -40f), new Vector2(820f, 360f));

            next = MakeButton(rp, "NextButton", "Επόμενο", new Vector2(240f, 64f));
            CenterTop(next.GetComponent<RectTransform>(), new Vector2(0f, -430f), new Vector2(240f, 64f));

            resultPanel = rp.gameObject;
            resultPanel.SetActive(false);
        }

        static void CenterTop(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static RectTransform Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
                                   Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.sprite = panel;
            img.type = Image.Type.Sliced;
            img.color = color;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
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

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return t;
        }

        static Button MakeButton(Transform parent, string name, string label, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.sprite = panel;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.20f, 0.45f, 0.70f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);

            var t = textGo.AddComponent<TextMeshProUGUI>();
            t.text = label;
            t.fontSize = 26f;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.raycastTarget = false;

            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return btn;
        }

        // ---------------- assets ----------------

        static ResistancePhase[] BuildPhaseAssets()
        {
            var p1 = MakePhase("Phase1_R2", Topology.SingleR2, VoltmeterTarget.AcrossR2, false,
                "Βήμα 1: Συναρμολόγησε το κύκλωμα με την αντίσταση R2.\n" +
                "Το αμπερόμετρο (RS102) είναι ήδη συνδεδεμένο σε σειρά.\n" +
                "Σύρε τα δύο καλώδια του βολτομέτρου (RS101) στα άκρα της R2.\n" +
                "Κλείσε τον διακόπτη και πάτησε Start (F6).",
                "Μέτρηση: V = {V} V, I = {I} A\n\n" +
                "Νόμος του Ohm:\nR2 = V / I = {R} Ω\n\n" +
                "Η αντίσταση των καλωδίων θεωρείται αμελητέα.",
                new List<CablePreset>
                {
                    Fixed(0, NodeId.BAT_POS, NodeId.SW_A),
                    Fixed(1, NodeId.SW_B, NodeId.AMM_A),
                    Fixed(2, NodeId.AMM_B, NodeId.R2_A),
                    Fixed(3, NodeId.R2_B, NodeId.BAT_NEG),
                    LooseB(6, NodeId.VOLT_A),
                    LooseB(7, NodeId.VOLT_B),
                });

            var p2 = MakePhase("Phase2_Series", Topology.Series, VoltmeterTarget.AcrossR1, true,
                "Βήμα 2: Πρόσθεσε την αντίσταση R1 σε σειρά με την R2.\n" +
                "Το αμπερόμετρο παραμένει σε σειρά. Σύνδεσε το βολτόμετρο\n" +
                "παράλληλα στα άκρα της R1.",
                "Μέτρηση: V = {V} V, I = {I} A\n\n" +
                "R1 = V / I = {R} Ω\n\n" +
                "Ολική αντίσταση: Rολ = R1 + R2 = 100 + 50 = 150 Ω\n" +
                "Έλεγχος: V πηγής = I · Rολ = {I} · 150 ≈ 5 V",
                new List<CablePreset>
                {
                    Fixed(0, NodeId.BAT_POS, NodeId.SW_A),
                    Fixed(1, NodeId.SW_B, NodeId.AMM_A),
                    LooseB(2, NodeId.AMM_B),
                    Fixed(3, NodeId.R2_B, NodeId.BAT_NEG),
                    LooseBoth(4),
                    LooseB(6, NodeId.VOLT_A),
                    LooseB(7, NodeId.VOLT_B),
                });

            var p3 = MakePhase("Phase3_Parallel", Topology.Parallel, VoltmeterTarget.AcrossR1, true,
                "Βήμα 3: Αναδιάταξε τα καλώδια ώστε η R1 να συνδεθεί\n" +
                "παράλληλα με την R2. Το βολτόμετρο μετρά την τάση\n" +
                "στα άκρα της R1.",
                "Μέτρηση: V = {V} V, I = {I} A\n\n" +
                "Ολική αντίσταση: Rολ = V / I = {R} Ω\n\n" +
                "1/R = 1/R1 + 1/R2 = 1/100 + 1/50  ->  R = 33.33 Ω\n\n" +
                "Η ολική αντίσταση είναι μικρότερη από κάθε επιμέρους\n" +
                "αντίσταση, γιατί το ρεύμα έχει περισσότερους δρόμους\n" +
                "να ακολουθήσει.",
                new List<CablePreset>
                {
                    Fixed(0, NodeId.BAT_POS, NodeId.SW_A),
                    Fixed(1, NodeId.SW_B, NodeId.AMM_A),
                    Fixed(2, NodeId.AMM_B, NodeId.R1_A),
                    Fixed(3, NodeId.R2_B, NodeId.BAT_NEG),
                    LooseBoth(4),
                    LooseBoth(5),
                    Fixed(6, NodeId.VOLT_A, NodeId.R1_A),
                    Fixed(7, NodeId.VOLT_B, NodeId.R1_B),
                });

            return new[] { p1, p2, p3 };
        }

        static ResistancePhase MakePhase(string name, Topology topo, VoltmeterTarget volt, bool switchClosed,
                                         string instruction, string result, List<CablePreset> presets)
        {
            string path = DataFolder + "/" + name + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<ResistancePhase>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ResistancePhase>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.requiredTopology = topo;
            asset.requiredVoltmeter = volt;
            asset.switchStartsClosed = switchClosed;
            asset.instruction = instruction;
            asset.resultText = result;
            asset.presets = presets;

            EditorUtility.SetDirty(asset);
            return asset;
        }

        static CablePreset Fixed(int index, NodeId a, NodeId b)
        {
            return new CablePreset
            {
                cableIndex = index,
                endAAttached = true,
                endANode = a,
                endALocked = true,
                endBAttached = true,
                endBNode = b,
                endBLocked = true,
            };
        }

        static CablePreset LooseB(int index, NodeId a)
        {
            return new CablePreset
            {
                cableIndex = index,
                endAAttached = true,
                endANode = a,
                endALocked = true,
                endBAttached = false,
                endBLocked = false,
            };
        }

        static CablePreset LooseBoth(int index)
        {
            return new CablePreset
            {
                cableIndex = index,
                endAAttached = false,
                endALocked = false,
                endBAttached = false,
                endBLocked = false,
            };
        }

        static Material LoadOrCreateCableMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(CableMatPath);
            if (mat != null) return mat;

            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, CableMatPath);
            return mat;
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Data");
            EnsureFolder(DataFolder);
            EnsureFolder(MatFolder);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string leaf = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}