using WSB_Management.Models;

namespace WSB_Management.Services;

public class EventService
{
    public event Action? EventsChanged;

    public void NotifyEventsChanged()
    {
        EventsChanged?.Invoke();
    }
}
