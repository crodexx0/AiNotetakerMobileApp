using AiNotetakerApp.Models;
using Plugin.Maui.CalendarStore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AiNotetakerApp.Services
{
  public class CalendarService
  {
    public async Task<string> CreateMeetingEventAsync(Meeting meeting)
    {
      try
      {
        // 1. Request calendar permissions from the user
        var status = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();
        if (status != PermissionStatus.Granted)
        {
          status = await Permissions.RequestAsync<Permissions.CalendarWrite>();
        }

        if (status != PermissionStatus.Granted)
        {
          // Handle denied permission (e.g., show an alert)
          return "Permission denied.";
        }

        // Get the default calendar on the device
        var calendars = await CalendarStore.Default.GetCalendars();
        var defaultCalendar = calendars.FirstOrDefault();

        if (defaultCalendar == null)
        {
          return "No default calendar found.";
        }

        // 3. Format the event details
        var eventId = await CalendarStore.Default.CreateEvent(
          defaultCalendar.Id,
          meeting.Title,
          $"AI SUMMARY:\n{meeting.AiSummary}\n\nACTION ITEMS:\n{meeting.ActionItems}",
          string.Empty,
          meeting.StartTime,
          meeting.StartTime.AddMinutes(30)
        );
        var calendarEvent = new CalendarEvent(eventId, defaultCalendar.Id, meeting.Title);

        // Return the event ID so we can store it in our SQLiteDB
        return calendarEvent.Id;
      } catch (Exception ex)
      {
        Console.WriteLine($"Calendar Sync Error: {ex.Message}");
        return ex.Message;
      }
    }
  }
}