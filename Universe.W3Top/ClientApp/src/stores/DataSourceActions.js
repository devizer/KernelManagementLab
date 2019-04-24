// Separate file per kind of message
import dispatcher from "./DataSourceDispatcher";

export const BRIEF_UPDATED_ACTION = "BRIEF_UPDATED_ACTION";
export const DATA_SOURCE_UPDATED_ACTION = "DATA_SOURCE_UPDATED_ACTION";
export const CONNECTION_STATUS_UPDATED_ACTION = "CONNECTION_STATUS_UPDATED_ACTION";


export function DataSourceUpdated(dataSource) {
    dispatcher.dispatch({
        type: DATA_SOURCE_UPDATED_ACTION,
        value: dataSource
    })
}

export function BriefUpdated(briefInfo) {
    dispatcher.dispatch({
        type: BRIEF_UPDATED_ACTION,
        value: briefInfo
    })
}

export function ConnectionStatusUpdated(connectionStatus) {
    dispatcher.dispatch({
        type: CONNECTION_STATUS_UPDATED_ACTION,
        value: connectionStatus
    })
}
