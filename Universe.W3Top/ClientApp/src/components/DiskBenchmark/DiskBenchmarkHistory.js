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
        
        // on first render always null
        const idSelected = props.selected;

        this.state = {
            history: this.tryBuildDiskHistorySource(),
            selected: null, // token
            selectedRow: null,
        };
    }
    
    componentDidMount() {
        this.fetchDiskHistorySource();
    }
    
    componentDidUpdate(prevProps, prevState, snapshot) {
        if (prevProps.selected !== this.props.selected) {
            this.setState({selected:this.props.selected});
        }
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
        let sizeCell = row => <React.Fragment>{row.value ? Helper.Common.formatBytes(row.value) : ""}</React.Fragment>;
        let speedCell = row => <React.Fragment>{row.value ? `${Helper.Common.formatBytes(row.value, 2)}/s` : ""}</React.Fragment>;
        const defaultGreyCell = <span style={{color:"grey"}}>default</span>;
        let engineCell = row => <React.Fragment>{row.original.engine ? <>{row.original.engine} <span style={{opacity:0.55}}>{row.original.engineVersion}</span></> : defaultGreyCell}</React.Fragment>
        let rightAlign = {textAlign: "right" };
        let centerAlign = {textAlign: "center" };

        // use token instead of index
        const selectedRowHandler = (state, rowInfo, column) => {
            if (rowInfo && rowInfo.row) {
                const token = rowInfo && rowInfo.original ? rowInfo.original.token : null;
                const isSelected = token === this.state.selected;  
                return {
                    onClick: (e) => {
                        const selectedRow = rowInfo.original;
                        this.setState({
                            selected: token,
                            selectedRow: selectedRow,
                        });
                        Helper.toConsole("Benchmark Selected", selectedRow);
                        if (this.props.onBenchmarkSelected)
                            this.props.onBenchmarkSelected(selectedRow);
                    },
                    style: {
                        background: token === this.state.selected ? '#ADDDFF' : '',
                        color: token === this.state.selected ? '' : '',
                        cursor: "pointer",
                    }
                }
            } else {
                return {}
            }
        }


        return (
            <div>
                <ReactTable
                    data={this.state.history}
                    showPagination={false}
                    defaultPageSize={pageSize}
                    getTrProps={selectedRowHandler}
                    pageSizeOptions={[pageSize]}
                    pageSize={pageSize}
                    noDataText="history is empty"
                    getNoDataProps={() => {return {style:{color:"gray", marginTop:30}}}}

                    columns={
                        [
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
                                        Header: "Engine",
                                        id: "Engine",
                                        Cell: engineCell,
                                        // accessor: x => x.o_Direct !== undefined ? x.o_Direct.toLowerCase() : "",
                                        accessor: x => x.engine,
                                        minWidth: 88,
                                        // style: ,
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