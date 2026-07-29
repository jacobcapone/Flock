using Flock.Api;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var developmentWebRoot = Path.GetFullPath("../../apps/web-admin", builder.Environment.ContentRootPath);
if (Directory.Exists(developmentWebRoot))
    builder.WebHost.UseWebRoot(developmentWebRoot);
builder.Services.AddSingleton<FlockStore>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapPost("/auth/login", (LoginRequest request, FlockStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["credentials"] = ["Enter a valid email and a password of at least 6 characters."] });
    var member = store.Members.Values.FirstOrDefault(item => item.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));
    return member is null ? Results.Unauthorized() : Results.Ok(new { token = "development-token", member, church = store.Church });
});

api.MapGet("/me", (FlockStore store) => Results.Ok(store.Members[store.CurrentMemberId]));
api.MapGet("/dashboard", (FlockStore store) => Results.Ok(new
{
    church = store.Church,
    memberCount = store.Members.Count,
    activeGroups = store.Groups.Count,
    upcomingEvents = store.Events.Values.Count(item => item.StartsAt > DateTimeOffset.UtcNow),
    pendingInvitations = 3
}));

api.MapGet("/groups", (FlockStore store) => Results.Ok(store.Groups.Values.OrderBy(item => item.Name)));
api.MapPost("/groups", (CreateGroupRequest request, FlockStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Name)) return Results.BadRequest(new { error = "Group name is required." });
    var item = new Group(Guid.NewGuid(), store.Church.Id, request.Name.Trim(), request.Description.Trim(), request.Icon, request.DiscussionEnabled, 1);
    store.Groups[item.Id] = item;
    return Results.Created($"/api/groups/{item.Id}", item);
});

api.MapGet("/announcements", (Guid? groupId, FlockStore store) =>
    Results.Ok(store.Announcements.Values
        .Where(item => groupId is null || item.GroupId == groupId)
        .OrderByDescending(item => item.IsPinned).ThenByDescending(item => item.CreatedAt)));
api.MapPost("/announcements", (CreateAnnouncementRequest request, FlockStore store) =>
{
    if (!store.Groups.ContainsKey(request.GroupId)) return Results.NotFound(new { error = "Group was not found." });
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) return Results.BadRequest(new { error = "Title and message are required." });
    var item = store.AddAnnouncement(request.GroupId, store.CurrentMemberId, request.Title, request.Body, request.IsPinned);
    return Results.Created($"/api/announcements/{item.Id}", item);
});
api.MapGet("/groups/{groupId:guid}/messages", (Guid groupId, FlockStore store) =>
    store.Groups.TryGetValue(groupId, out var group) && group.DiscussionEnabled
        ? Results.Ok(store.Messages.Values.Where(item => item.GroupId == groupId).OrderBy(item => item.CreatedAt))
        : Results.NotFound());
api.MapPost("/groups/{groupId:guid}/messages", (Guid groupId, CreateMessageRequest request, FlockStore store) =>
{
    if (!store.Groups.TryGetValue(groupId, out var group) || !group.DiscussionEnabled) return Results.NotFound();
    if (string.IsNullOrWhiteSpace(request.Body)) return Results.BadRequest(new { error = "Message cannot be empty." });
    return Results.Created($"/api/groups/{groupId}/messages", store.AddMessage(groupId, store.CurrentMemberId, request.Body));
});

api.MapGet("/events", (FlockStore store) => Results.Ok(store.Events.Values.OrderBy(item => item.StartsAt)));
api.MapPost("/events", (CreateEventRequest request, FlockStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || request.StartsAt <= DateTimeOffset.UtcNow) return Results.BadRequest(new { error = "A title and future start date are required." });
    var item = store.AddEvent(request);
    return Results.Created($"/api/events/{item.Id}", item);
});
api.MapPost("/events/{eventId:guid}/rsvp", (Guid eventId, RsvpRequest request, FlockStore store) =>
{
    if (!store.Events.ContainsKey(eventId)) return Results.NotFound();
    store.Rsvps[(eventId, store.CurrentMemberId)] = request.Status;
    return Results.Ok(new { eventId, memberId = store.CurrentMemberId, request.Status });
});
api.MapPost("/events/{eventId:guid}/volunteer", (Guid eventId, SignupRequest request, FlockStore store) =>
    store.Signup(eventId, store.CurrentMemberId, request.PositionId) ? Results.Ok() : Results.Conflict(new { error = "This role is unavailable." }));

api.MapGet("/notifications", (FlockStore store) => Results.Ok(store.Notifications.Values.OrderByDescending(item => item.CreatedAt)));
api.MapPost("/notifications/{id:guid}/read", (Guid id, FlockStore store) =>
{
    if (!store.Notifications.TryGetValue(id, out var item)) return Results.NotFound();
    store.Notifications[id] = item with { IsRead = true };
    return Results.NoContent();
});

app.MapFallbackToFile("index.html");
app.Run();

public partial class Program;
