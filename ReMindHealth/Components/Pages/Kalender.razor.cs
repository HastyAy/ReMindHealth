using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;
using System.Globalization;

namespace ReMindHealth.Components.Pages
{
    public partial class Kalender
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private ApplicationDbContext Context { get; set; } = default!;
        [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

        private bool isLoading = true;
        private List<ExtractedAppointment> termine = new();
        private ExtractedAppointment? selectedTermin;

        protected override async Task OnInitializedAsync()
        {
            await LoadAppointments();
        }

        private async Task LoadAppointments()
        {
            try
            {
                isLoading = true;
                var userId = await CurrentUserService.GetUserIdAsync();

                // Load all appointments for this user, ordered by date
                termine = await Context.ExtractedAppointments
                    .Include(a => a.Conversation)
                    .Where(a => a.Conversation.UserId == userId && !a.Conversation.IsDeleted)
                    .OrderBy(a => a.AppointmentDateTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading appointments: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private void SelectTermin(ExtractedAppointment termin)
        {
            selectedTermin = termin;
        }

        private void NavigateTo(string url)
        {
            NavigationManager.NavigateTo(url);
        }
    }
}