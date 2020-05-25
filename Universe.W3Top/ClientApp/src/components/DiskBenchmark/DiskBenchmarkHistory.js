import React from 'react';
import MomentFormat from 'moment';

import {faCheck, faCheckDouble} from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'

import * as Helper from "../../Helper";

import ReactTable from "react-table";
import "react-table/react-table.css";

const renderODirectIcon = o_Direct => {
    if (o_Direct === "True") return <FontAwesomeIcon style={{color:"#555"}} icon={faCheckDouble} />;
    if (o_Direct === "False") return <span style={{color:"grey"}}>&mdash;</span>;
    return <span/>;
};

export class DiskBenchmarkHistory extends React.Component {
    static displayName =  DiskBenchmarkHistory.name;

    prevTrigger = null;
    
    constructor(props) {
        super(props);

        this.tryBuildDiskHistorySource = this.tryBuildDiskHistorySource.bind(this);

        this.state = {
            history: this.tryBuildDiskHistorySource(),
        };

    }
    
    componentDidMount() {
        this.fetchDiskHistorySource();
    }
    
    historyProjection(history) {
        history.forEach(bechmark => {
            let mo = MomentFormat(bechmark.createdAt);
            bechmark.createdDate = mo.format("MMM DD, YYYY");
        })
    }
    
    fetchDiskHistorySource() {
        let apiUrl = 'api/benchmark/disk/get-disk-benchmark-history';
        try {
            fetch(apiUrl, {method: "POST"})
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    Helper.log(response);
                    return response.ok ? response.json() : {error: response.status, details: response}
                })
                .then(history => {
                    this.historyProjection(history);
                    this.setState({history:history});
                    Helper.toConsole("HISTORY for disk benchmark", history);
                })
                .catch(error => Helper.log(error));
        }
        catch(err)
        {
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }
    }

    tryBuildDiskHistorySource() {
        // return [{mountPath:"/a"},{mountPath:"/b"},{mountPath:"/c"},{mountPath:"/d"}];
        const emptyRow = {mountPath:"\xA0"};
        return [emptyRow,emptyRow,emptyRow,emptyRow,emptyRow,emptyRow,];
    }
    
    shouldComponentUpdate(nextProps, nextState, nextContext) {
        if (this.prevTrigger != nextProps.trigger)
        {
            this.prevTrigger = nextProps.trigger;
            this.fetchDiskHistorySource();
        }

        return true;
    }

    render() {
        
        let pageSize = this.state.history.length === 0 ? 6 : Math.max(this.state.history.length, 1);
        pageSize = Math.max(this.state.history.length, 6);
        let formatSpeed = field => row => {
            let ret = Helper.Common.formatBytes(row[field],1); 
            return ret === null ? "" : `${ret}/s`
        };

        let cellMountPath = row => (<span><span style={{fontWeight:'bold'}}>{row.original.mountPath}{row.original.fileSystem ? <span style={{opacity:0.55,fontWeight:'normal'}}> ({row.original.fileSystem})</span> : ""}</span></span>);
        let sizeCell = row => <span>{row.value ? Helper.Common.formatBytes(row.value) : ""}</span>;
        let speedCell = row => <span>{row.value ? `${Helper.Common.formatBytes(row.value, 2)}/s` : ""}</span>;
        let rightAlign = {textAlign: "right" };
        let centerAlign = {textAlign: "center" };


        return (
            <div>
                <ReactTable
                    data={this.state.history}
                    showPagination={false}
                    defaultPageSize={pageSize}
                    pageSizeOptions={[pageSize]}
                    pageSize={pageSize}
                    noDataText="history is empty"
                    getNoDataProps={() => {return {style:{color:"gray", marginTop:30}}}}
                    // pivotBy={["createdDate"]}
                    // defaultExpanded={{2:true}}
                    // pivotDefaults={}
                    // defaultExpanded={{0:true,}}
                    
                    /*
/ Special
  pivot: false,
  // Turns this column into a special column for specifying pivot position in your column definitions.
  // The `pivotDefaults` options will be applied on top of this column's options.
  // It will also let you specify rendering of the header (and header group if this special column is placed in the `columns` option of another column)
  expander: false,
  // Turns this column into a special column for specifying expander position and options in your column definitions.
  // The `expanderDefaults` options will be applied on top of this column's options.
  // It will also let you specify rendering of the header (and header group if this special column is placed in the `columns` option of another column) and
  // the rendering of the expander itself via the `Expander` property
                     
                     */

                    columns={
                        [
/*
                            {
                                Header: "Groups",
                                columns: [
                                    {
                                        Header: "Created At",
                                        accessor: "createdDate",
                                        width: 180,
                                    }
                                ],
                                show: false,
                            },
*/
                            {
                                
                                Header: "Disk Benchmark History",
                                columns: [
                                    {
                                        Header: "Mount Path",
                                        accessor: "mountPath",
                                        minWidth: 135,
                                        Cell: cellMountPath,
                                        // style: ,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "Working Set",
                                        id: "workingSetSize",
                                        Cell: sizeCell,
                                        accessor: "workingSetSize",
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "O_DIRECT",
                                        id: "O_DIRECT",
                                        Cell: row => renderODirectIcon(row.value),
                                        // accessor: x => x.o_Direct !== undefined ? x.o_Direct.toLowerCase() : "",
                                        accessor: x => x.o_Direct,
                                        minWidth: 65,
                                        style: centerAlign,
                                        aggregate: () => null,
                                    },
                                ]
                            },
                            {
                                Header: "Sequential Access",
                                columns: [
                                    {
                                        Header: "Allocate",
                                        id: "allocate",
                                        Cell: speedCell,
                                        accessor: "allocate",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "Read",
                                        id: "seqRead",
                                        Cell: speedCell,
                                        accessor: "seqRead",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "Write",
                                        id: "seqWrite",
                                        Cell: speedCell,
                                        accessor: "seqWrite",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                ]
                            },
                            {
                                Header: "Random Access (single & multi threaded)",
                                columns: [
                                    {
                                        Header: "Block",
                                        accessor: "randomAccessBlockSize",
                                        Cell: (row) => <span>{Helper.Common.formatBytes(row.value,0)}</span>,
                                        minWidth: 65,
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "Read",
                                        Cell: speedCell,
                                        accessor: "randRead1T",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "Write",
                                        id: "randWrite1T",
                                        Cell: speedCell,
                                        accessor: "randWrite1T",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: "Threads",
                                        id: "threadsNumber",
                                        // Cell: speedCell,
                                        accessor: "threadsNumber",
                                        style: centerAlign,
                                        minWidth: 48, width: 48,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: () => <span>Read <sup>n</sup></span>,
                                        id: "randReadNT",
                                        Cell: speedCell,
                                        accessor: "randReadNT",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                    {
                                        Header: () => <span>Write <sup>n</sup></span>,
                                        id: "randWriteNT",
                                        Cell: speedCell,
                                        accessor: "randWriteNT",
                                        style: rightAlign,
                                        aggregate: () => null,
                                    },
                                ]
                            }
                        ]
                    }
                    className="-striped -highlight"
                />
                <br />
            </div>
        );
    }

}