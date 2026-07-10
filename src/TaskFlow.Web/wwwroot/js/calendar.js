/* ==========================================================================
   TaskFlow - calendar.js
   Renders a lightweight month-view calendar grid in pure JS/CSS (no
   external calendar library dependency) and plots each task on its due
   date. Kept intentionally simple: one month at a time, with prev/next
   navigation, sufficient for the "Calendar View" requirement without
   pulling in a large third-party component.
   ========================================================================== */

(function () {
    'use strict';

    const container = document.getElementById('calendarGrid');
    if (!container) return;

    const tasks = JSON.parse(container.getAttribute('data-tasks') || '[]');
    let currentMonth = new Date().getMonth();
    let currentYear = new Date().getFullYear();

    function tasksForDay(year, month, day) {
        return tasks.filter(function (t) {
            if (!t.dueDate) return false;
            const d = new Date(t.dueDate);
            return d.getFullYear() === year && d.getMonth() === month && d.getDate() === day;
        });
    }

    function render() {
        const firstDay = new Date(currentYear, currentMonth, 1);
        const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
        const startWeekday = firstDay.getDay();
        const monthName = firstDay.toLocaleString('default', { month: 'long' });

        let html = '<div class="d-flex justify-content-between align-items-center mb-3">' +
            '<button class="btn btn-sm btn-outline-secondary" id="calPrev"><i class="fa-solid fa-chevron-left"></i></button>' +
            '<h5 class="mb-0">' + monthName + ' ' + currentYear + '</h5>' +
            '<button class="btn btn-sm btn-outline-secondary" id="calNext"><i class="fa-solid fa-chevron-right"></i></button>' +
            '</div>';

        html += '<div class="calendar-grid">';
        ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].forEach(function (d) {
            html += '<div class="calendar-weekday">' + d + '</div>';
        });

        for (let i = 0; i < startWeekday; i++) {
            html += '<div class="calendar-cell empty"></div>';
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const dayTasks = tasksForDay(currentYear, currentMonth, day);
            const isToday = new Date().toDateString() === new Date(currentYear, currentMonth, day).toDateString();

            html += '<div class="calendar-cell' + (isToday ? ' today' : '') + '">' +
                '<div class="calendar-date">' + day + '</div>';

            dayTasks.slice(0, 3).forEach(function (t) {
                html += '<a href="/Tasks/Details/' + t.id + '" class="calendar-task-pill" style="background:' + t.projectColorHex + '22;border-left:3px solid ' + t.projectColorHex + ';">' + t.title + '</a>';
            });

            if (dayTasks.length > 3) {
                html += '<div class="text-muted small">+' + (dayTasks.length - 3) + ' more</div>';
            }

            html += '</div>';
        }

        html += '</div>';
        container.innerHTML = html;

        document.getElementById('calPrev').addEventListener('click', function () {
            currentMonth--;
            if (currentMonth < 0) { currentMonth = 11; currentYear--; }
            render();
        });
        document.getElementById('calNext').addEventListener('click', function () {
            currentMonth++;
            if (currentMonth > 11) { currentMonth = 0; currentYear++; }
            render();
        });
    }

    // Inject minimal grid CSS once (kept here since it's specific to this widget).
    const style = document.createElement('style');
    style.textContent =
        '.calendar-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 4px; }' +
        '.calendar-weekday { text-align: center; font-weight: 600; font-size: 0.8rem; color: var(--tf-text-muted); padding: 4px; }' +
        '.calendar-cell { min-height: 90px; border: 1px solid var(--tf-border); border-radius: 6px; padding: 4px; font-size: 0.75rem; }' +
        '.calendar-cell.empty { border: none; }' +
        '.calendar-cell.today { background: rgba(79,70,229,0.08); border-color: #4f46e5; }' +
        '.calendar-date { font-weight: 600; margin-bottom: 4px; }' +
        '.calendar-task-pill { display: block; padding: 2px 4px; border-radius: 3px; margin-bottom: 2px; text-decoration: none; color: inherit; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }';
    document.head.appendChild(style);

    render();
})();
