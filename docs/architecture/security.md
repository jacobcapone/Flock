# MVP security boundary

The current login route is a visibly development-only seam. It does not issue a
real JWT or validate a stored password and must never be exposed publicly.

Private beta requires:

1. ASP.NET Core Identity-compatible password hashing and lockout.
2. Short-lived JWT access tokens with rotating refresh tokens.
3. Google and Apple authorization-code flows with PKCE.
4. Church and group membership authorization on every scoped route.
5. HTTPS, secret storage, audit persistence, rate limiting, and request limits.
6. Account deletion, data export, and notification-consent workflows.

Authorization policies should express Administrator, GroupLeader, and Member
capabilities explicitly. Never accept church or member identity from the client
when it can be derived from the authenticated principal.

