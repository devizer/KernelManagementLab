import React from 'react';
import MomentFormat from 'moment';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {faCheck, faCheckDouble} from '@fortawesome/free-solid-svg-icons'
import * as Helper from "../../Helper";

import { withStyles, MuiThemeProvider, createMuiTheme } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';

const widths = {
    iops: 116,     // right aligned
    spaceIops: 4,
    iopsScale: 24, // static label IOPS, left aligned, grey
    bandwidth: 116, // right aligned
    spaceBandwidth: 4, // bandwidth ]---[ bandwidth units 
    bandwidthUnits: 78, // grey, left aligned
    operation: 22, // vertical
};
widths.panel = 22 + 78 + 4 + 116 + 24 + 4 + 116; // 364
widths.panelSpace = 22;
widths.parameters = 11;

const heights = {
    panel: 72,
    panelSpace: 16,
    metrics: 38, 
};

const styles = {
    main: {
        width: widths.panel * 2 + widths.panelSpace + widths.parameters,
        height: 4 * heights.panel + 3 * heights.panelSpace,
        position: "relative",
        // border: "1px solid green",
    },
    panel: {
        position: "absolute",
        width: widths.panel,
        height: heights.panel,
        margin: 0,
        padding: 0,
        backgroundColor: "#EBECEF",
        border: "1px solid #B7B9BF",
        color: "black",
    },
    verticalAction: {
        position: "absolute",
        zIndex: 9999,
        left: 322,
        top: 24,
        // paddingTop: "-30px",
        transform: "translate(0px, 0px) rotate(-90deg)",
        whiteSpace: "nowrap",
        display: "block",
        // bottom: 0px, 
        width: heights.panel,
        height: widths.operation,
        lineHeight: `${widths.operation}px`,
        verticalAlign: "middle",
        border: "none",
        textAlign: "center",
        fontSize: "11px", fontWeight: 'bold', letterSpacing: "0.5px",
        backgroundColor: "#B7B9BF",
        color: "white",
    },
    verticalParameters: {
        position: "absolute",
        // zIndex: 9999,
        transform: "translate(0px, 0px) rotate(-90deg)",
        whiteSpace: "nowrap",
        display: "block",
        width: heights.panel,
        height: widths.parameters,
        lineHeight: `${widths.parameters + 2}px`,
        verticalAlign: "middle",
        border: "none",
        textAlign: "center",
        fontSize: "13px", fontWeight: 'bold', letterSpacing: "0.5px",
        backgroundColor: "", // #CDAFCE 
        color: "black",
    },
}

function ParametersPanel(yPosition, parameters) {
    const left = widths.parameters - 48;
    const top = 31 + yPosition * (heights.panel + heights.panelSpace);
    const panelStyles = {...styles.verticalParameters, left: left, top: top};
    return (
        <React.Fragment>
            <div style={panelStyles}>{parameters}</div>
        </React.Fragment>
    );
}
function ActionPanel(xPosition, yPosition, action) {
    const left = widths.parameters + xPosition * (widths.panel + widths.panelSpace);
    const top = yPosition * (heights.panel + heights.panelSpace);
    const panelStyles = {...styles.panel, left: left, top: top};
    return (
        <React.Fragment>
            <div style={panelStyles}>
                <br/>under contruction
                <div style={styles.verticalAction}>
                    {action}
                </div>
            </div>
        </React.Fragment>
    );
}

export class DiskBenchmarkResult extends React.Component {
    static displayName =  DiskBenchmarkResult.name;

    constructor(props) {
        super(props);
        
        this.state = {
            opened: false,
            selectedRow: null,
        };
        
        this.handleClose = this.handleClose.bind(this);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        const isOpenedChanged = prevProps.opened !== this.props.opened;
        const isRowChanged = (prevProps.selectedRow ? prevProps.selectedRow.token : '') !== (this.props.selectedRow ? this.props.selectedRow.token : '');   
        if (isOpenedChanged || isRowChanged) {
            this.setState({
                opened: this.props.opened,
                selectedRow: this.props.selectedRow,
            });
        }
    }

    handleClose() {
        this.setState({opened: false});
        // alert("CLOSED, opened: " + this.state.opened);
    }

    render() {
        // Helper.toConsole(`[DiskBenchmarkResult::render] this.state.opened=${this.state.opened}`);
        
        return (
            <Dialog open={this.state.opened} onClose={this.handleClose} aria-labelledby="form-dialog-title" fullWidth={false} maxWidth={"md"}>
                <DialogContent style={{textAlign: "center"}} >
                    <div style={styles.main}>
                        {ActionPanel(1, 0, "Allocate")}
                        {ActionPanel(0, 1, "Read")}
                        {ActionPanel(1, 1, "Write")}
                        {ActionPanel(0, 2, "Read 4K")}
                        {ActionPanel(1, 2, "Write 4K")}
                        {ActionPanel(0, 3, "Read 4K")}
                        {ActionPanel(1, 3, "Write 4K")}
                        {ParametersPanel(1,"SEQ")}
                        {ParametersPanel(2,"RND 1Q")}
                        {ParametersPanel(3,"RND 64Q")}
                    </div>
                    <div style={{wordBreak:"break-all", wordWrap: "break-word", display: "none"}}>
                        {JSON.stringify(this.state.selectedRow)}
                    </div>
{/*
                    <br/>
                    a<br/>
                    aa<br/>
                    aaa<br/>
                    aaaaa<br/>
                    aaaaaa<br/>
                    aaaaaa<br/>
                    aaaaaaa<br/>
                    aaaaaaaa<br/>
                    aaaaaaaaaa<br/>
                    aaaaaaaaaaa<br/>
                    aaaaaaaaaaaa
*/}
                </DialogContent>
            </Dialog>
        );
    }

}