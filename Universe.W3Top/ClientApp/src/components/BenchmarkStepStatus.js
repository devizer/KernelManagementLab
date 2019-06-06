import React, { Component } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import { faEllipsisH } from '@fortawesome/free-solid-svg-icons'
import { faArrowAltCircleRight } from '@fortawesome/free-solid-svg-icons'

const iconStyle = {width:20, minWidth: 20, display: "inline-block", marginRight: 8, textAlign: "center"};
const iconUnknown = <span style={iconStyle}>&nbsp;</span>;
// const iconPending = <span style={{color:"#888"}}><FontAwesomeIcon style={iconStyle} icon={faEllipsisH} /></span>;
const iconPending = <span style={{color:"#555"}}><span style={iconStyle}>&middot;&middot;&middot;</span></span>;   
const iconInProgress = <FontAwesomeIcon style={iconStyle} icon={faArrowAltCircleRight} />;
const iconComplete = <FontAwesomeIcon style={iconStyle} icon={faCheck} />;


export const BenchmarkStepStatus = function(props) {
    const statuses = {Pending: iconPending, InProgress: iconInProgress, Completed: iconComplete};
    let icon = statuses[props.status];
    if (!icon) icon = iconUnknown;
    return icon;
}
