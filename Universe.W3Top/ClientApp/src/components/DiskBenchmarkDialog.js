import React from 'react';
import { withStyles } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import TextField from '@material-ui/core/TextField';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import Stepper from '@material-ui/core/Stepper';
import Step from '@material-ui/core/Step';
import StepLabel from '@material-ui/core/StepLabel';
import LinearProgress from '@material-ui/core/LinearProgress';
import Typography from '@material-ui/core/Typography';

import Avatar from '@material-ui/core/Avatar';
import Chip from '@material-ui/core/Chip';
import FaceIcon from '@material-ui/icons/Face';
import DoneIcon from '@material-ui/icons/Done';

import MenuItem from '@material-ui/core/MenuItem';

import * as Enumerable from "linq-es2015";
import * as DataSourceActions from "../stores/DataSourceActions";
import { BenchmarkStepStatusIcon } from "./BenchmarkStepStatusIcon"
import * as Helper from "../Helper";

const PROGRESS_TICK_INTERVAL = 499;
const EMPTY_PROGRESS = {isCompleted: false, steps: []};

const ProgressStyle = theme => ({
    root: {
        margin: theme.spacing.unit * 0,
        // color: '#777',
        animationDuration: "1ms",
        height: "1px",
    },
    colorPrimary: {backgroundColor: '#EEE'},
    barColorPrimary: {backgroundColor: '#888'}
});
const LinearProgress2 = withStyles(ProgressStyle)(LinearProgress); 


const styles = {
    root: {
        width: '99%',
    },
    wizardButton: {
        marginLeft: 0,
        marginRight: 24,
        marginBottom: 8,
    },
    wizardReset: {
        marginLeft: 12,
        marginRight: 12,
        marginBottom: 8,
    },
    instructions: {
        marginTop: 8,
        marginBottom: 8,
    },
    debug : {
        green : { border : "1px solid green", },
        red : { border : "1px solid red", },
    },
    actions: {
        width: "100%",
        textAlign: "center",
    },
    diskChips : {
        marginBottom: 8,
        marginLeft: 2,
        marginRight: 2,
    }
};

const optionStyles = {
    container: {
        display: 'flex',
        flexWrap: 'wrap',
    },
    textField: {
        marginLeft: 0,
        marginRight: 24,
        width: 160,
    },
    dense: {
        marginTop: 19,
    },
    menu: {
        width: 160,
    },
};

const defaultOptions = {
    workingSet: 1024,
    randomAccessDuration: 30,
    disableODirect: false,
    blockSize: 4096,
    threads: 16,
    errors: {isValid: true},
};

const validateOptions = (options) => {
    options.errors = {isValid: true};
    const numberFields = ["workingSet", "randomAccessDuration", "blockSize", "threads"];
    numberFields.map(name => {
        let errorText = Helper.Numbers.isInt(options[name]) && Helper.Numbers.greaterOrEqual(options[name],1)
            ? null
            : "Should be a valid number";
        
        options.errors[name] = errorText;
        if (errorText) options.errors.isValid = false; 
    });
};

let timer = null;
let token = null;

function DiskBenchmarkDialog() {
    const [open, setOpen] = React.useState(true);
    const [activeStep, setActiveStep] = React.useState(0);
    const [disks, setDisks] = React.useState(null);
    const [selectedDisk, setSelectedDisk] = React.useState(null);
    const [options, setOptions] = React.useState(defaultOptions);
    const [progress, setProgress] = React.useState(null);

    React.useEffect(() => {
        if (disks === null) initDisksSource(); 
    });

    function getStepContent(stepIndex) {
        switch (stepIndex) {
            case 0:
                return renderStepSelectDisk();
            case 1:
                return renderStepTuneOptions();
            case 2:
                return renderStepProgress();
            case 3:
                return renderStepProgress();
            default:
                return 'Unknown stepIndex';
        }
    }

    const handleChangeOption = name => event => {
        let tempOptions = {...options};
        tempOptions[name] = event.target.value;
        validateOptions(tempOptions);
        setOptions(tempOptions);
    };
    
    
    function renderStepTuneOptions() {
        const errorText = value => value ? value : " ";
        return (
            <form className={optionStyles.container} noValidate autoComplete="off">
                <Typography>Benchmark options:</Typography>

                <TextField
                    id="benchmark-options-working-set"
                    label="Working Set (Mb)"
                    style={optionStyles.textField}
                    value={options.workingSet}
                    onChange={handleChangeOption('workingSet')}
                    margin="normal"
                    error={options.errors.workingSet}
                    helperText={errorText(options.errors.workingSet)}
                />

                <TextField
                    id="benchmark-options-block-size"
                    label="Block Size (bytes)"
                    style={optionStyles.textField}
                    value={options.blockSize}
                    onChange={handleChangeOption('blockSize')}
                    margin="normal"
                    error={options.errors.blockSize}
                    helperText={errorText(options.errors.blockSize)}
                />

                <TextField
                    id="benchmark-options-threads"
                    label="Threads Number"
                    style={optionStyles.textField}
                    value={options.threads}
                    onChange={handleChangeOption('threads')}
                    margin="normal"
                    error={options.errors.threads}
                    helperText={errorText(options.errors.threads)}
                />

                <TextField
                    id="benchmark-options-duration"
                    label="Duration (seconds)"
                    style={optionStyles.textField}
                    value={options.randomAccessDuration}
                    onChange={handleChangeOption('randomAccessDuration')}
                    margin="normal"
                    error={options.errors.randomAccessDuration}
                    helperText={errorText(options.errors.randomAccessDuration)}
                />
                
            </form>
        )
    }
    
    function initDisksSource() {
        setSelectedDisk(null);
        setDisks(null);
        try {
            let apiUrl = 'api/benchmark/disk/get-disks';
            fetch(apiUrl)
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    Helper.log(response);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(disks => {
                    setDisks(disks);
                    Helper.toConsole("DISKS for benchmark", disks);
                })
                .catch(error => Helper.log(error));
        }
        catch(err)
        {
            console.error('FETCH failed. ' + err);
        }
    }
    
    function renderStepSelectDisk() {
        if (disks === null)
            return (<div>waiting for actual disks info ...</div>);
        
        return (
            <React.Fragment>
                {/* <Typography>Choose disk:</Typography> */}   
                {disks.map(disk => (
                <React.Fragment key={disk.mountEntry.mountPath}>
                    <Chip 
                        label={disk.mountEntry.mountPath} 
                        style={styles.diskChips} 
                        color={disk === selectedDisk ? "primary" : "default"} 
                        onClick={() => handleSelectDisk(disk)}
                    />{' '}
                </React.Fragment>
            ))}
            </React.Fragment>
        )
    }

    function handleClickOpen() {
        setDisks(null);
        setOptions(defaultOptions);
        setSelectedDisk(null);
        setActiveStep(0);
        setProgress(EMPTY_PROGRESS);
        setOpen(true);
    }

    function handleClose() {
        setOpen(false);
        //setDisks(null);
        //setOptions(defaultOptions);
        //setSelectedDisk(null);
        //setActiveStep(0);
    }
    
    const renderStepProgress = function() {
        const pro = progress ? progress : {isCompleted: false, steps: []};
        const formatSpeed = (x) => {let ret = Helper.Common.formatBytes(x,1); return ret === null ? "" : `${ret}/s`};
        const statuses = {Pending: "⚪", InProgress: "⇢", Completed: "⚫"};
        const formatStepStatus = (status) => statuses[status];
        const progressValue = step => step.perCents >= 0.99999 ? 99.999 : step.perCents * 100.0; 
        return (
            <React.Fragment>
                <center>
                <table className="benchmark-progress" border="0" cellPadding={0} cellSpacing={0}>
                {pro.steps.map(step => (
                    <React.Fragment key={step.name}>
                        <tr>
                            <td className="step-status">
                                <BenchmarkStepStatusIcon status={step.state}/>
                                {/* formatStepStatus(step.state) */}
                            </td>
                            <td className="step-name">
                                {step.name}
                            </td>
                            <td className="step-duration">{step.seconds > 0 ? `${step.duration}` : "\xA0" }</td>
                            <td className="step-speed">{step.avgBytesPerSecond > 0 ? `${formatSpeed(step.avgBytesPerSecond)}` : "\xA0"}</td>
                        </tr>
                        <tr>
                            <td></td>
                            <td colSpan="3">
                                <LinearProgress2 value={progressValue(step)} variant={"determinate"} className={"step-progress"}/>
                            </td>
                        </tr>
                    </React.Fragment>
                ))}
                </table>
                </center>
            </React.Fragment>
        )
    };
    
    const progressTick = () => {
        try {
            const apiUrl = `api/benchmark/disk/get-disk-progress-${token}`;
            fetch(apiUrl, {method: "POST"})
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    return response.ok ? response.json() : {error: response.status, details: response}
                })
                .then(benchInfo => {
                    Helper.toConsole("Disk Benchmark Progress", benchInfo);
                    if (benchInfo.progress) {
                        setProgress(benchInfo.progress);
                        if (benchInfo.progress.isCompleted) {
                            clearInterval(timer); timer = 0;
                            token = null;
                            setActiveStep(3);
                        }
                    }
                    else {
                        // benchmark failed.
                        clearInterval(timer);timer = 0;
                    }
                })
                .catch(error => Helper.toConsole(`FETCH for ${apiUrl} failed`, error));
        }
        catch(err)
        {
            console.error('FETCH failed. ' + err);
        }
    };

    const startBenchmark = () => {
        try {
            const apiUrl = 'api/benchmark/disk/start';
            const payload = {...options, mountPath: selectedDisk.mountEntry.mountPath};
            const post={
                headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
                method: "POST",
                body: JSON.stringify(payload)
            };
            
            fetch(apiUrl, post)
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    return response.ok ? response.json() : {error: response.status, details: response}
                })
                .then(benchInfo => {
                    Helper.toConsole("Disk Benchmark Info at start", benchInfo);
                    token = benchInfo.token;
                    setProgress(benchInfo.progress);
                    timer = setInterval(progressTick, PROGRESS_TICK_INTERVAL);
                })
                .catch(error => Helper.log(error));
        }
        catch(err)
        {
            console.error('FETCH failed. ' + err);
        }
    };
    
    const handleSelectDisk = (disk) => setSelectedDisk(disk);
    const handleNext = () => {
        setActiveStep(activeStep + 1);
        if (activeStep === 1) // Perform
        {
            startBenchmark();
        }
    };
    
    const handleBack = () => setActiveStep(activeStep - 1);
    const handleReset = () => { 
        setDisks(null);
        setOptions(defaultOptions);
        setSelectedDisk(null);
        setActiveStep(0);
        setProgress(EMPTY_PROGRESS);
    };

    const steps = ['Select', 'Configure', activeStep === 3 ?"Done" : "Run"];
    const classes = styles;
    const fakeContent = (<Typography className={classes.instructions}>{getStepContent(activeStep)}</Typography>);
    const nextButtonNames = ["Next »", "Start", "Waiting", "Done", "Done"];
    const closeButtonNames = ["Cancel", "Cancel", "Cancel", "Cancel", "Close"];
    const isBackAllowed = activeStep === 1;
    const isNextDisabled = selectedDisk === null || (activeStep === 1 && !options.errors.isValid) || activeStep === 2;
    
    return (
        <div>
            <Button variant="outlined" color="primary" onClick={handleClickOpen}>
                Open benchmark dialog
            </Button>
            <Dialog open={open} onClose={handleClose} aria-labelledby="form-dialog-title" fullWidth={true} maxWidth={"sm"}>
                <DialogTitle id="form-dialog-title" style={{textAlign:"center"}}>Benchmark a local or network disk</DialogTitle>
                <DialogContent style={{textAlign: "center"}}>
                    <Stepper activeStep={activeStep} alternativeLabel style={styles.root}>
                        {steps.map(label => (
                            <Step key={label}>
                                <StepLabel>{label}</StepLabel>
                            </Step>
                        ))}
                    </Stepper>
                    
                    {/*<Typography className={classes.instructions}>{getStepContent(activeStep)}</Typography>*/}
                    {getStepContent(activeStep)}
                </DialogContent>
                <DialogActions>

                    {activeStep === steps.length ? (
                        <div style={styles.actions}>
                            <Button onClick={handleReset} style={classes.wizardReset}>New Benchmark</Button>
                        </div>
                    ) : (
                        <div>
                            
                            <Button variant="contained"
                                    disabled={!isBackAllowed}
                                    onClick={handleBack}
                                    style={classes.wizardButton}
                            >
                                « Back
                            </Button>
                            
                            <Button variant="contained" 
                                    disabled={isNextDisabled}
                                    color="primary" 
                                    onClick={handleNext} 
                                    style={classes.wizardButton}>
                                {nextButtonNames[activeStep]}
                            </Button>

                            <Button variant="contained"
                                    disabled={activeStep === 0 && false}
                                    onClick={handleClose}
                                    style={classes.wizardButton}
                            >
                                {closeButtonNames[activeStep]}
                            </Button>
                        </div>
                    )}
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default DiskBenchmarkDialog;

