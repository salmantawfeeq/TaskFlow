/* ==========================================================================
   TaskFlow - task-details.js
   Handles the checklist and comment AJAX interactions on the task detail
   page: toggling checkbox state, adding/removing checklist items, and
   posting new comments, all without a full page reload.
   ========================================================================== */

(function () {
    'use strict';

    const checklistContainer = document.getElementById('checklistContainer');
    const commentsContainer = document.getElementById('commentsContainer');

    // ---- Checklist: toggle completion ----
    if (checklistContainer) {
        const taskId = checklistContainer.getAttribute('data-task-id');

        checklistContainer.addEventListener('change', function (e) {
            if (!e.target.classList.contains('checklist-checkbox')) return;

            const item = e.target.closest('.checklist-item');
            const itemId = item.getAttribute('data-item-id');
            const isCompleted = e.target.checked;
            const label = item.querySelector('label');

            window.postJson('/Tasks/ToggleChecklistItem', { itemId: parseInt(itemId, 10), isCompleted: isCompleted })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.success) {
                        label.classList.toggle('text-decoration-line-through', isCompleted);
                        label.classList.toggle('text-muted', isCompleted);
                    }
                });
        });

        checklistContainer.addEventListener('click', function (e) {
            if (!e.target.closest('.remove-checklist-item')) return;

            const item = e.target.closest('.checklist-item');
            const itemId = item.getAttribute('data-item-id');

            window.postJson('/Tasks/RemoveChecklistItem', { itemId: parseInt(itemId, 10) })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.success) {
                        item.remove();
                    }
                });
        });

        const addForm = document.getElementById('addChecklistForm');
        if (addForm) {
            addForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const input = document.getElementById('newChecklistText');
                const text = input.value.trim();
                if (!text) return;

                window.postJson('/Tasks/AddChecklistItem', { taskId: parseInt(taskId, 10), text: text })
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (data.success) {
                            const div = document.createElement('div');
                            div.className = 'form-check mb-2 checklist-item';
                            div.setAttribute('data-item-id', data.id);
                            div.innerHTML = '<input class="form-check-input checklist-checkbox" type="checkbox" />' +
                                '<label class="form-check-label">' + text + '</label>' +
                                '<button class="btn btn-sm btn-link text-danger float-end remove-checklist-item p-0"><i class="fa-solid fa-xmark"></i></button>';
                            checklistContainer.appendChild(div);
                            input.value = '';
                        }
                    });
            });
        }
    }

    // ---- Comments: add new ----
    if (commentsContainer) {
        const taskId = commentsContainer.getAttribute('data-task-id');
        const addCommentForm = document.getElementById('addCommentForm');

        if (addCommentForm) {
            addCommentForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const input = document.getElementById('newCommentText');
                const content = input.value.trim();
                if (!content) return;

                window.postJson('/Tasks/AddComment', { taskId: parseInt(taskId, 10), content: content })
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (data.success) {
                            window.location.reload(); // simplest correct refresh to show the new comment with author info
                        }
                    });
            });
        }
    }
})();
