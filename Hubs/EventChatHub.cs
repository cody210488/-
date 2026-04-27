using Microsoft.AspNetCore.SignalR;

namespace 打球啊.Hubs
{
    public class EventChatHub : Hub
    {
        public async Task JoinEventGroup(string eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Event-{eventId}");
        }

        public async Task LeaveEventGroup(string eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Event-{eventId}");
        }
    }
}