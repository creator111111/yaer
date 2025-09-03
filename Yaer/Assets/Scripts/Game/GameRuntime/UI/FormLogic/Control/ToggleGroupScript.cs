using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Control
{
    public class ToggleGroupScript : MonoBehaviour
    {
        [SerializeField] private List<Toggle> toggles = new List<Toggle>();
        private ToggleGroup tgg;

        private void Awake()
        {
            tgg = GetComponent<ToggleGroup>();
            if (tgg == null) tgg = gameObject.AddComponent<ToggleGroup>();
        }

        private void OnValidate()
        {
            if (tgg == null)
            {
                tgg = GetComponent<ToggleGroup>();
                if (tgg == null) tgg = gameObject.AddComponent<ToggleGroup>();
            }
        }

        public void ActiveOption(string name)
        {
            tgg.allowSwitchOff = true;
            var changed = false;
            foreach (var toggle in toggles)
                if (toggle.name == name)
                {
                    toggle.isOn = true;
                    changed = true;
                }
                else
                {
                    toggle.isOn = false;
                }

            if (changed == false) Debug.LogError("ToggleGroupScript: " + name + " not found");

            tgg.allowSwitchOff = false;
        }
    }
}