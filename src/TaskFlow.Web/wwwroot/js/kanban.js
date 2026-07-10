/* ==========================================================================
   TaskFlow - kanban.js
   Native HTML5 drag-and-drop (no external library needed) for the Kanban
   board. On drop, posts the new status + position to TasksController's
   MoveTask AJAX endpoint. Optimistic UI: the card moves immediately in the
   DOM; if the server call fails, the page is reloaded to resync state.
   ========================================================================== */

(function () {
    'use strict';

    const board = document.getElementById('kanbanBoard');
    if (!board) return;

    let draggedCard = null;

    board.querySelectorAll('.kanban-card').forEach(function (card) {
        card.addEventListener('dragstart', function () {
            draggedCard = card;
            setTimeout(function () { card.classList.add('dragging'); }, 0);
        });

        card.addEventListener('dragend', function () {
            card.classList.remove('dragging');
            draggedCard = null;
        });
    });

    board.querySelectorAll('.kanban-column-body').forEach(function (columnBody) {
        columnBody.addEventListener('dragover', function (e) {
            e.preventDefault();
            const afterElement = getDragAfterElement(columnBody, e.clientY);
            if (!draggedCard) return;

            if (afterElement == null) {
                columnBody.appendChild(draggedCard);
            } else {
                columnBody.insertBefore(draggedCard, afterElement);
            }
        });

        columnBody.addEventListener('drop', function (e) {
            e.preventDefault();
            if (!draggedCard) return;

            const newStatus = columnBody.getAttribute('data-status');
            const taskId = parseInt(draggedCard.getAttribute('data-task-id'), 10);

            const siblings = Array.from(columnBody.querySelectorAll('.kanban-card'));
            const newOrder = siblings.indexOf(draggedCard) + 1;

            updateColumnCount(columnBody);

            window.postJson('/Tasks/MoveTask', {
                taskId: taskId,
                newStatus: newStatus,
                newBoardOrder: newOrder
            }).then(function (res) { return res.json(); })
              .then(function (data) {
                  if (!data.success) {
                      console.error('Move failed:', data.errors);
                      window.location.reload();
                  }
              })
              .catch(function () {
                  window.location.reload();
              });
        });
    });

    function getDragAfterElement(container, y) {
        const draggableElements = Array.from(container.querySelectorAll('.kanban-card:not(.dragging)'));

        return draggableElements.reduce(function (closest, child) {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;
            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child };
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY, element: null }).element;
    }

    function updateColumnCount(columnBody) {
        const column = columnBody.closest('.kanban-column');
        const badge = column.querySelector('.kanban-column-header .badge');
        if (badge) {
            badge.textContent = columnBody.querySelectorAll('.kanban-card').length;
        }
    }
})();
