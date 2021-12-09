import dispatcher from "./NavDispatcher";

export const NAV_UPDATED_ACTION = "NAV_UPDATED_ACTION";

export function NavUpdated(navKind) {
    dispatcher.dispatch({
        type: NAV_UPDATED_ACTION,
        value: navKind
    })
}

export const NavDefinitions = {
    Welcome: {        title: "Welcome"},
    NetLiveChart: {   title: "Network Live Chart", path: "/net-v2" },
    LiveMounts: {     title: "Live Mounts",        path: "/mounts" },
    DisksLiveChart: { title: "Disks Live Chart",   path: "/disks" },
    DiskBenchmark: {  title: "Disk Benchmark",     path: "/disk-benchmark" },
    TopProcesses: {   title: "Top Processes",      path: "/processes"}
}

export const RootNavKind = "TopProcesses";

