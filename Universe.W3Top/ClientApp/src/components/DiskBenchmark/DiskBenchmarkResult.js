import React from 'react';
import MomentFormat from 'moment';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {faCheck, faCheckDouble} from '@fortawesome/free-solid-svg-icons'
import * as Helper from "../../Helper";
import { ReactComponent as CopyToCloudIconSvg } from '../../icons/copy2cloud-v2.svg';
import { ReactComponent as ShareIconSvg } from '../../icons/Share-Icon.svg';
import { ReactComponent as ShareOutlinedIconSvg } from '../../icons/Share-Outlined-Icon.svg';
import { withStyles, MuiThemeProvider, createMuiTheme } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import {SharedDiskBenchmarkFlow} from "../../SharedDiskBenchmarkFlow";
import {ReactComponent as MainIconSvg} from "../../icons/w3top-3.svg";
import IconButton from "@material-ui/core/IconButton";
import classNames from "classnames";

const CopyToCloudIcon = ({size=16,color='black'}) => (<CopyToCloudIconSvg style={{width: size,height:size,fill:color,strokeWidth:'3px',stroke:color }} />);
const ShareIconSize=18;
const ShareIcon = ({size=ShareIconSize,color='#BBB'}) => (<ShareOutlinedIconSvg style={{width: size, height: size, fill:color,strokeWidth:'1px',stroke:color }} />);


function selectNodeText(containerid) {
    if (document.selection) { // IE
        var range = document.body.createTextRange();
        range.moveToElementText(document.getElementById(containerid));
        range.select();
    } else if (window.getSelection) {
        var range = document.createRange();
        range.selectNode(document.getElementById(containerid));
        window.getSelection().removeAllRanges();
        window.getSelection().addRange(range);
    }
}

function canAccessClipboard() {
    const loc = window && window.location ? window.location : null;
    if (!loc) return false;
    return loc.protocol === "https:" || loc.hostname === "localhost";
}

function fallbackCopyTextToClipboard(text) {
    var textArea = document.createElement("textarea");
    textArea.value = text;

    // Avoid scrolling to bottom
    textArea.style.top = "0";
    textArea.style.left = "0";
    textArea.style.position = "fixed";

    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        var successful = document.execCommand('copy');
        var msg = successful ? 'successful' : 'unsuccessful';
        console.log('Fallback: Copying text command was ' + msg);
    } catch (err) {
        console.error('Fallback: Oops, unable to copy', err);
    }

    document.body.removeChild(textArea);
}

function copyTextToClipboard(text) {
    if (!navigator.clipboard) {
        fallbackCopyTextToClipboard(text);
        return;
    }
    navigator.clipboard.writeText(text).then(function() {
        console.log('Async: Copying to clipboard was successful!');
    }, function(err) {
        console.error('Async: Could not copy text: ', err);
    });
}

const widths = {
    iops: 88,     // right aligned
    spaceIops: 4,
    iopsScale: 24, // static label IOPS, left aligned, grey
    bandwidth: 88, // right aligned
    spaceBandwidth: 4, // bandwidth ]---[ bandwidth units 
    bandwidthUnits: 66, // grey, left aligned
    operation: 22, // vertical
};
widths.panel = Object.values(widths).reduce((sum, current) => sum + current);
widths.panelSpace = 10;
widths.parameters = 24;

const heights = {
    panel: 72,
    panelSpace: 18,
    metrics: 38, 
    share: 20,
};

const isSharedBenchmarkResult = SharedDiskBenchmarkFlow.isSharedBenchmarkResult();

const styles = {
    main: {
        width: widths.panel * 2 + widths.panelSpace + widths.parameters + 2, // +2 for IE
        height: 4 * heights.panel + (3+1) * heights.panelSpace + heights.share - (isSharedBenchmarkResult ? 16 : 0),
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
    copyToCloud: {
        position: "absolute",
        width: 50,
        height: 50,
        margin: 0,
        padding: 0,
        fontSize: 11,
        left: -15,
        top: -12,
        textAlign: "left",
        zIndex: 99999,
    },

    shareIcon: {
        position: "absolute",
        width: ShareIconSize+1, height: ShareIconSize+1,
        margin: 0,
        padding: 0,
        left: -5, 
        textAlign: "left",
        fontSize: "1px",
        top: (heights.panel + heights.panelSpace) * 4 + 1,
        zIndex: 99999,
        whiteSpace: "nowrap",
        overflow: "hidden",
        // border: "1px solid blue",
        
    },
    shareBar: {
        position: "absolute",
        width: 2*widths.panel + widths.panelSpace,
        height: heights.share,
        margin: 0,
        padding: 2,
        fontSize: 10,
        left: widths.parameters,
        top: (heights.panel + heights.panelSpace) * 4,
        textAlign: "center",
        zIndex: 99999,
        border: "1px solid #B7B9BF",
        color: "#666",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
        overflow: "hidden",
        cursor: "pointer",
    },
    poweredByBar: {
        position: "absolute",
        width: 2*widths.panel + widths.panelSpace,
        height: /*heights.share*/ 16,
        margin: 0,
        padding: 2,
        fontSize: 10,
        left: widths.parameters,
        top: (heights.panel + heights.panelSpace) * 4 - 6,
        textAlign: "center",
        zIndex: 99999,
        color: "#666",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
        overflow: "hidden",
    },
    details: {
        position: "absolute",
        width: widths.panel,
        height: heights.panel,
        margin: 0,
        padding: 0,
        fontSize: 11,
        left: widths.parameters,
        textAlign: "left",
        // backgroundColor: "#EBECEF",
        // border: "1px solid #B7B9BF",
        // color: "black",
    },
    verticalAction: {
        position: "absolute",
        zIndex: 9999,
        left: widths.panel - 47,
        top: 24,
        // paddingTop: "-30px",
        transform: "translate(0px, 0px) rotate(-90deg)",
        whiteSpace: "nowrap",
        display: "block",
        // bottom: 0px, 
        width: heights.panel,
        height: widths.operation,
        lineHeight: `${widths.operation + 2}px`,
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
        lineHeight: `${widths.parameters}px`,
        verticalAlign: "bottom",
        border: "none",
        textAlign: "center",
        fontSize: "13px", fontWeight: 'bold', letterSpacing: "0.5px",
        backgroundColor: "#CDAFCE", // #CDAFCE 
        color: "black",
    },
}

function Details({row, allowShare}) {
    const full = row ? row : {};
    const theRootSpan = _ => (<span style={{opacity:0.55}}>&nbsp;(the root)</span>);
    return <React.Fragment>

        {allowShare === true && canAccessClipboard() && <div style={styles.copyToCloud}>
            <IconButton id={"COPY_TO_CLOUD_ICON"}
                        color="inherit"
                        aria-label="Copy to cloud"
                        onClick={() => copyTextToClipboard(SharedDiskBenchmarkFlow.buildLink(row))}
            >
                <CopyToCloudIcon />
            </IconButton>
        </div>}
        
        <div style={styles.details}>
            <div>Disk Benchmark</div>
            <div style={{fontSize: 16}}>
                {full.mountPath}{full.mountPath === '/' ? theRootSpan() : ""}
                {full.fileSystem ? `, ${full.fileSystem}` : ""}
            </div>
            <div>
                <span className='nowrap'>engine: {full.engine ? full.engine : "default"}</span>,{' '}
                {full.engineVersion && <><span className='nowrap'>{`fio: v${full.engineVersion}`},</span>{' '}</>}
                <span className='nowrap'>file system: {full.fileSystem}</span>
                , <span className='nowrap'>working set: {Helper.Common.formatAnything(full.workingSetSize, 0, ' ', 'B')}</span>
                , <span className='nowrap'>{full.o_Direct === "True" ? "direct access: present" : "missing direct access"}</span>            
            </div>
        </div>
    </React.Fragment>
}
function ParametersPanel(yPosition, parameters) {
    const left = widths.parameters - 56;
    const top = 24 + yPosition * (heights.panel + heights.panelSpace);
    const panelStyles = {...styles.verticalParameters, left: left, top: top};
    return (
        <React.Fragment>
            <div style={panelStyles}>{parameters}</div>
        </React.Fragment>
    );
}
function CpuUsagePanel(cpuUsage) {
    let format = num => (Math.round(num * 1000) / 10).toLocaleString(undefined, {useGrouping: true});
    const style={
        position: "absolute",
        left: 0, width: widths.panel - widths.operation, top: 3 + heights.metrics, height: heights.metrics,
        textAlign: "center",
        fontSize: 12,
        // color: 'grey',
        // border: "1px solid darkgreen",
    };
    const cpu = {fontWeight: 'bold', fontSize: 13, color: 'black'};
    return <div style={style}>cpu: <span style={cpu}>{format(cpuUsage.user)}</span>% user + <span style={cpu}>{format(cpuUsage.kernel)}</span>% kernel</div>;
}

function Metrics(text, align, left, right, style) {
    style={...style,
        position: "absolute",
        left: left, width: right-left+1, top: 0, height: heights.metrics, 
        verticalAlign: "bottom",
        lineHeight: `${heights.metrics+6}px`,
        textAlign: align,
        // border: "1px solid darkred",
    };
    return <div style={style}>{text}</div>;
}
function ActionPanel(xPosition, yPosition, action, bandwidth, blockSize, cpuUsage) {
    const left = widths.parameters + xPosition * (widths.panel + widths.panelSpace);
    const top = yPosition * (heights.panel + heights.panelSpace);
    const panelStyles = {...styles.panel, left: left, top: top};
    const metricsStyle={fontSize:16, fontWeight:900};
    const unitsStyle={fontSize:14, opacity:"0.55"};
    const iopsRaw = blockSize ? bandwidth / blockSize : undefined;
    let iops = Helper.Common.formatStructured(iopsRaw, 1, "");
    let iopsUnits = iopsRaw ? `${iops.units} IOPS` : "";
    iopsUnits = bandwidth && blockSize ? "IOPS" : "";
    let iops2 = "";
    let format = (num,tens) => (Math.round(num * tens) / tens).toLocaleString(undefined, {useGrouping: true});
    if (iopsRaw > 0) iops2 = format(iopsRaw, 1);
    if (iopsRaw > 0 && iopsRaw < 299) iops2 = format(iopsRaw, 10);
    // iops2 = "17,234,567";
    let bw = Helper.Common.formatStructured(bandwidth, 2, "B/s");
    // if (bw.value) bw.value = "1,567.99";
    return (
        <React.Fragment>
            <div style={panelStyles}>
                {Metrics(bandwidth ? /*iops.value*/ iops2 : "", "right", 0, widths.iops, metricsStyle)}
                {Metrics(iopsUnits, "left", 4 + widths.iops, 4 + widths.iops + 60, unitsStyle)}
                {Metrics(bw.value, "right", widths.iops + widths.iopsScale, widths.iops + widths.iopsScale + widths.bandwidth, metricsStyle)}
                {Metrics(bw.units, "left", 4 + widths.iops + widths.iopsScale + widths.bandwidth, widths.iops + widths.iopsScale + widths.bandwidth + widths.bandwidthUnits, unitsStyle)}
                {cpuUsage && CpuUsagePanel(cpuUsage)}
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
            increment: props.increment,
            opened: props.opened,
            selectedRow: props.selectedRow,
        };
        
        this.handleClose = this.handleClose.bind(this);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        const isOpenedChanged = prevProps.opened !== this.props.opened;
        const isRowChanged = (prevProps.selectedRow ? prevProps.selectedRow.token : '') !== (this.props.selectedRow ? this.props.selectedRow.token : '');
        const isIncrementChanged = prevProps.increment !== this.props.increment;
        if (isOpenedChanged || isRowChanged || isIncrementChanged) {
            this.setState({
                opened: this.props.opened,
                selectedRow: this.props.selectedRow,
            });
            if (this.props.selectedRow) {
                const link = SharedDiskBenchmarkFlow.buildLink(this.props.selectedRow);
                Helper.toConsole("shared link", link);
            }
        }
    }

    handleClose() {
        if (!this.props.forced) {
            this.setState({opened: false});
            // alert("CLOSED, opened: " + this.state.opened);
        }
    }

    render() {
        
        const onSharedLinkClick = () => {
            selectNodeText('SHARED_DRIVE_BENCHMARK_BAR');
        };
        // Helper.toConsole(`[DiskBenchmarkResult::render] this.state.opened=${this.state.opened}`);
        const full = this.state.selectedRow ? this.state.selectedRow : {};
        const blockSize = full.randomAccessBlockSize;
        let textBlockSize = `${blockSize / 1024}K`;
        if (blockSize >= 512 * 1024) textBlockSize = `${blockSize / 1024.0 / 1024}M`;
        return (
            <Dialog open={this.state.opened} onClose={this.handleClose} aria-labelledby="form-dialog-title" fullWidth={false} maxWidth={"md"}>
                <DialogContent style={{textAlign: "center"}} >
                    <div style={styles.main}>
                        {!this.props.forced && <div style={styles.shareIcon}><ShareIcon /></div>}
                        <Details row={this.state.selectedRow} allowShare={!this.props.forced} />
                        {ActionPanel(1, 0, "Allocate", full.allocate, null, full.allocateCpuUsage)}
                        {ActionPanel(0, 1, "Read", full.seqRead, null, full.seqReadCpuUsage)}
                        {ActionPanel(1, 1, "Write", full.seqWrite, null, full.seqWriteCpuUsage)}
                        {ActionPanel(0, 2, `Read ${textBlockSize}`, full.randRead1T, blockSize, full.randRead1TCpuUsage)}
                        {ActionPanel(1, 2, `Write ${textBlockSize}`, full.randWrite1T, blockSize, full.randWrite1TCpuUsage)}
                        {ActionPanel(0, 3, `Read ${textBlockSize}`, full.randReadNT, blockSize, full.randReadNTCpuUsage)}
                        {ActionPanel(1, 3, `Write ${textBlockSize}`, full.randWriteNT, blockSize, full.randWriteNTCpuUsage)}
                        {ParametersPanel(1,"SEQ")}
                        {ParametersPanel(2,<span>RND 1Q</span>)}
                        {ParametersPanel(3,<span>RND {full.threadsNumber ? `${full.threadsNumber}Q` : ""}</span>)}

                        {!this.props.forced && <div id="SHARED_DRIVE_BENCHMARK_BAR" style={styles.shareBar} onClick={onSharedLinkClick} title="Click & Ctrl-C to share">
                            {/*https://stackoverflow.com/questions/1173194/select-all-div-text-with-single-mouse-click/1173319*/}
                            {SharedDiskBenchmarkFlow.buildLink(full)}
                        </div>}
                        

                        {this.props.forced && <div style={styles.poweredByBar}>
                            powered by <a href="https://github.com/devizer/w3top-bin">w3top</a>
                        </div>}


                    </div>
{/*
                    <div style={{wordBreak:"break-all", wordWrap: "break-word", display: "none"}}>
                        {JSON.stringify(this.state.selectedRow)}
                    </div>
*/}
                </DialogContent>
            </Dialog>
        );
    }

}