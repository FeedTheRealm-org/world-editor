using System.Collections.Generic;
using UnityEngine;

namespace FeedTheRealm.UI.Common
{
    public class MenuOption : MonoBehaviour
    {
        [SerializeField]
        private string label = "Menu Option";

        [Header("Dropdown Menu Options")]
        [SerializeField]
        private List<MenuOption> menuOptions = new();
        public string Label => label;
        public IReadOnlyList<MenuOption> MenuOptions => menuOptions;

        public virtual void Execute()
        {
            Debug.Log($"Executing {label}");
        }
    }
}
