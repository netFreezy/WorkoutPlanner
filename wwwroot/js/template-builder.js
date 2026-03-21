// Template builder drag-and-drop interop module
// Uses SortableJS (loaded globally via script tag in App.razor)
//
// ARCHITECTURE NOTE: The .exercise-list-sortable container holds a flat
// sequence of SectionHeader divs and ExerciseRow divs. SortableJS operates
// on the entire container as one sortable list. The filter option excludes
// section headers from being draggable. On drop, we compute which section
// the item landed in by scanning backwards from the drop position for the
// nearest section header's data-section attribute, then pass that to .NET
// so OnItemReordered can update both Position and SectionType (D-20).

let sortableInstance = null;

export function initSortable(dotNetRef, containerSelector) {
    // Destroy previous instance if exists
    if (sortableInstance) {
        sortableInstance.destroy();
        sortableInstance = null;
    }

    const el = document.querySelector(containerSelector);
    if (!el) {
        console.warn('SortableJS: container not found:', containerSelector);
        return;
    }

    sortableInstance = new Sortable(el, {
        handle: '.drag-handle',
        animation: 150,
        ghostClass: 'drag-placeholder',
        chosenClass: 'drag-chosen',
        dragClass: 'drag-active',
        fallbackOnBody: true,
        swapThreshold: 0.65,
        filter: '.section-header',  // Section headers are not draggable
        onEnd: function (evt) {
            // CRITICAL: Revert DOM change -- let Blazor re-render
            // SortableJS moves the DOM element, but Blazor owns the DOM.
            // We must put it back and let .NET update the list.
            const item = evt.item;
            const from = evt.from;

            // Determine which section the item landed in (D-20 cross-section drag).
            // Walk backwards from the new index through siblings to find the
            // nearest section header's data-section attribute.
            let newSection = 'Working'; // default fallback
            const children = Array.from(from.children);
            for (let i = evt.newIndex; i >= 0; i--) {
                const child = children[i];
                if (child && child.dataset && child.dataset.section) {
                    newSection = child.dataset.section;
                    break;
                }
            }

            // Remove the moved item from its new position
            item.remove();

            // Re-insert at original position
            if (evt.oldIndex < from.children.length) {
                from.insertBefore(item, from.children[evt.oldIndex]);
            } else {
                from.appendChild(item);
            }

            // Compute exercise-only indices (excluding section headers)
            // so .NET receives indices into its flat Items list, not DOM indices.
            const exerciseRows = children.filter(c => !c.classList.contains('section-header'));
            const oldExerciseIndex = exerciseRows.indexOf(item);
            // For newIndex, count exercise rows before the new position
            let exerciseCountBefore = 0;
            for (let i = 0; i < evt.newIndex; i++) {
                if (!children[i].classList.contains('section-header')) {
                    exerciseCountBefore++;
                }
            }
            const newExerciseIndex = exerciseCountBefore;

            // Notify .NET of the reorder with section info
            dotNetRef.invokeMethodAsync('OnItemReordered', oldExerciseIndex, newExerciseIndex, newSection);
        }
    });
}

export function refreshSortable(containerSelector) {
    // Call after Blazor re-renders the list (add/remove exercises)
    if (sortableInstance) {
        // SortableJS may lose track of new elements
        // Easiest fix: just ensure the instance is still bound
        const el = document.querySelector(containerSelector);
        if (el && sortableInstance.el !== el) {
            // Container element was replaced by Blazor
            sortableInstance.destroy();
            sortableInstance = null;
            // Will be re-initialized on next call to initSortable
        }
    }
}

export function destroySortable() {
    if (sortableInstance) {
        sortableInstance.destroy();
        sortableInstance = null;
    }
}
