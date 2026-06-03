using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.HeadsUpDisplay
{
    public class EditorStatePillUI : MonoBehaviour
    {
        [Inject]
        private EditorStateChangedEvent editorStateChangedEvent;

        // UI elements
        private VisualElement root;
        private Label stateLabel;
        private VisualElement stateBackground;

        [Header("Colors")]
        [SerializeField]
        private Color placingColor = new Color(0.20f, 0.60f, 1.00f); // blue

        [SerializeField]
        private Color removingColor = new Color(1.00f, 0.30f, 0.30f); // red

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            stateLabel = root.Q<Label>("StateLabel");
            stateBackground = root.Q<VisualElement>("StateContainer");
            Hide();
            editorStateChangedEvent.OnRaised += OnStateChanged;
        }

        private void OnDisable() => editorStateChangedEvent.OnRaised -= OnStateChanged;

        private void OnStateChanged(EditorStates state)
        {
            switch (state)
            {
                case EditorStates.Placing:
                    Show("Placing", placingColor);
                    break;
                case EditorStates.Removing:
                    Show("Removing", removingColor);
                    break;
                case EditorStates.None:
                    Hide();
                    break;
                default:
                    Hide();
                    break;
            }
        }

        private void Show(string text, Color color)
        {
            root.style.display = DisplayStyle.Flex;
            stateLabel.text = text;
            stateBackground.style.backgroundColor = color;
        }

        private void Hide() => root.style.display = DisplayStyle.None;
    }
}
