// import {store} from "store"
import ProcessColumnsDefinition from "../ProcessColumnsDefinition";
import {ProcessRowsFilters} from "../ProcessRowsFilters";

// session store with watch
import engine from 'store/src/store-engine';
import sessionStorage from 'store/storages/sessionStorage';
import cookieStorage from 'store/storages/cookieStorage';
import localStorage from 'store/storages/localStorage';

import defaultPlugin from 'store/plugins/defaults';
import expiredPlugin from 'store/plugins/expire';
import eventsPlugin from 'store/plugins/events';

const storages = [localStorage, sessionStorage, cookieStorage, ];
const plugins = [defaultPlugin, expiredPlugin, eventsPlugin];
const store = engine.createStore(storages, plugins);

export const StoreVersion = "1.4";

export const setSelectedColumns = (selectedColumns) => {
    store.set("selectedColumns", {ver: StoreVersion, selectedColumns});
};

export const setProcessRowsFilters = (processRowsFilters) => {
    store.set("processRowsFilters", {ver: StoreVersion, processRowsFilters});
};

export const getSelectedColumns = () => {
    let ret = ProcessColumnsDefinition.DefaultColumnKeys;
    let stored = store.get("selectedColumns");
    if (stored && stored.ver === StoreVersion && typeof stored.selectedColumns === "object") {
        ret = stored.selectedColumns;
    }

    return [...ret];
};

export const getProcessRowsFilters = () => {
    let ret = ProcessRowsFilters.getDefault();
    let stored = store.get("processRowsFilters");
    if (stored && stored.ver === StoreVersion && typeof stored.processRowsFilters === "object") {
        ret = stored.processRowsFilters;
    }

    return ret;
};

export const fillCalculatedFields = (processList) =>
{
    const kinds = ["", "Init", "Service", "Container"];
    processList.forEach(process => {
        process.priority = process.mixedPriority;
        if (process.kind && process.kind >= 1 && process.kind <=3)
            process.kind = kinds[process.kind];
        else
            process.kind = "";
    });
    
}