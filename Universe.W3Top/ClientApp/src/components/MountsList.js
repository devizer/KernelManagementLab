import React, { Component } from 'react';

import ReactTable from "react-table";
import "react-table/react-table.css";
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";
import DataSourceStore from "../stores/DataSourceStore";

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faServer } from '@fortawesome/free-solid-svg-icons'
import { faNetworkWired } from '@fortawesome/free-solid-svg-icons'
import { faMemory } from '@fortawesome/free-solid-svg-icons'
import { faFile } from '@fortawesome/free-regular-svg-icons'

const iconStyle = {width:24, minWidth: 24, display: "inline-block", marginRight: 8};
const iconUnknown = <span style={iconStyle}>&nbsp;</span>;
const iconBlock = <FontAwesomeIcon style={iconStyle} icon={faServer}/>;
const iconRam = <FontAwesomeIcon style={iconStyle} icon={faMemory}/>;
const iconNet = <FontAwesomeIcon style={iconStyle} icon={faNetworkWired}/>;
const iconSwap = <FontAwesomeIcon style={iconStyle} icon={faFile}/>;


export class MountsList extends React.Component {

    timerId = null;

    constructor(props) {
        super(props);

        this.tryInitMountsSource = this.tryInitMountsSource.bind(this);
        this.tryBuildMountsSource = this.tryBuildMountsSource.bind(this);

        this.state = {
            mounts: this.tryBuildMountsSource(),
        };

    }

    componentDidMount() {
        let isAlreadyBound = this.state.mounts !== null;
        if (!isAlreadyBound)
            this.timerId = setInterval(this.waiterTick.bind(this), 1000);

        let x = DataSourceStore.on('storeUpdated', this.tryInitMountsSource);
    }

    componentWillUnmount() {
        if (this.timerId !== null)
            clearInterval(this.timerId);
    }

    waiterTick()
    {
        if (this.state.mounts === null)
            this.tryInitMountsSource();
    }

    // used by timer callback
    tryInitMountsSource()
    {
        if (this.state.mounts === null || true)
        {
            let mounts = this.tryBuildMountsSource();
            if (mounts !== null) {
                this.setState({
                    mounts: mounts,
                });
            }
        }
    }

    tryBuildMountsSource()
    {
        let globalDataSource = dataSourceStore.getDataSource();
        let mounts = Helper.Disks.getOptionalMountsProperty(globalDataSource);
        return mounts;
    }

    renderLoading() {
        return (
            <h1 id="Mounts_is_Waiting_for_connection" style={{textAlign: "center"}}>
                Mounts is waiting for connection
            </h1>
        )
    }

    getTrProps = (state, rowInfo, instance) => {
        // rowInfo.row - fields from RwactTable
        // row.original - fields from data (dataSource)
        let suffix = "";
        if (rowInfo.original.isBlockDevice) {
            suffix = "block";
        } else if (rowInfo.original.isTmpFs) {
            suffix = "ram";
        } else if (rowInfo.original.isNetworkShare) {
            suffix = "network";
        }
        return { className: `disk-kind-${suffix}`};
    };

    
    
    mountPathCell(rowInfo){
        let icon = iconUnknown;
        if (rowInfo.original.isBlockDevice) {
            icon = iconBlock; 
        } else if (rowInfo.original.isTmpFs) {
            icon = iconRam;
        } else if (rowInfo.original.isNetworkShare) {
            icon = iconNet;
        }
        
        return <span>{icon}{rowInfo.original.mountEntry.mountPath}</span>;
    }

    renderNormal() {
        let pageSize = Math.max(this.state.mounts.length, 1);
        let sizeCell = row => <span>{row.value ? Helper.Common.formatBytes(row.value) : ""}</span>;
        let rightAlign = {textAlign: "right" };
        return (
            <div id="Mounts">
                <br/>
                <br/>
                <ReactTable
                    data={this.state.mounts}
                    showPagination={false}
                    defaultPageSize={pageSize}
                    pageSizeOptions={[pageSize]}
                    pageSize={pageSize}
                    getTrProps={this.getTrProps}

                    columns={[
                        {
                            id: "mountPath",
                            Header: "Mount Path",
                            accessor: x => x.mountEntry.mountPath,
                            minWidth: 256,
                            Cell: this.mountPathCell
                        },
                        {
                            id: "device",
                            Header: "Device",
                            accessor: x => x.mountEntry.device,
                            minWidth: 256,
                        },
                        {
                            id: "fs",
                            Header: "FS",
                            accessor: x => x.mountEntry.fileSystem,
                            minWidth: 100,
                        },
                        {
                            id: "ro",
                            Header: "R/O",
                            accessor: x => x.freeSpace > 0 ? "" : "R/O",
                            width: 45,
                        },
                        {
                            id: "totalSize",
                            Header: "Size",
                            accessor: "totalSize",
                            style: rightAlign,
                            Cell: sizeCell,
                            width: 130,
                        },
                        {
                            Header: "Free",
                            accessor: "freeSpace",
                            style: rightAlign,
                            Cell: sizeCell,
                            width: 130,
                        },

                    ]}
                    
                    defaultSorted={[
                        {
                            id: "mountPath",
                            desc: false
                        }
                    ]}
                    defaultPageSize={10}
                    className="-highlight"
                />
                
                
                <div class="disk-legend">
                    {/* Legend:&nbsp;&nbsp; */}
                    <span className="disk-legend-item disk-kind-block">{iconBlock} Block device</span>&nbsp;&nbsp;
                    <span className="disk-legend-item disk-kind-network">{iconNet} Network share</span>&nbsp;&nbsp;
                    <span className="disk-legend-item disk-kind-ram">{iconRam} RAM disk</span>&nbsp;&nbsp;
                    <span className="disk-legend-item disk-kind-swap">{iconSwap} SWAP disk</span>
                </div>

            </div>
        );
    }

    render () {
        return (
            <div id="Mounts">
                {this.state.mounts === null ? this.renderLoading() : this.renderNormal()}
            </div>
        );
    }



}