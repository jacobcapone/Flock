using System.Collections.Concurrent;

namespace Flock.Api;

/// <summary>Development repository for the complete MVP workflow.</summary>
public sealed class FlockStore
{
    private readonly object gate = new();
    public Church Church { get; } = new(Guid.Parse("59d1d962-6835-4bd4-b243-94307749d516"), "Grace Community Church", "Austin, TX", "America/Chicago");
    public ConcurrentDictionary<Guid, Member> Members { get; } = new();
    public ConcurrentDictionary<Guid, Group> Groups { get; } = new();
    public ConcurrentDictionary<Guid, Announcement> Announcements { get; } = new();
    public ConcurrentDictionary<Guid, Message> Messages { get; } = new();
    public ConcurrentDictionary<Guid, ChurchEvent> Events { get; } = new();
    public ConcurrentDictionary<(Guid EventId, Guid MemberId), RsvpStatus> Rsvps { get; } = new();
    public ConcurrentDictionary<Guid, Notification> Notifications { get; } = new();

    public Guid CurrentMemberId { get; } = Guid.Parse("e03c5896-aa0a-426f-8dba-dba241050d7a");

    public FlockStore()
    {
        var jordan = new Member(CurrentMemberId, Church.Id, "Jordan Miller", "jordan@gracecommunity.org", "JM", MemberRole.Administrator);
        var sam = new Member(Guid.Parse("6d7f321f-525a-4036-b356-c29ea3fb846d"), Church.Id, "Sam Rivera", "sam@gracecommunity.org", "SR", MemberRole.GroupLeader);
        Members[jordan.Id] = jordan;
        Members[sam.Id] = sam;

        var all = new Group(Guid.Parse("a1111111-1111-4111-8111-111111111111"), Church.Id, "Church-wide", "News and updates for everyone at Grace.", "church", false, 486);
        var worship = new Group(Guid.Parse("a2222222-2222-4222-8222-222222222222"), Church.Id, "Worship Team", "Schedules, set lists, and team conversation.", "music", true, 28);
        var youngAdults = new Group(Guid.Parse("a3333333-3333-4333-8333-333333333333"), Church.Id, "Young Adults", "Community for adults in their 20s and 30s.", "people", true, 64);
        Groups[all.Id] = all; Groups[worship.Id] = worship; Groups[youngAdults.Id] = youngAdults;

        AddAnnouncement(all.Id, sam.Id, "Summer Serve Day", "Join us Saturday as we serve three neighborhood schools. Sign up for a team by Wednesday.", true, DateTimeOffset.UtcNow.AddHours(-2));
        AddAnnouncement(worship.Id, sam.Id, "Sunday set list is ready", "Please review the arrangements before Thursday rehearsal. Keys and charts are attached in Planning Center.", false, DateTimeOffset.UtcNow.AddDays(-1));
        AddAnnouncement(youngAdults.Id, jordan.Id, "Dinner location update", "We moved Friday dinner to the park pavilion. Bring a lawn chair!", false, DateTimeOffset.UtcNow.AddDays(-2));
        AddMessage(worship.Id, sam.Id, "Looking forward to rehearsal—please listen through the new closing song.");
        AddMessage(worship.Id, jordan.Id, "Thanks, Sam. I’ll be there a few minutes early.");

        var sunday = new ChurchEvent(Guid.Parse("b1111111-1111-4111-8111-111111111111"), Church.Id, null, "Sunday Gathering", "Worship, teaching, and community.", Next(DayOfWeek.Sunday, 10), "Main Auditorium",
        [
            new VolunteerPosition(Guid.Parse("c1111111-1111-4111-8111-111111111111"), "Welcome Team", 6, []),
            new VolunteerPosition(Guid.Parse("c2222222-2222-4222-8222-222222222222"), "Kids Check-in", 4, [])
        ]);
        var rehearsal = new ChurchEvent(Guid.Parse("b2222222-2222-4222-8222-222222222222"), Church.Id, worship.Id, "Worship Rehearsal", "Full band rehearsal for Sunday.", Next(DayOfWeek.Thursday, 18), "Worship Center", []);
        Events[sunday.Id] = sunday; Events[rehearsal.Id] = rehearsal;

        AddNotification("Welcome to Flock", "Your church communication is now all in one place.");
        AddNotification("Sunday Gathering", "Two volunteer roles still need help this Sunday.");
    }

    public Announcement AddAnnouncement(Guid groupId, Guid authorId, string title, string body, bool pinned, DateTimeOffset? createdAt = null)
    {
        var item = new Announcement(Guid.NewGuid(), groupId, authorId, title.Trim(), body.Trim(), createdAt ?? DateTimeOffset.UtcNow, pinned);
        Announcements[item.Id] = item;
        AddNotification(title, body);
        return item;
    }

    public ChurchEvent AddEvent(CreateEventRequest request)
    {
        var item = new ChurchEvent(Guid.NewGuid(), Church.Id, request.GroupId, request.Title.Trim(), request.Description.Trim(), request.StartsAt, request.Location.Trim(), []);
        Events[item.Id] = item;
        AddNotification(request.Title, $"New event at {request.StartsAt:MMM d, h:mm tt}.");
        return item;
    }

    public Message AddMessage(Guid groupId, Guid authorId, string body)
    {
        var item = new Message(Guid.NewGuid(), groupId, authorId, body.Trim(), DateTimeOffset.UtcNow);
        Messages[item.Id] = item;
        return item;
    }

    public bool Signup(Guid eventId, Guid memberId, Guid positionId)
    {
        lock (gate)
        {
            if (!Events.TryGetValue(eventId, out var churchEvent)) return false;
            var position = churchEvent.Positions.FirstOrDefault(item => item.Id == positionId);
            if (position is null || position.MemberIds.Count >= position.Capacity) return false;
            if (position.MemberIds.Contains(memberId)) return true;
            var updated = position with { MemberIds = position.MemberIds.Append(memberId).ToArray() };
            Events[eventId] = churchEvent with { Positions = churchEvent.Positions.Select(item => item.Id == positionId ? updated : item).ToArray() };
            return true;
        }
    }

    private void AddNotification(string title, string body)
    {
        var item = new Notification(Guid.NewGuid(), CurrentMemberId, title, body, DateTimeOffset.UtcNow, false);
        Notifications[item.Id] = item;
    }

    private static DateTimeOffset Next(DayOfWeek day, int hour)
    {
        var date = DateTime.Today.AddDays(((int)day - (int)DateTime.Today.DayOfWeek + 7) % 7);
        if (date == DateTime.Today && DateTime.Now.Hour >= hour) date = date.AddDays(7);
        return new DateTimeOffset(date.AddHours(hour));
    }
}
