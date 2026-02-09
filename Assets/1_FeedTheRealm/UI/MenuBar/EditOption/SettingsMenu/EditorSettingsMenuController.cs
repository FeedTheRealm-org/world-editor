using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorSettingsMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;
    private DropdownField _resolutionSelect;
    private Toggle _fullscreenToggle;
    private Button _closeButton;
    private List<Resolution> _availableResolutions;
    private const float baseHeight = 800f;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        /* Display settings */
        _resolutionSelect = root.Q<DropdownField>("ResolutionSelect");
        _fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        _closeButton = root.Q<Button>("Close");
        if (_resolutionSelect == null || _fullscreenToggle == null)
        {
            logger.Log(
                "One or more display settings UI elements not found in the UI Document.",
                this,
                Logging.LogType.Error
            );
            return;
        }
        initializeDisplaySettings();
        adjustUIToScreenSize();
        registerButtonCallbacks(true);
    }

    private void OnDisable()
    {
        registerButtonCallbacks(false);
    }

    private void adjustUIToScreenSize()
    {
        float scaleFactor = Screen.height / baseHeight;

        float resolutionFontSize = _resolutionSelect.resolvedStyle.fontSize;
        float fullscreenFontSize = _fullscreenToggle.resolvedStyle.fontSize;

        _resolutionSelect.style.fontSize = new StyleLength(
            new Length(resolutionFontSize * scaleFactor, LengthUnit.Pixel)
        );
        _fullscreenToggle.style.fontSize = new StyleLength(
            new Length(fullscreenFontSize * scaleFactor, LengthUnit.Pixel)
        );
    }

    private void initializeDisplaySettings()
    {
        _availableResolutions = Screen
            .resolutions.GroupBy(r => new { r.width, r.height })
            .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value).First())
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)
            .ToList();

        List<string> resolutionStrings = _availableResolutions
            .Select(r => $"{r.width}x{r.height}")
            .ToList();

        _resolutionSelect.choices = resolutionStrings;

        // Set current resolution
        string currentResolution =
            $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        _resolutionSelect.value = currentResolution;

        _fullscreenToggle.value = Screen.fullScreen;

        logger.Log(
            $"Initialized {_availableResolutions.Count} resolutions. Current: {currentResolution}",
            this
        );
    }

    private void registerButtonCallbacks(bool register)
    {
        if (!register)
        {
            _fullscreenToggle.UnregisterValueChangedCallback(onFullscreenToggleChanged);
            _resolutionSelect.UnregisterValueChangedCallback(onResolutionChanged);
            _closeButton.clicked -= CloseMenu;
            return;
        }
        _fullscreenToggle.RegisterValueChangedCallback(onFullscreenToggleChanged);
        _resolutionSelect.RegisterValueChangedCallback(onResolutionChanged);
        _closeButton.clicked += CloseMenu;
    }

    private void onFullscreenToggleChanged(ChangeEvent<bool> evt)
    {
        bool newValue = evt.newValue;
        logger.Log("Fullscreen toggle: " + newValue, this, Logging.LogType.Info);

        Screen.fullScreen = newValue;
    }

    private void onResolutionChanged(ChangeEvent<string> evt)
    {
        string selected = evt.newValue;
        logger.Log("Resolution changed to: " + selected, this, Logging.LogType.Info);

        var parts = selected.Split('x');
        if (
            parts.Length == 2
            && int.TryParse(parts[0], out int w)
            && int.TryParse(parts[1], out int h)
        )
        {
            // Find the matching resolution to get the correct refresh rate
            Resolution targetResolution = _availableResolutions.FirstOrDefault(r =>
                r.width == w && r.height == h
            );
            if (targetResolution.width != 0)
            {
                FullScreenMode mode = Screen.fullScreenMode;
                Screen.SetResolution(w, h, mode, targetResolution.refreshRateRatio);
                logger.Log(
                    $"Resolution set to {w}x{h} @ {targetResolution.refreshRateRatio.value}Hz",
                    this
                );
            }
        }
    }
}
