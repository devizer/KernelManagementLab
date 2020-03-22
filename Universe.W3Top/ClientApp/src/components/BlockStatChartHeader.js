import React, { Component } from 'react';
import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import dataSourceStore from "../stores/DataSourceStore";
import { findLimit } from './LimitFinder'
import * as Helper from "../Helper";
import {BlockDevChart} from "./BlockStatChart";
import {NetDevChart} from "./NetDevChart";

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowAltCircleUp } from '@fortawesome/free-regular-svg-icons'
import { faArrowAltCircleDown } from '@fortawesome/free-regular-svg-icons'
import { faArrowUp } from '@fortawesome/free-solid-svg-icons'
import { faArrowDown } from '@fortawesome/free-solid-svg-icons'

import * as Enumerable from "linq-es2015";

const iconSent = faArrowUp, iconReceived = faArrowDown;

export class BlockStatDevChartHeader extends Component {
    static displayName = BlockStatDevChartHeader.name;

    constructor(props) {
        super(props);

        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
    }

    componentDidMount() {
        let x = dataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
    }
    
    componentWillUnmount() {
        dataSourceStore.removeListener('storeUpdated', this.updateGlobalDataSource);
    }

    updateGlobalDataSource()
    {
        this.forceUpdate();
        // console.log("BlockStatDevChartHeader::updateGlobalDataSource");
        
    }
    

    dd = {
        container: {
            position: "relative",
            width: NetDevChart.ChartSize.width,
            // width: "100%",
            marginBottom: 4,
            marginTop: "20px",
            // border: "1px solid green",
        },
        top: {
            paddingTop: 3,
            fontSize: 16,
            position: "absolute",
            left: 0, top: 0,
            textAlign: "left",
            whiteSpace: "nowrap",
            overflow: "hidden",
            textOverflow: "ellipsis",
            width: NetDevChart.ChartSize.width - 2* NetDevChart.Padding,
            // border: "1px solid transparent",
            borderBottom: "1px solid #DDD",
            marginLeft: NetDevChart.Padding,
            marginRight: 0,

            // display: "none"
        },
        // BIG
        // center: {
        //     paddingTop: 3+16,
        //     fontSize: 16,
        //     position: "absolute",
        //     left: 0, top: 0+16,
        //     textAlign: "center",
        //     width: "100%",
        // },
        // small
        center: {
            paddingTop: 8+16,
            fontSize: 12,
            position: "absolute",
            left: 0, top: 0+16,
            textAlign: "center",
            width: "100%",
        },
        left: {
            paddingTop: 8+16,
            fontSize: 12,
            verticalAlign: "bottom",
            position: "absolute",
            left: 0, top: 0+16,
            textAlign: "left",
            width: "100%",
            paddingLeft: NetDevChart.Padding - 4,
            paddingRight: NetDevChart.Padding,
        },
        right: {
            paddingTop: 8+16,
            fontSize: 12,
            verticalAlign: "bottom",
            position: "absolute",
            left: 0, top: 0+16,
            textAlign: "right",
            width: "100%",
            paddingLeft: NetDevChart.Padding,
            paddingRight: NetDevChart.Padding - 4,
        },
    };

    render() {

        let totals;
        try {
            let glo = dataSourceStore.getDataSource();
            // TODO: use associative array
            let totalsRaw = Enumerable.asEnumerable(glo.block.blockTotals)
                .FirstOrDefault(x => x.diskVolKey === this.props.name);
            
            totals = {rxBytes: totalsRaw.stat.readSectors*512, txBytes: totalsRaw.stat.writeSectors*512};  
        }
        catch{
            totals = {rxBytes: 0, txBytes: 0};
        }

        if (!Helper.Common.objectIsNotEmpty(totals))
            totals = {rxBytes: 0, txBytes: 0};

        let format = x => x > 0 ? Helper.Common.formatBytes(x) : "";
        // totals = {rxBytes: 0, txBytes: 0};
        
        // mounted volume
        let globalDataSource = dataSourceStore.getDataSource();
        let mounts = Helper.Disks.getOptionalMountsProperty(globalDataSource);
        mounts = mounts == null ? [] : mounts;
        
        let deviceCopy = "/dev/" + this.props.name;
        let mountInfo = mounts.find(x => x && x.mountEntry && x.mountEntry.device === deviceCopy);
        if (mountInfo === undefined) mountInfo = null;
        let mountPath = mountInfo && mountInfo.mountEntry ? mountInfo.mountEntry.mountPath : null;
        let fileSystem = mountInfo && mountInfo.mountEntry ? mountInfo.mountEntry.fileSystem : null;
        let totalSize = mountInfo && mountInfo.totalSize && mountInfo.totalSize > 0 ? mountInfo.totalSize : null;
        let isReadonly = mountInfo && (mountInfo.freeSpace === 0);
        if (fileSystem === "swap") {
            mountPath="swap";
            fileSystem = null;
        } 
        
        let componentMountPath = mountPath === null ? null : (
            <span><b>{mountPath}</b>{mountPath === "/" ? " (the root)" : ""}</span>
        );

        let componentTotalSize = !totalSize ? null : (
            <span>,&nbsp;&nbsp;<small>{Helper.Common.formatBytes(totalSize,0)}</small></span>
        );

        let componentReadOnly = (<>, <small>R/O</small></>);
        let componentFileSystem = fileSystem === null ? null : (
            <span style={{color:"grey"}}>({fileSystem}{isReadonly ? componentReadOnly:null})</span>
        );

        return (<React.Fragment>
            
            <div style={this.dd.container}>&nbsp;<br/>&nbsp;
                <div style={this.dd.top}>{componentMountPath}{componentTotalSize}&nbsp;&nbsp;{componentFileSystem}</div>
                <div style={this.dd.left}><span title={"TOTAL RECEIVED"}><FontAwesomeIcon icon={iconSent} /> {format(totals.rxBytes)}</span></div>
                <div style={this.dd.center}>{this.props.name.toUpperCase()}</div>
                <div style={this.dd.right}><span title={"TOTAL SENT"}>{format(totals.txBytes)} <FontAwesomeIcon icon={iconReceived} /></span></div>
            </div>
        </React.Fragment>)
    };

}

BlockStatDevChartHeader.propTypes = {
    name: PropTypes.string.isRequired,
};
