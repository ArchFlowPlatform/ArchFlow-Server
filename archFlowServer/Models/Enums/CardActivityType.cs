namespace archFlowServer.Models.Enums;

public enum CardActivityType
{
    Created,
    Updated,
    MovedColumn,
    Reordered,
    Assigned,
    Unassigned,
    LabelAdded,
    LabelRemoved,
    CommentAdded,
    CommentEdited,
    CommentDeleted,
    AttachmentAdded,
    AttachmentRemoved,
    DueDateChanged,
    PriorityChanged,
    Archived,
    Restored
}