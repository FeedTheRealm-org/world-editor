using FeedTheRealm.Core.EventChannels.UIEvents;
using FTR.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar
{
    public class CreatablesTabController : MonoBehaviour
    {
        [SerializeField]
        private GameObject creatableOption;

        [Inject]
        private EnableInputEvent enableInputEvent;

        [Inject]
        private Logging.Logger logger;

        [Inject]
        private IObjectResolver resolver;

        private Button creatables;
        private VisualElement creatablesPanel;
        private bool panelOpen = false;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            creatables = root.Q<Button>("Creatables");
            creatablesPanel = root.Q<VisualElement>("CreatablesPanel");

            if (creatableOption != null)
                resolver.InjectGameObject(creatableOption);

            creatables.RegisterCallback<MouseEnterEvent>(_ => enableInputEvent.Raise(false));
            creatables.RegisterCallback<MouseLeaveEvent>(_ => enableInputEvent.Raise(true));
            creatables.clicked += TogglePanel;

            if (
                creatableOption != null
                && creatableOption.TryGetComponent<MenuOption>(out var menuOption)
            )
            {
                creatables.text = menuOption.Label;
                BuildCreatablesPanel(menuOption);
            }
        }

        private void TogglePanel()
        {
            panelOpen = !panelOpen;
            creatablesPanel.style.display = panelOpen ? DisplayStyle.Flex : DisplayStyle.None;
            creatables.EnableInClassList("hud-tab--active", panelOpen);
        }

        private void BuildCreatablesPanel(MenuOption menuOption)
        {
            creatablesPanel.Clear();

            foreach (var option in menuOption.MenuOptions)
            {
                var capturedOption = option;
                var btn = new Button();
                btn.text = option.Label;
                btn.AddToClassList("hud-tab__zone-item");
                btn.clicked += () =>
                {
                    capturedOption.Execute();
                    TogglePanel();
                };
                creatablesPanel.Add(btn);
            }
        }
    }
}
