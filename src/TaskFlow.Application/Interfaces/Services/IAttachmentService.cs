using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Interfaces.Services;

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
