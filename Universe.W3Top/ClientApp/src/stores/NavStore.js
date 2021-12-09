import dispatcher from "./NavDispatcher";
import {EventEmitter} from "events";
import * as NavActions from "./NavActions";

export const NavDefinitions = {
    Welcome: {        title: "Welcome"},
    NetLiveChart: {   title: "Network Live Chart"},
    LiveMounts: {     title: "Live Mounts"},
    DisksLiveChart: { title: "Disks Live Chart"},
    DiskBenchmark: {  title: "Disk Benchmark"},
    TopProcesses: {   title: "Top Processes"}
}

class NavStore extends EventEmitter {

    constructor() {
        super();
        this.NavKind = 'Welcome';
    }
    
    getNavKind() { 
        return this.NavKind;
    }

    handleActions(action) {
        switch (action.type) {
            // a cast per message
            case NavActions.NAV_UPDATED_ACTION: {
                this.NavKind = action.value;
                this.emit("storeUpdated");
                break;
            }
        }

    }
}

const navStore = new NavStore();
dispatcher.register(navStore.handleActions.bind(navStore));
export default navStore;
