import * as Enumerable from "linq-es2015"
import * as Helper from "../../Helper";

class ProcessColumnsDefinition {

    static Headers = [
        {
            caption: "Process", id: "Process",
            columns: [
                { caption: "Pid", field: "Pid" },
                { caption: "Name", field: "Name" }, // calculated
                { caption: "User", field: "User" }, // calculated: UID + Name
                { caption: "Priority", field: "Priority" }, // calculated
                { caption: "Threads", field: "NumThreads" },
                { caption: "Uptime", field: "Uptime" }
            ]
        },
        {
            caption: "Memory", id: "Memory",
            columns: [
                { caption: "RSS", field: "RSS" },
                { caption: "Shared", field: "Shared" },
                { caption: "Swapped", field: "Swapped" }
            ]
        },
        {
            caption: "CPU Usage", id: "CpuUsage",
            columns: [
                { caption: "User",      field: "UserCpuUsage" }, // double in seconds
                { caption: "User, %%",  field: "UserCpuUsage_PerCents" },
                { caption: "Kernel",        field: "KernelCpuUsage" },
                { caption: "Kernel, %%",    field: "KernelCpuUsage_PerCents" },
                { caption: "∑ = User+Kernel",       field: "TotalCpuUsage" }, // calculated
                { caption: "∑ = User+Kernel, %%",   field: "TotalCpuUsage_PerCents" },
            ]
        },
        {
            caption: "Children CPU Usage", id: "ChildrenCpuUsage",
            columns: [
                { caption: "User",      field: "ChildrenUserCpuUsage" }, // double in seconds
                { caption: "User, %%",  field: "ChildrenUserCpuUsage_PerCents" },
                { caption: "Kernel",        field: "ChildrenKernelCpuUsage" },
                { caption: "Kernel, %%",    field: "ChildrenKernelCpuUsage_PerCents" },
                { caption: "∑ = User+Kernel",       field: "ChildrenTotalCpuUsage" }, // calculated
                { caption: "∑ = User+Kernel, %%",   field: "ChildrenTotalCpuUsage_PerCents" },
            ]
        },
        {
            caption: "IO Time", id: "IoTime",
            columns: [
                { caption: "Total", field: "IoTime" }, // hh:mm:ss, double in seconds
                { caption: "Current, %%", field: "IoTime_PerCents" },
            ]
        },
        {
            // total: GB/s, current: MB/s
            caption: "IO Transfer", id: "IoTransfer",
            columns: [
                { caption: "Read (logical), total",     field: "ReadBytes" },
                { caption: "Read (logical), current",   field: "ReadBytes_Current" }, // todo
                { caption: "Write (logical), total",    field: "WriteBytes" },
                { caption: "Write (logical), current",  field: "WriteBytes_Current" }, // todo
                { caption: "Read (block level)",            field: "ReadBlockBackedBytes" },
                { caption: "Read (block level), current",   field: "ReadBlockBackedBytes_Current" },
                { caption: "Write (block level)",           field: "WriteBlockBackedBytes" },
                { caption: "Write (block level), current",  field: "WriteBlockBackedBytes_Current" },
                { caption: "Read Calls",            field: "ReadSysCalls" },
                { caption: "Read Calls, current",   field: "ReadSysCalls_Current" },
                { caption: "Write Calls",           field: "WriteSysCalls" },
                { caption: "Write Calls, current",  field: "WriteSysCalls_Current" },
            ]
        },
        {
            caption: "Page Faults", id: "PageFaults",
            columns: [
                { caption: "Minor",             field: "MinorPageFaults" },
                { caption: "Minor, current",    field: "MinorPageFaults_Current" },
                { caption: "Swapins",           field: "MajorPageFaults" },
                { caption: "Swapins, current",  field: "MajorPageFaults_Current" },
            ]
        },
        {
            caption: "Children Page Faults", id: "ChildrenPageFaults",
            columns: [
                { caption: "Minor",             field: "ChildrenMinorPageFaults" },
                { caption: "Minor, current",    field: "ChildrenMinorPageFaults_Current" },
                { caption: "Swapins",           field: "ChildrenMajorPageFaults" },
                { caption: "Swapins, current",  field: "ChildrenMajorPageFaults_Current" },
            ]
            // ChildrenMinorPageFaults
        },
        {
            caption: "Command Line", id: "CommandLine",
            columns: [
                { caption:"", field: "CommandLine" },
            ]
        }

    ];
    
    static DefaultColumnKeys = [
        "Process.Pid",
        "Process.Name",
        "Process.Priority",
        "Process.Uptime",
        "Memory.RSS",
        "Memory.Shared",
        "Memory.Swapped",
        "IoTime.IoTime",
        "IoTime.IoTime_PerCents",
        "IoTransfer.ReadBytes",
        "IoTransfer.ReadBytes_Current",
        "IoTransfer.WriteBytes",
        "IoTransfer.WriteBytes_Current",
        "PageFaults.MajorPageFaults_Current",
        "CpuUsage.TotalCpuUsage_PerCents",
        "ChildrenCpuUsage.ChildrenTotalCpuUsage_PerCents",
        "CommandLine.CommandLine"
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