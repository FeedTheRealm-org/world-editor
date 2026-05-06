using System;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.UI.Common;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar
{
    public class TransactionsMenuOption : MenuOption
    {
        [Inject]
        private Logging.Logger logger;

        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        [Inject]
        private IObjectResolver resolver;

        public override void Execute()
        {
            try
            {
                resolver.Instantiate(worldUIObjectProvider.transactionsMenuObject);
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[TransactionsMenuOption] Error executing: {ex.Message}",
                    Logging.LogType.Error
                );
            }
        }
    }
}
