using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace STEM.Experiments.Microscope
{
    public class MicroscopeSceneManager : MonoBehaviour
    {
        [SerializeField] MicroscopeState state;
        [SerializeField] List<MicroscopePhase> phases = new List<MicroscopePhase>();
        [SerializeField] string nextSceneName = "MicroscopeQuizScene";

        [Header("UI")]
        [SerializeField] TMP_Text instructionText;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text progressText;
        [SerializeField] Button nextButton;
        [SerializeField] Color hintColor = new Color(1f, 0.85f, 0.4f);
        [SerializeField] Color successColor = new Color(0.5f, 1f, 0.6f);

        [Header("Messages")]
        [SerializeField] string thisIsFormat = "Αυτό είναι: {0}.";
        [SerializeField] string wrongSlideFormat = "Λάθος δείγμα. Χρειάζεσαι: {0}.";
        [SerializeField] string notFocusedText = "Η εικόνα δεν είναι καθαρή. Εστίασε πρώτα.";
        [SerializeField] string needZoomText = "Χρειάζεσαι μεγαλύτερη μεγέθυνση.";
        [SerializeField] string wrongTargetFormat = "Αυτό είναι {0}. Ψάχνουμε: {1}.";
        [SerializeField] string missFormat = "Δεν είναι αυτό. Ψάχνουμε: {0}.";
        [SerializeField] string coarseBlockedText = "Στον φακό 40x χρησιμοποιούμε μόνο τον μικρομετρικό κοχλία.";
        [SerializeField] string crashText = "Προσοχή! Ο φακός πλησιάζει πολύ το πλακίδιο.";

        int index = -1;
        bool done;

        MicroscopePhase Current => index >= 0 && index < phases.Count ? phases[index] : null;

        void OnEnable()
        {
            state.Changed += CheckState;
            state.CoarseBlocked += OnCoarseBlocked;
            state.Crashed += OnCrashed;
        }

        void OnDisable()
        {
            state.Changed -= CheckState;
            state.CoarseBlocked -= OnCoarseBlocked;
            state.Crashed -= OnCrashed;
        }

        void Start()
        {
            nextButton.onClick.AddListener(Next);
            Next();
        }

        void Next()
        {
            index++;
            if (index >= phases.Count)
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            done = false;
            nextButton.gameObject.SetActive(false);
            instructionText.text = Current.instruction;
            SetStatus("", hintColor);
            if (progressText != null) progressText.text = $"{index + 1} / {phases.Count}";
            CheckState();
        }

        void Complete()
        {
            done = true;
            SetStatus(Current.successText, successColor);
            nextButton.gameObject.SetActive(true);
        }

        void SetStatus(string text, Color color)
        {
            statusText.text = text;
            statusText.color = color;
        }

        // Goals that depend only on the microscope state.
        void CheckState()
        {
            var p = Current;
            if (p == null || done) return;

            switch (p.goal)
            {
                case PhaseGoal.PowerOn:
                    if (state.Power) Complete();
                    break;
                case PhaseGoal.LowerLightOn:
                    if (state.LowerLightOn) Complete();
                    break;
                case PhaseGoal.SelectObjective:
                    if (state.ObjectiveIndex == p.objectiveIndex) Complete();
                    break;
                case PhaseGoal.PlaceSlide:
                    if (state.Slide == p.slide) Complete();
                    else if (state.Slide != null)
                        SetStatus(string.Format(wrongSlideFormat, p.slide.displayName), hintColor);
                    break;
                case PhaseGoal.Focus:
                    if (state.Slide != null && state.ObjectiveIndex == p.objectiveIndex && state.InFocus)
                        Complete();
                    break;
            }
        }

        public void OnPartClicked(MicroscopePart part)
        {
            var p = Current;
            if (p == null || done || p.goal != PhaseGoal.ClickPart) return;

            if (part.Id == p.part) Complete();
            else SetStatus(string.Format(thisIsFormat, part.DisplayName), hintColor);
        }

        public void OnSpecimenClicked(Vector2 uv)
        {
            var p = Current;
            if (p == null || done || p.goal != PhaseGoal.FindTarget) return;

            var slide = state.Slide;
            var target = slide.FindKind(p.targetKind);
            string targetName = target != null ? target.displayName : p.targetKind;

            if (!state.InFocus)
                SetStatus(notFocusedText, hintColor);
            else if (state.ObjectiveIndex < p.minObjective)
                SetStatus(needZoomText, hintColor);
            else if (slide.HasKind(uv, p.targetKind))
                Complete();
            else
            {
                var other = slide.CellKindAt(uv);
                string msg = other != null
                    ? string.Format(wrongTargetFormat, other.displayName, targetName)
                    : string.Format(missFormat, targetName);
                SetStatus(msg, hintColor);
            }
        }

        void OnCoarseBlocked()
        {
            if (!done) SetStatus(coarseBlockedText, hintColor);
        }

        void OnCrashed()
        {
            if (!done) SetStatus(crashText, hintColor);
        }
    }
}
