/* =====================================================================
   HotelBill — Theme Switcher
   Persists the chosen theme ("light" | "dark") in localStorage and
   applies it as a data-theme attribute on <html> as early as possible
   to avoid a flash of the wrong theme.
   ===================================================================== */
(function () {
    "use strict";

    var STORAGE_KEY = "hb-theme";
    var root = document.documentElement;

    function getStoredTheme() {
        try {
            return localStorage.getItem(STORAGE_KEY);
        } catch (e) {
            return null;
        }
    }

    function storeTheme(theme) {
        try {
            localStorage.setItem(STORAGE_KEY, theme);
        } catch (e) { /* storage unavailable — ignore */ }
    }

    function systemPrefersDark() {
        return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
    }

    function applyTheme(theme, animate) {
        if (animate) {
            document.body.classList.add("theme-transitioning");
            window.setTimeout(function () {
                document.body.classList.remove("theme-transitioning");
            }, 400);
        }
        root.setAttribute("data-theme", theme);
        updateToggleIcon(theme);
        window.dispatchEvent(new CustomEvent("hb:themechange", { detail: { theme: theme } }));
    }

    function updateToggleIcon(theme) {
        var btn = document.getElementById("hbThemeToggle");
        if (!btn) return;
        var icon = btn.querySelector("i");
        if (!icon) return;
        icon.className = theme === "dark" ? "fa-solid fa-sun" : "fa-solid fa-moon";
        btn.setAttribute("title", theme === "dark" ? "Switch to Light Theme" : "Switch to Dark Theme");
        btn.setAttribute("aria-pressed", theme === "dark" ? "true" : "false");
    }

    // Apply immediately (this script is loaded in <head> before paint)
    var initial = getStoredTheme() || (systemPrefersDark() ? "dark" : "light");
    root.setAttribute("data-theme", initial);

    document.addEventListener("DOMContentLoaded", function () {
        updateToggleIcon(initial);

        var btn = document.getElementById("hbThemeToggle");
        if (!btn) return;

        btn.addEventListener("click", function () {
            var current = root.getAttribute("data-theme") === "dark" ? "dark" : "light";
            var next = current === "dark" ? "light" : "dark";

            // Icon flip micro-interaction
            var icon = btn.querySelector("i");
            if (icon) {
                icon.style.transform = "rotate(-90deg) scale(0.6)";
                icon.style.opacity = "0";
                window.setTimeout(function () {
                    applyTheme(next, true);
                    icon.style.transform = "rotate(0deg) scale(1)";
                    icon.style.opacity = "1";
                }, 150);
            } else {
                applyTheme(next, true);
            }

            storeTheme(next);
        });
    });
})();
/* =====================================================================
   HotelBill — Sidebar behavior
   - Desktop (>=992px): collapse/expand between 280px and 90px
   - Tablet/Mobile (<992px), OR when a device-preview mode is active:
     overlay / offcanvas slide-in
   - Tooltips on nav icons while collapsed
   - Collapse preference persisted in localStorage
   ===================================================================== */
(function () {
    "use strict";

    var STORAGE_KEY = "hb-sidebar-collapsed";
    var shell = document.getElementById("hbShell");
    var burger = document.getElementById("hbBurger");
    var overlay = document.getElementById("hbOverlay");
    var tooltip = document.getElementById("hbTooltip");

    if (!shell) return;

    function isPreviewMode() {
        var p = document.body.getAttribute("data-preview");
        return p === "tablet" || p === "mobile";
    }

    // While a Tablet/Mobile preview frame is active, the sidebar must
    // always behave like a real tablet/mobile viewport (offcanvas),
    // no matter how wide the actual browser window is.
    function isMobile() {
        if (isPreviewMode()) return true;
        return window.innerWidth < 992;
    }

    function getStoredCollapsed() {
        try {
            return localStorage.getItem(STORAGE_KEY) === "1";
        } catch (e) {
            return false;
        }
    }

    function storeCollapsed(collapsed) {
        try {
            localStorage.setItem(STORAGE_KEY, collapsed ? "1" : "0");
        } catch (e) { /* ignore */ }
    }

    // Restore desktop collapse state on load (skip on mobile — always starts closed)
    if (!isMobile() && getStoredCollapsed()) {
        shell.classList.add("hb-collapsed");
    }

    function toggleSidebar() {
        if (isMobile()) {
            shell.classList.toggle("hb-sidebar-open");
        } else {
            shell.classList.toggle("hb-collapsed");
            storeCollapsed(shell.classList.contains("hb-collapsed"));
        }
    }

    function closeMobileSidebar() {
        shell.classList.remove("hb-sidebar-open");
    }

    if (burger) {
        burger.addEventListener("click", toggleSidebar);
    }

    if (overlay) {
        overlay.addEventListener("click", closeMobileSidebar);
    }

    // Close mobile sidebar when a nav link is tapped
    document.querySelectorAll(".hb-nav-link").forEach(function (link) {
        link.addEventListener("click", function () {
            if (isMobile()) closeMobileSidebar();
        });
    });

    // Reset transient states on resize crossing the breakpoint
    var lastIsMobile = isMobile();
    window.addEventListener("resize", function () {
        var nowMobile = isMobile();
        if (nowMobile !== lastIsMobile) {
            shell.classList.remove("hb-sidebar-open");
            if (nowMobile) {
                shell.classList.remove("hb-collapsed");
            } else if (getStoredCollapsed()) {
                shell.classList.add("hb-collapsed");
            }
            lastIsMobile = nowMobile;
        }
    });

    // Expose so the device-preview script (below) can reset state cleanly
    window.__hbSidebar = {
        shell: shell,
        isMobile: isMobile,
        closeMobileSidebar: closeMobileSidebar,
        refreshMobileState: function () {
            lastIsMobile = isMobile();
        }
    };

    // ---------------------------------------------------------------
    // Tooltips — only meaningful while collapsed on desktop
    // ---------------------------------------------------------------
    if (tooltip) {
        document.querySelectorAll(".hb-nav-link").forEach(function (link) {
            link.addEventListener("mouseenter", function () {
                if (!shell.classList.contains("hb-collapsed") || isMobile()) return;
                var label = link.getAttribute("data-label") || link.textContent.trim();
                var rect = link.getBoundingClientRect();
                tooltip.textContent = label;
                tooltip.style.top = (rect.top + rect.height / 2 - tooltip.offsetHeight / 2 - 8) + "px";
                tooltip.classList.add("hb-tooltip-visible");
                // Recalculate vertical center once rendered
                requestAnimationFrame(function () {
                    tooltip.style.top = (rect.top + rect.height / 2 - tooltip.offsetHeight / 2) + "px";
                });
            });
            link.addEventListener("mouseleave", function () {
                tooltip.classList.remove("hb-tooltip-visible");
            });
        });
    }

    // ---------------------------------------------------------------
    // Fullscreen toggle
    // ---------------------------------------------------------------
    var fsBtn = document.getElementById("hbFullscreen");
    if (fsBtn) {
        fsBtn.addEventListener("click", function () {
            var icon = fsBtn.querySelector("i");
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(function () { });
                if (icon) icon.className = "fa-solid fa-compress";
            } else {
                document.exitFullscreen().catch(function () { });
                if (icon) icon.className = "fa-solid fa-expand";
            }
        });
    }

    // ---------------------------------------------------------------
    // Button ripple micro-interaction (applies to any .hb-ripple button)
    // ---------------------------------------------------------------
    document.querySelectorAll(".hb-ripple").forEach(function (btn) {
        btn.addEventListener("click", function (e) {
            var rect = btn.getBoundingClientRect();
            var span = document.createElement("span");
            var size = Math.max(rect.width, rect.height);
            span.className = "hb-ripple-effect";
            span.style.width = span.style.height = size + "px";
            span.style.left = (e.clientX - rect.left - size / 2) + "px";
            span.style.top = (e.clientY - rect.top - size / 2) + "px";
            btn.appendChild(span);
            window.setTimeout(function () { span.remove(); }, 550);
        });
    });
})();
/* =====================================================================
   HotelBill — Dashboard charts
   Reads colors from CSS variables so charts stay in sync with the
   active theme, and redraws automatically on "hb:themechange".
   ===================================================================== */
(function () {
    "use strict";

    if (typeof ApexCharts === "undefined") return;

    function cssVar(name) {
        return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
    }

    var charts = {};

    function baseTheme() {
        return {
            textColor: cssVar("--text-muted"),
            gridColor: cssVar("--chart-grid"),
            labelColor: cssVar("--chart-label"),
            fontFamily: "Poppins, sans-serif"
        };
    }

    // ---------------------------------------------------------------
    // Revenue Overview — Area chart
    // ---------------------------------------------------------------
    function renderRevenueChart() {
        var el = document.querySelector("#hbRevenueChart");
        if (!el) return;
        var t = baseTheme();

        var options = {
            chart: { type: "area", height: 210, toolbar: { show: false }, fontFamily: t.fontFamily, sparkline: { enabled: false } },
            series: [
                { name: "This Month", data: [18, 24, 20, 32, 28, 40, 36, 46, 42, 52, 48, 60] },
                { name: "Last Month", data: [12, 18, 15, 22, 20, 28, 26, 32, 30, 36, 34, 40] }
            ],
            xaxis: {
                categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                labels: { style: { colors: t.labelColor, fontSize: "11px" } },
                axisBorder: { show: false },
                axisTicks: { show: false }
            },
            yaxis: { labels: { style: { colors: t.labelColor, fontSize: "11px" }, formatter: function (v) { return "$" + v + "k"; } } },
            grid: { borderColor: t.gridColor, strokeDashArray: 4, xaxis: { lines: { show: false } } },
            colors: [cssVar("--primary-color"), cssVar("--text-faint")],
            fill: { type: "gradient", gradient: { shadeIntensity: 1, opacityFrom: 0.35, opacityTo: 0.02, stops: [0, 90] } },
            stroke: { curve: "smooth", width: 3 },
            dataLabels: { enabled: false },
            legend: { show: false },
            tooltip: { theme: document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light" }
        };

        if (charts.revenue) charts.revenue.destroy();
        charts.revenue = new ApexCharts(el, options);
        charts.revenue.render();
    }

    // ---------------------------------------------------------------
    // Room Status — Donut chart
    // ---------------------------------------------------------------
    function renderRoomStatusChart() {
        var el = document.querySelector("#hbRoomStatusChart");
        if (!el) return;

        var options = {
            chart: { type: "donut", height: 165, fontFamily: baseTheme().fontFamily },
            series: [64, 22, 10, 4],
            labels: ["Occupied", "Available", "Reserved", "Maintenance"],
            colors: [cssVar("--primary-color"), cssVar("--success-color"), cssVar("--warning-color"), cssVar("--danger-color")],
            stroke: { width: 0 },
            dataLabels: { enabled: false },
            legend: { show: false },
            plotOptions: { pie: { donut: { size: "72%" } } },
            tooltip: { theme: document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light" }
        };

        if (charts.roomStatus) charts.roomStatus.destroy();
        charts.roomStatus = new ApexCharts(el, options);
        charts.roomStatus.render();
    }

    // ---------------------------------------------------------------
    // Occupancy — Radial bar
    // ---------------------------------------------------------------
    function renderOccupancyChart() {
        var el = document.querySelector("#hbOccupancyChart");
        if (!el) return;

        var options = {
            chart: { type: "radialBar", height: 150, fontFamily: baseTheme().fontFamily },
            series: [78],
            labels: ["Occupancy"],
            colors: [cssVar("--primary-color")],
            plotOptions: {
                radialBar: {
                    hollow: { size: "62%" },
                    track: { background: cssVar("--chart-grid") },
                    dataLabels: {
                        name: { show: false },
                        value: { show: false }
                    }
                }
            },
            stroke: { lineCap: "round" }
        };

        if (charts.occupancy) charts.occupancy.destroy();
        charts.occupancy = new ApexCharts(el, options);
        charts.occupancy.render();
    }

    // ---------------------------------------------------------------
    // Mini KPI sparklines
    // ---------------------------------------------------------------
    function renderSparklines() {
        document.querySelectorAll("[data-spark]").forEach(function (el) {
            var raw = el.getAttribute("data-spark") || "";
            var data = raw.split(",").map(Number).filter(function (n) { return !isNaN(n); });
            if (!data.length) return;

            var options = {
                chart: { type: "area", height: 34, sparkline: { enabled: true } },
                series: [{ data: data }],
                stroke: { curve: "smooth", width: 2 },
                fill: { type: "gradient", gradient: { opacityFrom: 0.45, opacityTo: 0 } },
                colors: ["#ffffff"],
                tooltip: { enabled: false }
            };

            var key = "spark_" + el.id;
            if (charts[key]) charts[key].destroy();
            charts[key] = new ApexCharts(el, options);
            charts[key].render();
        });
    }

    function renderAll() {
        renderRevenueChart();
        renderRoomStatusChart();
        renderOccupancyChart();
        renderSparklines();
    }

    document.addEventListener("DOMContentLoaded", renderAll);

    // Redraw with new theme colors after a switch (small delay lets CSS vars update)
    window.addEventListener("hb:themechange", function () {
        window.setTimeout(renderAll, 60);
    });
})();
/* =====================================================================
   HotelBill — Navbar: device preview toggle (Tablet / Mobile only)
   + Notification / Email / Chat dropdowns
   ===================================================================== */
(function () {
    "use strict";

    // ---------------------------------------------------------------
    // Device preview toggle
    // Only two buttons now: Tablet and Mobile. Clicking the already-
    // active one turns the preview off (back to normal desktop page).
    // Clicking the other one switches directly between them.
    // ---------------------------------------------------------------
    var deviceBtns = document.querySelectorAll("#hbDeviceToggle .hb-device-btn");
    var shellEl = document.getElementById("hbShell");

    function setActiveDeviceBtn(device) {
        deviceBtns.forEach(function (b) {
            b.classList.toggle("active", b.getAttribute("data-device") === device);
        });
    }

    function resetShellForPreviewChange() {
        if (!shellEl) return;
        // Always start a new preview from a closed sidebar / expanded state
        shellEl.classList.remove("hb-sidebar-open");
        shellEl.classList.remove("hb-collapsed");
        if (window.__hbSidebar && window.__hbSidebar.refreshMobileState) {
            window.__hbSidebar.refreshMobileState();
        }
    }

    deviceBtns.forEach(function (btn) {
        btn.addEventListener("click", function () {
            var device = btn.getAttribute("data-device"); // "tablet" | "mobile"
            var current = document.body.getAttribute("data-preview");

            if (current === device) {
                // Toggle off -> back to full desktop page
                document.body.removeAttribute("data-preview");
                setActiveDeviceBtn(null);
            } else {
                document.body.setAttribute("data-preview", device);
                setActiveDeviceBtn(device);
            }
            resetShellForPreviewChange();
        });
    });

    // ---------------------------------------------------------------
    // Generic dropdown open/close handling (Notifications / Email / Chat)
    // ---------------------------------------------------------------
    var dropdowns = [
        { btn: "hbNotifBtn", panel: "hbNotifPanel" },
        { btn: "hbMailBtn", panel: "hbMailPanel" },
        { btn: "hbChatBtn", panel: "hbChatPanel" }
    ];

    function closeAllDropdowns() {
        dropdowns.forEach(function (d) {
            var panel = document.getElementById(d.panel);
            if (panel) panel.classList.remove("show");
        });
    }

    dropdowns.forEach(function (d) {
        var btn = document.getElementById(d.btn);
        var panel = document.getElementById(d.panel);
        if (!btn || !panel) return;

        btn.addEventListener("click", function (e) {
            e.stopPropagation();
            var isOpen = panel.classList.contains("show");
            closeAllDropdowns();
            if (!isOpen) panel.classList.add("show");
        });
    });

    document.addEventListener("click", function (e) {
        var wraps = document.querySelectorAll(".hb-dropdown-wrap");
        var clickedInside = false;
        wraps.forEach(function (w) {
            if (w.contains(e.target)) clickedInside = true;
        });
        if (!clickedInside) closeAllDropdowns();
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") closeAllDropdowns();
    });
})();
