import * as Helper from "../../Helper";
import React from "react";
import {BenchmarkStepStatusIcon} from "./BenchmarkStepStatusIcon";
import {withStyles} from "@material-ui/core";
import LinearProgress from "@material-ui/core/LinearProgress";

const ProgressStyle = theme => ({
    root: {
        margin: theme.spacing.unit * 0,
        // color: '#777',
        animationDuration: "1ms",
        height: "1px",
    },
    colorPrimary: {backgroundColor: '#EEE'},
    barColorPrimary: {backgroundColor: '#888'}
});

const LinearProgress2 = withStyles(ProgressStyle)(LinearProgress);

const BenchmarkProgressTable = function(props) {
    const progress = props.progress;
    const pro = progress ? progress : {isCompleted: false, steps: []};
    const formatSpeed = (x) => {let ret = Helper.Common.formatBytes(x,1); return ret === null ? "" : `${ret}/s`};
    const statuses = {Pending: "⚪", InProgress: "⇢", Completed: "⚫"};
    const formatStepStatus = (status) => statuses[status];
    const progressValue = step => step.perCents >= 0.99999 ? 99.999 : step.perCents * 100.0;
    const hasMetrics = step => (step.seconds > 0 && step.avgBytesPerSecond > 0) || step.canHaveMetrics;
    return (
        <React.Fragment>
            <center>
                <table className="benchmark-progress" border="0" cellPadding={0} cellSpacing={0}><tbody>
                {pro.steps.map((step, stepIndex) => (
                    <React.Fragment key={`${stepIndex}:${step.name}`}>
                        <tr>
                            <td className="step-status">
                                <BenchmarkStepStatusIcon status={step.state}/>
                                {/* formatStepStatus(step.state) */}
                            </td>
                            <td className="step-name" colSpan={hasMetrics(step) ? 1 : 3}>
                                {step.name}
                            </td>
                            {hasMetrics(step) ? ( <React.Fragment>
                                <td className="step-duration">{step.seconds > 0 ? `${step.duration}` : "\xA0" }</td>
                                <td className="step-speed">{step.avgBytesPerSecond > 0 ? `${formatSpeed(step.avgBytesPerSecond)}` : "\xA0"}</td>
                            </React.Fragment>) : ("")}
                        </tr>
                        <tr>
                            <td></td>
                            <td colSpan="3">
                                <LinearProgress2 value={progressValue(step)} variant={"determinate"} className={"step-progress"}/>
                            </td>
                        </tr>
                    </React.Fragment>
                ))}
                </tbody></table>
            </center>
        </React.Fragment>
    )
};

export default BenchmarkProgressTable;
