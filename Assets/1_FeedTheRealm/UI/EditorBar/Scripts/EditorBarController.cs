using System.Collections.Generic;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar
{
    public class EditorBarController : MonoBehaviour
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private UIDocument menuBarUI;

        [Header("Menu Options")]
        //[SerializeField]
        //private MenuOption ZoneOption;

        [SerializeField]
        private MenuOption PlacementOption;

        [SerializeField]
        private MenuOption ElementOption;
        private VisualElement root;
        private MenuStack menuStack;

        void Start()
        {
            if (menuBarUI == null)
            {
                logger.Log("menuBarUI (UIDocument) is not assigned.", this, Logging.LogType.Error);
                return;
            }

            root = menuBarUI.rootVisualElement;

            if (root == null)
            {
                logger.Log(
                    "UIDocument.rootVisualElement is null. Make sure the UIDocument has a PanelSettings and a VisualTreeAsset assigned.",
                    this,
                    Logging.LogType.Error
                );
                return;
            }

            menuStack = new MenuStack(root);

            //BindButton("Zone", ZoneOption);
            BindButton("Placement", PlacementOption);
            BindButton("Element", ElementOption);
        }

        private void BindButton(string buttonName, MenuOption option)
        {
            Button button = root.Q<Button>(buttonName);

            if (button == null)
            {
                logger.Log(
                    $"Button '{buttonName}' not found in MenuBar UI.",
                    this,
                    Logging.LogType.Error
                );
                return;
            }

            if (option == null)
            {
                button.SetEnabled(false);
                return;
            }

            button.text = option.Label;
            button.clicked += () =>
            {
                if (option.MenuOptions.Count == 0)
                    return;
                menuStack.Open(button, option.MenuOptions);
            };
        }
    }
}
