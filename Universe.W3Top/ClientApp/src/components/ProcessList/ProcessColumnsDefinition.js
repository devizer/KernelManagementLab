import * as Enumerable from "linq-es2015"
import * as Helper from "../../Helper";

class ProcessColumnsDefinition {

    static Headers = [
        {
            caption: "Process", id: "Process",
            columns: [
                { caption: "PID", field: "pid" },
                { caption: "Name", field: "name" }, // calculated
                { caption: "User", field: "user" }, // calculated: UID + Name
                { caption: "Priority", field: "priority" }, // priority // calculated
                { caption: "Threads", field: "numThreads" },
                { caption: "Uptime", field: "uptime" }
            ]
        },
        {
            caption: "Memory", id: "Memory",
            columns: [
                { caption: "RSS", field: "rss" },
                { caption: "Shared", field: "shared" },
                { caption: "Swapped", field: "swapped" }
            ]
        },
        {
            caption: "CPU Usage", id: "CpuUsage",
            columns: [
                { caption: "User",      field: "userCpuUsage" }, // double in seconds
                { caption: "User, %%",  field: "userCpuUsage_PerCents" },
                { caption: "Kernel",        field: "kernelCpuUsage" },
                { caption: "Kernel, %%",    field: "kernelCpuUsage_PerCents" },
                { caption: "∑ = User+Kernel",       field: "totalCpuUsage" }, // calculated
                { caption: "∑ = User+Kernel, %%",   field: "totalCpuUsage_PerCents" },
            ]
        },
        {
            caption: "Children CPU Usage", id: "ChildrenCpuUsage",
            columns: [
                { caption: "User",      field: "childrenUserCpuUsage" }, // double in seconds
                { caption: "User, %%",  field: "childrenUserCpuUsage_PerCents" },
                { caption: "Kernel",        field: "childrenKernelCpuUsage" },
                { caption: "Kernel, %%",    field: "childrenKernelCpuUsage_PerCents" },
                { caption: "∑ = User+Kernel",       field: "childrenTotalCpuUsage" }, // calculated
                { caption: "∑ = User+Kernel, %%",   field: "childrenTotalCpuUsage_PerCents" },
            ]
        },
        {
            caption: "IO Time", id: "IoTime",
            columns: [
                { caption: "Total", field: "ioTime" }, // hh:mm:ss, double in seconds
                { caption: "Current, %%", field: "ioTime_PerCents" },
            ]
        },
        {
            // total: GB/s, current: MB/s
            caption: "IO Transfer", id: "IoTransfer",
            columns: [
                { caption: "Read (logical), total",     field: "readBytes" },
                { caption: "Read (logical), current",   field: "readBytes_Current" }, // todo
                { caption: "Write (logical), total",    field: "writeBytes" },
                { caption: "Write (logical), current",  field: "writeBytes_Current" }, // todo
                { caption: "Read (block level)",            field: "readBlockBackedBytes" },
                { caption: "Read (block level), current",   field: "readBlockBackedBytes_Current" },
                { caption: "Write (block level)",           field: "writeBlockBackedBytes" },
                { caption: "Write (block level), current",  field: "writeBlockBackedBytes_Current" },
                { caption: "Read Calls",            field: "readSysCalls" },
                { caption: "Read Calls, current",   field: "readSysCalls_Current" },
                { caption: "Write Calls",           field: "writeSysCalls" },
                { caption: "Write Calls, current",  field: "writeSysCalls_Current" },
            ]
        },
        {
            caption: "Page Faults", id: "PageFaults",
            columns: [
                { caption: "Minor",             field: "minorPageFaults" },
                { caption: "Minor, current",    field: "minorPageFaults_Current" },
                { caption: "Swapins",           field: "majorPageFaults" },
                { caption: "Swapins, current",  field: "majorPageFaults_Current" },
            ]
        },
        {
            caption: "Children Page Faults", id: "ChildrenPageFaults",
            columns: [
                { caption: "Minor",             field: "childrenMinorPageFaults" },
                { caption: "Minor, current",    field: "childrenMinorPageFaults_Current" },
                { caption: "Swapins",           field: "childrenMajorPageFaults" },
                { caption: "Swapins, current",  field: "childrenMajorPageFaults_Current" },
            ]
            // ChildrenMinorPageFaults
        },
        {
            caption: "Command Line", id: "CommandLine",
            columns: [
                { caption:"", field: "commandLine" },
            ]
        }

    ];
    
    static DefaultColumnKeys = [
        "Process.pid",
        "Process.name",
        "Process.priority",
        "Process.uptime",
        "Memory.rss",
        "Memory.shared",
        "Memory.swapped",
        "IoTime.ioTime",
        "IoTime.ioTime_PerCents",
        "IoTransfer.readBytes",
        "IoTransfer.readBytes_Current",
        "IoTransfer.writeBytes",
        "IoTransfer.writeBytes_Current",
        "PageFaults.majorPageFaults_Current",
        "CpuUsage.totalCpuUsage_PerCents",
        "ChildrenCpuUsage.childrenTotalCpuUsage_PerCents",
        // "CommandLine.CommandLine",
    ];
    
    static AllColumnKeys = []; // array of group.field
    
    static isValidColumnKey(columnKey) {
        return ProcessColumnsDefinition.AllColumnKeys.indexOf(columnKey) >= 0;
    }
    
}

function fillAllColumnKeys() {
    ProcessColumnsDefinition.Headers.forEach(header => {
        header.columns.forEach(column => {
            ProcessColumnsDefinition.AllColumnKeys.push(`${header.id}.${column.field}`);
        });
    });
}

fillAllColumnKeys();

function validateDefaultColumnKeys() {
    let all = ProcessColumnsDefinition.AllColumnKeys;
    Helper.log(`ProcessColumnsDefinition.getAllColumnKeys(): ${all}`);
    let errors = [];
    ProcessColumnsDefinition.DefaultColumnKeys.forEach(columnKey => {
        if (!Enumerable.AsEnumerable(all).Any(x => x === columnKey))
            errors.push(columnKey);
    });
    
    if (errors.length > 0)
        throw `Wrong keys in ProcessColumnsDefinition.DefaultColumnKeys: ${errors}`;
}

validateDefaultColumnKeys();
export default ProcessColumnsDefinition;