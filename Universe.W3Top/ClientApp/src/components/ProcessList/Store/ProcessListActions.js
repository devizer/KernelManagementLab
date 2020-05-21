import dispatcher from "./ProcessListDispatcher";

export const SELECTED_COLUMNS_UPDATED_ACTION = "SELECTED_COLUMNS_UPDATED_ACTION";
export const PROCESS_LIST_UPDATED_ACTION = "PROCESS_LIST_UPDATED_ACTION";

export function SelectedColumnsUpdated(selectedColumns) {
    dispatcher.dispatch({
        type: SELECTED_COLUMNS_UPDATED_ACTION,
        value: selectedColumns
    })
}

export function ProcessListUpdated(processes) {
    dispatcher.dispatch({
        type: PROCESS_LIST_UPDATED_ACTION,
        value: processes
    })
}
