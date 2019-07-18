import React from 'react';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faServer } from '@fortawesome/free-solid-svg-icons'
import { faNetworkWired } from '@fortawesome/free-solid-svg-icons'
import { faMemory } from '@fortawesome/free-solid-svg-icons'
import { faFile } from '@fortawesome/free-regular-svg-icons'

// const iconStyle = {width:16, minWidth: 16, display: "inline-block", marginRight: 0, fontSize_ignore: 6};
const chipStyle = {};

const chipIcons = {
    Unknown: <span style={chipStyle}>&nbsp;</span>,
    Block: <FontAwesomeIcon style={chipStyle} icon={faServer}/>,
    Ram: <FontAwesomeIcon style={chipStyle} icon={faMemory}/>,
    Net: <FontAwesomeIcon style={chipStyle} icon={faNetworkWired}/>,
    Swap: <FontAwesomeIcon style={chipStyle} icon={faFile}/>
};

export default function DiskAvatarContent (props){
    if (props.disk.isTmpFs) return chipIcons.Ram;
    if (props.disk.isBlockDevice) return chipIcons.Block;
    if (props.disk.isNetworkShare) return chipIcons.Net;
    if (props.disk.isSwap) return chipIcons.Swap;
    return chipIcons.Unknown;
}
