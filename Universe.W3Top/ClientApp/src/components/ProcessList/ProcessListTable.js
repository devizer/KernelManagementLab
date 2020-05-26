import React from 'react';
import MomentFormat from 'moment';

import {faCheck, faCheckDouble} from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'

import * as Helper from "../../Helper";

import ReactTable from "react-table";
import "react-table/react-table.css";
import processListStore from "./Store/ProcessListStore";
import ProcessColumnsDefinition from "./ProcessColumnsDefinition";

// require('typeface-noto-sans');

export class ProcessListTable extends React.Component {
    static displayName = ProcessListTable.name;

    constructor(props) {
        super(props);

        this.updatedProcessList = this.updatedProcessList.bind(this);
        this.state = {
            processList: []
        };

        processListStore.on('storeUpdated', this.updatedProcessList);
   }

    updatedProcessList() {
        this.setState({
            processList: processListStore.getProcessList()
        });
    }
    
    static FilterProcessesByKind = (processList, rowsFilters) => {
        if (rowsFilters.NeedNoFilter) return processList;
        let ret = [];
        processList.forEach(process => {
            let isIt = false;
            if (rowsFilters.NeedKernelThreads && process.kind === "Kernel") isIt = true;
            else if (rowsFilters.NeedServices && process.kind === "Service") isIt = true;
            else if (rowsFilters.NeedContainers && process.kind === "Container") isIt = true;
            else if (process.kind === "Init") isIt = true;
            if (isIt) ret.push(process);
        });
        
        return ret;
    }
    
    render() {

        
        let processListRaw = this.state.processList;
        let rowsFilters = processListStore.getRowsFilters();
        let selectedColumns = processListStore.getSelectedColumns();
        let processList = ProcessListTable.FilterProcessesByKind(processListRaw, rowsFilters);
        // page size
        let maxRows = processList.length;
        let pageSize;
        if (rowsFilters.TopFilter > 0) {
            maxRows = Math.min(rowsFilters.TopFilter, processList.length);
            pageSize = maxRows;
            if (processListRaw.length === 0)
                pageSize = 6;
        }
        else {
            pageSize = Math.max(processList.length, 6);
        }
            
        const noop = () => null;
        const isColumnVisible = (columnKey) => selectedColumns.indexOf(columnKey) >= 0;

        // fontFamily: "Noto Sans"
        const tableFontSize = 14
        let styleHeader1 = { fontSize: tableFontSize };
        let styleHeader2 = { fontSize: tableFontSize };

        styleHeader1 = {};
        styleHeader2 = {};
        
        function cellPriority(row) {
            const priority = row.value;
            if (priority == 20) return (<span className="default-priority">0 (default)</span>);
            if (priority >= 0 && priority <= 19) return (<span className="bad-priority">{(priority-20)} (nasty)</span>);
            if (priority >= 21 && priority <= 39) return (<span className="nice-priority">{(priority-20)} (nice)</span>);
            if (priority >= -100 && priority <= -2) return (<span className="rt-priority">RT {(-priority-1)}</span>);
            return priority;
        }
        function cellPercents(row) {
            const perCents = row.value * 100;
            if (perCents < 0.0001) return null;
            return (<span style={{textAlign:"right"}}>{Math.round(perCents*10)/10}&nbsp;%</span>);
        }
        
        const cells = {
            priority: cellPriority,
            userCpuUsage_PerCents: cellPercents,
            kernelCpuUsage_PerCents: cellPercents,
            totalCpuUsage_PerCents: cellPercents,
            childrenUserCpuUsage_PerCents: cellPercents,
            childrenKernelCpuUsage_PerCents: cellPercents,
            childrenTotalCpuUsage_PerCents: cellPercents,
            ioTime_PerCents: cellPercents
        };
        
        // should be cached by 
        let tableHeaders = [];
        ProcessColumnsDefinition.Headers.forEach(header => {
            let tableHeader = {
                Header: header.caption,
                // getHeaderProps: (state, rowInfo, column) => {return {style: styleHeader1}},
                columns: []
            };
            
            header.columns.forEach(column => {
                if (isColumnVisible(`${header.id}.${column.field}`)) {
                    const cell = cells[column.field];
                    let tableColumn = {
                        Header: column.caption,
                        // getHeaderProps: (state, rowInfo, column) => {return {style: styleHeader2}},
                        accessor: column.field,
                        minWidth: 55,
                        Cell: cell,
                        // aggregate: noop,
                    };
                    tableHeader.columns.push(tableColumn);
                }
            });
            
            if (tableHeader.columns.length > 0)
                tableHeaders.push(tableHeader);
        });
        
        
        return (

            <ReactTable
                data={processList}
                showPagination={false}
                defaultPageSize={pageSize}
                pageSizeOptions={[pageSize]}
                pageSize={pageSize}
                noDataText="waiting for ..."
                getNoDataProps={() => {return {style:{fontSize: tableFontSize,width: 200,textAlign: "center", color:"gray", marginTop:30, border: "1px solid #CCC"}}}}
                className={"-striped -highlight"}
                columns={tableHeaders}
            />

        );
        
    }
}
