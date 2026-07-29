using Flock.Api;

var store = new FlockStore();

Assert(store.Groups.Count == 3, "Seed creates three relevant groups.");
Assert(store.Events.Count == 2, "Seed creates upcoming events.");
Assert(store.Notifications.Count >= 2, "Important updates create notifications.");
Assert(store.Messages.Count == 2, "Seed creates a group discussion.");

var group = store.Groups.Values.First();
var before = store.Announcements.Count;
var announcement = store.AddAnnouncement(group.Id, store.CurrentMemberId, "Test update", "A clear test message.", false);
Assert(store.Announcements.Count == before + 1, "Publishing persists an announcement.");
Assert(store.Notifications.Values.Any(item => item.Title == announcement.Title), "Publishing creates an in-app notification.");

var eventWithRoles = store.Events.Values.Single(item => item.Positions.Count > 0);
var role = eventWithRoles.Positions.First();
Assert(store.Signup(eventWithRoles.Id, store.CurrentMemberId, role.Id), "A member can claim an available volunteer role.");
Assert(store.Events[eventWithRoles.Id].Positions.First(item => item.Id == role.Id).MemberIds.Contains(store.CurrentMemberId), "Volunteer signup is attached to the event.");
Assert(!store.Signup(Guid.NewGuid(), store.CurrentMemberId, role.Id), "A missing event cannot be signed up for.");

Console.WriteLine("All Flock domain smoke tests passed.");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException($"FAILED: {message}");
    Console.WriteLine($"PASS: {message}");
}
