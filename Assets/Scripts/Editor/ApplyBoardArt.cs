using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using STEM.Experiments.Resistance;

namespace STEM.EditorTools
{
    // Non-destructive edits to the open ResistanceScene.
    //   STEM > Apply Board Art          swaps board sprites, snaps nodes, tidies layout
    //   STEM > Update Resistance Text   rewrites the phase assets to match the pptx exactly
    public static class ApplyBoardArt
    {
        // Measured jack positions (x from left, y from top) on the PNGs.
        static readonly Vector2 R1_left  = new Vector2(0.244f, 0.108f);
        static readonly Vector2 R1_right = new Vector2(0.756f, 0.108f);
        static readonly Vector2 R2_left  = new Vector2(0.244f, 0.192f);
        static readonly Vector2 R2_right = new Vector2(0.756f, 0.192f);
        static readonly Vector2 Bat_pos  = new Vector2(0.269f, 0.030f);
        static readonly Vector2 Bat_neg  = new Vector2(0.280f, 0.965f);
        static readonly Vector2 Sw_a     = new Vector2(0.720f, 0.800f); // GUESS
        static readonly Vector2 Sw_b     = new Vector2(0.720f, 0.720f); // GUESS
        static readonly Vector2 Toggle   = new Vector2(0.480f, 0.950f);

        [MenuItem("STEM/Apply Board Art")]
        public static void Apply()
        {
            Sprite resSpr = Load("PT2013_2");
            Sprite batSpr = Load("PT2013_3");
            Sprite swSpr  = Load("PT2031_1");

            if (resSpr == null || batSpr == null || swSpr == null)
            {
                Debug.LogError("Missing sprite. Import PT2013_2, PT2013_3, PT2031_1 as Sprite (2D and UI).");
                return;
            }

            Vector2 resSize = SetBoard("Board2_PT2013.2", resSpr, 5.2f, new Vector2(2.4f, 1.3f));
            Vector2 batSize = SetBoard("BatteryHolder_PT2013.3", batSpr, 3.6f, new Vector2(2.4f, -3.0f));
            Vector2 swSize  = SetBoard("Board1_PS2031.1", swSpr, 8.2f, new Vector2(-7.6f, -1.0f));

            GameObject resBoard = GameObject.Find("Board2_PT2013.2");
            GameObject batBoard = GameObject.Find("BatteryHolder_PT2013.3");
            GameObject swBoard  = GameObject.Find("Board1_PS2031.1");

            PlaceNode(NodeId.R1_A, resBoard, R1_left,  resSize);
            PlaceNode(NodeId.R1_B, resBoard, R1_right, resSize);
            PlaceNode(NodeId.R2_A, resBoard, R2_left,  resSize);
            PlaceNode(NodeId.R2_B, resBoard, R2_right, resSize);

            PlaceNode(NodeId.BAT_POS, batBoard, Bat_pos, batSize);
            PlaceNode(NodeId.BAT_NEG, batBoard, Bat_neg, batSize);

            PlaceNode(NodeId.SW_A, swBoard, Sw_a, swSize);
            PlaceNode(NodeId.SW_B, swBoard, Sw_b, swSize);

            GameObject sw = GameObject.Find("Switch");
            if (sw != null)
                sw.transform.position = LocalToWorld(swBoard, Toggle, swSize);

            // Current sensor (ammeter) off the switch board, into the open main-line gap.
            MoveSensor("CurrentSensor_RS102", new Vector2(-2.5f, 0.5f));
            SetNode(NodeId.AMM_A, new Vector2(-2.5f, -0.6f));
            SetNode(NodeId.AMM_B, new Vector2(-2.5f,  1.6f));
            MoveLabel("Amm_Label", new Vector2(-2.5f, 0.5f));

            // Remove labels the real board art already prints.
            DeleteObject("R1_Label");
            DeleteObject("R2_Label");
            DeleteObject("Bat_Label");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Board art applied. Ammeter moved clear of the switch board. Confirm SW_A/SW_B.");
        }

        [MenuItem("STEM/Update Resistance Text")]
        public static void UpdateText()
        {
            SetPhase("Assets/Data/Resistance/Phase1_R2.asset",
                "Βήμα 2: Συναρμολόγησε το κύκλωμα με την αντίσταση R2.\n" +
                "Το αμπερόμετρο (RS102) συνδέεται σε σειρά.\n" +
                "Σύνδεσε το βολτόμετρο (RS101) παράλληλα στα άκρα της R2.\n" +
                "Κλείσε τον διακόπτη και πάτησε Start (F6).",
                "Μέτρηση: V = {V} V, I = {I} A\n\n" +
                "Νόμος του Ohm:\n" +
                "R2 = V / I = {Vn} / {In} = {R} Ω\n\n" +
                "Η αντίσταση των καλωδίων θεωρείται αμελητέα.");

            SetPhase("Assets/Data/Resistance/Phase2_Series.asset",
                "Βήμα 12: Πρόσθεσε την αντίσταση R1 σε σειρά με την R2.\n" +
                "Το αμπερόμετρο συνδέεται σε σειρά και το βολτόμετρο\n" +
                "παράλληλα στα άκρα της R1.",
                "Μέτρηση: V = {V} V, I = {I} A\n\n" +
                "R1 = V / I = {Vn} / {In} ≈ {R} Ω\n\n" +
                "Ολική αντίσταση: Rολ = R1 + R2 = 100 + 50 = 150 Ω\n" +
                "Έλεγχος τάσης πηγής: V = I · Rολ = {In} · 150 ≈ 5 V");

            SetPhase("Assets/Data/Resistance/Phase3_Parallel.asset",
                "Βήμα 17: Αναδιάταξε τα καλώδια ώστε η R1 να συνδεθεί\n" +
                "παράλληλα με την R2. Το αμπερόμετρο σε σειρά,\n" +
                "το βολτόμετρο παράλληλα στα άκρα της R1.",
                "Μέτρηση: V = {V} V, I = {I} A\n\n" +
                "Ολική αντίσταση: Rολ = V / I = {Vn} / {In} = {R} Ω\n\n" +
                "1/R = 1/R1 + 1/R2 = 1/100 + 1/50  ->  R = 33.33 Ω\n\n" +
                "Η ολική αντίσταση είναι μικρότερη από κάθε επιμέρους\n" +
                "αντίσταση, γιατί το ρεύμα έχει περισσότερους δρόμους\n" +
                "να ακολουθήσει.");

            AssetDatabase.SaveAssets();
            Debug.Log("Phase text updated to match the pptx.");
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

        static Vector2 SetBoard(string name, Sprite spr, float targetHeight, Vector2 pos)
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

        static void MoveSensor(string name, Vector2 pos)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) go.transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        static void MoveLabel(string name, Vector2 pos)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) go.transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        static void DeleteObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        static void SetNode(NodeId id, Vector2 pos)
        {
            CircuitNode node = FindNode(id);
            if (node != null) node.transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        static void PlaceNode(NodeId id, GameObject board, Vector2 norm, Vector2 worldSize)
        {
            if (board == null) return;
            CircuitNode node = FindNode(id);
            if (node == null) { Debug.LogWarning("Node missing: " + id); return; }
            node.transform.position = LocalToWorld(board, norm, worldSize);
        }

        static Vector3 LocalToWorld(GameObject board, Vector2 norm, Vector2 worldSize)
        {
            Vector3 c = board.transform.position;
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

        static Sprite Load(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName + " t:Texture2D");
            foreach (string guid in guids)
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