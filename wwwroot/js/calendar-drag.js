// Calendar drag-to-reschedule interop module
// Uses SortableJS (loaded globally via script tag in App.razor)
// Uses window.calendarDrag namespace for non-module access from Blazor

window.calendarDrag = {
    sortables: [],

    init: function (dotNetHelper) {
        // Destroy existing instances
        this.sortables.forEach(function (s) { s.destroy(); });
        this.sortables = [];

        // Find all day cells with data-day attribute
        var dayCells = document.querySelectorAll('.day-cell[data-day]');

        dayCells.forEach(function (cell) {
            var sortable = Sortable.create(cell, {
                group: 'calendar-days',
                animation: 150,
                ghostClass: 'workout-chip--dragging',
                dragClass: 'workout-chip--sortable',
                filter: '.day-add-btn, .day-date, .day-header-mobile',
                draggable: '.workout-chip-wrapper',

                onEnd: function (evt) {
                    var wrapper = evt.item;
                    var workoutId = parseInt(wrapper.getAttribute('data-workout-id'));
                    var newDay = evt.to.getAttribute('data-day');

                    if (evt.from !== evt.to && workoutId && newDay) {
                        // Revert DOM change -- let Blazor re-render
                        evt.from.insertBefore(evt.item, evt.from.children[evt.oldIndex]);

                        dotNetHelper.invokeMethodAsync('OnWorkoutDropped', workoutId, newDay);
                    }
                }
            });
            window.calendarDrag.sortables.push(sortable);
        });
    },

    dispose: function () {
        this.sortables.forEach(function (s) { s.destroy(); });
        this.sortables = [];
    }
};
