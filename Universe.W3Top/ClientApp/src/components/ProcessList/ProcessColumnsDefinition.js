class ProcessColumnsDefinition {

    static Headers = [
        {
            caption: "Process", id: "Process",
            columns: [
                { caption: "Pid", field: "Pid" },
                { caption: "Name", field: "Name" },
                { caption: "Priority", field: "Priority" },
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
                { caption: "User+Kernel",       field: "TotalCpuUsage" }, // calculated
                { caption: "User+Kernel, %%",   field: "TotalCpuUsage_PerCents" },
            ]
        },
        {
            caption: "Children CPU Usage", id: "CpuUsage",
            columns: [
                { caption: "User",      field: "ChildrenUserCpuUsage" }, // double in seconds
                { caption: "User, %%",  field: "ChildrenUserCpuUsage_PerCents" },
                { caption: "Kernel",        field: "ChildrenKernelCpuUsage" },
                { caption: "Kernel, %%",    field: "ChildrenKernelCpuUsage_PerCents" },
                { caption: "User+Kernel",       field: "ChildrenTotalCpuUsage" }, // calculated
                { caption: "User+Kernel, %%",   field: "ChildrenTotalCpuUsage_PerCents" },
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
                { caption: "Swapins, current",  field: "MajorPageFaults" },
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
        },
        {
            caption: "", id: "CommandLine",
            columns: [
                { caption:"Command Line", field: "CommandLine" },
            ]
        }

    ]
    
    static DefaultColumns = [
        "Process.Pid",
        "Process.Name",
        "Process.Priority",
        "Process.Uptime",
        "CommandLine.CommandLine"
    ]
}


export default ProcessColumnsDefinition;