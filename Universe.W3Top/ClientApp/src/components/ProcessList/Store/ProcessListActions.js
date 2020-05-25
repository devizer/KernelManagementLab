import dispatcher from "./ProcessListDispatcher";
import * as ProcessListLocalStore from "./ProcessListLocalStore";

export const SELECTED_COLUMNS_UPDATED_ACTION = "SELECTED_COLUMNS_UPDATED_ACTION";
export const PROCESS_LIST_UPDATED_ACTION = "PROCESS_LIST_UPDATED_ACTION";
export const ROWS_FILTERS_UPDATED_ACTION = "ROWS_FILTERS_UPDATED_ACTION";

export function SelectedColumnsUpdated(selectedColumns) {
    dispatcher.dispatch({
        type: SELECTED_COLUMNS_UPDATED_ACTION,
        value: selectedColumns
    });
    
    ProcessListLocalStore.setSelectedColumns(selectedColumns);
}

export function ProcessListUpdated(processes) {
    dispatcher.dispatch({
        type: PROCESS_LIST_UPDATED_ACTION,
        value: processes
    })
}

export function RowsFiltersUpdated(rowsFilters) {
    dispatcher.dispatch({
        type: ROWS_FILTERS_UPDATED_ACTION,
        value: rowsFilters
    });

    ProcessListLocalStore.setProcessRowsFilters(rowsFilters);
}
