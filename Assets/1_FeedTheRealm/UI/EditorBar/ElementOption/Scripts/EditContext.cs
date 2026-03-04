using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using UnityEngine;

/// <summary>
/// Static context holder for passing objects between list and creator menus when editing.
/// This allows the list menu controller to set an object for editing, which the creator
/// menu controller then retrieves when it opens.
/// </summary>
public static class EditContext
{
    private static CreatorObject objectToEdit;

    /// <summary>
    /// Set the object that should be edited. This is called by list menu controllers
    /// before opening a creator menu in edit mode.
    /// </summary>
    public static void SetObjectToEdit(CreatorObject obj)
    {
        objectToEdit = obj;
    }

    /// <summary>
    /// Get the object to edit and clear the context. This is called by creator menu
    /// controllers when they initialize.
    /// </summary>
    public static T GetAndClearObjectToEdit<T>()
        where T : CreatorObject
    {
        var obj = objectToEdit as T;
        objectToEdit = null;
        return obj;
    }

    /// <summary>
    /// Check if there's an object pending edit.
    /// </summary>
    public static bool HasObjectToEdit()
    {
        return objectToEdit != null;
    }

    /// <summary>
    /// Clear any pending edit context.
    /// </summary>
    public static void Clear()
    {
        objectToEdit = null;
    }
}
