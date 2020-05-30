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

export const StoreVersion = "1.5";

const GenericStore = (key, getDefault) => {
    return {
        set: (properties) => {
            store.set(key, {ver: StoreVersion, properties});
        },
        get: () => {
            const stored = store.get(key);
            if (stored && stored.ver === StoreVersion && typeof stored.properties === "object") 
                return stored.properties;
            else
                return typeof getDefault === "function" ? getDefault() : getDefault; 
        }
    }
};


// ┌───────────────────────────────────┐
// │  --===## Three Storages ##===--   │
// └───────────────────────────────────┘
export const SelectedColumns = GenericStore("processSelectedColumns", () => ProcessColumnsDefinition.DefaultColumnKeys);
export const RowsFilters = GenericStore("processRowsFilters", () => ProcessRowsFilters.getDefault());
// const defaultSorting = [{ id: 'totalCpuUsage_PerCents', desc: true }]
const defaultSorting = [{ id: 'rss', desc: true }]
export const Sorting = GenericStore("processSorting", () => defaultSorting);
