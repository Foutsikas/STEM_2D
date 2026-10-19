using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace STEM.Experiments.Microscope.EditorTools
{
    // One-time builder. Run once, check the scenes, then delete this file.
    public static class MicroscopeSceneBuilder
    {
        const string Art = "Assets/Sprites/Microscope";
        const string Data = "Assets/Data/Microscope";
        const string SceneFolder = "Assets/Scenes/Microscope";
        const string ScenePath = SceneFolder + "/MicroscopeScene.unity";
        const string QuizPath = SceneFolder + "/MicroscopeQuizScene.unity";

        // Photo cutout is 666 x 1304 px. Positions below are in its pixels (y down).
        const float PhotoW = 666f, PhotoH = 1304f;

        [MenuItem("STEM/Microscope/Build Scenes")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ImportArt();
            var settings = CreateSettings();
            var blood = CreateSlide("Slide_Blood", "Επίχρισμα αίματος", "blood", new[]
            {
                Kind("RBC", "ερυθρό αιμοσφαίριο", 0, 40),
                Kind("WBC", "λευκό αιμοσφαίριο", 0, 80),
                Kind("Platelet", "αιμοπετάλιο", 0, 120),
            });
            var root = CreateSlide("Slide_OnionRoot", "Ρίζα κρεμμυδιού", "root", new[]
            {
                Kind("Interphase", "κύτταρο στη μεσόφαση", 0, 40),
                Kind("Prophase", "κύτταρο στην πρόφαση", 0, 80),
                Kind("Metaphase", "κύτταρο στη μετάφαση", 0, 120),
                Kind("Anaphase", "κύτταρο στην ανάφαση", 0, 160),
                Kind("Telophase", "κύτταρο στην τελόφαση", 0, 200),
                Kind("Meristem", "μερίστωμα", 1, 255),
            });
            var phases = CreatePhases(blood, root);
            var questions = CreateQuestions();

            BuildMainScene(settings, blood, root, phases);
            BuildQuizScene(questions);
            AddToBuildSettings(ScenePath, QuizPath);

            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Microscope scenes built. Delete MicroscopeSceneBuilder.cs when done.");
        }

        // ---------- Art ----------

        static void ImportArt()
        {
            ImportSprite(Art + "/microscope_side.png", 100f, 2048, true);
            ImportSprite(Art + "/screen_frame.png", 200f, 2048, true);
            ImportSprite(Art + "/white.png", 100f, 64, false);
            ImportSprite(Art + "/circle.png", 100f, 128, true);
            ImportSprite(Art + "/slide_blood.png", 100f, 1024, true);
            ImportSprite(Art + "/slide_root.png", 100f, 1024, true);
            foreach (var n in new[] { "blood", "root" })
            {
                ImportSprite($"{Art}/{n}_sharp.png", 100f, 4096, true);
                ImportSprite($"{Art}/{n}_blur.png", 100f, 4096, true);
                LabelMap($"{Art}/{n}_labels.png");
            }
            AssetDatabase.Refresh();
        }

        static void ImportSprite(string path, float ppu, int maxSize, bool smooth)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (ti == null) throw new Exception("Missing art: " + path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = smooth ? FilterMode.Bilinear : FilterMode.Point;
            ti.maxTextureSize = maxSize;
            ti.SaveAndReimport();
        }

        static void LabelMap(string path)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (ti == null) throw new Exception("Missing art: " + path);
            ti.textureType = TextureImporterType.Default;
            ti.isReadable = true;
            ti.sRGBTexture = false;
            ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 2048;
            ti.SaveAndReimport();
        }

        static Sprite LoadSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/{file}");

        // ---------- Data assets ----------

        static T Asset<T>(string path) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) return a;
            EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        static MicroscopeSettings CreateSettings()
        {
            var s = Asset<MicroscopeSettings>(Data + "/MicroscopeSettings.asset");
            s.objectives = new[]
            {
                new Objective { label = "4x", zoom = 1f, focusHeight = 0.52f, tolerance = 0.02f, blurRange = 0.08f, ringColor = new Color(0.95f, 0.3f, 0.3f) },
                new Objective { label = "10x", zoom = 2.5f, focusHeight = 0.535f, tolerance = 0.01f, blurRange = 0.05f, ringColor = new Color(1f, 0.8f, 0.2f) },
                new Objective { label = "40x", zoom = 8f, focusHeight = 0.54f, tolerance = 0.005f, blurRange = 0.03f, ringColor = new Color(0.35f, 0.6f, 1f) },
            };
            s.startObjective = 1;
            s.viewAspect = 231f / 430f;
            EditorUtility.SetDirty(s);
            return s;
        }

        static SlideKind Kind(string id, string name, int channel, int value) =>
            new SlideKind { id = id, displayName = name, channel = channel, value = value };

        static SlideData CreateSlide(string asset, string displayName, string art, SlideKind[] kinds)
        {
            var s = Asset<SlideData>($"{Data}/{asset}.asset");
            s.displayName = displayName;
            s.sharp = LoadSprite(art + "_sharp.png");
            s.blurred = LoadSprite(art + "_blur.png");
            s.labelMap = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Art}/{art}_labels.png");
            s.kinds = kinds;
            EditorUtility.SetDirty(s);
            return s;
        }

        static List<MicroscopePhase> CreatePhases(SlideData blood, SlideData root)
        {
            var list = new List<MicroscopePhase>();
            int n = 0;

            MicroscopePhase P(PhaseGoal goal, string instruction, string success)
            {
                n++;
                var p = Asset<MicroscopePhase>($"{Data}/Phases/Phase_{n:00}.asset");
                p.goal = goal;
                p.instruction = instruction;
                p.successText = success;
                p.part = PartId.None;
                p.slide = null;
                p.objectiveIndex = 0;
                p.targetKind = "";
                p.minObjective = 0;
                list.Add(p);
                return p;
            }

            // Part 1: parts of the microscope
            P(PhaseGoal.ClickPart,
                "Γνωριμία με το μικροσκόπιο.\nΚάνε κλικ στην περιστρεφόμενη κεφαλή με τους αντικειμενικούς φακούς.",
                "Σωστά! Κάθε φακός δίνει διαφορετική μεγέθυνση: 4x, 10x και 40x.").part = PartId.Nosepiece;
            P(PhaseGoal.ClickPart,
                "Κάνε κλικ στην τράπεζα.",
                "Πάνω στην τράπεζα τοποθετούμε το πλακίδιο με το παρασκεύασμα. Το έλασμα το κρατά σταθερό.").part = PartId.Stage;
            P(PhaseGoal.ClickPart,
                "Κάνε κλικ σε έναν από τους κοχλίες κίνησης της τράπεζας (οι ασημένιοι).",
                "Με αυτούς μετακινούμε το πλακίδιο αριστερά-δεξιά και μπρος-πίσω.").part = PartId.StageKnobs;
            P(PhaseGoal.ClickPart,
                "Κάνε κλικ στον μακρομετρικό κοχλία (ο μεγάλος τροχός στο πλάι).",
                "Ο μακρομετρικός κοχλίας ανεβοκατεβάζει γρήγορα την τράπεζα. Τον χρησιμοποιούμε για πρώτη εστίαση.").part = PartId.CoarseFocus;
            P(PhaseGoal.ClickPart,
                "Κάνε κλικ στον μικρομετρικό κοχλία (το μικρότερο κομμάτι στο κέντρο του τροχού).",
                "Ο μικρομετρικός κοχλίας κινεί την τράπεζα πολύ λίγο. Τον χρησιμοποιούμε για λεπτή εστίαση.").part = PartId.FineFocus;
            P(PhaseGoal.ClickPart,
                "Κάνε κλικ στον κάτω φωτισμό, κάτω από την τράπεζα.",
                "Ο κάτω φωτισμός στέλνει φως μέσα από το πλακίδιο προς τον φακό.").part = PartId.LowerLight;

            // Part 2: using it with a blood smear
            P(PhaseGoal.PowerOn,
                "Ας ξεκινήσουμε την παρατήρηση.\nΠάτα το κουμπί λειτουργίας στο πλάι της οθόνης.",
                "Η οθόνη άναψε. Η κάμερα του μικροσκοπίου δείχνει την εικόνα εδώ.");
            P(PhaseGoal.LowerLightOn,
                "Άναψε τον κάτω φωτισμό: σύρε προς τα πάνω τον δεξί τροχό φωτεινότητας στη βάση.",
                "Το παρασκεύασμα είναι διαφανές, γι' αυτό το φως πρέπει να περάσει μέσα από αυτό. Ο πάνω φωτισμός είναι για αδιαφανή αντικείμενα.");
            P(PhaseGoal.SelectObjective,
                "Γύρισε την κεφαλή (κλικ πάνω της) στον μικρότερο φακό, 4x.",
                "Ξεκινάμε πάντα από τη μικρότερη μεγέθυνση. Βλέπουμε μεγάλη περιοχή και εστιάζουμε εύκολα.").objectiveIndex = 0;
            P(PhaseGoal.PlaceSlide,
                "Σύρε το πλακίδιο με το αίμα πάνω στην τράπεζα.",
                "Το πλακίδιο είναι στη θέση του, κάτω από τον φακό.").slide = blood;
            P(PhaseGoal.Focus,
                "Σύρε πάνω ή κάτω τον μακρομετρικό κοχλία μέχρι η εικόνα να γίνει καθαρή.",
                "Η εικόνα είναι καθαρή. Τα μικρά ροζ στρογγυλά είναι τα κύτταρα του αίματος.").objectiveIndex = 0;
            P(PhaseGoal.Focus,
                "Γύρισε την κεφαλή στον φακό 10x.\nΔιόρθωσε την εστίαση με τον μικρομετρικό κοχλία.",
                "Οι φακοί είναι σχεδόν εστιασμένοι μεταξύ τους, οπότε αρκεί μια μικρή διόρθωση.").objectiveIndex = 1;
            P(PhaseGoal.Focus,
                "Γύρισε στον φακό 40x. Εστίασε μόνο με τον μικρομετρικό κοχλία.",
                "Στη μεγάλη μεγέθυνση ο φακός είναι πολύ κοντά στο πλακίδιο. Ο μακρομετρικός θα μπορούσε να το σπάσει.").objectiveIndex = 2;
            var rbc = P(PhaseGoal.FindTarget,
                "Κάνε κλικ σε ένα ερυθρό αιμοσφαίριο.\nΜε τους ασημένιους κοχλίες μετακινείς το δείγμα.",
                "Τα ερυθρά αιμοσφαίρια δεν έχουν πυρήνα. Το ανοιχτό κέντρο οφείλεται στο σχήμα τους (αμφίκοιλος δίσκος).");
            rbc.targetKind = "RBC"; rbc.minObjective = 2;
            var wbc = P(PhaseGoal.FindTarget,
                "Βρες και κάνε κλικ σε ένα λευκό αιμοσφαίριο.\nΈχει πυρήνα που βάφεται μωβ.",
                "Τα λευκά αιμοσφαίρια προστατεύουν τον οργανισμό. Είναι πολύ λιγότερα από τα ερυθρά.");
            wbc.targetKind = "WBC"; wbc.minObjective = 2;
            var plt = P(PhaseGoal.FindTarget,
                "Βρες και κάνε κλικ σε ένα αιμοπετάλιο.\nΕίναι μικρά μωβ κομματάκια ανάμεσα στα κύτταρα.",
                "Τα αιμοπετάλια είναι κομμάτια κυττάρων. Βοηθούν στην πήξη του αίματος.");
            plt.targetKind = "Platelet"; plt.minObjective = 2;

            // Part 3: mitosis in an onion root tip
            P(PhaseGoal.SelectObjective,
                "Τώρα θα παρατηρήσουμε μίτωση.\nΠριν αλλάξεις πλακίδιο, γύρισε ξανά στον φακό 4x.",
                "Με τον μικρό φακό ο φακός είναι μακριά από το πλακίδιο και δεν κινδυνεύει να το χτυπήσει.").objectiveIndex = 0;
            P(PhaseGoal.PlaceSlide,
                "Σύρε το πλακίδιο με τη ρίζα κρεμμυδιού πάνω στην τράπεζα.",
                "Η άκρη της ρίζας μεγαλώνει γρήγορα, γι' αυτό εκεί πολλά κύτταρα διαιρούνται.").slide = root;
            P(PhaseGoal.Focus,
                "Εστίασε την εικόνα στον φακό 4x.",
                "Βλέπεις όλη την άκρη της ρίζας.").objectiveIndex = 0;
            var mer = P(PhaseGoal.FindTarget,
                "Κάνε κλικ στην περιοχή με τα μικρά, τετράγωνα κύτταρα, λίγο πάνω από τη μύτη της ρίζας.",
                "Αυτό είναι το μερίστωμα. Εδώ τα κύτταρα διαιρούνται συνεχώς με μίτωση.");
            mer.targetKind = "Meristem"; mer.minObjective = 0;
            P(PhaseGoal.Focus,
                "Κεντράρισε το μερίστωμα και πέρνα στον φακό 40x.\nΕστίασε με τον μικρομετρικό κοχλία.",
                "Τώρα φαίνονται τα χρωμοσώματα μέσα στα κύτταρα.").objectiveIndex = 2;
            Find(P(PhaseGoal.FindTarget,
                "Κάνε κλικ σε κύτταρο στη μεσόφαση.\nΈχει στρογγυλό πυρήνα, δεν φαίνονται χρωμοσώματα.",
                "Μεσόφαση: το κύτταρο μεγαλώνει και διπλασιάζει το DNA του. Είναι η μεγαλύτερη φάση."), "Interphase");
            Find(P(PhaseGoal.FindTarget,
                "Βρες κύτταρο στην πρόφαση.\nΤα χρωμοσώματα φαίνονται σαν μπερδεμένα νήματα.",
                "Πρόφαση: τα χρωμοσώματα συμπυκνώνονται και η πυρηνική μεμβράνη αρχίζει να διαλύεται."), "Prophase");
            Find(P(PhaseGoal.FindTarget,
                "Βρες κύτταρο στη μετάφαση.\nΤα χρωμοσώματα είναι σε μια γραμμή στη μέση.",
                "Μετάφαση: τα χρωμοσώματα παρατάσσονται στο ισημερινό επίπεδο του κυττάρου."), "Metaphase");
            Find(P(PhaseGoal.FindTarget,
                "Βρες κύτταρο στην ανάφαση.\nΔύο ομάδες χρωμοσωμάτων απομακρύνονται.",
                "Ανάφαση: οι αδελφές χρωματίδες χωρίζουν και κινούνται προς τους δύο πόλους."), "Anaphase");
            Find(P(PhaseGoal.FindTarget,
                "Βρες κύτταρο στην τελόφαση.\nΔύο νέοι πυρήνες στις άκρες, νέο τοίχωμα στη μέση.",
                "Τελόφαση: σχηματίζονται δύο πυρήνες και το κύτταρο χωρίζεται σε δύο θυγατρικά. Τέλος! Πάμε στο κουίζ."), "Telophase");

            foreach (var p in list) EditorUtility.SetDirty(p);
            return list;
        }

        static void Find(MicroscopePhase p, string kind)
        {
            p.targetKind = kind;
            p.minObjective = 2;
        }

        static List<MicroscopeQuizQuestion> CreateQuestions()
        {
            var raw = new[]
            {
                new[] { "Με ποιον αντικειμενικό φακό ξεκινάμε την παρατήρηση;", "4x", "10x", "40x", "Δεν έχει σημασία",
                    "Ξεκινάμε από τη μικρότερη μεγέθυνση, γιατί βλέπουμε μεγάλη περιοχή και εστιάζουμε εύκολα." },
                new[] { "Στον φακό 40x εστιάζουμε με:", "Τον μικρομετρικό κοχλία", "Τον μακρομετρικό κοχλία", "Τους κοχλίες της τράπεζας", "Τον τροχό φωτεινότητας",
                    "Ο φακός 40x είναι πολύ κοντά στο πλακίδιο. Ο μακρομετρικός μπορεί να το σπάσει." },
                new[] { "Γιατί χρησιμοποιούμε τον κάτω φωτισμό για ένα πλακίδιο;", "Το φως περνά μέσα από το διαφανές δείγμα", "Ζεσταίνει το δείγμα", "Μεγαλώνει την εικόνα", "Είναι για αδιαφανή αντικείμενα",
                    "Τα παρασκευάσματα είναι λεπτά και διαφανή, γι' αυτό τα φωτίζουμε από κάτω." },
                new[] { "Η εικόνα στην οθόνη είναι πολύ σκοτεινή. Τι κάνουμε;", "Αυξάνουμε τη φωτεινότητα του κάτω φωτισμού", "Γυρίζουμε στον φακό 40x", "Γυρίζουμε τον μακρομετρικό κοχλία", "Βγάζουμε το πλακίδιο",
                    "Η φωτεινότητα ρυθμίζεται από τον τροχό του φωτισμού." },
                new[] { "Ποια κύτταρα του αίματος δεν έχουν πυρήνα;", "Τα ερυθρά αιμοσφαίρια", "Τα λευκά αιμοσφαίρια", "Όλα τα κύτταρα του αίματος", "Κανένα",
                    "Τα ώριμα ερυθρά αιμοσφαίρια χάνουν τον πυρήνα τους, έτσι χωράνε περισσότερη αιμοσφαιρίνη." },
                new[] { "Ποιος είναι ο ρόλος των αιμοπεταλίων;", "Βοηθούν στην πήξη του αίματος", "Μεταφέρουν οξυγόνο", "Καταπολεμούν τα μικρόβια", "Μεταφέρουν θρεπτικές ουσίες",
                    "Τα αιμοπετάλια συγκεντρώνονται σε ένα τραύμα και βοηθούν να σταματήσει η αιμορραγία." },
                new[] { "Γιατί παρατηρούμε μίτωση στην άκρη της ρίζας κρεμμυδιού;", "Εκεί τα κύτταρα διαιρούνται συνεχώς", "Εκεί τα κύτταρα δεν έχουν πυρήνα", "Εκεί τα κύτταρα είναι νεκρά", "Εκεί υπάρχουν αιμοσφαίρια",
                    "Το μερίστωμα της ρίζας μεγαλώνει γρήγορα, οπότε βρίσκουμε κύτταρα σε όλες τις φάσεις." },
                new[] { "Σε ποια φάση τα χρωμοσώματα παρατάσσονται στη μέση του κυττάρου;", "Μετάφαση", "Πρόφαση", "Ανάφαση", "Τελόφαση",
                    "Στη μετάφαση τα χρωμοσώματα βρίσκονται στο ισημερινό επίπεδο." },
                new[] { "Σε ποια φάση οι αδελφές χρωματίδες κινούνται προς τους πόλους;", "Ανάφαση", "Μετάφαση", "Πρόφαση", "Μεσόφαση",
                    "Στην ανάφαση οι χρωματίδες χωρίζουν και τραβιούνται προς αντίθετες πλευρές." },
                new[] { "Στο μικροσκόπιο, σε ποια φάση βρίσκονται τα περισσότερα κύτταρα;", "Μεσόφαση", "Πρόφαση", "Μετάφαση", "Τελόφαση",
                    "Η μεσόφαση διαρκεί πολύ περισσότερο από τις φάσεις της μίτωσης." },
            };

            var list = new List<MicroscopeQuizQuestion>();
            for (int i = 0; i < raw.Length; i++)
            {
                var q = Asset<MicroscopeQuizQuestion>($"{Data}/Quiz/Question_{i + 1:00}.asset");
                q.question = raw[i][0];
                q.answers = new[] { raw[i][1], raw[i][2], raw[i][3], raw[i][4] };
                q.explanation = raw[i][5];
                EditorUtility.SetDirty(q);
                list.Add(q);
            }
            AssetDatabase.SaveAssets();
            return list;
        }

        // ---------- Main scene ----------

        static void BuildMainScene(MicroscopeSettings settings, SlideData blood, SlideData root, List<MicroscopePhase> phases)
        {
            EnsureFolder(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = MakeCamera();

            var stateGo = new GameObject("MicroscopeState");
            var state = stateGo.AddComponent<MicroscopeState>();
            Set(state, "settings", settings);

            var white = LoadSprite("white.png");
            var circle = LoadSprite("circle.png");

            // Microscope photo, 7.8 units tall
            var scope = new GameObject("Microscope");
            scope.transform.position = new Vector3(-5.4f, -1.1f, 0f);
            float s = 7.8f / (PhotoH / 100f);
            scope.transform.localScale = new Vector3(s, s, 1f);
            scope.AddComponent<SpriteRenderer>().sprite = LoadSprite("microscope_side.png");

            Part(scope, "Part_Arm", PartId.Arm, "Βραχίονας", 150, 600, 32);
            Part(scope, "Part_Base", PartId.Base, "Βάση", 300, 1100, 38);
            Part(scope, "Part_ScreenSide", PartId.Screen, "Οθόνη", 520, 200, 70);
            Part(scope, "Part_UpperLight", PartId.UpperLight, "Πάνω φωτισμός", 315, 600, 38);
            Part(scope, "Part_SlideClip", PartId.SlideClip, "Έλασμα συγκράτησης", 445, 772, 22);
            Part(scope, "Part_LowerLight", PartId.LowerLight, "Κάτω φωτισμός", 455, 950, 52);

            var nose = Part(scope, "Part_Nosepiece", PartId.Nosepiece, "Περιστρεφόμενη κεφαλή φακών", 560, 600, 50);
            Set(nose.AddComponent<Nosepiece>(), "state", state);

            var stage = new GameObject("Part_Stage");
            stage.transform.SetParent(scope.transform, false);
            stage.transform.localPosition = Px(460, 787);
            var stageBox = stage.AddComponent<BoxCollider2D>();
            stageBox.size = new Vector2(3.8f, 1.05f);
            SetPart(stage.AddComponent<MicroscopePart>(), PartId.Stage, "Τράπεζα");

            var anchor = new GameObject("StageAnchor");
            anchor.transform.SetParent(scope.transform, false);
            anchor.transform.localPosition = Px(470, 732);

            Drag(scope, state, "Part_StageKnobX", PartId.StageKnobs, "Κοχλίας τράπεζας", ControlType.StageX, 125, 662, 30, null);
            Drag(scope, state, "Part_StageKnobY", PartId.StageKnobs, "Κοχλίας τράπεζας", ControlType.StageY, 345, 668, 30, null);

            var finePtr = MakePointer(scope, "FinePointer", 132, 925, 52f, white);
            var coarsePtr = MakePointer(scope, "CoarsePointer", 132, 925, 92f, white);
            Drag(scope, state, "Part_FineFocus", PartId.FineFocus, "Μικρομετρικός κοχλίας", ControlType.FineFocus, 132, 925, 62, finePtr);
            Drag(scope, state, "Part_CoarseFocus", PartId.CoarseFocus, "Μακρομετρικός κοχλίας", ControlType.CoarseFocus, 222, 925, 30, coarsePtr);

            Drag(scope, state, "Part_UpperBrightness", PartId.UpperBrightness, "Φωτεινότητα πάνω φωτισμού", ControlType.UpperBrightness, 180, 1192, 32, null);
            Drag(scope, state, "Part_LowerBrightness", PartId.LowerBrightness, "Φωτεινότητα κάτω φωτισμού", ControlType.LowerBrightness, 390, 1192, 32, null);

            Glow(scope, state, "UpperGlow", false, 335, 625, 0.7f, circle);
            Glow(scope, state, "LowerGlow", true, 455, 902, 0.9f, circle);

            // Screen
            var frame = new GameObject("Screen");
            frame.transform.position = new Vector3(4.4f, -0.3f, 0f);
            frame.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
            var frameSr = frame.AddComponent<SpriteRenderer>();
            frameSr.sprite = LoadSprite("screen_frame.png");
            frameSr.sortingOrder = 10;
            frame.AddComponent<BoxCollider2D>().size = new Vector2(5.23f, 3.69f);
            SetPart(frame.AddComponent<MicroscopePart>(), PartId.Screen, "Οθόνη");

            var viewSize = new Vector2(4.30f, 2.31f);
            var view = new GameObject("SpecimenView");
            view.transform.SetParent(frame.transform, false);
            view.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            view.AddComponent<BoxCollider2D>().size = viewSize;

            var mask = new GameObject("Mask");
            mask.transform.SetParent(view.transform, false);
            mask.transform.localScale = new Vector3(viewSize.x / 0.08f, viewSize.y / 0.08f, 1f);
            mask.AddComponent<SpriteMask>().sprite = white;

            var empty = Masked(view, "EmptyField", white, 1);
            empty.transform.localScale = mask.transform.localScale;
            empty.color = Color.black;
            var blurSr = Masked(view, "Blurred", null, 2);
            var sharpSr = Masked(view, "Sharp", null, 3);

            var label = new GameObject("ObjectiveLabel").AddComponent<TextMeshPro>();
            label.transform.SetParent(view.transform, false);
            label.transform.localPosition = new Vector3(-1.75f, 0.95f, 0f);
            label.rectTransform.sizeDelta = new Vector2(0.8f, 0.3f);
            label.fontSize = 2.5f;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.sortingOrder = 5;

            var sv = view.AddComponent<SpecimenView>();
            Set(sv, "state", state);
            Set(sv, "viewSize", viewSize);
            Set(sv, "emptyField", empty);
            Set(sv, "blurred", blurSr);
            Set(sv, "sharp", sharpSr);
            Set(sv, "objectiveLabel", label);

            var power = new GameObject("Part_PowerButton");
            power.transform.SetParent(frame.transform, false);
            power.transform.localPosition = new Vector3(2.38f, 1.3f, 0f);
            power.transform.localScale = new Vector3(0.22f, 0.22f, 1f);
            var powerSr = power.AddComponent<SpriteRenderer>();
            powerSr.sprite = circle;
            powerSr.sortingOrder = 11;
            power.AddComponent<CircleCollider2D>().radius = 0.64f;
            SetPart(power.AddComponent<MicroscopePart>(), PartId.PowerButton, "Κουμπί λειτουργίας");
            var pb = power.AddComponent<PowerButton>();
            Set(pb, "state", state);
            Set(pb, "indicator", powerSr);

            // Slides
            MakeSlide("Slide_Blood", "slide_blood.png", blood, new Vector3(1.6f, -4.3f, 0f), state, stageBox, anchor.transform);
            MakeSlide("Slide_OnionRoot", "slide_root.png", root, new Vector3(4.7f, -4.3f, 0f), state, stageBox, anchor.transform);

            // UI
            var canvas = MakeCanvas();
            var panel = UI("InstructionPanel", canvas);
            Place(panel, new Vector2(0.5f, 1f), new Vector2(0f, -15f), new Vector2(1100f, 135f));
            panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            var instr = MakeText(panel, "InstructionText", "", 28, TextAlignmentOptions.Center);
            Stretch(instr.rectTransform, 20f, 10f);

            var status = MakeText(canvas, "StatusText", "", 26, TextAlignmentOptions.Top);
            Place(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(1000f, 110f));

            var progress = MakeText(canvas, "ProgressText", "", 24, TextAlignmentOptions.TopLeft);
            Place(progress.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -15f), new Vector2(200f, 50f));

            var next = MakeButton(canvas, "NextButton", "Επόμενο", 28);
            Place((RectTransform)next.transform, new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(240f, 70f));

            var mgrGo = new GameObject("MicroscopeSceneManager");
            var mgr = mgrGo.AddComponent<MicroscopeSceneManager>();
            Set(mgr, "state", state);
            SetArray(mgr, "phases", phases.Cast<Object>().ToArray());
            Set(mgr, "nextSceneName", "MicroscopeQuizScene");
            Set(mgr, "instructionText", instr);
            Set(mgr, "statusText", status);
            Set(mgr, "progressText", progress);
            Set(mgr, "nextButton", next);

            var input = mgrGo.AddComponent<MicroscopeInputController>();
            Set(input, "cam", cam);
            Set(input, "manager", mgr);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static Vector3 Px(float x, float y) => new Vector3((x - PhotoW / 2f) / 100f, (PhotoH / 2f - y) / 100f, 0f);

        static GameObject Part(GameObject parent, string name, PartId id, string display, float x, float y, float r)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Px(x, y);
            go.AddComponent<CircleCollider2D>().radius = r / 100f;
            SetPart(go.AddComponent<MicroscopePart>(), id, display);
            return go;
        }

        static void SetPart(MicroscopePart part, PartId id, string display)
        {
            Set(part, "id", id);
            Set(part, "displayName", display);
        }

        static void Drag(GameObject parent, MicroscopeState state, string name, PartId id, string display,
            ControlType control, float x, float y, float r, Transform pointer)
        {
            var go = Part(parent, name, id, display, x, y, r);
            var d = go.AddComponent<DragControl>();
            Set(d, "state", state);
            Set(d, "control", control);
            if (pointer != null) Set(d, "pointer", pointer);
        }

        static Transform MakePointer(GameObject parent, string name, float x, float y, float radiusPx, Sprite white)
        {
            var p = new GameObject(name);
            p.transform.SetParent(parent.transform, false);
            p.transform.localPosition = Px(x, y);
            var tick = new GameObject("Tick");
            tick.transform.SetParent(p.transform, false);
            tick.transform.localPosition = new Vector3(0f, radiusPx / 100f, 0f);
            tick.transform.localScale = new Vector3(0.6f, 2.2f, 1f);
            var sr = tick.AddComponent<SpriteRenderer>();
            sr.sprite = white;
            sr.color = new Color(1f, 1f, 1f, 0.85f);
            sr.sortingOrder = 3;
            return p.transform;
        }

        static void Glow(GameObject parent, MicroscopeState state, string name, bool lower, float x, float y, float size, Sprite circle)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Px(x, y);
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.color = new Color(1f, 0.95f, 0.75f, 0f);
            sr.sortingOrder = 2;
            var g = go.AddComponent<LampGlow>();
            Set(g, "state", state);
            Set(g, "lower", lower);
        }

        static SpriteRenderer Masked(GameObject parent, string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            return sr;
        }

        static void MakeSlide(string name, string art, SlideData data, Vector3 pos, MicroscopeState state, Collider2D zone, Transform anchor)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(art);
            sr.sortingOrder = 4;
            go.AddComponent<BoxCollider2D>();

            var label = new GameObject(name + "_Label").AddComponent<TextMeshPro>();
            label.transform.position = pos + new Vector3(0f, -0.65f, 0f);
            label.rectTransform.sizeDelta = new Vector2(3f, 0.5f);
            label.text = data.displayName;
            label.fontSize = 3f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 4;

            var slide = go.AddComponent<MicroscopeSlide>();
            Set(slide, "data", data);
            Set(slide, "state", state);
            Set(slide, "stageZone", zone);
            Set(slide, "stageAnchor", anchor);
            Set(slide, "trayLabel", label.gameObject);
        }

        // ---------- Quiz scene ----------

        static void BuildQuizScene(List<MicroscopeQuizQuestion> questions)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MakeCamera();
            var canvas = MakeCanvas();

            var progress = MakeText(canvas, "ProgressText", "", 26, TextAlignmentOptions.TopLeft);
            Place(progress.rectTransform, new Vector2(0f, 1f), new Vector2(30f, -20f), new Vector2(200f, 50f));

            var question = MakeText(canvas, "QuestionText", "", 40, TextAlignmentOptions.Center);
            Place(question.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1500f, 200f));

            var answers = new Button[4];
            var slots = new[] { new Vector2(-380f, 40f), new Vector2(380f, 40f), new Vector2(-380f, -100f), new Vector2(380f, -100f) };
            for (int i = 0; i < 4; i++)
            {
                answers[i] = MakeButton(canvas, $"Answer_{i + 1}", "", 28);
                Place((RectTransform)answers[i].transform, new Vector2(0.5f, 0.5f), slots[i], new Vector2(700f, 110f));
                var colors = answers[i].colors;
                colors.disabledColor = Color.white;
                answers[i].colors = colors;
            }

            var explanation = MakeText(canvas, "ExplanationText", "", 28, TextAlignmentOptions.Center);
            Place(explanation.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1500f, 130f));

            var next = MakeButton(canvas, "NextButton", "Επόμενο", 28);
            Place((RectTransform)next.transform, new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(240f, 70f));

            var result = UI("ResultPanel", canvas);
            Stretch(result, 0f, 0f);
            result.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.95f);
            var resultText = MakeText(result, "ResultText", "", 44, TextAlignmentOptions.Center);
            Place(resultText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(1400f, 200f));
            var cont = MakeButton(result, "ContinueButton", "Συνέχεια", 30);
            Place((RectTransform)cont.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(320f, 80f));
            var retry = MakeButton(result, "RetryButton", "Ξανά", 30);
            Place((RectTransform)retry.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(320f, 80f));

            var mgr = new GameObject("MicroscopeQuizManager").AddComponent<MicroscopeQuizManager>();
            SetArray(mgr, "questions", questions.Cast<Object>().ToArray());
            Set(mgr, "passScore", 7);
            Set(mgr, "progressText", progress);
            Set(mgr, "questionText", question);
            SetArray(mgr, "answerButtons", answers.Cast<Object>().ToArray());
            Set(mgr, "explanationText", explanation);
            Set(mgr, "nextButton", next);
            Set(mgr, "resultPanel", result.gameObject);
            Set(mgr, "resultText", resultText);
            Set(mgr, "continueButton", cont);
            Set(mgr, "retryButton", retry);

            EditorSceneManager.SaveScene(scene, QuizPath);
        }

        // ---------- Shared helpers ----------

        static Camera MakeCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.14f, 0.17f);
            return cam;
        }

        static Transform MakeCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            go.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            return go.transform;
        }

        static RectTransform UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void Stretch(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }

        static TextMeshProUGUI MakeText(Transform parent, string name, string text, float size, TextAlignmentOptions align)
        {
            var t = UI(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            return t;
        }

        static Button MakeButton(Transform parent, string name, string label, float size)
        {
            var rt = UI(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var t = MakeText(rt, "Label", label, size, TextAlignmentOptions.Center);
            t.color = new Color(0.1f, 0.1f, 0.12f);
            Stretch(t.rectTransform, 12f, 6f);
            return btn;
        }

        static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogError($"{target.GetType().Name} has no field '{field}'");
                return;
            }
            switch (value)
            {
                case Object o: p.objectReferenceValue = o; break;
                case Enum e: p.intValue = Convert.ToInt32(e); break;
                case int i: p.intValue = i; break;
                case float f: p.floatValue = f; break;
                case bool b: p.boolValue = b; break;
                case string s: p.stringValue = s; break;
                case Vector2 v2: p.vector2Value = v2; break;
                case Vector3 v3: p.vector3Value = v3; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(Object target, string field, Object[] items)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            p.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddToBuildSettings(params string[] paths)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            foreach (var path in paths)
                if (scenes.All(s => s.path != path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}