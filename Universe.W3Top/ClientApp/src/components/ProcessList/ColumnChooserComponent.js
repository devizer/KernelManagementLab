import React, {Component} from 'react';
import { withStyles } from '@material-ui/core/styles';
import DialogContent from "@material-ui/core/DialogContent";

import processListStore from "./Store/ProcessListStore";
import Switch from '@material-ui/core/Switch';
import {Checkbox,FormControlLabel,FormControl} from '@material-ui/core';


function CustomCheckbox(props) {
    return (
        <Checkbox {...props}/>
    );
}

class Welcome extends React.Component {
    render() {
        return <h1>Hello, {this.props.name}</h1>;
    }
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



export class ColumnChooserComponent extends Component {
    static displayName = ColumnChooserComponent.name;

    constructor(props) {
        super(props);

        this.state = {
            selectedColumns: processListStore.getSelectedColumns(),
        };
    }

    render () {
        let nowrap={whiteSpace:"nowrap"};
        let space=(<>&nbsp;&nbsp;&nbsp;</>);
        let chbox= (caption) => (
            <><span style={nowrap}><CustomCheckbox color="primary" onChange={() => {}} value={"PID"} />{caption}</span>{space}</>
        );
        
        let tmpOnChange = (key) => {
            return (event) => {
                console.log(`${key} CHANGED: ${event.target.checked}`);
            };
        };
        
        chbox = (caption) => (
            <>&nbsp;<input type="checkbox" onChange={tmpOnChange(caption)} />&nbsp;{caption}&nbsp;&nbsp;&nbsp;</>
        );
        
        let tdSpace=(<><td style={{borderRight:"1px solid grey", width:8}}>&nbsp;</td><td style={{width: 16}}>&nbsp;</td></>)

        let colMemory=88, colProcess=88;
        let colA1=128, colA2=132;
        
        return (
            <>

                <div className="cs-group">
                    Process
                </div>
                <div className="cs-line">
                    <FixedSpan width={colProcess}>{chbox("PID")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Name")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("User")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Priority")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Threads")}</FixedSpan>
                    <FixedSpan width={colProcess}>{chbox("Uptime")}</FixedSpan>
                    {chbox("Command line")}
                </div>

                <div className="cs-group">
                    Memory
                </div>
                <div className="cs-line">
                    <FixedSpan width={colMemory}>{chbox("RSS")}</FixedSpan>
                    <FixedSpan width={colMemory}>{chbox("Shared")}</FixedSpan>
                    {chbox("Swapped")}
                </div>

                <div className="cs-group">
                    IO Time
                </div>
                <div className="cs-line">
                    {chbox("Total, hh:mm:ss")}&nbsp;&nbsp;&nbsp;
                    {chbox("Current, %%")}
                </div>

                <div className="cs-group">
                    IO Transfer
                </div>
                <div style={{position:"relative", height: 83, border: ""}}>

                    <ABS x={0} y={0}>
                        <FixedSpan width={colA1}>Logical Read:</FixedSpan> 
                        {chbox("Total")} 
                        {chbox("Current")}
                    </ABS>
                    <ABS x={4} y={0}>
                        <FixedSpan width={colA2}>Logical Write:</FixedSpan>
                        {chbox("Total")} 
                        {chbox("Current")}
                    </ABS>

                    <ABS x={0} y={1}>
                        <FixedSpan width={colA1}>Block-level Read:</FixedSpan>
                        {chbox("Total")} 
                        {chbox("Current")}
                    </ABS>
                    <ABS x={4} y={1}>
                        <FixedSpan width={colA2}>Block-level Write:</FixedSpan>
                        {chbox("Total")} 
                        {chbox("Current")}
                    </ABS>

                    <ABS x={0} y={2}>
                        <FixedSpan width={colA1}>Read Calls:</FixedSpan>
                        {chbox("Total")} 
                        {chbox("Current")}
                    </ABS>
                    <ABS x={4} y={2}>
                        <FixedSpan width={colA2}>Write Calls:</FixedSpan> 
                        {chbox("Total")} 
                        {chbox("Current")}
                    </ABS>

                </div>

                
                <div className="cs-group">
                    Page Faults
                </div>
                <div style={{position:"relative", height: 58, border: ""}}>

                    <ABS x={0} y={0}>
                        <FixedSpan width={colA1}>Minor:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS>
                    <ABS x={4} y={0}>
                        <FixedSpan width={colA2}>Swap-in:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS>

                    <ABS x={0} y={1}>
                        <FixedSpan width={colA1}>Children Minor:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS>
                    <ABS x={4} y={1}>
                        <FixedSpan width={colA2}>Children Swap-in:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS>

                </div>

                <div className="cs-group">
                    CPU Usage
                </div>
                <div style={{position:"relative", height: 55, border: ""}}>

                    <ABS3 x={0} y={0}>
                        <FixedSpan width={110}><u>Own</u> User:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS3>
                    <ABS3 x={1} y={0}>
                        <FixedSpan width={56}>Kernel:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS3>
                    <ABS3 x={2} y={0}>
                        <FixedSpan width={43}>Sum:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS3>

                    <ABS3 x={0} y={1}>
                        <FixedSpan width={110}><u>Children</u> User:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS3>
                    <ABS3 x={1} y={1}>
                        <FixedSpan width={56}>Kernel:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS3>
                    <ABS3 x={2} y={1}>
                        <FixedSpan width={43}>Sum:</FixedSpan>
                        {chbox("Total")}
                        {chbox("Current")}
                    </ABS3>

                </div>

            </>
        );
    }
}
