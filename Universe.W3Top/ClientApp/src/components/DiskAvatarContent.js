import React from 'react';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faServer } from '@fortawesome/free-solid-svg-icons'
import { faNetworkWired } from '@fortawesome/free-solid-svg-icons'
import { faMemory } from '@fortawesome/free-solid-svg-icons'
import { faFile } from '@fortawesome/free-regular-svg-icons'

// const iconStyle = {width:16, minWidth: 16, display: "inline-block", marginRight: 0, fontSize_ignore: 6};
const iconStyle = {};
const iconUnknown = <span style={iconStyle}>&nbsp;</span>;
const iconBlock = <FontAwesomeIcon style={iconStyle} icon={faServer}/>;
const iconRam = <FontAwesomeIcon style={iconStyle} icon={faMemory}/>;
const iconNet = <FontAwesomeIcon style={iconStyle} icon={faNetworkWired}/>;
const iconSwap = <FontAwesomeIcon style={iconStyle} icon={faFile}/>;

export default function DiskAvatarContent (props){
    if (props.disk.isTmpFs) return iconRam;
    if (props.disk.isBlockDevice) return iconBlock;
    if (props.disk.isNetworkShare) return iconNet;
    if (props.disk.isSwap) return iconSwap;
    return iconUnknown;
}
