import React, {Component} from "react";
import ProcessColumnsDefinition from "./ProcessColumnsDefinition";
import processListStore from "./Store/ProcessListStore";
import * as ProcessListActions from "./Store/ProcessListActions"
import {ProcessColumnChooserDialog} from "./ProcessColumnChooserDialog"
import Button from "@material-ui/core/Button";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {faCog} from '@fortawesome/free-solid-svg-icons'
import {Dialog} from "@material-ui/core";
import DialogTitle from "@material-ui/core/DialogTitle";
import DialogContent from "@material-ui/core/DialogContent";
import DialogActions from "@material-ui/core/DialogActions";

import "./ProcessList.css"
import {ColumnChooserComponent} from "./ColumnChooserComponent";
import DataSourceStore from "../../stores/DataSourceStore";

export class ProcessListContainerV1 extends Component {
    static displayName = ProcessListContainerV1.name;

    timerId = null;

    constructor(props) {
        super(props);

        this.updatedSelectedColumns = this.updatedSelectedColumns.bind(this);
        
        this.state = {
            openedColumnsChooser: false,
        };

        processListStore.on('storeUpdated', this.updatedSelectedColumns);
    }
    
    componentDidMount() {
        this.timerId = setInterval(this.waiterTick.bind(this), 1000);
        
    }

    componentWillUnmount() {
        if (this.timerId !== null) clearInterval(this.timerId);
        processListStore.removeListener('storeUpdated', this.updatedSelectedColumns);
    }
    
    updatedSelectedColumns() {
        this.setState({selectedColumns: processListStore.getSelectedColumns()})
    }

    waiterTick()
    {
        this.refreshProcessList();
    }

    refreshProcessList() {
        // TODO: call API
        let processes = [{},{},{},{}];
        ProcessListActions.ProcessListUpdated(processes);
    }
    
    render() {
        // TODO: columns chooser button and list of processes
        let numberOfColumns = processListStore.getSelectedColumns().length;
        let handleOpenColumnsChooser = _ => this.setState({openedColumnsChooser: true});
        let handleCloseColumnsChooser = _ => this.setState({openedColumnsChooser: false});
        
        let columnsCounter = null;
        if (numberOfColumns > 1) columnsCounter = () => (<>{numberOfColumns} columns</>);
        else if (numberOfColumns === 1) columnsCounter = () => (<>Just one column?</>);
        else columnsCounter = () => (<>No columns?</>);

        return (
            <div>
                <div style={{textAlign: "right"}}>
                    <Button variant="text" color="default" onClick={handleOpenColumnsChooser}>
                        {columnsCounter()} &nbsp;<FontAwesomeIcon style={{}} icon={faCog}/>
                    </Button>
                </div>

                <p>.... list of processes</p>
                
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

