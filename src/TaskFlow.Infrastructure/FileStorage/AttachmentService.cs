using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.FileStorage;

public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly string _uploadsRootPath;

    private static readonly string[] DefaultAllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".xlsx", ".zip" };

    private const long DefaultMaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public AttachmentService(IUnitOfWork uow, IMapper mapper, string uploadsRootPath)
    {
        _uow = uow;
        _mapper = mapper;
        _uploadsRootPath = uploadsRootPath;
    }

    public async Task<ServiceResult<AttachmentDto>> UploadAsync(
        IFormFile file,
        EntityRelationType relatedTo,
        int relatedId,
        string uploadedByUserId)
    {
        if (file.Length == 0)
        {
            return ServiceResult<AttachmentDto>.Failure("The uploaded file is empty.");
        }

        var maxSizeSetting = await _uow.SystemSettings.GetValueAsync("Uploads.MaxFileSizeMb");
        var maxSizeBytes = long.TryParse(maxSizeSetting, out var mb) ? mb * 1024 * 1024 : DefaultMaxFileSizeBytes;

        if (file.Length > maxSizeBytes)
        {
            return ServiceResult<AttachmentDto>.Failure($"File exceeds the maximum allowed size of {maxSizeBytes / (1024 * 1024)} MB.");
        }

        var allowedExtensionsSetting = await _uow.SystemSettings.GetValueAsync("Uploads.AllowedExtensions");
        var allowedExtensions = allowedExtensionsSetting?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? DefaultAllowedExtensions;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return ServiceResult<AttachmentDto>.Failure($"File type \"{extension}\" is not allowed.");
        }

        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var subFolder = relatedTo == EntityRelationType.Project ? "projects" : "tasks";
        var targetDirectory = Path.Combine(_uploadsRootPath, subFolder, relatedId.ToString());

        Directory.CreateDirectory(targetDirectory);

        var fullPath = Path.Combine(targetDirectory, safeFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = Path.Combine("uploads", subFolder, relatedId.ToString(), safeFileName)
            .Replace('\\', '/');

        var attachment = new Attachment
        {
            FileName = Path.GetFileName(file.FileName), // original name for display only
            FilePath = relativePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            RelatedTo = relatedTo,
            ProjectId = relatedTo == EntityRelationType.Project ? relatedId : null,
            TaskItemId = relatedTo == EntityRelationType.TaskItem ? relatedId : null,
            UploadedByUserId = uploadedByUserId
        };

        await _uow.Attachments.AddAsync(attachment);

        if (relatedTo == EntityRelationType.TaskItem)
        {
            await _uow.ActivityLogs.AddAsync(new ActivityLog
            {
                TaskItemId = relatedId,
                RelatedTo = EntityRelationType.TaskItem,
                UserId = uploadedByUserId,
                ActionType = ActivityActionType.AttachmentAdded,
                Description = $"attached the file \"{attachment.FileName}\"."
            });
        }

        await _uow.SaveChangesAsync();

        var dto = _mapper.Map<AttachmentDto>(attachment);
        return ServiceResult<AttachmentDto>.Success(dto);
    }

    public async Task<ServiceResult> DeleteAsync(int attachmentId, string requestingUserId)
    {
        var attachment = await _uow.Attachments.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            return ServiceResult.Failure("Attachment not found.");
        }

        // Physical file deletion - resolve full path and remove if present.
        var physicalPath = Path.Combine(_uploadsRootPath, "..", attachment.FilePath);
        var normalizedPath = Path.GetFullPath(physicalPath);

        var uploadsRootFull = Path.GetFullPath(Path.Combine(_uploadsRootPath, ".."));
        if (normalizedPath.StartsWith(uploadsRootFull, StringComparison.OrdinalIgnoreCase) && File.Exists(normalizedPath))
        {
            File.Delete(normalizedPath);
        }

        _uow.Attachments.Remove(attachment);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetForTaskAsync(int taskId)
    {
        var attachments = await _uow.Attachments.Query()
            .Include(a => a.UploadedByUser)
            .Where(a => a.TaskItemId == taskId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<AttachmentDto>>(attachments);
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetForProjectAsync(int projectId)
    {
        var attachments = await _uow.Attachments.Query()
            .Include(a => a.UploadedByUser)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<AttachmentDto>>(attachments);
    }
}
