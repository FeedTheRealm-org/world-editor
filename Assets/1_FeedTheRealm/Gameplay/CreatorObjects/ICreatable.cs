public interface ICreatable
{
    string DisplayName { get; }
    string ObjectId { get; }
    bool IsDeleted { get; } // this implies that all object are safe-deleted
    void Delete();
}
