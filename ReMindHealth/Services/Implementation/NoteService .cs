using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class NoteService : INoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public NoteService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public Task<ExtractedNote?> GetNoteAsync(Guid noteId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.NoteRepository.GetByIdAsync(noteId, cancellationToken);
    }

    public async Task<List<ExtractedNote>> GetUserNotesAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.NoteRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<List<ExtractedNote>> GetPinnedNotesAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.NoteRepository.GetPinnedByUserIdAsync(userId, cancellationToken);
    }

    public async Task<ExtractedNote> CreateNoteAsync(ExtractedNote note, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.NoteRepository.AddAsync(note, cancellationToken);
        return note;
    }

    public async Task UpdateNoteAsync(ExtractedNote note, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.NoteRepository.UpdateAsync(note, cancellationToken);
    }

    public async Task DeleteNoteAsync(Guid noteId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.NoteRepository.DeleteAsync(noteId, cancellationToken);
    }
}