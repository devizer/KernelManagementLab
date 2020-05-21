import dispatcher from "./ProcessListDispatcher";

export const SELECTED_COLUMNS_UPDATED_ACTION = "SELECTED_COLUMNS_UPDATED_ACTION";

export function DataSourceUpdated(selectedColumns) {
    dispatcher.dispatch({
        type: SELECTED_COLUMNS_UPDATED_ACTION,
        value: selectedColumns
    })
}
