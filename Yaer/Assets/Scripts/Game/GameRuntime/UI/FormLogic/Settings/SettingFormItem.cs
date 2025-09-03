using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Settings
{
    [RequireComponent(typeof(Toggle))]
    public class SettingFormItem : MonoBehaviour
    {
        [SerializeField] private GameObject SelectedTag;

        private void Awake()
        {
            SelectedTag.SetActive(false);
            GetComponent<Toggle>().onValueChanged.AddListener(isOn => { SelectedTag.SetActive(isOn); });
        }

        public void Selected()
        {
            GetComponent<Toggle>().isOn = true;
        }
    }
}