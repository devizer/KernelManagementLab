import React, { Component } from 'react';

import ReactTable from "react-table";
import "react-table/react-table.css";
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";
import DataSourceStore from "../stores/DataSourceStore";

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
        let color = "black";
        if (rowInfo.original.isBlockDevice) {
            color = "black";
        } else if (rowInfo.original.isTmpFs) {
            color = "#159B30";
        } else if (rowInfo.original.isNetworkShare) {
            color = "#161A9E";
        }
        return {style: {color: color}};
    };


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
                            id: "totalSize",
                            desc: true
                        }
                    ]}
                    defaultPageSize={10}
                    className="-highlight"
                />

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