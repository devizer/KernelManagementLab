import React, { Component } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import { faEllipsisH } from '@fortawesome/free-solid-svg-icons'
import { faArrowAltCircleRight } from '@fortawesome/free-solid-svg-icons'

import ErrorOutlinedIcon  from "@material-ui/icons/ErrorOutlineOutlined"

const iconStyle = {width:25, minWidth: 25, display: "inline-block", marginRight: 8, textAlign: "center", };
const iconUnknown = <span style={iconStyle}>&nbsp;</span>;
// const iconPending = <span style={{color:"#888"}}><FontAwesomeIcon style={iconStyle} icon={faEllipsisH} /></span>;
const iconPending = <span style={{color:"#555"}}><span style={iconStyle}>&middot;&middot;&middot;</span></span>;   
const iconInProgress = <FontAwesomeIcon style={iconStyle} icon={faArrowAltCircleRight} />;
const iconComplete = <FontAwesomeIcon style={iconStyle} icon={faCheck} />;
const iconError = <span style={{...iconStyle, color: "#FF0F17" }}><ErrorOutlinedIcon/></span>;
const iconSkipped = <span style={{...iconStyle, color:"#FF9B9F"}}>?</span>;


export const BenchmarkStepStatusIcon = function(props) {
    // for keys see enum ProgressStepState
    const statuses = {Pending: iconPending, InProgress: iconInProgress, Completed: iconComplete, Error: iconError, Skipped: iconSkipped};
    let icon = statuses[props.status];
    if (!icon) icon = iconUnknown;
    return icon;
}
