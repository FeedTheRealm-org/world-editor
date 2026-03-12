using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Gameplay.MainMenuSetup.Services;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class SetupServices
    {
        public void RegisterAll(IContainerBuilder builder)
        {
            var setupServices = new[]
            {
                typeof(BaseplateSetupService),
                typeof(CameraSetupService),
                typeof(LightingSetupService),
                typeof(PlayerSetupService),
                typeof(LibrarySetupService),
                typeof(WorldEditorSetupService),
                typeof(WorldUISetupService),
            };

            foreach (var serviceType in setupServices)
            {
                builder.Register(serviceType, Lifetime.Scoped).As(serviceType).As(typeof(ISetup));
            }

            builder.Register<WorldSetupService>(Lifetime.Scoped);
        }
    }
}
