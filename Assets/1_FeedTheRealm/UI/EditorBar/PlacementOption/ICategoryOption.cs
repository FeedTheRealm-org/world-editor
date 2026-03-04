using FeedTheRealm.Core.EventChannels.UIEvents;

/// <summary>
/// Implemented by any MenuOption that needs to raise a CategorySelectedEvent.
/// The event is injected by the parent controller so the option itself
/// does not need to hold a serialized reference.
/// </summary>
public interface ICategoryOption
{
    void SetCategoryEvent(CategorySelectedEvent categorySelectedEvent);
}
