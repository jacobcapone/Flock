# Database

The first persistence migration should cover Church, Member, Group,
GroupMembership, Announcement, Event, RSVP, VolunteerPosition,
VolunteerSignup, Notification, UserSettings, and AuditLog.

Use UUID primary keys, tenant-scoped indexes, foreign keys, UTC timestamps, and
soft deletion for member-created records. See `docs/architecture/0001-mvp.md`.

