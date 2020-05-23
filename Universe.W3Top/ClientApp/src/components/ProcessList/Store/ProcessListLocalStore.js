// import {store} from "store"
import ProcessColumnsDefinition from "../ProcessColumnsDefinition";

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

export const StoreVersion = "1.0";

export const setSelectedColumns = (selectedColumns) => {
    store.set("selectedColumns", {ver: StoreVersion, selectedColumns});
};

export const getSelectedColumns = () => {
    let ret = ProcessColumnsDefinition.DefaultColumnKeys;
    let stored = store.get("selectedColumns");
    if (stored && stored.ver === StoreVersion && typeof stored.selectedColumns === "object") {
        ret = stored.selectedColumns;
    }
    
    return ret;
};

