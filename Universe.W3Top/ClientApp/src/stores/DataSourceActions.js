// Separate file per kind of message
import dispatcher from "./DataSourceDispatcher";

export const DATA_SOURCE_UPDATED_ACTION = "DATA_SOURCE_UPDATED_ACTION";
export const CONNECTION_STATUS_UPDATED_ACTION = "DATA_SOURCE_UPDATED_ACTION";


export function DataSourceUpdated(dataSource) {
    dispatcher.dispatch({
        type: DATA_SOURCE_UPDATED_ACTION,
        value: dataSource
    })
}

export function ConnectionStatusUpdated(connectionStatus) {
    dispatcher.dispatch({
        type: CONNECTION_STATUS_UPDATED_ACTION,
        value: connectionStatus
    })
}
