import React from 'react';
import { withStyles, MuiThemeProvider, createMuiTheme } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import TextField from '@material-ui/core/TextField';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import Paper from '@material-ui/core/Paper';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import Stepper from '@material-ui/core/Stepper';
import Step from '@material-ui/core/Step';
import StepLabel from '@material-ui/core/StepLabel';
import LinearProgress from '@material-ui/core/LinearProgress';
import Typography from '@material-ui/core/Typography';
import Popper from '@material-ui/core/Popper';

import Avatar from '@material-ui/core/Avatar';
import Chip from '@material-ui/core/Chip';
import FaceIcon from '@material-ui/icons/Face';
import DoneIcon from '@material-ui/icons/Done';
import { faServer } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'

import MenuItem from '@material-ui/core/MenuItem';

import BenchmarkProgressTable from "./BenchmarkProgressTable";
import DiskAvatarContent from "./DiskAvatarContent"

import * as Enumerable from "linq-es2015";
import classNames from "classnames";
import * as queryString from 'query-string';
import * as DataSourceActions from "../../stores/DataSourceActions";
import { BenchmarkStepStatusIcon } from "./BenchmarkStepStatusIcon"
import * as Helper from "../../Helper";

import ReactTable from "react-table";
import "react-table/react-table.css";

// TODO: add threads count for random access
// TODO: Icon for O_DIRECT

export class DiskBenchmarkHistory extends React.Component {

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
    
    fetchDiskHistorySource() {
        try {
            let apiUrl = 'api/benchmark/disk/get-disk-benchmark-history';
            fetch(apiUrl, {method: "POST"})
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    Helper.log(response);
                    return response.ok ? response.json() : {error: response.status, details: response}
                })
                .then(history => {
                    this.setState({history:history});
                    Helper.toConsole("HISTORY for disk benchmark", history);
                })
                .catch(error => Helper.log(error));
        }
        catch(err)
        {
            console.error('FETCH failed. ' + err);
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
                    noDataText="no history"
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
                                        style: {fontWeight: "bold"}
                                    },
                                    {
                                        Header: "Working Set",
                                        id: "workingSetSize",
                                        Cell: sizeCell,
                                        accessor: "workingSetSize"
                                    },
                                    {
                                        Header: "O_DIRECT",
                                        id: "O_DIRECT",
                                        accessor: x => x.o_Direct !== undefined ? x.o_Direct.toLowerCase() : "",
                                        minWidth: 65,
                                        style: centerAlign,
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
                                    },
                                    {
                                        Header: "Read",
                                        id: "seqRead",
                                        Cell: speedCell,
                                        accessor: "seqRead",
                                        style: rightAlign,
                                    },
                                    {
                                        Header: "Write",
                                        id: "seqWrite",
                                        Cell: speedCell,
                                        accessor: "seqWrite",
                                        style: rightAlign,
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
                                    },
                                    {
                                        Header: "Read",
                                        Cell: speedCell,
                                        accessor: "randRead1T",
                                        style: rightAlign,
                                    },
                                    {
                                        Header: "Write",
                                        id: "randWrite1T",
                                        Cell: speedCell,
                                        accessor: "randWrite1T",
                                        style: rightAlign,
                                    },
                                    {
                                        Header: "Read",
                                        id: "randReadNT",
                                        Cell: speedCell,
                                        accessor: "randReadNT",
                                        style: rightAlign,
                                    },
                                    {
                                        Header: "Write",
                                        id: "randWriteNT",
                                        Cell: speedCell,
                                        accessor: "randWriteNT",
                                        style: rightAlign,
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