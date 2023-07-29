using System.Collections.Generic;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class FaultNavigation : MonoBehaviour
    {
        [SerializeField] private FastInputField inputField;
        [SerializeField] private TMP_Text warningCountText;
        [SerializeField] private TMP_Text errorCountText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        private readonly List<ChartFault> faults = new List<ChartFault>();
        private int warningCount = 0;
        private int errorCount = 0;
        private int previousPos;

        public void Clear()
        {
            faults.Clear();
            warningCount = 0;
            errorCount = 0;
            UpdateDisplay();
        }

        public void RegisterFault(ChartFault fault)
        {
            faults.Add(fault);
            switch (fault.Severity)
            {
                case Popups.Severity.Warning:
                    warningCount++;
                    break;
                case Popups.Severity.Error:
                    errorCount++;
                    break;
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            warningCountText.text = warningCount.ToString();
            errorCountText.text = errorCount.ToString();
        }

        private void Awake()
        {
            nextButton.onClick.AddListener(NextFault);
            previousButton.onClick.AddListener(PreviousFault);
        }

        private void OnDestroy()
        {
            nextButton.onClick.RemoveListener(NextFault);
            previousButton.onClick.RemoveListener(PreviousFault);
        }

        private void NextFault()
        {
            if (faults.Count == 0)
            {
                return;
            }

            int caretPos = previousPos;
            ChartFault nextClosest = default;
            bool found = false;
            int minDist = int.MaxValue;

            foreach (var fault in faults)
            {
                int faultPos = inputField.GetCharacterPosition(fault.LineNumber.Or(0), fault.StartCharPos.Or(0));
                if (faultPos > caretPos && faultPos - caretPos < minDist)
                {
                    minDist = faultPos - caretPos;
                    found = true;
                    nextClosest = fault;
                }
            }

            if (!found)
            {
                foreach (var fault in faults)
                {
                    int faultPos = inputField.GetCharacterPosition(fault.LineNumber.Or(0), fault.StartCharPos.Or(0));
                    if (faultPos < minDist)
                    {
                        minDist = faultPos;
                        found = true;
                        nextClosest = fault;
                    }
                }
            }

            if (found)
            {
                JumpToFault(nextClosest);
            }
        }

        private void PreviousFault()
        {
            if (faults.Count == 0)
            {
                return;
            }

            int caretPos = previousPos;
            ChartFault nextClosest = default;
            bool found = false;
            int minDist = int.MaxValue;

            foreach (var fault in faults)
            {
                int faultPos = inputField.GetCharacterPosition(fault.LineNumber.Or(0), fault.StartCharPos.Or(0));
                if (faultPos < caretPos && caretPos - faultPos < minDist)
                {
                    minDist = caretPos - faultPos;
                    found = true;
                    nextClosest = fault;
                }
            }

            if (!found)
            {
                int maxPos = int.MinValue;
                foreach (var fault in faults)
                {
                    int faultPos = inputField.GetCharacterPosition(fault.LineNumber.Or(0), fault.StartCharPos.Or(0));
                    if (faultPos > maxPos)
                    {
                        maxPos = faultPos;
                        found = true;
                        nextClosest = fault;
                    }
                }
            }

            if (found)
            {
                JumpToFault(nextClosest);
            }
        }

        private void JumpToFault(ChartFault fault)
        {
            int faultPos = inputField.GetCharacterPosition(fault.LineNumber.Or(0), fault.StartCharPos.Or(0));
            inputField.SetCurrentPosition(faultPos);
            previousPos = faultPos;
        }
    }
}