using System;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar
{
    public class SubscriptionMenuOption : MenuOption
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
                resolver.Instantiate(worldUIObjectProvider.subscriptionMenuObject);
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[SubscriptionMenuOption] Error executing: {ex.Message}",
                    Logging.LogType.Error
                );
            }
        }
    }
}
