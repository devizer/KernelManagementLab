import React, {Component} from "react";
import processListStore from "./Store/ProcessListStore";
import * as ProcessListActions from "./Store/ProcessListActions"
import Button from "@material-ui/core/Button";
import {Dialog, TextField} from "@material-ui/core";
import DialogTitle from "@material-ui/core/DialogTitle";
import DialogContent from "@material-ui/core/DialogContent";

import Radio from '@material-ui/core/Radio';
import RadioGroup from '@material-ui/core/RadioGroup';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import FormControl from '@material-ui/core/FormControl';
import FormLabel from '@material-ui/core/FormLabel';


import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {faCog} from '@fortawesome/free-solid-svg-icons'

import "./ProcessList.css"
import {ColumnChooserComponent} from "./ColumnChooserComponent";
import {RowsFiltersComponent} from "./RowsFiltersComponent"
import {ProcessListTable} from "./ProcessListTable";
import * as ProcessListLocalStorage from "./Store/ProcessListLocalStore";
import * as DataSourceActions from "../../stores/DataSourceActions";
import * as Helper from "../../Helper";
import Checkbox from "@material-ui/core/Checkbox";

export class ProcessListContainerV1 extends Component {
    static displayName = ProcessListContainerV1.name;

    timerId = null;
    isRunning = false;

    constructor(props) {
        super(props);

        this.updatedSelectedColumns = this.updatedSelectedColumns.bind(this);
        this.refreshProcessList = this.refreshProcessList.bind(this);
        this.requestProcessListUpdate = this.requestProcessListUpdate.bind(this);
        
        this.state = {
            openedColumnsChooser: false,
            openedRowsFilters: false
        };

        processListStore.on('storeUpdated', this.updatedSelectedColumns);
    }
    
    componentDidMount() {
        // do nothing
        this.timerId = setInterval(this.waiterTick.bind(this), 1000);
        // it works well on slow connections
        this.isRunning = true;
        this.requestProcessListUpdate();
    }

    componentWillUnmount() {
        this.isRunning = false;
        if (this.timerId !== null) clearInterval(this.timerId);
        processListStore.removeListener('storeUpdated', this.updatedSelectedColumns);
    }
    
    updatedSelectedColumns() {
        this.setState({selectedColumns: processListStore.getSelectedColumns()})
    }

    waiterTick()
    {
        // setTimeout
        // this.refreshProcessList();
    }

    refreshProcessList() {

        let apiUrl = 'api/ProcessList';
        try {
            fetch(apiUrl)
                .then(response => {
                    Helper.toConsole(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    // Helper.toConsole(response);
                    // Helper.toConsole(response.body);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(processes => {
                    if (processes.length > 42 && process.env.NODE_ENV !== 'production')
                        processes = processes.slice(0,42);

                    ProcessListLocalStorage.fillCalculatedFields(processes);
                    ProcessListActions.ProcessListUpdated(processes);
                    Helper.notifyTrigger("ProcessListArrived", "wow!");
                    Helper.toConsole("ProcessList", processes);
                    this.requestProcessListUpdate();
                })
                .catch(error => { 
                    console.log(error);
                    this.requestProcessListUpdate();
                });
        }
        catch(err)
        {
            this.requestProcessListUpdate();
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }
    }
    
    requestProcessListUpdate() {
        if (this.isRunning)
            setTimeout(this.refreshProcessList, 1000);
    }
    
    
    render() {
        // TODO: columns chooser button and list of processes
        let handleOpenColumnsChooser =  _ => this.setState({openedColumnsChooser: true});
        let handleCloseColumnsChooser = _ => this.setState({openedColumnsChooser: false});
        let handleOpenRowsFilters =     _ => this.setState({openedRowsFilters: true});
        let handleCloseRowsFilters =    _ => {
            this.setState({openedRowsFilters: false}); 
        }

        function ColumnChooserButtonText() {
            let numberOfColumns = processListStore.getSelectedColumns().length;
            let columnsCounter = null;
            if (numberOfColumns > 1) columnsCounter = (<>{numberOfColumns} columns</>);
            else if (numberOfColumns === 1) columnsCounter = (<>One column</>);
            else columnsCounter = (<>No columns?</>);
            return columnsCounter;
        }

        return (
            <div>
                <div style={{textAlign: "right"}}>
                    <Button variant="text" color="default" onClick={handleOpenRowsFilters}>
                        Filter rows&nbsp;&nbsp;<FontAwesomeIcon style={{}} icon={faCog}/>
                    </Button>
                    &nbsp;&nbsp;&nbsp;
                    <Button variant="text" color="default" onClick={handleOpenColumnsChooser}>
                        <ColumnChooserButtonText/>&nbsp;&nbsp;<FontAwesomeIcon style={{}} icon={faCog}/>
                    </Button>
                </div>
                
                <div style={{height: 8, fontSize: 8}}>&nbsp;</div>

                <ProcessListTable/>

                <Dialog open={this.state.openedColumnsChooser} onClose={handleCloseColumnsChooser}
                        aria-labelledby="columns-chooser-title" fullWidth={true} maxWidth={"md"}>
                    <DialogTitle id="columns-chooser-title" style={{textAlign:"center"}}>
                        <Button variant="text" onClick={handleCloseColumnsChooser}>
                            Selected Columns
                        </Button>
                    </DialogTitle>
                    <DialogContent style={{textAlign_ignore: "center"}} id="columns-chooser-content">
                        <ColumnChooserComponent />
                    </DialogContent>
                </Dialog>
                
                <Dialog open={this.state.openedRowsFilters} onClose={handleCloseRowsFilters}
                        aria-labelledby="rows-chooser-title" fullWidth={true} maxWidth={"sm"}>
                    <DialogTitle id="rows-chooser-title" style={{textAlign:"center", borderBottom: "1px solid #AAA"}}>
                        <Button variant="text" onClick={handleCloseRowsFilters}>
                            Process Filters
                        </Button>
                    </DialogTitle>
                    <DialogContent style={{textAlign_ignore: "center"}} id="columns-chooser-content">
                        <RowsFiltersComponent />
                    </DialogContent>
                </Dialog>

            </div>
        )
    }
}

