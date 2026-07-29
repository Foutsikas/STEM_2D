using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using STEM.Experiments.Resistance;

namespace STEM.EditorTools
{
    //   STEM > Apply Board Art          swaps sprites, snaps nodes, adds sensor labels, parks cables
    //   STEM > Update Resistance Text   rewrites the phase assets (English)
    public static class ApplyBoardArt
    {
        static readonly Vector2 R1_left  = new Vector2(0.244f, 0.108f);
        static readonly Vector2 R1_right = new Vector2(0.756f, 0.108f);
        static readonly Vector2 R2_left  = new Vector2(0.244f, 0.192f);
        static readonly Vector2 R2_right = new Vector2(0.756f, 0.192f);
        static readonly Vector2 Bat_pos  = new Vector2(0.269f, 0.030f);
        static readonly Vector2 Bat_neg  = new Vector2(0.280f, 0.965f);
        static readonly Vector2 Sw_a     = new Vector2(0.694f, 0.934f);
        static readonly Vector2 Sw_b     = new Vector2(0.258f, 0.944f);
        static readonly Vector2 Toggle   = new Vector2(0.480f, 0.965f);
        static readonly Vector2 Lead_left  = new Vector2(0.30f, 0.85f);
        static readonly Vector2 Lead_right = new Vector2(0.70f, 0.85f);

        // Where loose cable ends park. Keyed by rest-point index (Rest_{2i}=cableI.endA,
        // Rest_{2i+1}=cableI.endB). Only the ends that go loose in some phase matter;
        // each is parked near its origin so the lead does not stretch across the scene.
        static readonly Dictionary<int, Vector2> RestPos = new Dictionary<int, Vector2>
        {
            { 5,  new Vector2(-3.6f,  0.5f) },  // cable2 endB  (from ammeter)
            { 8,  new Vector2(-1.6f,  2.2f) },  // cable4 endA  (jumper)
            { 9,  new Vector2(-0.8f,  2.2f) },  // cable4 endB  (jumper)
            { 10, new Vector2(-1.6f,  1.3f) },  // cable5 endA  (jumper)
            { 11, new Vector2(-0.8f,  1.3f) },  // cable5 endB  (jumper)
            { 13, new Vector2( 4.4f,  0.0f) },  // cable6 endB  (voltmeter probe)
            { 15, new Vector2( 5.0f,  0.0f) },  // cable7 endB  (voltmeter probe)
        };
        static readonly Vector2 RestPark = new Vector2(-9.0f, -5.2f); // unused ends, off to the corner

        [MenuItem("STEM/Apply Board Art")]
        public static void Apply()
        {
            Sprite resSpr  = Load("PT2013_2");
            Sprite batSpr  = Load("PT2013_3");
            Sprite swSpr   = Load("PT2031_1");
            Sprite ammSpr  = Load("RS102", "RS102_Current_Sensor");
            Sprite voltSpr = Load("Voltage", "RS101_Voltage_Sensor");

            if (resSpr == null || batSpr == null || swSpr == null || ammSpr == null || voltSpr == null)
            {
                Debug.LogError("Missing sprite. Need PT2013_2, PT2013_3, PT2031_1, RS102_-_Current, Voltage_Sensor.");
                return;
            }

            Vector2 resSize = SetSprite("Board2_PT2013.2", resSpr, 5.2f, new Vector2(2.4f, 1.3f));
            Vector2 batSize = SetSprite("BatteryHolder_PT2013.3", batSpr, 3.6f, new Vector2(2.4f, -3.0f));
            Vector2 swSize  = SetSprite("Board1_PS2031.1", swSpr, 8.2f, new Vector2(-7.6f, -1.0f));
            Vector2 ammSize = SetSprite("CurrentSensor_RS102", ammSpr, 2.2f, new Vector2(-2.5f, 0.5f));
            Vector2 volSize = SetSprite("VoltageSensor_RS101", voltSpr, 3.0f, new Vector2(5.3f, 1.6f));

            GameObject resBoard = GameObject.Find("Board2_PT2013.2");
            GameObject batBoard = GameObject.Find("BatteryHolder_PT2013.3");
            GameObject swBoard  = GameObject.Find("Board1_PS2031.1");
            GameObject ammObj   = GameObject.Find("CurrentSensor_RS102");
            GameObject volObj   = GameObject.Find("VoltageSensor_RS101");

            PlaceNode(NodeId.R1_A, resBoard, R1_left,  resSize);
            PlaceNode(NodeId.R1_B, resBoard, R1_right, resSize);
            PlaceNode(NodeId.R2_A, resBoard, R2_left,  resSize);
            PlaceNode(NodeId.R2_B, resBoard, R2_right, resSize);
            PlaceNode(NodeId.BAT_POS, batBoard, Bat_pos, batSize);
            PlaceNode(NodeId.BAT_NEG, batBoard, Bat_neg, batSize);
            PlaceNode(NodeId.SW_A, swBoard, Sw_a, swSize);
            PlaceNode(NodeId.SW_B, swBoard, Sw_b, swSize);
            PlaceNode(NodeId.AMM_A, ammObj, Lead_left,  ammSize);
            PlaceNode(NodeId.AMM_B, ammObj, Lead_right, ammSize);
            PlaceNode(NodeId.VOLT_A, volObj, Lead_left,  volSize);
            PlaceNode(NodeId.VOLT_B, volObj, Lead_right, volSize);

            GameObject sw = GameObject.Find("Switch");
            if (sw != null) sw.transform.position = LocalToWorld(swBoard, Toggle, swSize);

            // Sensor identity labels below each sensor.
            CreateLabel("Label_RS102", new Vector2(-2.5f, -0.95f), "RS102\nAmmeter (A)", 1.0f);
            CreateLabel("Label_RS101", new Vector2(5.3f, -0.35f), "RS101\nVoltmeter (V)", 1.0f);

            // Remove the old single-letter labels the sprites already carry.
            DeleteObject("Amm_Label");
            DeleteObject("Volt_Label");
            DeleteObject("R1_Label");
            DeleteObject("R2_Label");
            DeleteObject("Bat_Label");

            // Park loose cable ends near their origins.
            for (int i = 0; i < 16; i++)
            {
                GameObject r = GameObject.Find("Rest_" + i);
                if (r == null) continue;
                Vector2 p = RestPos.TryGetValue(i, out Vector2 v) ? v : RestPark;
                r.transform.position = new Vector3(p.x, p.y, 0f);
            }

            // Next button label.
            GameObject nb = GameObject.Find("NextButton");
            if (nb != null)
            {
                TMP_Text lbl = nb.GetComponentInChildren<TMP_Text>(true);
                if (lbl != null) lbl.text = "Next";
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Board art, sensor labels and cable parking applied.");
        }

        [MenuItem("STEM/Update Resistance Text")]
        public static void UpdateText()
        {
            SetPhase("Assets/Data/Resistance/Phase1_R2.asset",
                "Step 1: Assemble the circuit with resistor R2.\n" +
                "The ammeter (RS102) is connected in series.\n" +
                "Connect the voltmeter (RS101) in parallel across R2.\n" +
                "Close the switch and press OK on the DL120.",
                "Measurement: V = {V} V, I = {I} A\n\n" +
                "Ohm's Law:\n" +
                "R2 = V / I = {Vn} / {In} = {R} Ohm\n\n" +
                "The resistance of the cables is negligible.");

            SetPhase("Assets/Data/Resistance/Phase2_Series.asset",
                "Step 2: Add resistor R1 in series with R2.\n" +
                "The ammeter stays in series and the voltmeter\n" +
                "connects in parallel across R1.",
                "Measurement: V = {V} V, I = {I} A\n\n" +
                "R1 = V / I = {Vn} / {In} ~ {R} Ohm\n\n" +
                "Total resistance: Rtot = R1 + R2 = 100 + 50 = 150 Ohm\n" +
                "Source check: V = I * Rtot = {In} * 150 ~ 5 V");

            SetPhase("Assets/Data/Resistance/Phase3_Parallel.asset",
                "Step 3: Rearrange the cables so R1 is connected\n" +
                "in parallel with R2. The ammeter stays in series,\n" +
                "the voltmeter in parallel across R1.",
                "Measurement: V = {V} V, I = {I} A\n\n" +
                "Total resistance: Rtot = V / I = {Vn} / {In} = {R} Ohm\n\n" +
                "1/R = 1/R1 + 1/R2 = 1/100 + 1/50  ->  R = 33.33 Ohm\n\n" +
                "The total resistance is smaller than either resistor,\n" +
                "because the current has more paths to flow through.");

            AssetDatabase.SaveAssets();
            Debug.Log("Phase text updated (English).");
        }

        // ---------------- helpers ----------------

        static void SetPhase(string path, string instruction, string result)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ResistancePhase>(path);
            if (asset == null) { Debug.LogWarning("Phase asset missing: " + path); return; }
            asset.instruction = instruction;
            asset.resultText = result;
            EditorUtility.SetDirty(asset);
        }

        static void CreateLabel(string name, Vector2 pos, string text, float size)
        {
            DeleteObject(name);
            var go = new GameObject(name);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            var t = go.AddComponent<TextMeshPro>();
            t.text = text;
            t.fontSize = size * 4f;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.rectTransform.sizeDelta = new Vector2(4f, 1.6f);

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 4;
        }

        static Vector2 SetSprite(string name, Sprite spr, float targetHeight, Vector2 pos)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) { Debug.LogWarning("Not found: " + name); return Vector2.one; }

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.color = Color.white;
            sr.sortingOrder = 0;

            float scale = targetHeight / spr.bounds.size.y;
            go.transform.localScale = Vector3.one * scale;
            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            return new Vector2(spr.bounds.size.x * scale, spr.bounds.size.y * scale);
        }

        static void DeleteObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        static void PlaceNode(NodeId id, GameObject anchor, Vector2 norm, Vector2 worldSize)
        {
            if (anchor == null) return;
            CircuitNode node = FindNode(id);
            if (node == null) { Debug.LogWarning("Node missing: " + id); return; }
            node.transform.position = LocalToWorld(anchor, norm, worldSize);
        }

        static Vector3 LocalToWorld(GameObject anchor, Vector2 norm, Vector2 worldSize)
        {
            Vector3 c = anchor.transform.position;
            return new Vector3(
                c.x + (norm.x - 0.5f) * worldSize.x,
                c.y + (0.5f - norm.y) * worldSize.y,
                0f);
        }

        static CircuitNode FindNode(NodeId id)
        {
            foreach (var n in Object.FindObjectsByType<CircuitNode>(FindObjectsSortMode.None))
                if (n.nodeId == id) return n;
            return null;
        }

        static Sprite Load(string token, string exactName)
        {
            foreach (string guid in AssetDatabase.FindAssets(token + " t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != exactName) continue;
                foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (o is Sprite s) return s;
            }
            return null;
        }

        static Sprite Load(string name) => Load(name, name);
    }
}