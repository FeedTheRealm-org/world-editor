using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    public class EditorSettingsMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;
        private Toggle _fullscreenToggle;
        private Button _closeButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
            _closeButton = root.Q<Button>("Close");
            initializeDisplaySettings();
            registerButtonCallbacks(true);
        }

        private void OnDisable()
        {
            registerButtonCallbacks(false);
        }

        private void initializeDisplaySettings()
        {
            _fullscreenToggle.SetValueWithoutNotify(Screen.fullScreen);
        }

        private void registerButtonCallbacks(bool register)
        {
            if (!register)
            {
                _fullscreenToggle.UnregisterValueChangedCallback(onFullscreenToggleChanged);
                _closeButton.clicked -= CloseMenu;
                return;
            }
            _fullscreenToggle.RegisterValueChangedCallback(onFullscreenToggleChanged);
            _closeButton.clicked += CloseMenu;
        }

        private void onFullscreenToggleChanged(ChangeEvent<bool> evt)
        {
            bool newValue = evt.newValue;

            FullScreenMode mode = newValue
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;

            if (!newValue)
            {
                // Going windowed — restore native resolution
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, mode);
            }
            else
            {
                Screen.fullScreenMode = mode;
            }
        }
    }
}
