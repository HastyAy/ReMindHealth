using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public Task<ExtractedTask?> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.TaskRepository.GetByIdAsync(taskId, cancellationToken);
    }

    public async Task<List<ExtractedTask>> GetUserTasksAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.TaskRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<List<ExtractedTask>> GetPendingTasksAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.TaskRepository.GetPendingByUserIdAsync(userId, cancellationToken);
    }

    public async Task<ExtractedTask> CreateTaskAsync(ExtractedTask task, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.TaskRepository.AddAsync(task, cancellationToken);
        return task;
    }

    public async Task UpdateTaskAsync(ExtractedTask task, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.TaskRepository.UpdateAsync(task, cancellationToken);
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.TaskRepository.DeleteAsync(taskId, cancellationToken);
    }
}