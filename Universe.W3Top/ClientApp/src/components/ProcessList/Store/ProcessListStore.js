import dispatcher from "./ProcessListDispatcher";
import {EventEmitter} from "events";
import * as ProcessListActions from "./ProcessListActions";
import * as ProcessListLocalStore from "./ProcessListLocalStore";
import ProcessColumnsDefinition from "../ProcessColumnsDefinition";

class ProcessListStore extends EventEmitter {

    constructor() {
        super();
        // local copy per message
        // this.selectedColumns = ProcessColumnsDefinition.DefaultColumnKeys;
        this.selectedColumns = ProcessListLocalStore.getSelectedColumns();
        this.rowsFilters = ProcessListLocalStore.getProcessRowsFilters();
        this.processList = [];
    }
    
/*
    preloadSelectedColumns()
    {
        
    }
*/

    // single handler for the app for each kind of message
    handleActions(action) {
        switch (action.type) {
            // a cast per message
            case ProcessListActions.SELECTED_COLUMNS_UPDATED_ACTION: {
                this.selectedColumns = action.value;
                this.emit("storeUpdated");
                break;
            }
            case ProcessListActions.PROCESS_LIST_UPDATED_ACTION: {
                this.processList = action.value;
                this.emit("storeUpdated");
                break;
            }
            case ProcessListActions.ROWS_FILTERS_UPDATED_ACTION: {
                this.rowsFilters = action.value;
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
    
    getProcessList() {
        return this.processList;
    }
    
    getRowsFilters() {
        return this.rowsFilters;
    }
}

const processListStore = new ProcessListStore();
dispatcher.register(processListStore.handleActions.bind(processListStore));
export default processListStore;

