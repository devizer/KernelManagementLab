import React, { Component } from 'react';

import ReactTable from "react-table";
import "react-table/react-table.css";
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";

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
            this.timerId = setInterval(this.waiterTick.bind(this));
    }

    componentWillUnmount() {
        if (this.timerId !== null)
            clearInterval(this.timerId);
    }

    waiterTick()
    {
        this.tryInitMountsSource();
    }



    // used by timer callback
    tryInitMountsSource()
    {
        if (this.state.mounts === null)
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

    renderNormal() {
        let pageSize = Math.max(this.state.mounts.length, 1);
        let sizeCell = row => <span>{Helper.Common.formatBytes(row.value)}</span>;
        let rightAlign = {textAlign: "right" };
        return (
            <div id="Mounts">
                <ReactTable
                    data={this.state.mounts}
                    showPagination={false}
                    defaultPageSize={pageSize}
                    pageSizeOptions={[pageSize]}
                    pageSize={pageSize}

                    columns={[
                        {
                            id: "mountPath",
                            Header: "Mount Path",
                            accessor: x => x.mountEntry.mountPath,
                        },
                        {
                            id: "device",
                            Header: "Device",
                            accessor: x => x.mountEntry.device,
                        },
                        {
                            id: "fs",
                            Header: "FS",
                            accessor: x => x.mountEntry.fileSystem,
                        },
                        {
                            id: "totalSize",
                            Header: "Size",
                            accessor: "totalSize",
                            style: rightAlign,
                            Cell: sizeCell,
                        },
                        {
                            Header: "Free",
                            accessor: "freeSpace",
                            style: rightAlign,
                            Cell: sizeCell,
                        },

                    ]}
                    
                    defaultSorted={[
                        {
                            id: "totalSize",
                            desc: true
                        }
                    ]}
                    defaultPageSize={10}
                    className="-striped -highlight"
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