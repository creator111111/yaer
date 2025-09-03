using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Settings
{
    public class SettingFormResolutionSelector : MonoBehaviour
    {
        [SerializeField] private Button Selector;
        [SerializeField] private ScrollRect Options;
        [SerializeField] private Slider VSlider;
        [SerializeField] private Image SelectedResolution;

        [SerializeField] private ResolutionSelectedEvent onResolutionChanged = new ResolutionSelectedEvent();

        private Transform content;
        private float contentHeight;
        private bool isSelected;
        private float scrollHeight;

        // content
        private readonly List<Toggle> toggles = new List<Toggle>();

        private bool useResolution;
        private float viewHeight;

        private Transform viewport;
        private int windowHeight;

        private int windowWidth;

        public ResolutionSelectedEvent OnResolutionChanged
        {
            get => onResolutionChanged;
            set => onResolutionChanged = value;
        }

        private void Awake()
        {
            viewport = Options.transform.Find("Viewport").transform;
            content = viewport.Find("Content");
            contentHeight = content.GetComponent<RectTransform>().rect.height;
            viewHeight = Options.GetComponent<RectTransform>().rect.height -
                         (viewport.GetComponent<RectTransform>().offsetMin.y -
                          viewport.GetComponent<RectTransform>().offsetMax.y);

            scrollHeight = contentHeight - viewHeight;

            windowWidth = Screen.width;
            windowHeight = Screen.height;

            // VSlider.onValueChanged.AddListener((value) =>
            // {
            //     content.localPosition = new Vector3(0, value * scrollHeight, 0);
            // });

            Selector.onClick.AddListener(() =>
            {
                isSelected = !isSelected;
                //Debug.Log($"showoff {isSelected}");
                Selected(isSelected);
            });

            for (var i = 0; i < content.childCount; i++)
            {
                var toggle = content.GetChild(i).GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(value =>
                {
                    if (value)
                    {
                        SetSelected(toggle);
                        SetResolution(toggle.transform.Find("Background").GetComponent<Image>());
                        isSelected = false;
                    }
                });
                toggles.Add(toggle);
            }
        }

        public void UseResolution(bool isOn)
        {
            useResolution = isOn;
            SelectedResolution.gameObject.SetActive(isOn);
        }

        public void SetResolutionImage((int w, int h) resolution)
        {
            for (var i = 0; i < toggles.Count; i++)
            {
                var toggle = toggles[i];
                if (toggle.name == $"{resolution.w}x{resolution.h}") SelectedResolution.sprite = toggle.transform.Find("Background").GetComponent<Image>().sprite;
            }
        }

        public void SetSelected(bool isOn)
        {
            isSelected = isOn;
            Selected(isSelected);
        }

        private void Selected(bool selected)
        {
            if (selected && useResolution)
                Options.gameObject.SetActive(true);
            else
                Options.gameObject.SetActive(false);
            // VSlider.gameObject.SetActive(selected);
        }

        public void SetResolution(Image sprite)
        {
            SelectedResolution.sprite = sprite.sprite;
            var spName = sprite.sprite.name;
            var rs = spName.Split('x');
            if (int.TryParse(rs[0], out var widthp)) windowWidth = widthp;
            if (int.TryParse(rs[1], out var heightp)) windowHeight = heightp;
            SelectedResolution.SetNativeSize();
            Options.gameObject.SetActive(false);
            OnResolutionChanged.Invoke(windowWidth, windowHeight);
            //Debug.Log($"ResolutionChange {spName}  {windowWidth}x{windowHeight}");
        }

        public void ActiveOption()
        {
        }

        [Serializable]
        public class ResolutionSelectedEvent : UnityEvent<int, int>
        {
        }
    }
}