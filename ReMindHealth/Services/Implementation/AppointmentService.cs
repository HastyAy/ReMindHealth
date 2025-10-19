using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public Task<ExtractedAppointment?> GetAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.AppointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
    }

    public async Task<List<ExtractedAppointment>> GetUserAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.AppointmentRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<List<ExtractedAppointment>> GetUpcomingAppointmentsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.AppointmentRepository.GetUpcomingByUserIdAsync(userId, days, cancellationToken);
    }

    public async Task<ExtractedAppointment> CreateAppointmentAsync(ExtractedAppointment appointment, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.AppointmentRepository.AddAsync(appointment, cancellationToken);
        return appointment;
    }

    public async Task UpdateAppointmentAsync(ExtractedAppointment appointment, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.AppointmentRepository.UpdateAsync(appointment, cancellationToken);
    }

    public async Task DeleteAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.AppointmentRepository.DeleteAsync(appointmentId, cancellationToken);
    }
}