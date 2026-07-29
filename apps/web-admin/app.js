const state = { groups: [], announcements: [], events: [], notifications: [], dashboard: null };
const symbols = { church: "⌂", music: "♫", people: "○", default: "◇" };

async function api(path, options = {}) {
  const response = await fetch(`/api${path}`, { headers: { "Content-Type": "application/json" }, ...options });
  if (!response.ok) {
    const error = await response.json().catch(() => ({ error: "Something went wrong." }));
    throw new Error(error.error || error.title || "Something went wrong.");
  }
  return response.status === 204 ? null : response.json();
}

async function load() {
  try {
    [state.groups, state.announcements, state.events, state.notifications, state.dashboard] = await Promise.all([
      api("/groups"), api("/announcements"), api("/events"), api("/notifications"), api("/dashboard")
    ]);
    render();
  } catch (error) {
    toast("The Flock API is not available.");
  }
}

function render() {
  const byGroup = Object.fromEntries(state.groups.map(group => [group.id, group]));
  document.querySelector("#announcement-list").innerHTML = state.announcements.slice(0, 6).map(item => {
    const group = byGroup[item.groupId] || { name: "Church", icon: "default" };
    return `<article class="announcement-card">
      <div class="card-top"><span class="group-chip"><span class="group-icon">${symbols[group.icon] || symbols.default}</span>${escapeHtml(group.name)}</span>${item.isPinned ? '<span class="pin">◆ Pinned</span>' : ""}</div>
      <h3>${escapeHtml(item.title)}</h3><p>${escapeHtml(item.body)}</p>
      <span class="card-meta">${relativeTime(item.createdAt)}</span>
    </article>`;
  }).join("");
  document.querySelector("#group-grid").innerHTML = state.groups.map(group => `<article class="group-card">
    <span class="group-icon">${symbols[group.icon] || symbols.default}</span><h3>${escapeHtml(group.name)}</h3>
    <p>${escapeHtml(group.description)}</p><span class="member-count">${group.memberCount} members ${group.discussionEnabled ? "· Discussion on" : "· Announcements only"}</span>
    ${group.discussionEnabled ? `<button class="text-button" onclick="openDiscussion('${group.id}')">Open discussion →</button>` : ""}
  </article>`).join("");
  document.querySelector("#announcement-group").innerHTML = state.groups.map(group => `<option value="${group.id}">${escapeHtml(group.name)}</option>`).join("");
  document.querySelector("#event-list").innerHTML = state.events.map(event => {
    const date = new Date(event.startsAt);
    const roles = event.positions?.filter(position => position.memberIds.length < position.capacity) || [];
    return `<article class="event-card">
      <div class="date-block"><span>${date.toLocaleDateString([], { month: "short" }).toUpperCase()}</span><strong>${date.getDate()}</strong></div>
      <div><h3>${escapeHtml(event.title)}</h3><p>${date.toLocaleTimeString([], { hour: "numeric", minute: "2-digit" })} · ${escapeHtml(event.location || "Location coming soon")}${roles.length ? ` · ${roles.length} volunteer roles open` : ""}</p></div>
      <div class="event-actions"><button class="secondary-button" onclick="rsvp('${event.id}')">RSVP</button>${roles[0] ? `<button class="primary-button" onclick="volunteer('${event.id}','${roles[0].id}')">Volunteer</button>` : ""}</div>
    </article>`;
  }).join("");
  const metrics = [
    ["Members", state.dashboard.memberCount], ["Active groups", state.dashboard.activeGroups],
    ["Upcoming events", state.dashboard.upcomingEvents], ["Pending invites", state.dashboard.pendingInvitations]
  ];
  document.querySelector("#metric-grid").innerHTML = metrics.map(([label, value]) => `<article class="metric"><span class="eyebrow">${label}</span><strong>${value}</strong><small>Grace Community Church</small></article>`).join("");
  document.querySelector("#notification-list").innerHTML = state.notifications.length ? state.notifications.map(item => `<article class="notification ${item.isRead ? "" : "unread"}"><strong>${escapeHtml(item.title)}</strong><p>${escapeHtml(item.body)}</p></article>`).join("") : "<p>You’re all caught up.</p>";
  document.querySelector(".unread-dot").hidden = !state.notifications.some(item => !item.isRead);
}

function setView(name) {
  document.querySelectorAll(".view").forEach(view => view.classList.toggle("active", view.id === `${name}-view`));
  document.querySelectorAll("[data-view]").forEach(button => button.classList.toggle("active", button.dataset.view === name));
  const titles = { today: "Here’s what’s happening", groups: "Find your people", calendar: "Make room for community", admin: "Help your church connect" };
  document.querySelector("#page-title").textContent = titles[name];
  window.scrollTo({ top: 0, behavior: "smooth" });
}

document.querySelectorAll("[data-view]").forEach(button => button.addEventListener("click", () => setView(button.dataset.view)));
document.querySelectorAll("[data-view-link]").forEach(button => button.addEventListener("click", () => setView(button.dataset.viewLink)));
document.querySelector("#compose-button").addEventListener("click", () => document.querySelector("#composer").showModal());
document.querySelector("#create-group-button").addEventListener("click", () => document.querySelector("#group-dialog").showModal());
document.querySelector("#create-event-button").addEventListener("click", () => document.querySelector("#event-dialog").showModal());
document.querySelector("#notification-button").addEventListener("click", () => document.querySelector("#notifications-dialog").showModal());
document.querySelector("#invite-button").addEventListener("click", () => toast("Member invitations are queued for the beta-ready milestone."));

document.querySelector("#announcement-form").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.currentTarget);
  try {
    const item = await api("/announcements", { method: "POST", body: JSON.stringify({ groupId: form.get("groupId"), title: form.get("title"), body: form.get("body"), isPinned: form.get("isPinned") === "on" }) });
    state.announcements.unshift(item); event.currentTarget.reset(); document.querySelector("#composer").close(); render(); toast("Announcement published.");
  } catch (error) { toast(error.message); }
});

document.querySelector("#group-form").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.currentTarget);
  try {
    const item = await api("/groups", { method: "POST", body: JSON.stringify({ name: form.get("name"), description: form.get("description"), icon: "default", discussionEnabled: form.get("discussionEnabled") === "on" }) });
    state.groups.push(item); event.currentTarget.reset(); document.querySelector("#group-dialog").close(); render(); toast("Group created.");
  } catch (error) { toast(error.message); }
});

document.querySelector("#event-form").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.currentTarget);
  try {
    const item = await api("/events", { method: "POST", body: JSON.stringify({ groupId: null, title: form.get("title"), description: form.get("description"), startsAt: new Date(form.get("startsAt")).toISOString(), location: form.get("location") }) });
    state.events.push(item); state.events.sort((a, b) => new Date(a.startsAt) - new Date(b.startsAt)); event.currentTarget.reset(); document.querySelector("#event-dialog").close(); render(); toast("Event added.");
  } catch (error) { toast(error.message); }
});

async function rsvp(eventId) {
  try { await api(`/events/${eventId}/rsvp`, { method: "POST", body: JSON.stringify({ status: "Going" }) }); toast("You’re going!"); }
  catch (error) { toast(error.message); }
}
async function volunteer(eventId, positionId) {
  try { await api(`/events/${eventId}/volunteer`, { method: "POST", body: JSON.stringify({ positionId }) }); state.events = await api("/events"); render(); toast("Thanks for volunteering!"); }
  catch (error) { toast(error.message); }
}
async function openDiscussion(groupId) {
  const group = state.groups.find(item => item.id === groupId);
  try {
    const messages = await api(`/groups/${groupId}/messages`);
    document.querySelector("#discussion-title").textContent = group.name;
    document.querySelector("#message-form [name=groupId]").value = groupId;
    document.querySelector("#message-list").innerHTML = messages.map(item => `<article class="message ${item.authorId === "e03c5896-aa0a-426f-8dba-dba241050d7a" ? "mine" : ""}"><strong>${item.authorId === "e03c5896-aa0a-426f-8dba-dba241050d7a" ? "You" : "Sam Rivera"}</strong><p>${escapeHtml(item.body)}</p></article>`).join("") || "<p>Start the conversation.</p>";
    const dialog = document.querySelector("#discussion-dialog");
    if (!dialog.open) dialog.showModal();
  } catch (error) { toast(error.message); }
}
document.querySelector("#message-form").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.currentTarget);
  try {
    await api(`/groups/${form.get("groupId")}/messages`, { method: "POST", body: JSON.stringify({ body: form.get("body") }) });
    event.currentTarget.elements.body.value = "";
    await openDiscussion(form.get("groupId"));
  } catch (error) { toast(error.message); }
});
function toast(message) {
  const node = document.querySelector("#toast"); node.textContent = message; node.classList.add("show");
  clearTimeout(window.toastTimer); window.toastTimer = setTimeout(() => node.classList.remove("show"), 2800);
}
function relativeTime(value) {
  const hours = Math.round((Date.now() - new Date(value)) / 3600000);
  if (hours < 1) return "Just now"; if (hours < 24) return `${hours}h ago`; return `${Math.round(hours / 24)}d ago`;
}
function escapeHtml(value = "") {
  return String(value).replace(/[&<>"']/g, character => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" })[character]);
}

document.querySelector("#greeting").textContent = new Date().getHours() < 12 ? "Good morning, Jordan" : new Date().getHours() < 18 ? "Good afternoon, Jordan" : "Good evening, Jordan";
load();
