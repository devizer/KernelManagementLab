// _________________________________________________
import React from "react";
import processListStore from "./ProcessListStore";

// on fetch
export const fillCalculatedFields = (processList) =>
{
    const kinds = ["", "Init", "Service", "Container"];
    processList.forEach(process => {

        // rename mixedPriority --> priority? 
        process.priority = process.mixedPriority;

        // convert numeric enum ProcessKind to string
        if (process.rss === 0)
            process.kind = "Kernel";
        else if (process.kind && process.kind >= 1 && process.kind <=3)
            process.kind = kinds[process.kind];
        else
            process.kind = "";

    });
}

export const filterByKind = (processList, rowsFilters) => {
    if (rowsFilters.NeedNoFilter) return processList;
    let ret = [];
    processList.forEach(process => {
        let isIt = false;
        if (rowsFilters.NeedKernelThreads && process.kind === "Kernel") isIt = true;
        else if (rowsFilters.NeedServices && process.kind === "Service") isIt = true;
        else if (rowsFilters.NeedContainers && process.kind === "Container") isIt = true;
        else if (process.kind === "Init") isIt = true;
        if (isIt) ret.push(process);
    });

    return ret;
}

// for debugger only: slice long list 
export const trimLongListInDebugger = (processes) => {
    if (processes.length > 42 && process.env.NODE_ENV !== 'production' && processListStore.getRowsFilters().NeedNoFilter && processListStore.getRowsFilters().TopFilter === 0)
        processes = processes.slice(0, 42);

    return processes;
}

