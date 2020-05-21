import dispatcher from "./ProcessListDispatcher";
import {EventEmitter} from "events";
import * as ProcessListActions from "./ProcessListActions";
import ProcessColumnsDefinition from "../ProcessColumnsDefinition";

class ProcessListStore extends EventEmitter {

    constructor() {
        super();
        // local copy per message
        this.selectedColumns = ProcessColumnsDefinition.DefaultColumns;
    }

    // single handler for the app for each kind of message
    handleActions(action) {
        switch (action.type) {
            // a cast per message
            case ProcessListActions.SELECTED_COLUMNS_UPDATED_ACTION: {
                this.selectedColumns = action.value;
                this.emit("storeUpdated");
                break;
            }
            default: {
            }
        }
    }

    // a method per message
    getSelectedColumns() {
        return this.selectedColumns;
    }

}

const processListStore = new ProcessListStore();
dispatcher.register(processListStore.handleActions.bind(processListStore));
export default processListStore;

