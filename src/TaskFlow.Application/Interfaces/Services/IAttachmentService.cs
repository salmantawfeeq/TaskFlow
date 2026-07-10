using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Interfaces.Services;

/// <summary>
/// Handles secure file upload/storage for task and project attachments.
/// Takes IFormFile (an ASP.NET Core web abstraction) - a deliberate,
/// narrow exception, since representing "an uploaded file" any other way
/// in the Application layer would just reinvent the same shape.
/// </summary>
public interface IAttachmentService
{
    Task<ServiceResult<AttachmentDto>> UploadAsync(
        IFormFile file,
        EntityRelationType relatedTo,
        int relatedId,
        string uploadedByUserId);

    Task<ServiceResult> DeleteAsync(int attachmentId, string requestingUserId);

    Task<IReadOnlyList<AttachmentDto>> GetForTaskAsync(int taskId);

    Task<IReadOnlyList<AttachmentDto>> GetForProjectAsync(int projectId);
}
