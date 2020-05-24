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
    
    render() {

        
        let processList = this.state.processList;
        let selectedColumns = processListStore.getSelectedColumns();
        let pageSize = Math.max(processList.length, 6);
        const noop = () => null;
        const isColumnVisible = (columnKey) => selectedColumns.indexOf(columnKey) >= 0;

        // fontFamily: "Noto Sans"
        const tableFontSize = 14
        const styleHeader1 = { fontSize: tableFontSize };
        const styleHeader2 = { fontSize: tableFontSize };
        
        // should be cached by 
        let tableHeaders = [];
        ProcessColumnsDefinition.Headers.forEach(header => {
            let tableHeader = {
                Header: header.caption,
                getHeaderProps: (state, rowInfo, column) => {return {style:styleHeader1}},
                columns: []
            };
            
            header.columns.forEach(column => {
                if (isColumnVisible(`${header.id}.${column.field}`)) {
                    let tableColumn = {
                        Header: column.caption,
                        getHeaderProps: (state, rowInfo, column) => {return {style:styleHeader2}},
                        accessor: column.field,
                        minWidth: 55,
                        Cell: undefined,
                        aggregate: noop,
                    }
                    tableHeader.columns.push(tableColumn);
                }
            });
            
            if (tableHeader.columns.length > 0)
                tableHeaders.push(tableHeader);
        });
        
        
        return (

            <ReactTable
                data={this.state.history}
                showPagination={false}
                defaultPageSize={pageSize}
                pageSizeOptions={[pageSize]}
                pageSize={pageSize}
                noDataText="waiting for ..."
                getNoDataProps={() => {return {style:{fontSize: tableFontSize,width: 200,textAlign: "center", color:"gray", marginTop:30, border: "1px solid #CCC"}}}}
                className="-striped -highlight"
                columns={tableHeaders}
            />

        );
        
    }
}
