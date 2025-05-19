namespace ITSM.Services.Archive;

public interface ISoftDeletableEntity
{
    int Id { get; set; }
    bool IsDeleted { get; set; }
}