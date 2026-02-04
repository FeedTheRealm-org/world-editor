using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.Common
{
    public abstract class MenuOption : MonoBehaviour
    {
        [SerializeField]
        private string label;

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
