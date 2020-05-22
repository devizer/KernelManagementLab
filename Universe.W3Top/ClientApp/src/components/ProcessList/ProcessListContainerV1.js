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

export class ProcessListContainerV1 extends Component {
    static displayName = ProcessListContainerV1.name;

    timerId = null;

    constructor(props) {
        super(props);
        
        this.state = {
            openedColumnsChooser: false,
        }
    }
    
    componentDidMount() {
        this.timerId = setInterval(this.waiterTick.bind(this), 1000);
    }

    componentWillUnmount() {
        if (this.timerId !== null) clearInterval(this.timerId);
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

        return (
            <div>
                <div style={{textAlign: "right"}}>
                    <Button variant="text" color="default" onClick={handleOpenColumnsChooser}>
                        {numberOfColumns} column(s) choosen &nbsp;&nbsp;<FontAwesomeIcon style={{}} icon={faCog}/>
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

