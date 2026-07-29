namespace Flock.Api;

public enum MemberRole { Administrator, GroupLeader, Member }
public enum RsvpStatus { Going, Maybe, NotGoing }

public sealed record Church(Guid Id, string Name, string City, string TimeZone);
public sealed record Member(Guid Id, Guid ChurchId, string Name, string Email, string Initials, MemberRole Role);
public sealed record Group(Guid Id, Guid ChurchId, string Name, string Description, string Icon, bool DiscussionEnabled, int MemberCount);
public sealed record Announcement(Guid Id, Guid GroupId, Guid AuthorId, string Title, string Body, DateTimeOffset CreatedAt, bool IsPinned);
public sealed record Message(Guid Id, Guid GroupId, Guid AuthorId, string Body, DateTimeOffset CreatedAt);
public sealed record VolunteerPosition(Guid Id, string Name, int Capacity, IReadOnlyList<Guid> MemberIds);
public sealed record ChurchEvent(Guid Id, Guid ChurchId, Guid? GroupId, string Title, string Description, DateTimeOffset StartsAt, string Location, IReadOnlyList<VolunteerPosition> Positions);
public sealed record Notification(Guid Id, Guid MemberId, string Title, string Body, DateTimeOffset CreatedAt, bool IsRead);

public sealed record LoginRequest(string Email, string Password);
public sealed record CreateGroupRequest(string Name, string Description, string Icon, bool DiscussionEnabled);
public sealed record CreateAnnouncementRequest(Guid GroupId, string Title, string Body, bool IsPinned);
public sealed record CreateMessageRequest(string Body);
public sealed record CreateEventRequest(Guid? GroupId, string Title, string Description, DateTimeOffset StartsAt, string Location);
public sealed record RsvpRequest(RsvpStatus Status);
public sealed record SignupRequest(Guid PositionId);
