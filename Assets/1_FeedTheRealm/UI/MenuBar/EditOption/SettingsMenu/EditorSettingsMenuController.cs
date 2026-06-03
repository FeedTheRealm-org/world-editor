using System.Collections.Generic;
using System.Linq;
using FTR.UI;
using FTRShared.Runtime.Core.Cache;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    public class EditorSettingsMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;
        private Toggle _fullscreenToggle;
        private Button _closeButton;
        private Button _clearCacheButton;
        private Label _cacheStatusLabel;

        [Inject]
        private CacheManager cacheManager;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
            _closeButton = root.Q<Button>("Close");
            _clearCacheButton = root.Q<Button>("ClearCacheButton");
            _cacheStatusLabel = root.Q<Label>("CacheStatusLabel");
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
                _clearCacheButton.clicked -= OnClearCacheClicked;
                return;
            }
            _fullscreenToggle.RegisterValueChangedCallback(onFullscreenToggleChanged);
            _closeButton.clicked += CloseMenu;
            _clearCacheButton.clicked += OnClearCacheClicked;
        }

        private void onFullscreenToggleChanged(ChangeEvent<bool> evt)
        {
            bool newValue = evt.newValue;

            FullScreenMode mode = newValue
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;

            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, mode);
            if (newValue)
                Screen.fullScreenMode = mode;
        }

        private void OnClearCacheClicked()
        {
            int deletedCount = cacheManager.ClearAllCache();
            if (_cacheStatusLabel != null)
            {
                _cacheStatusLabel.text =
                    deletedCount > 0
                        ? $"Cleared cache: {deletedCount} files removed."
                        : "Cache already empty.";
            }
        }
    }
}
