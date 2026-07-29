# API

All routes use JSON and live below `/api`.

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/auth/login` | Development login |
| GET | `/me` | Current member |
| GET | `/dashboard` | Church summary |
| GET/POST | `/groups` | List or create groups |
| GET/POST | `/groups/{id}/messages` | Read or send discussion messages |
| GET/POST | `/announcements` | List or publish announcements |
| GET/POST | `/events` | List or create events |
| POST | `/events/{id}/rsvp` | Set the current member RSVP |
| POST | `/events/{id}/volunteer` | Claim a volunteer position |
| GET | `/notifications` | Current member notifications |

Error responses use appropriate HTTP status codes. Validation is enforced at the
API boundary; production authentication and authorization are beta gates.
