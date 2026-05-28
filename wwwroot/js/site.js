(function () {
  const pendingKey = "edutrack.pendingSubmissions";
  const lessonsKey = "edutrack.offlineLessons";
  const appearanceKey = "edutrack.appearance";

  function readJson(key, fallback) {
    try {
      return JSON.parse(localStorage.getItem(key) || JSON.stringify(fallback));
    } catch {
      return fallback;
    }
  }

  function writeJson(key, value) {
    localStorage.setItem(key, JSON.stringify(value));
  }

  function showToast(message) {
    const toast = document.createElement("div");
    toast.className = "app-toast";
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 4200);
  }

  function appearanceDefaults() {
    return {
      themeMode: "Light",
      layoutStyle: "Comfortable",
      sidebarState: "Expanded",
      fontSize: "Medium",
      cardStyle: "Default",
      highContrastMode: false,
      reduceAnimations: false,
      readableSpacing: true,
      showOfflineBanner: true,
      autoSyncWhenOnline: true,
      showSyncSuccessMessage: true,
      enableOfflineSyncNotifications: true,
      lowDataMode: false,
      enableOfflineMode: true,
      studentDashboardPriority: "Upcoming Assessments",
      teacherDashboardPriority: "Students Needing Support",
      adminDashboardPriority: "System Statistics"
    };
  }

  function toBoolean(value, fallback) {
    if (typeof value === "boolean") return value;
    if (typeof value === "string") {
      if (value.toLowerCase() === "true") return true;
      if (value.toLowerCase() === "false") return false;
    }

    return fallback;
  }

  function normalizeAppearance(settings) {
    const defaults = appearanceDefaults();
    const next = Object.assign({}, defaults, settings || {});
    const valid = {
      themeMode: ["Light", "Dark", "System"],
      layoutStyle: ["Comfortable", "Compact"],
      sidebarState: ["Expanded", "Collapsed", "Hidden"],
      fontSize: ["Small", "Medium", "Large"],
      cardStyle: ["Default", "Minimal"],
      studentDashboardPriority: ["Upcoming Assessments", "Recent Scores", "Course Progress", "Offline Lessons", "Announcements"],
      teacherDashboardPriority: ["Students Needing Support", "Pending Submissions", "Recent Submissions", "Upcoming Assessments", "Class Performance"],
      adminDashboardPriority: ["System Statistics", "Recent Users", "Reports Summary", "Announcements", "Overall Performance"]
    };

    Object.keys(valid).forEach((key) => {
      if (!valid[key].includes(next[key])) {
        next[key] = defaults[key];
      }
    });

    [
      "highContrastMode",
      "reduceAnimations",
      "readableSpacing",
      "showOfflineBanner",
      "autoSyncWhenOnline",
      "showSyncSuccessMessage",
      "enableOfflineSyncNotifications",
      "lowDataMode",
      "enableOfflineMode"
    ].forEach((key) => {
      next[key] = toBoolean(next[key], defaults[key]);
    });

    return next;
  }

  function getAppearanceSettings() {
    const server = window.eduTrackServerSettings || {};
    const saved = readJson(appearanceKey, {});
    return normalizeAppearance(Object.assign({}, appearanceDefaults(), server, saved));
  }

  function applyAppearance(settings, persist) {
    const normalized = normalizeAppearance(settings);
    const root = document.documentElement;
    const systemDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
    const effectiveTheme = normalized.themeMode === "System" ? (systemDark ? "Dark" : "Light") : normalized.themeMode;

    root.dataset.themeMode = effectiveTheme.toLowerCase();
    root.dataset.themeChoice = normalized.themeMode.toLowerCase();
    root.dataset.layoutStyle = normalized.layoutStyle.toLowerCase();
    root.dataset.sidebarState = normalized.sidebarState.toLowerCase();
    root.dataset.fontSize = normalized.fontSize.toLowerCase();
    root.dataset.cardStyle = normalized.cardStyle.toLowerCase();
    root.dataset.highContrast = normalized.highContrastMode ? "true" : "false";
    root.dataset.reduceAnimations = normalized.reduceAnimations ? "true" : "false";
    root.dataset.readableSpacing = normalized.readableSpacing ? "true" : "false";
    root.dataset.lowDataMode = normalized.lowDataMode ? "true" : "false";

    const toggle = document.getElementById("themeToggle");
    if (toggle) {
      toggle.textContent = effectiveTheme === "Dark" ? "Light Mode" : "Dark Mode";
      toggle.setAttribute("aria-label", `Switch to ${toggle.textContent}`);
    }

    if (persist !== false) {
      writeJson(appearanceKey, normalized);
    }

    requestAnimationFrame(drawCharts);
    requestAnimationFrame(() => applyDashboardPreference(normalized));
    updateSidebarToggle(normalized);
  }

  function getSettingsFromForm(form) {
    const data = new FormData(form);
    const readChecked = (name) => data.get(name) === "true";

    return normalizeAppearance({
      themeMode: data.get("ThemeMode"),
      layoutStyle: data.get("LayoutStyle"),
      sidebarState: data.get("SidebarState"),
      fontSize: data.get("FontSize"),
      cardStyle: data.get("CardStyle"),
      highContrastMode: readChecked("HighContrastMode"),
      reduceAnimations: readChecked("ReduceAnimations"),
      readableSpacing: readChecked("ReadableSpacing"),
      showOfflineBanner: readChecked("ShowOfflineBanner"),
      autoSyncWhenOnline: readChecked("AutoSyncWhenOnline"),
      showSyncSuccessMessage: readChecked("ShowSyncSuccessMessage"),
      enableOfflineSyncNotifications: readChecked("EnableOfflineSyncNotifications"),
      lowDataMode: readChecked("LowDataMode"),
      enableOfflineMode: readChecked("EnableOfflineMode"),
      studentDashboardPriority: data.get("StudentDashboardPriority"),
      teacherDashboardPriority: data.get("TeacherDashboardPriority"),
      adminDashboardPriority: data.get("AdminDashboardPriority")
    });
  }

  function applyDashboardPreference(settings) {
    const role = document.body.dataset.role;
    const priority = role === "Admin"
      ? settings.adminDashboardPriority
      : role === "Teacher" ? settings.teacherDashboardPriority : settings.studentDashboardPriority;

    if (!priority) return;

    const escapedPriority = window.CSS && CSS.escape
      ? CSS.escape(priority)
      : String(priority).replace(/"/g, '\\"');
    const target = document.querySelector(`[data-dashboard-section="${escapedPriority}"]`);
    const parent = target?.parentElement;
    if (!target || !parent) return;

    parent.insertBefore(target, parent.firstElementChild);
  }

  function setupAppearanceSettings() {
    applyAppearance(getAppearanceSettings(), false);

    if (window.matchMedia) {
      window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", () => {
        applyAppearance(getAppearanceSettings(), false);
      });
    }

    const form = document.querySelector("[data-appearance-form]");
    if (form) {
      form.addEventListener("change", () => {
        applyAppearance(getSettingsFromForm(form));
      });

      form.addEventListener("submit", () => {
        applyAppearance(getSettingsFromForm(form));
      });
    }

    const toggle = document.getElementById("themeToggle");
    if (!toggle) return;

    toggle.addEventListener("click", async () => {
      const settings = getAppearanceSettings();
      const effectiveTheme = document.documentElement.dataset.themeMode === "dark" ? "Dark" : "Light";
      settings.themeMode = effectiveTheme === "Dark" ? "Light" : "Dark";
      applyAppearance(settings);

      const token = document.querySelector('meta[name="csrf-token"]')?.getAttribute("content");
      if (document.body.dataset.authenticated !== "true" || !token) return;

      try {
        const response = await fetch("/Settings/QuickTheme", {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token
          },
          body: JSON.stringify({ themeMode: settings.themeMode })
        });

        if (response.ok) {
          showToast("Settings saved successfully.");
        }
      } catch {
        showToast("Theme changed on this device. It will sync when settings can be saved.");
      }
    });
  }

  function updateSidebarToggle(settings) {
    const toggle = document.getElementById("sidebarToggle");
    if (!toggle) return;

    const isHidden = settings.sidebarState === "Hidden";
    toggle.textContent = isHidden ? "Show Menu" : "Hide Menu";
    toggle.setAttribute("aria-expanded", String(!isHidden));
  }

  function setupSidebarToggle() {
    const toggle = document.getElementById("sidebarToggle");
    if (!toggle) return;

    const saved = readJson(appearanceKey, {});
    if (!saved.sidebarState && window.innerWidth <= 900) {
      const settings = getAppearanceSettings();
      settings.sidebarState = "Hidden";
      applyAppearance(settings);
    }

    toggle.addEventListener("click", async () => {
      const settings = getAppearanceSettings();
      settings.sidebarState = settings.sidebarState === "Hidden" ? "Expanded" : "Hidden";
      applyAppearance(settings);

      const token = document.querySelector('meta[name="csrf-token"]')?.getAttribute("content");
      if (document.body.dataset.authenticated !== "true" || !token) return;

      try {
        await fetch("/Settings/QuickSidebar", {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token
          },
          body: JSON.stringify({ sidebarState: settings.sidebarState })
        });
      } catch {
        showToast("Menu preference saved on this device.");
      }
    });
  }

  function updateConnectionBanner() {
    const banner = document.getElementById("connectionBanner");
    if (!banner) return;

    const settings = getAppearanceSettings();
    if (!settings.showOfflineBanner || !settings.enableOfflineMode) {
      banner.hidden = true;
      return;
    }

    banner.hidden = false;

    if (navigator.onLine) {
      banner.textContent = "Online Mode";
      banner.classList.add("online");
      banner.classList.remove("offline");
    } else {
      banner.textContent = "Offline Mode";
      banner.classList.add("offline");
      banner.classList.remove("online");
    }
  }

  async function registerServiceWorker() {
    if (!getAppearanceSettings().enableOfflineMode) return;
    if (!("serviceWorker" in navigator)) return;

    try {
      await navigator.serviceWorker.register("/service-worker.js");
    } catch {
      console.warn("Service worker registration failed.");
    }
  }

  function setupOfflineLessonSave() {
    const button = document.getElementById("saveLessonOffline");
    const reader = document.querySelector(".lesson-reader");
    const content = document.getElementById("lessonContent");

    if (!button || !reader || !content) return;

    if (!getAppearanceSettings().autoSaveOfflineLessons) {
      button.disabled = true;
      button.textContent = "Offline Save Disabled";
      return;
    }

    button.addEventListener("click", async () => {
      const lessons = readJson(lessonsKey, []);
      const lesson = {
        lessonId: reader.dataset.lessonId,
        title: reader.dataset.lessonTitle,
        courseTitle: reader.dataset.courseTitle,
        content: content.textContent,
        savedAt: new Date().toISOString(),
        url: window.location.pathname
      };

      const next = lessons.filter((item) => item.lessonId !== lesson.lessonId);
      next.unshift(lesson);
      writeJson(lessonsKey, next);

      if ("caches" in window) {
        const cache = await caches.open("edutrack-user-content-v1");
        await cache.add(window.location.href);
      }

      showToast("Lesson saved for offline access.");
    });
  }

  function renderOfflineLessons() {
    const list = document.getElementById("offlineLessonsList");
    const message = document.getElementById("offlineLessonsMessage");
    if (!list) return;

    const lessons = readJson(lessonsKey, []);
    if (!lessons.length) return;

    if (message) {
      message.textContent = "These lessons are saved on this device.";
    }

    list.innerHTML = lessons.map((lesson) => `
      <article>
        <h3>${escapeHtml(lesson.title)}</h3>
        <p><strong>${escapeHtml(lesson.courseTitle || "Course")}</strong></p>
        <p>${escapeHtml(lesson.content || "").slice(0, 600)}</p>
      </article>
    `).join("");
  }

  function updateOfflineLibraryLabels() {
    const savedLessons = readJson(lessonsKey, []);
    const pendingSubmissions = readJson(pendingKey, []);
    const savedLessonIds = new Set(savedLessons.map((lesson) => String(lesson.lessonId)));
    const pendingAssessmentIds = new Set(pendingSubmissions.map((item) => String(item.assessmentId)));

    document.querySelectorAll("[data-offline-lesson-row]").forEach((row) => {
      const badge = row.querySelector("[data-offline-status]");
      if (!badge) return;

      const lessonId = row.dataset.lessonId;
      const isOfflineEnabled = row.dataset.offlineEnabled === "true";

      if (!isOfflineEnabled) {
        badge.textContent = "Needs Internet";
        badge.className = "badge text-bg-secondary";
      } else if (savedLessonIds.has(String(lessonId))) {
        badge.textContent = "Saved for Offline Use";
        badge.className = "badge text-bg-primary";
      } else {
        badge.textContent = "Available Offline";
        badge.className = "badge text-bg-success";
      }
    });

    document.querySelectorAll("[data-offline-assessment-row]").forEach((row) => {
      const badge = row.querySelector("[data-offline-status]");
      if (!badge) return;

      const assessmentId = row.dataset.assessmentId;
      const isOfflineEnabled = row.dataset.offlineEnabled === "true";

      if (!isOfflineEnabled) {
        badge.textContent = "Needs Internet";
        badge.className = "badge text-bg-secondary";
      } else if (pendingAssessmentIds.has(String(assessmentId))) {
        badge.textContent = "Saved for Offline Use";
        badge.className = "badge text-bg-primary";
      } else {
        badge.textContent = "Available Offline";
        badge.className = "badge text-bg-success";
      }
    });
  }

  function escapeHtml(value) {
    return String(value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  function setupOfflineAssessmentForm() {
    const form = document.getElementById("assessmentForm");
    if (!form) return;

    form.addEventListener("submit", (event) => {
      if (navigator.onLine) return;

      event.preventDefault();

      if (form.dataset.offlineEnabled !== "true") {
        showToast("This assessment is not available offline.");
        return;
      }

      const questionIds = Array.from(form.querySelectorAll('input[type="hidden"][name$=".QuestionId"]'));
      const answers = [];
      let missing = false;

      questionIds.forEach((hidden) => {
        const match = hidden.name.match(/Questions\[(\d+)\]/);
        if (!match) return;

        const index = match[1];
        const selected = form.querySelector(`input[name="Questions[${index}].SelectedAnswer"]:checked`);

        if (!selected) {
          missing = true;
        } else {
          answers.push({
            questionId: Number(hidden.value),
            selectedAnswer: selected.value
          });
        }
      });

      if (missing) {
        showToast("Answer every question before saving offline.");
        return;
      }

      const pending = readJson(pendingKey, []);
      pending.push({
        id: Date.now().toString(),
        assessmentId: Number(form.dataset.assessmentId),
        answers,
        createdAt: new Date().toISOString()
      });
      writeJson(pendingKey, pending);
      showToast("Answers saved offline. They will sync when you are back online.");
      window.location.href = "/Student";
    });
  }

  async function syncPendingSubmissions() {
    if (!navigator.onLine) return;
    const settings = getAppearanceSettings();
    if (!settings.enableOfflineMode || !settings.autoSyncWhenOnline) return;

    const pending = readJson(pendingKey, []);
    if (!pending.length) return;

    if (settings.enableOfflineSyncNotifications !== false) {
      showToast("You are back online. Syncing your data...");
    }
    const remaining = [];

    for (const item of pending) {
      try {
        const response = await fetch("/Assessments/SyncOffline", {
          method: "POST",
          credentials: "same-origin",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            assessmentId: item.assessmentId,
            answers: item.answers
          })
        });

        if (!response.ok) {
          remaining.push(item);
          continue;
        }

        const result = await response.json();
        if (!result.success) {
          remaining.push(item);
        }
      } catch {
        remaining.push(item);
      }
    }

    writeJson(pendingKey, remaining);

    if (remaining.length === 0 && settings.showSyncSuccessMessage) {
      showToast("Offline submissions synced successfully.");
    }
  }

  function splitData(value) {
    return (value || "")
      .split(value && value.includes("|") ? "|" : ",")
      .map((item) => item.trim())
      .filter(Boolean);
  }

  function themeColors() {
    const styles = getComputedStyle(document.documentElement);
    return {
      text: styles.getPropertyValue("--ink").trim() || "#102033",
      muted: styles.getPropertyValue("--muted").trim() || "#64748b",
      line: styles.getPropertyValue("--line").trim() || "#dbe4ee",
      surface: styles.getPropertyValue("--surface").trim() || "#ffffff",
      primary: styles.getPropertyValue("--primary").trim() || "#0f766e",
      blue: styles.getPropertyValue("--blue").trim() || "#2563eb",
      amber: styles.getPropertyValue("--amber").trim() || "#f59e0b",
      rose: styles.getPropertyValue("--rose").trim() || "#e11d48"
    };
  }

  function formatValue(value, suffix) {
    return `${Number(value).toFixed(value % 1 ? 1 : 0)}${suffix || ""}`;
  }

  function colorForValue(value, colors) {
    if (value >= 85) return colors.primary;
    if (value >= 75) return colors.blue;
    if (value >= 60) return colors.amber;
    return colors.rose;
  }

  function setupChartTooltip(canvas) {
    if (canvas.dataset.tooltipReady === "true") return;
    canvas.dataset.tooltipReady = "true";

    const tooltip = document.createElement("div");
    tooltip.className = "chart-tooltip";
    tooltip.hidden = true;
    document.body.appendChild(tooltip);

    canvas.addEventListener("mousemove", (event) => {
      const items = canvas._chartItems || [];
      const rect = canvas.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;
      const item = items.find((candidate) => {
        if (candidate.type === "point") {
          return Math.hypot(candidate.x - x, candidate.y - y) < 12;
        }

        return x >= candidate.x && x <= candidate.x + candidate.width && y >= candidate.y && y <= candidate.y + candidate.height;
      });

      if (!item) {
        tooltip.hidden = true;
        return;
      }

      tooltip.innerHTML = `<strong>${escapeHtml(item.fullLabel || item.label)}</strong><span>${escapeHtml(item.labelPrefix || "Value")}: ${escapeHtml(item.valueText)}</span>`;
      tooltip.style.left = `${event.clientX + 14}px`;
      tooltip.style.top = `${event.clientY + 14}px`;
      tooltip.hidden = false;
    });

    canvas.addEventListener("mouseleave", () => {
      tooltip.hidden = true;
    });
  }

  function prepareCanvas(canvas) {
    const width = canvas.clientWidth || 500;
    const height = Number(canvas.getAttribute("height")) || 260;
    const ratio = window.devicePixelRatio || 1;
    canvas.width = Math.floor(width * ratio);
    canvas.height = Math.floor(height * ratio);
    canvas.style.height = `${height}px`;
    const ctx = canvas.getContext("2d");
    if (!ctx) return null;
    ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
    return { ctx, width, height };
  }

  function drawCharts() {
    document.querySelectorAll(".analytics-chart").forEach((canvas) => {
      const prepared = prepareCanvas(canvas);
      if (!prepared) return;

      const { ctx, width, height } = prepared;
      const labels = splitData(canvas.dataset.labels);
      const fullLabels = splitData(canvas.dataset.fullLabels);
      const values = splitData(canvas.dataset.values).map((value) => Number(value) || 0);
      const chartType = canvas.dataset.chart || "bar";
      const colors = themeColors();
      canvas._chartItems = [];
      setupChartTooltip(canvas);

      ctx.clearRect(0, 0, width, height);
      ctx.font = "13px Segoe UI, sans-serif";
      ctx.fillStyle = colors.text;

      const hasData = values.length && values.some((value) => value > 0);
      if (!hasData) {
        ctx.fillStyle = colors.muted;
        ctx.fillText(canvas.dataset.empty || "No chart data yet.", 16, 32);
        return;
      }

      if (chartType === "horizontalBar") {
        drawHorizontalBars(ctx, canvas, values, labels, fullLabels, width, height, colors);
      } else if (chartType === "pie" || chartType === "doughnut") {
        drawDoughnut(ctx, canvas, values, labels, fullLabels, width, height, colors, chartType === "doughnut");
      } else if (chartType === "line") {
        drawLine(ctx, canvas, values, labels, fullLabels, width, height, colors);
      } else {
        drawVerticalBars(ctx, canvas, values, labels, fullLabels, width, height, colors);
      }
    });
  }

  function drawChartTitle(ctx, title, colors) {
    if (!title) return 16;
    ctx.fillStyle = colors.text;
    ctx.font = "700 14px Segoe UI, sans-serif";
    ctx.fillText(title, 16, 20);
    ctx.font = "13px Segoe UI, sans-serif";
    return 34;
  }

  function drawHorizontalBars(ctx, canvas, values, labels, fullLabels, width, height, colors) {
    const top = drawChartTitle(ctx, canvas.dataset.title, colors);
    const left = 104;
    const right = 54;
    const bottom = 22;
    const max = Number(canvas.dataset.scaleMax) || Math.max(...values, 1);
    const rowHeight = Math.max(26, (height - top - bottom) / values.length);
    const barHeight = Math.min(18, rowHeight * 0.52);

    ctx.strokeStyle = colors.line;
    ctx.beginPath();
    ctx.moveTo(left, top);
    ctx.lineTo(left, height - bottom);
    ctx.lineTo(width - right, height - bottom);
    ctx.stroke();

    values.forEach((value, index) => {
      const y = top + index * rowHeight + (rowHeight - barHeight) / 2;
      const barWidth = Math.max(4, ((width - left - right) * Math.min(value, max)) / max);
      const valueSuffix = canvas.dataset.scaleMax === "100" ? "%" : "";

      ctx.fillStyle = colors.muted;
      ctx.fillText(labels[index] || "", 8, y + barHeight - 3);
      ctx.fillStyle = colorForValue(canvas.dataset.scaleMax === "100" ? value : 85, colors);
      ctx.fillRect(left, y, barWidth, barHeight);
      ctx.fillStyle = colors.text;
      ctx.fillText(formatValue(value, valueSuffix), Math.min(width - 44, left + barWidth + 8), y + barHeight - 3);

      canvas._chartItems.push({
        type: "bar",
        x: left,
        y,
        width: barWidth,
        height: barHeight,
        label: labels[index] || "",
        fullLabel: fullLabels[index] || labels[index] || "",
        labelPrefix: canvas.dataset.tooltipLabel || (canvas.dataset.scaleMax === "100" ? "Progress" : "Count"),
        valueText: formatValue(value, valueSuffix)
      });
    });
  }

  function drawVerticalBars(ctx, canvas, values, labels, fullLabels, width, height, colors) {
    const top = drawChartTitle(ctx, canvas.dataset.title, colors);
    const padding = 38;
    const max = Number(canvas.dataset.scaleMax) || Math.max(...values, 1);
    const chartHeight = height - top - padding - 22;
    const usableWidth = width - padding * 2;
    const slotWidth = usableWidth / values.length;
    const barWidth = Math.max(24, Math.min(58, slotWidth * 0.55));

    ctx.strokeStyle = colors.line;
    ctx.beginPath();
    ctx.moveTo(padding, top);
    ctx.lineTo(padding, height - padding);
    ctx.lineTo(width - padding, height - padding);
    ctx.stroke();

    values.forEach((value, index) => {
      const x = padding + slotWidth * index + (slotWidth - barWidth) / 2;
      const barHeight = (Math.min(value, max) / max) * chartHeight;
      const y = height - padding - barHeight;
      const suffix = canvas.dataset.scaleMax === "100" || value <= 100 ? "%" : "";

      ctx.fillStyle = colorForValue(value, colors);
      ctx.fillRect(x, y, barWidth, barHeight);
      ctx.fillStyle = colors.text;
      ctx.fillText(formatValue(value, suffix), x, y - 8);
      ctx.fillStyle = colors.muted;
      ctx.fillText(labels[index] || "", Math.max(4, x - 4), height - 12);

      canvas._chartItems.push({
        type: "bar",
        x,
        y,
        width: barWidth,
        height: barHeight,
        label: labels[index] || "",
        fullLabel: fullLabels[index] || labels[index] || "",
        labelPrefix: canvas.dataset.tooltipLabel || "Score",
        valueText: formatValue(value, suffix)
      });
    });
  }

  function drawLine(ctx, canvas, values, labels, fullLabels, width, height, colors) {
    const top = drawChartTitle(ctx, canvas.dataset.title, colors);
    const max = Number(canvas.dataset.scaleMax) || Math.max(...values, 100);
    const padding = 38;
    const usableWidth = width - padding * 2;
    const usableHeight = height - top - padding * 1.5;

    ctx.strokeStyle = colors.line;
    ctx.beginPath();
    ctx.moveTo(padding, top);
    ctx.lineTo(padding, height - padding);
    ctx.lineTo(width - padding, height - padding);
    ctx.stroke();

    ctx.strokeStyle = colors.primary;
    ctx.lineWidth = 3;
    ctx.beginPath();
    values.forEach((value, index) => {
      const x = padding + (values.length === 1 ? usableWidth / 2 : (index / (values.length - 1)) * usableWidth);
      const y = height - padding - (Math.min(value, max) / max) * usableHeight;
      if (index === 0) ctx.moveTo(x, y);
      else ctx.lineTo(x, y);
    });
    ctx.stroke();

    values.forEach((value, index) => {
      const x = padding + (values.length === 1 ? usableWidth / 2 : (index / (values.length - 1)) * usableWidth);
      const y = height - padding - (Math.min(value, max) / max) * usableHeight;
      ctx.fillStyle = colors.blue;
      ctx.beginPath();
      ctx.arc(x, y, 5, 0, Math.PI * 2);
      ctx.fill();
      ctx.fillStyle = colors.muted;
      ctx.fillText(labels[index] || "", Math.max(4, x - 24), height - 10);

      canvas._chartItems.push({
        type: "point",
        x,
        y,
        label: labels[index] || "",
        fullLabel: fullLabels[index] || labels[index] || "",
        labelPrefix: "Progress",
        valueText: formatValue(value, "%")
      });
    });
  }

  function drawDoughnut(ctx, canvas, values, labels, fullLabels, width, height, colors, isDoughnut) {
    const top = drawChartTitle(ctx, canvas.dataset.title, colors);
    const total = values.reduce((sum, value) => sum + value, 0) || 1;
    const palette = [colors.primary, colors.rose, colors.amber, colors.blue];
    const radius = Math.min(width, height - top) / 3.2;
    const cx = Math.min(width * 0.38, width - 160);
    const cy = top + (height - top) / 2;
    let start = -Math.PI / 2;

    values.forEach((value, index) => {
      const slice = (value / total) * Math.PI * 2;
      ctx.fillStyle = palette[index % palette.length];
      ctx.beginPath();
      ctx.moveTo(cx, cy);
      ctx.arc(cx, cy, radius, start, start + slice);
      ctx.closePath();
      ctx.fill();
      start += slice;
    });

    if (isDoughnut) {
      ctx.fillStyle = colors.surface;
      ctx.beginPath();
      ctx.arc(cx, cy, radius * 0.55, 0, Math.PI * 2);
      ctx.fill();
    }

    labels.forEach((label, index) => {
      const x = width - 180;
      const y = top + 26 + index * 28;
      ctx.fillStyle = palette[index % palette.length];
      ctx.fillRect(x, y - 11, 14, 14);
      ctx.fillStyle = colors.text;
      ctx.fillText(`${label}: ${values[index] || 0}`, x + 22, y);

      canvas._chartItems.push({
        type: "bar",
        x,
        y: y - 16,
        width: 160,
        height: 24,
        label,
        fullLabel: fullLabels[index] || label,
        labelPrefix: "Count",
        valueText: String(values[index] || 0)
      });
    });
  }

  window.addEventListener("online", () => {
    updateConnectionBanner();
    syncPendingSubmissions();
  });

  window.addEventListener("offline", updateConnectionBanner);
  window.addEventListener("resize", drawCharts);

  document.addEventListener("DOMContentLoaded", () => {
    setupAppearanceSettings();
    setupSidebarToggle();
    updateConnectionBanner();
    registerServiceWorker();
    setupOfflineLessonSave();
    setupOfflineAssessmentForm();
    renderOfflineLessons();
    updateOfflineLibraryLabels();
    drawCharts();
    syncPendingSubmissions();
  });
})();
