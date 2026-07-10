/* ==========================================================================
   TaskFlow - dashboard.js
   Fetches chart data from DashboardController's AJAX endpoints and renders
   three Chart.js charts. Colors are chosen to read clearly in both light
   and dark themes since Chart.js canvases don't inherit CSS variables.
   ========================================================================== */

(function () {
    'use strict';

    const palette = {
        toDo: '#94a3b8',
        inProgress: '#3b82f6',
        inReview: '#f59e0b',
        done: '#10b981',
        blocked: '#ef4444',
        low: '#94a3b8',
        medium: '#f59e0b',
        high: '#fb923c',
        critical: '#ef4444',
        line: '#4f46e5'
    };

    // ---- Task Status Doughnut Chart ----
    const statusCanvas = document.getElementById('taskStatusChart');
    if (statusCanvas) {
        fetch('/Dashboard/TaskStatusChartData')
            .then(function (res) { return res.json(); })
            .then(function (data) {
                new Chart(statusCanvas, {
                    type: 'doughnut',
                    data: {
                        labels: ['To Do', 'In Progress', 'In Review', 'Done', 'Blocked'],
                        datasets: [{
                            data: [data.toDo, data.inProgress, data.inReview, data.done, data.blocked],
                            backgroundColor: [palette.toDo, palette.inProgress, palette.inReview, palette.done, palette.blocked],
                            borderWidth: 0
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { position: 'bottom', labels: { boxWidth: 10, font: { size: 11 } } } }
                    }
                });
            });
    }

    // ---- Task Priority Bar Chart ----
    const priorityCanvas = document.getElementById('taskPriorityChart');
    if (priorityCanvas) {
        fetch('/Dashboard/TaskPriorityChartData')
            .then(function (res) { return res.json(); })
            .then(function (data) {
                new Chart(priorityCanvas, {
                    type: 'bar',
                    data: {
                        labels: ['Low', 'Medium', 'High', 'Critical'],
                        datasets: [{
                            data: [data.low, data.medium, data.high, data.critical],
                            backgroundColor: [palette.low, palette.medium, palette.high, palette.critical],
                            borderRadius: 6,
                            maxBarThickness: 46
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
            });
    }

    // ---- Completion Trend Line Chart ----
    const trendCanvas = document.getElementById('completionTrendChart');
    if (trendCanvas) {
        fetch('/Dashboard/CompletionTrendChartData?days=14')
            .then(function (res) { return res.json(); })
            .then(function (points) {
                new Chart(trendCanvas, {
                    type: 'line',
                    data: {
                        labels: points.map(function (p) { return p.label; }),
                        datasets: [{
                            data: points.map(function (p) { return p.value; }),
                            borderColor: palette.line,
                            backgroundColor: 'rgba(79, 70, 229, 0.1)',
                            fill: true,
                            tension: 0.35,
                            pointRadius: 3
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
            });
    }
})();
