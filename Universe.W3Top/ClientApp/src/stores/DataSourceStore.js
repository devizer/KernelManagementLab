import dispatcher from "./DataSourceDispatcher";
import {EventEmitter} from "events";
import * as DataSourceActions from "./DataSourceActions";

class DataSourceStore extends EventEmitter {

    constructor() {
        super();
        // local copy per message
        this.activeDataSource = {kind: 'empty'};
        this.connectionStatus = false;
    }

    // single handler for the app for each kind of message
    handleActions(action) {
        switch (action.type) {
            // a cast per message
            case DataSourceActions.DATA_SOURCE_UPDATED_ACTION: {
                this.activeDataSource = action.value;
                this.emit("storeUpdated");
                break;
            }
            case DataSourceActions.CONNECTION_STATUS_UPDATED_ACTION: {
                this.connectionStatus = action.value;
                this.emit("storeUpdated");
                break;
            }
            default: {
            }
        }
    }

    // a method per message
    getDataSource() {
        return this.activeDataSource;
    }
}

const dataSourceStore = new DataSourceStore();
dispatcher.register(dataSourceStore.handleActions.bind(dataSourceStore));
export default dataSourceStore;
