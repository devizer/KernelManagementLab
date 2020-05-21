import React, {Component} from "react";
import ProcessColumnsDefinition from "./ProcessColumnsDefinition";
import processListStore from "./Store/ProcessListStore";
import * as ProcessListActions from "./Store/ProcessListActions"


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

import * as Enumerable from "linq-es2015";
import classNames from "classnames";
import * as queryString from 'query-string';

import * as Helper from "../../Helper";

import { ReactComponent as WizardIconSvg } from '../../icons/Wizard-Icon.svg';
const WizardIcon = (size=34,color='#333') => (<WizardIconSvg style={{width: size,height:size,fill:color,strokeWidth:'6px',stroke:color }} />);

var Color = require("color");

export class ProcessColumnChooserDialog extends Component {
    
    constructor(props) {
        super(props);
    }
    
    
}