# TaskFlow — Entity-Relationship Diagram

This diagram is written in [Mermaid](https://mermaid.js.org/) syntax and renders
automatically in GitHub's Markdown viewer, VS Code (with the Mermaid extension),
and most modern Markdown tools.

```mermaid
erDiagram
    DEPARTMENT ||--o{ USER : "has members"
    DEPARTMENT ||--o{ TEAM : "has teams"

    TEAM ||--o{ TEAM_MEMBER : "has members"
    USER ||--o{ TEAM_MEMBER : "belongs to teams"
    USER ||--o{ TEAM : "leads"

    TEAM ||--o{ PROJECT : "assigned to"
    USER ||--o{ PROJECT : "owns"
    CATEGORY ||--o{ PROJECT : "classifies"

    PROJECT ||--o{ PROJECT_MEMBER : "has members"
    USER ||--o{ PROJECT_MEMBER : "joins projects"

    PROJECT ||--o{ TASK_ITEM : "contains"
    TASK_ITEM ||--o{ TASK_ITEM : "has subtasks"
    USER ||--o{ TASK_ITEM : "creates"

    TASK_ITEM ||--o{ TASK_ASSIGNMENT : "assigned via"
    USER ||--o{ TASK_ASSIGNMENT : "assigned to"

    TASK_ITEM ||--o{ CHECKLIST_ITEM : "has checklist"

    PROJECT ||--o{ LABEL : "defines"
    TASK_ITEM ||--o{ TASK_LABEL : "tagged with"
    LABEL ||--o{ TASK_LABEL : "applied to"

    TASK_ITEM ||--o{ COMMENT : "has comments"
    COMMENT ||--o{ COMMENT : "has replies"
    USER ||--o{ COMMENT : "writes"

    PROJECT ||--o{ ATTACHMENT : "has files"
    TASK_ITEM ||--o{ ATTACHMENT : "has files"
    USER ||--o{ ATTACHMENT : "uploads"

    PROJECT ||--o{ ACTIVITY_LOG : "history"
    TASK_ITEM ||--o{ ACTIVITY_LOG : "history"
    USER ||--o{ ACTIVITY_LOG : "performs"

    USER ||--o{ NOTIFICATION : "receives"
    TASK_ITEM ||--o{ NOTIFICATION : "relates to"
    PROJECT ||--o{ NOTIFICATION : "relates to"

    TEAM ||--o{ USER_INVITE : "invites into"
    USER ||--o{ USER_INVITE : "sends"

    USER ||--o{ ROLE : "has (via UserRoles)"

    DEPARTMENT {
        int Id PK
        string Name
        string Description
        bool IsActive
    }

    USER {
        string Id PK
        string FirstName
        string LastName
        string Email
        string JobTitle
        bool IsActive
        int DepartmentId FK
    }

    ROLE {
        string Id PK
        string Name
    }

    TEAM {
        int Id PK
        string Name
        int DepartmentId FK
        string TeamLeadId FK
    }

    TEAM_MEMBER {
        int Id PK
        int TeamId FK
        string UserId FK
        bool IsTeamLead
    }

    USER_INVITE {
        int Id PK
        string Email
        string Token
        int TeamId FK
        string InvitedByUserId FK
        bool IsAccepted
    }

    CATEGORY {
        int Id PK
        string Name
        string ColorHex
    }

    PROJECT {
        int Id PK
        string Name
        string Status
        date StartDate
        date EndDate
        bool IsArchived
        bool IsDeleted
        int CategoryId FK
        string OwnerId FK
        int TeamId FK
    }

    PROJECT_MEMBER {
        int Id PK
        int ProjectId FK
        string UserId FK
        bool IsProjectManager
    }

    TASK_ITEM {
        int Id PK
        string Title
        string Status
        string Priority
        date DueDate
        int BoardOrder
        int ProjectId FK
        int ParentTaskId FK
        string CreatedByUserId FK
        bool IsDeleted
    }

    TASK_ASSIGNMENT {
        int Id PK
        int TaskItemId FK
        string UserId FK
        string AssignedByUserId FK
    }

    CHECKLIST_ITEM {
        int Id PK
        string Text
        bool IsCompleted
        int SortOrder
        int TaskItemId FK
    }

    LABEL {
        int Id PK
        string Name
        string ColorHex
        int ProjectId FK
    }

    TASK_LABEL {
        int TaskItemId FK
        int LabelId FK
    }

    COMMENT {
        int Id PK
        string Content
        int TaskItemId FK
        string UserId FK
        int ParentCommentId FK
        bool IsEdited
    }

    ATTACHMENT {
        int Id PK
        string FileName
        string FilePath
        long FileSizeBytes
        string RelatedTo
        int ProjectId FK
        int TaskItemId FK
        string UploadedByUserId FK
    }

    ACTIVITY_LOG {
        int Id PK
        string ActionType
        string Description
        string RelatedTo
        int ProjectId FK
        int TaskItemId FK
        string UserId FK
    }

    NOTIFICATION {
        int Id PK
        string Type
        string Title
        bool IsRead
        string UserId FK
        int RelatedTaskItemId FK
        int RelatedProjectId FK
    }
```

## Notes on modeling decisions

- **Attachment** and **ActivityLog** relate to *either* a Project or a TaskItem
  via two nullable foreign keys (`ProjectId`, `TaskItemId`) rather than true
  polymorphic association. This keeps referential integrity enforceable by
  the database itself, at the cost of one always-null column per row —
  a standard, pragmatic trade-off in relational modeling.
- **TaskItem** self-references via `ParentTaskId` to support subtasks.
- **Comment** self-references via `ParentCommentId` to support reply threads.
- Join tables (`TeamMember`, `ProjectMember`, `TaskAssignment`) are modeled
  as first-class entities (not implicit EF many-to-many) because each one
  carries extra metadata (joined-at date, manager flag, who assigned whom).
- `TaskLabel` is a pure join table with a composite primary key since it
  carries no metadata of its own.
- Not shown for diagram clarity: `SystemLog`, `EmailLog`, `SystemSetting` —
  these are administrative/logging tables with no meaningful FK relationships
  to the core domain graph above.
