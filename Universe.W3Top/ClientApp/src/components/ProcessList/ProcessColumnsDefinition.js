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