﻿using Microsoft.EntityFrameworkCore;
using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Repositories.Interfaces;

namespace ReMindHealth.Repositories.Implementations;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExtractedAppointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedAppointments
            .Include(a => a.Conversation)
            .FirstOrDefaultAsync(a => a.AppointmentId == id, cancellationToken);
    }

    public async Task<List<ExtractedAppointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedAppointments
            .Include(a => a.Conversation)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExtractedAppointment>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedAppointments
            .Where(a => a.ConversationId == conversationId)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExtractedAppointment>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedAppointments
            .Include(a => a.Conversation)
            .Where(a => a.Conversation.UserId == userId && !a.Conversation.IsDeleted)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExtractedAppointment>> GetUpcomingByUserIdAsync(string userId, int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(days);

        return await _context.ExtractedAppointments
            .Include(a => a.Conversation)
            .Where(a => a.Conversation.UserId == userId
                     && !a.Conversation.IsDeleted
                     && a.AppointmentDateTime >= startDate
                     && a.AppointmentDateTime <= endDate)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExtractedAppointment> AddAsync(ExtractedAppointment entity, CancellationToken cancellationToken = default)
    {
        entity.AppointmentId = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        await _context.ExtractedAppointments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ExtractedAppointment entity, CancellationToken cancellationToken = default)
    {
        _context.ExtractedAppointments.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await GetByIdAsync(id, cancellationToken);
        if (appointment != null)
        {
            _context.ExtractedAppointments.Remove(appointment);
        }
    }
}