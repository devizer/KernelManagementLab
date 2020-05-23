import React, {Component} from 'react';
import { withStyles } from '@material-ui/core/styles';
import DialogContent from "@material-ui/core/DialogContent";

import processListStore from "./Store/ProcessListStore";
import * as ProcessListActions from "./Store/ProcessListActions"
import Switch from '@material-ui/core/Switch';
import {Checkbox,FormControlLabel,FormControl} from '@material-ui/core';
import ProcessColumnsDefinition from "./ProcessColumnsDefinition";

function CustomCheckbox(props) {
    return (
        <Checkbox {...props}/>
    );
}

class FixedSpan extends React.Component {
    render() {
        return (<span style={{whiteSpace: "nowrap", display: "inline-block", width: this.props.width, minWidth: this.props.width}}>{this.props.children}</span>);
    }
}

class IoTransferLayout extends React.Component {
    render() {
        let x = this.props.x, y = this.props.y
        let widths = [115, 95, 115, 45, 120, 95, 115];
        let height = 28;
        let left = 0; for (let c = 0; c < x; c++) left += widths[c];
        let top = y * height - 2;
        return (
            <div style={{position: "absolute", left, top, whiteSpace:"nowrap"}}>
                {this.props.children}
            </div>
        );
    }
};

class CpUsageLayout extends React.Component {
    render() {
        let x = this.props.x, y = this.props.y;
        let widths = [302, 302, 302];
        let height = 28;
        let left = 0; for (let c = 0; c < x; c++) left += widths[c];
        let top = y * height - 2;
        return (
            <div style={{position: "absolute", left, top, whiteSpace:"nowrap"}}>
                {this.props.children}
            </div>
        );
    }
};

let ABS=IoTransferLayout;
let ABS3=CpUsageLayout;

let firstRender = true; 

export class ColumnChooserComponent extends Component {
    static displayName = ColumnChooserComponent.name;

    constructor(props) {
        super(props);

        this.state = {
            selectedColumns: processListStore.getSelectedColumns(),
        };
    }
    
    componentDidMount() {
        firstRender = false;
    }

    render () {
        let nowrap={whiteSpace:"nowrap"};
        let space=(<>&nbsp;&nbsp;&nbsp;</>);
        let chbox= (caption) => (
            <><span style={nowrap}><CustomCheckbox color="primary" onChange={() => {}} value={"PID"} />{caption}</span>{space}</>
        );
        
        let checkOnChange = (caption, id) => {
            return (event) => {
                console.log(`${caption} (#${id}) CHANGED: ${event.target.checked}`);
                let isChecked = event.target.checked;
                let copy = [...this.state.selectedColumns];
                if (isChecked) copy.push(id);
                else {
                    let filtered = copy.filter(x => x !== id); 
                    copy = filtered;
                }
                this.setState({selectedColumns:copy});
                ProcessListActions.SelectedColumnsUpdated(copy);
            };
        };
        
        // let selectedColumns = processListStore.getSelectedColumns(); // array of group.field
        let selectedColumns = this.state.selectedColumns;
        chbox = (caption, id) => {
            if (firstRender && ProcessColumnsDefinition.AllColumnKeys.indexOf(id) < 0)
                throw `Wrong columns id [${id}] on the ${ColumnChooserComponent.name}`;
            
            // let isChecked = processListStore.getSelectedColumns().indexOf(id) >= 0;
            let isChecked = this.state.selectedColumns.indexOf(id) >= 0;
            return (
                <>&nbsp;<input type="checkbox" checked={isChecked} onChange={checkOnChange(caption, id)} />&nbsp;{caption}&nbsp;&nbsp;&nbsp;</>
            )
        };
        
        let tdSpace=(<><td style={{borderRight:"1px solid grey", width:8}}>&nbsp;</td><td style={{width: 16}}>&nbsp;</td></>)

        let colMemory=88, colProcess=88;
        let colA1=128, colA2=132;
        
        return (
            <>

                <div className="cs-group">
                    Process
                </div>
                <div className="cs-line">
                    <FixedSpan width={colProcess}>{chbox("PID", "Process.Pid")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Name", "Process.Name")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("User", "Process.User")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Priority", "Process.Priority")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Threads", "Process.NumThreads")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Uptime", "Process.Uptime")}</FixedSpan>
                    {chbox("Command line", "CommandLine.CommandLine")}
                </div>

                <div className="cs-group">
                    Memory
                </div>
                <div className="cs-line">
                    <FixedSpan width={colMemory}>{chbox("RSS", "Memory.RSS")}</FixedSpan>
                    <FixedSpan width={colMemory}>{chbox("Shared", "Memory.Shared")}</FixedSpan>
                    {chbox("Swapped", "Memory.Swapped")}
                </div>

                <div className="cs-group">
                    IO Time
                </div>
                <div className="cs-line">
                    {chbox("Total, hh:mm:ss", "IoTime.IoTime")}&nbsp;&nbsp;&nbsp;
                    {chbox("Current, %%", "IoTime.IoTime_PerCents")}
                </div>

                <div className="cs-group">
                    IO Transfer
                </div>
                <div style={{position:"relative", height: 83, border: ""}}>

                    <ABS x={0} y={0}>
                        <FixedSpan width={colA1}>Logical Read:</FixedSpan> 
                        {chbox("Total", "IoTransfer.ReadBytes")} 
                        {chbox("Current", "IoTransfer.ReadBytes_Current")}
                    </ABS>
                    <ABS x={4} y={0}>
                        <FixedSpan width={colA2}>Logical Write:</FixedSpan>
                        {chbox("Total", "IoTransfer.WriteBytes")} 
                        {chbox("Current", "IoTransfer.WriteBytes_Current")}
                    </ABS>

                    <ABS x={0} y={1}>
                        <FixedSpan width={colA1}>Block-level Read:</FixedSpan>
                        {chbox("Total", "IoTransfer.ReadBlockBackedBytes")} 
                        {chbox("Current", "IoTransfer.ReadBlockBackedBytes_Current")}
                    </ABS>
                    <ABS x={4} y={1}>
                        <FixedSpan width={colA2}>Block-level Write:</FixedSpan>
                        {chbox("Total", "IoTransfer.WriteBlockBackedBytes")} 
                        {chbox("Current", "IoTransfer.WriteBlockBackedBytes_Current")}
                    </ABS>

                    <ABS x={0} y={2}>
                        <FixedSpan width={colA1}>Read Calls:</FixedSpan>
                        {chbox("Total", "IoTransfer.ReadSysCalls")} 
                        {chbox("Current", "IoTransfer.ReadSysCalls_Current")}
                    </ABS>
                    <ABS x={4} y={2}>
                        <FixedSpan width={colA2}>Write Calls:</FixedSpan> 
                        {chbox("Total", "IoTransfer.WriteSysCalls")} 
                        {chbox("Current", "IoTransfer.WriteSysCalls_Current")}
                    </ABS>

                </div>

                
                <div className="cs-group">
                    Page Faults
                </div>
                <div style={{position:"relative", height: 58, border: ""}}>

                    <ABS x={0} y={0}>
                        <FixedSpan width={colA1}>Minor:</FixedSpan>
                        {chbox("Total", "PageFaults.MinorPageFaults")}
                        {chbox("Current", "PageFaults.MinorPageFaults_Current")}
                    </ABS>
                    <ABS x={4} y={0}>
                        <FixedSpan width={colA2}>Swap-in:</FixedSpan>
                        {chbox("Total", "PageFaults.MajorPageFaults")}
                        {chbox("Current", "PageFaults.MajorPageFaults_Current")}
                    </ABS>

                    <ABS x={0} y={1}>
                        <FixedSpan width={colA1}>Children Minor:</FixedSpan>
                        {chbox("Total", "ChildrenPageFaults.ChildrenMinorPageFaults")}
                        {chbox("Current", "ChildrenPageFaults.ChildrenMinorPageFaults_Current")}
                    </ABS>
                    <ABS x={4} y={1}>
                        <FixedSpan width={colA2}>Children Swap-in:</FixedSpan>
                        {chbox("Total", "ChildrenPageFaults.ChildrenMajorPageFaults")}
                        {chbox("Current", "ChildrenPageFaults.ChildrenMajorPageFaults_Current")}
                    </ABS>

                </div>

                <div className="cs-group">
                    CPU Usage
                </div>
                <div style={{position:"relative", height: 55, border: ""}}>

                    <ABS3 x={0} y={0}>
                        <FixedSpan width={110}>Own User:</FixedSpan>
                        {chbox("Total", "CpuUsage.UserCpuUsage")}
                        {chbox("Current", "CpuUsage.UserCpuUsage_PerCents")}
                    </ABS3>
                    <ABS3 x={1} y={0}>
                        <FixedSpan width={56}>Kernel:</FixedSpan>
                        {chbox("Total", "CpuUsage.KernelCpuUsage")}
                        {chbox("Current", "CpuUsage.KernelCpuUsage_PerCents")}
                    </ABS3>
                    <ABS3 x={2} y={0}>
                        <FixedSpan width={43}>Sum:</FixedSpan>
                        {chbox("Total", "CpuUsage.TotalCpuUsage")}
                        {chbox("Current", "CpuUsage.TotalCpuUsage_PerCents")}
                    </ABS3>

                    <ABS3 x={0} y={1}>
                        <FixedSpan width={110}>Children User:</FixedSpan>
                        {chbox("Total", "ChildrenCpuUsage.ChildrenUserCpuUsage")}
                        {chbox("Current", "ChildrenCpuUsage.ChildrenUserCpuUsage_PerCents")}
                    </ABS3>
                    <ABS3 x={1} y={1}>
                        <FixedSpan width={56}>Kernel:</FixedSpan>
                        {chbox("Total", "ChildrenCpuUsage.ChildrenKernelCpuUsage")}
                        {chbox("Current", "ChildrenCpuUsage.ChildrenKernelCpuUsage_PerCents")}
                    </ABS3>
                    <ABS3 x={2} y={1}>
                        <FixedSpan width={43}>Sum:</FixedSpan>
                        {chbox("Total", "ChildrenCpuUsage.ChildrenKernelCpuUsage")}
                        {chbox("Current", "ChildrenCpuUsage.ChildrenKernelCpuUsage_PerCents")}
                    </ABS3>

                </div>

            </>
        );
    }
}
