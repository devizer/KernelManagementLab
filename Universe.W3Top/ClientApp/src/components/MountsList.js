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
    // static displayName = MountsList.name;

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
        DataSourceStore.removeListener('storeUpdated', this.tryInitMountsSource);
        
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
        return mounts == null ? [] : mounts;
    }

    getTrProps = (state, rowInfo, instance) => {
        // rowInfo.row - fields from RwactTable
        // row.original - fields from data (dataSource)
        let suffix = "";
        if (rowInfo !== undefined && rowInfo.original !== undefined) {
            if (rowInfo.original.isTmpFs) {
                suffix = "ram";
            } else if (rowInfo.original.isBlockDevice) {
                suffix = "block";
            } else if (rowInfo.original.isTmpFs) {
                suffix = "ram";
            } else if (rowInfo.original.isNetworkShare) {
                suffix = "network";
            } else if (rowInfo.original.isSwap) {
                suffix = "swap";
            }
        }
        return { className: `disk-kind-${suffix}`};
    };

    getNoDataProps()
    {
        return { className: "disk-no-data" };
    }

    
    
    mountPathCell(rowInfo){
        let icon = iconUnknown;
        if (rowInfo.original.isBlockDevice) {
            icon = iconBlock; 
        } else if (rowInfo.original.isTmpFs) {
            icon = iconRam;
        } else if (rowInfo.original.isNetworkShare) {
            icon = iconNet;
        }else if (rowInfo.original.isSwap) {
            icon = iconSwap;
        }
        
        return <span>{icon}{rowInfo.original.mountEntry.mountPath}</span>;
    }

    renderNormal() {
        let pageSize = this.state.mounts.length === 0 ? 6 : Math.max(this.state.mounts.length, 1);
        let sizeCell = row => <span>{row.value ? Helper.Common.formatBytes(row.value) : ""}</span>;
        let rightAlign = {textAlign: "right" };
        let centerAlign = {textAlign: "center" };

        let rows = this.state.mounts;
        const getColumnWidth = (minWidth, accessor, headerText) => {
            const maxWidth = 400;
            const magicSpacing = 10;
            const getValue = row => typeof accessor === "function" ? accessor(row) : row[accessor];
            const cellLength = Math.max(
                ...rows.map(row => (`${getValue(row)}` || '').length),
                headerText.length,
                minWidth / magicSpacing
            );
            return Math.min(maxWidth, cellLength * magicSpacing)
        };
        
        return (
            <div id="Mounts" style={{marginTop: 12}}>
                <ReactTable
                    data={this.state.mounts}
                    noDataText="Waiting for cells"
                    showPagination={false}
                    defaultPageSize={pageSize}
                    pageSizeOptions={[pageSize]}
                    pageSize={pageSize}
                    getTrProps={this.getTrProps}
                    getNoDataProps={this.getNoDataProps}

                    columns={[
                        {
                            id: "mountPath",
                            Header: "Mount Path",
                            accessor: x => x.mountEntry.mountPath,
                            // minWidth: 256,
                            Cell: this.mountPathCell,
                            minWidth: getColumnWidth(80, x => x.mountEntry.mountPath, "Mount Path a1a2a3a4a5"),
                        },
                        {
                            id: "device",
                            Header: "Device",
                            accessor: x => x.mountEntry.device,
                            // minWidth: 256,
                            minWidth: getColumnWidth(80,x => x.mountEntry.device, "Device"),
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
                            style: centerAlign,
                            width: 60,
                        },
                        {
                            id: "totalSize",
                            Header: "Size",
                            accessor: "totalSize",
                            style: rightAlign,
                            Cell: sizeCell,
                            // minWidth: 120,
                            minWidth: getColumnWidth(80,"totalSize", "Size"),
                        },
                        {
                            Header: "Free",
                            accessor: "freeSpace",
                            style: rightAlign,
                            Cell: sizeCell,
                            // minWidth: 120,
                            minWidth: getColumnWidth(80,"freeSpace", "Free"),
                        },

                    ]}
                    
                    defaultSorted={[
                        {
                            id: "mountPath",
                            desc: false
                        }
                    ]}
                    defaultPageSize={pageSize}
                    className="-highlight"
                />
                
                
                <div className="disk-legend">
                    {/* Legend:&nbsp;&nbsp; */}
                    <span className="disk-legend-item disk-kind-block">{iconBlock}&nbsp;Block&nbsp;device</span>
                    <span className="disk-legend-item disk-kind-network">{iconNet}&nbsp;Network&nbsp;share</span>
                    <span className="disk-legend-item disk-kind-ram">{iconRam}&nbsp;RAM&nbsp;disk</span>
                    <span className="disk-legend-item disk-kind-swap">{iconSwap}&nbsp;SWAP&nbsp;disk</span>
                </div>

            </div>
        );
    }

    render () {
        if (process.env.NODE_ENV === "test") console.log("MountsList.render()");
        return this.renderNormal();
    }

}