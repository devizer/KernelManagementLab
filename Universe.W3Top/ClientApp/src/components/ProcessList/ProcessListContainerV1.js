import React, {Component} from "react";
import processListStore from "./Store/ProcessListStore";
import * as ProcessListActions from "./Store/ProcessListActions"
import Button from "@material-ui/core/Button";
import {Dialog} from "@material-ui/core";
import DialogTitle from "@material-ui/core/DialogTitle";
import DialogContent from "@material-ui/core/DialogContent";

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {faCog} from '@fortawesome/free-solid-svg-icons'

import "./ProcessList.css"
import {ColumnChooserComponent} from "./ColumnChooserComponent";
import {ProcessListTable} from "./ProcessListTable";
import * as DataSourceActions from "../../stores/DataSourceActions";
import * as Helper from "../../Helper";

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

        try {
            let apiUrl = 'api/ProcessList';
            fetch(apiUrl)
                .then(response => {
                    Helper.toConsole(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    // Helper.toConsole(response);
                    // Helper.toConsole(response.body);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(processes => {
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
            console.warn('FETCH failed. ' + err);
            this.requestProcessListUpdate();
        }
    }
    
    requestProcessListUpdate() {
        if (this.isRunning)
            setTimeout(this.refreshProcessList, 1000);
    }
    
    render() {
        // TODO: columns chooser button and list of processes
        let handleOpenColumnsChooser = _ => this.setState({openedColumnsChooser: true});
        let handleCloseColumnsChooser = _ => this.setState({openedColumnsChooser: false});

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
                
            </div>
        )
    }
}

