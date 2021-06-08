import React from 'react';
import MomentFormat from 'moment';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {faCheck, faCheckDouble} from '@fortawesome/free-solid-svg-icons'
import * as Helper from "../../Helper";

import { withStyles, MuiThemeProvider, createMuiTheme } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';

export class DiskBenchmarkResult extends React.Component {
    static displayName =  DiskBenchmarkResult.name;

    constructor(props) {
        super(props);
        
        this.state = {
            opened: false,
            selectedRow: null,
        };
        
        this.handleClose = this.handleClose.bind(this);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        const isOpenedChanged = prevProps.opened !== this.props.opened;
        const isRowChanged = (prevProps.selectedRow ? prevProps.selectedRow.token : '') !== (this.props.selectedRow ? this.props.selectedRow.token : '');   
        if (isOpenedChanged || isRowChanged) {
            this.setState({
                opened: this.props.opened,
                selectedRow: this.props.selectedRow,
            });
        }
    }

    handleClose() {
        this.setState({opened: false});
        // alert("CLOSED, opened: " + this.state.opened);
    }

    render() {
        Helper.toConsole(`[DiskBenchmarkResult::render] this.state.opened=${this.state.opened}`);
        
        return (
            <Dialog open={this.state.opened} onClose={this.handleClose} aria-labelledby="form-dialog-title" fullWidth={true} maxWidth={"md"}>
                <DialogContent style={{textAlign: "center"}} >
                    {JSON.stringify(this.state.selectedRow)}
                    <br/>
                    a<br/>
                    aa<br/>
                    aaa<br/>
                    aaaaa<br/>
                    aaaaaa<br/>
                    aaaaaa<br/>
                    aaaaaaa<br/>
                    aaaaaaaa<br/>
                    aaaaaaaaaa<br/>
                    aaaaaaaaaaa<br/>
                    aaaaaaaaaaaa
                </DialogContent>
            </Dialog>
        );
    }

}