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
import {DiskBenchmarkHistory} from "./DiskBenchmarkHistory";
// import Paper from "./Popper-Lab";

import { ReactComponent as WizardIconSvg } from '../../icons/Wizard-Icon.svg';
// import {IoEngineSelector} from "./IoEngineSelector";
import FormControl from "@material-ui/core/FormControl";
import InputLabel from "@material-ui/core/InputLabel";
import Select from "@material-ui/core/Select";
import Input from "@material-ui/core/Input";
import FormHelperText from "@material-ui/core/FormHelperText";
const WizardIcon = (size=34,color='#333') => (<WizardIconSvg style={{width: size,height:size,fill:color,strokeWidth:'6px',stroke:color }} />);

var Color = require("color");

const PROGRESS_TICK_INTERVAL = 499;
const EMPTY_PROGRESS_STEP = {state:"Pending", canHaveMetrics:true, name:"\xA0".repeat(35)};
const EMPTY_PROGRESS_STEP2 = {state:"Pending", canHaveMetrics:false, name:"\xA0".repeat(45)};
const EMPTY_PROGRESS = {isCompleted: false, steps: [EMPTY_PROGRESS_STEP,EMPTY_PROGRESS_STEP2,EMPTY_PROGRESS_STEP,EMPTY_PROGRESS_STEP2,EMPTY_PROGRESS_STEP]};

const mainColorReadWrite="#3182bd";
const mainColorReadOnly= "#de2d26";
// const mainColorUnselectedReadOnly= "#DDAAB1";
const mainColorUnselectedReadOnly = "#E2DEDE";


// const getUnselectedColor = (c) => Color(c).lighten(0.8); 
// const getUnselectedColor = (c) => Color(c).whiten(3);
const getUnselectedColor = (c) => Color(c).desaturate(0.8).lighten(0.9);

const
    themeReadWrite = createMuiTheme({ palette: { primary: { main: getUnselectedColor(mainColorReadWrite).hex(), } }, }),
    themeReadWriteSelected = createMuiTheme({ palette: { primary: { main: mainColorReadWrite, } }, }),
    themeReadOnly = createMuiTheme({palette: { primary: { main: mainColorUnselectedReadOnly, } }, }),
    themeReadOnlySelected = createMuiTheme({ palette: { primary: { main: mainColorReadOnly, } }, });
    

const styles = {
    root: {
        width: '99%',
        paddingTop: 1,
    },
    wizardButton: {
        marginLeft: 12,
        marginRight: 12,
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
    actionButtons: {
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
        width: 201,
    },
    engineField: {
        marginTop: 4,
        marginLeft: 0,
        marginRight: 24,
        marginBottom: 8,
        width: 201,
        textAlign: "left"
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
    engine: "auto",
    errors: {isValid: true},
};

const validateOptions = (options) => {
    options.errors = {isValid: true};
    const numberFields = ["workingSet", "randomAccessDuration", "blockSize", "threads"];
    numberFields.forEach(name => {
        let errorText = Helper.Numbers.isInt(options[name]) && Helper.Numbers.greaterOrEqual(options[name],1)
            ? null
            : "Should be a valid number";
        
        options.errors[name] = errorText;
        if (errorText) options.errors.isValid = false; 
    });

    if (!options.errors.blockSize && Number(options.blockSize) % 512 != 0) {
        options.errors.blockSize = "Should be multiplier of 512"; 
        options.errors.isValid = false;
    }
};

let timer = null; // for progress
let token = null; // for progress

const getNeedHideDialog = (props) => {
    let needHideDialog = false;
    // do we need to calc it on each render?
    Helper.toConsole(`DiskBenchmarkDialog::props are`, props);
    let queryParams = queryString.parse(props.location ? props.location.search : "");
    Helper.toConsole(`DiskBenchmarkDialog::query string is`, queryParams);
    needHideDialog = queryParams['history'] !== undefined;
    Helper.log(`DiskBenchmarkDialog::needHideDialog ${needHideDialog}`);
    return needHideDialog;
}

function DiskBenchmarkDialog(props) {
    const [open, setOpen] = React.useState(!getNeedHideDialog(props));
    const [activeStep, setActiveStep] = React.useState(0);
    const [disks, setDisks] = React.useState(null);
    const [engines, setEngines] = React.useState(null);
    const [selectedDisk, setSelectedDisk] = React.useState(null);
    const [options, setOptions] = React.useState(defaultOptions);
    const [progress, setProgress] = React.useState(null);
    const [errorAnchor, setErrorAnchor] = React.useState(null);
    const [error, setError] = React.useState(null);
    const [historyTrigger, setHistoryTrigger] = React.useState(null);
    const errorElement = React.useRef(); // where is popper bound to?


    React.useEffect(() => {
        // on hide disks are nullified
        if (disks === null) initDisksSource();
    });

    function getStepContent(stepIndex) {
        let renderProgress = () => (<BenchmarkProgressTable progress={progress}/>);
        switch (stepIndex) {
            case 0:
                return renderStepSelectDisk();
            case 1:
                return renderStepTuneOptions();
            case 2:
            case 3:
                return renderProgress();
            default:
                return `Unknown stepIndex ${stepIndex}`;
        }
    }

    const handleChangeOption = name => event => {
        function kill_it() {
            const replacer = (key, value) => {
                if (key === 'FiberNode') return undefined;
                return value;
            };
            // Helper.log(`INTUP EVENT ${JSON.stringify(event, replacer)}`);
            Helper.log(`INPUT EVENT [${event.type}] [${JSON.stringify(event.target.id)}] [${JSON.stringify(event.target.value)}]`);
            Helper.toConsole(`INPUT EVENT`, event);
            const {type, nativeEvent} = event;
        }
        
        let tempOptions = {...options};
        tempOptions[name] = event.target.value;
        validateOptions(tempOptions);
        setOptions(tempOptions);
    };
    
    
    function renderStepTuneOptions() {
        const errorText = value => value ? value : " ";
        const safeEngines = engines === null ? [] : engines;
        const spanVersion = version => {
            let ret = "";
            ["major", "minor", "build", "revision"].forEach(part => {
                if (version !== null && version !== undefined && version[part] !== null && version[part] !== undefined && version[part] !== -1) 
                    ret = ret + (ret.length === 0 ? "" : ".") + version[part]; 
            });
            return ret === "" ? ret : (<span style={{color:"grey", fontSize:"85%"}}>&nbsp;&nbsp;v{ret}</span>);
        };
        return (
            <form className={optionStyles.container} noValidate autoComplete="off">
                {/*<Typography>Benchmark options:</Typography>*/}

                <FormControl className={classes.formControl} style={optionStyles.engineField}>
                    <InputLabel htmlFor="engine-helper">Engine</InputLabel>
                    <Select
                        value={options.engine}
                        onChange={handleChangeOption('engine')}
                        input={<Input name="benchmark-options-engine" id="engine-helper" />}
                    >
                        <MenuItem value="auto">auto</MenuItem>
                        {safeEngines.map((engine, engineIndex) => (
                            <MenuItem value={engine.idEngine}>{engine.idEngine}{spanVersion(engine.version)}</MenuItem>
                        ))}
                    </Select>
                    <FormHelperText>libaio is preferred</FormHelperText>
                </FormControl>
                <span style={{fontSize:"1px"}}><br/></span>

                <TextField
                    id="benchmark-options-working-set"
                    label="Working Set (MB)"
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
                    label="Threads or IO Depth"
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
        let apiUrl = 'api/benchmark/disk/get-disks';
        try {
            fetch(apiUrl)
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    Helper.log(response);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(disksAndEngines => {
                    setDisks(disksAndEngines.disks);
                    setEngines(disksAndEngines.engines);
                    Helper.toConsole("DISKS for benchmark", disksAndEngines.disks);
                    Helper.toConsole("Engines for benchmark", disksAndEngines.engines);
                })
                .catch(error => Helper.log(error));
        }
        catch(err)
        {
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }
    }
    
    function renderStepSelectDisk() {
        if (disks === null)
            return (<div><i>waiting for actual disks info ...</i></div>);

        const isReadOnly = (disk) => !(disk.freeSpace > 0);
        const getColorOfSelectedDisk = (disk) => !isReadOnly(disk) ? "primary" : "secondary";
        const getFsColor = (disk) => {
            if (disk === selectedDisk) return isReadOnly(disk) ? "#F2DCE4" : "#B9BECE";
            return "#666";
        };

        const getTheme = (disk) =>
            (disk === selectedDisk)
                ? (isReadOnly(disk) ? themeReadOnlySelected : themeReadWriteSelected)
                : (isReadOnly(disk) ? themeReadOnly : themeReadWrite);
        
        return (
            <React.Fragment>
                {/* <Typography>Choose disk:</Typography> */}   
                {disks.map((disk, diskIndex) => (
                <React.Fragment key={disk.mountEntry.mountPath}>
                    <MuiThemeProvider theme={getTheme(disk)}>
                    <Chip id={`DISK_${diskIndex+1}`}
                        avatar={<Avatar><DiskAvatarContent disk={disk}/></Avatar>}
                        clickable
                        label={(<span><b>{disk.mountEntry.mountPath}</b> <span style={{opacity:0.66}}>({disk.mountEntry.fileSystem})</span></span>)} 
                        style={styles.diskChips} 
                        /*color={disk === selectedDisk ? getColorOfSelectedDisk(disk) : "default"}*/
                        color={"primary"}
                        onClick={() => handleSelectDisk(disk)}
                    />{' '}
                    </MuiThemeProvider>
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
    
    function notifyCancelDiskBenchmark()
    {
        let apiUrl = `api/benchmark/disk/cancel-disk-benchmark-${token}`;
        try {
            fetch(apiUrl, {method: "POST"})
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    return response.ok ? response.json() : {error: response.status, details: response}
                })
                .then(data => {
                    // nothing to do
                })
                .catch(error => Helper.log(error));
        } catch (err) {
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }
    }

    function handleCancel() {
        setOpen(false);
        if (token) notifyCancelDiskBenchmark();
    }


    const progressTick = () => {
        const apiUrl = `api/benchmark/disk/get-disk-benchmark-progress-${token}`;
        try {
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
                            setHistoryTrigger(new Date());
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
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }
    };

    const startBenchmark = () => {
        const apiUrl = 'api/benchmark/disk/start-disk-benchmark';
        try {
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
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }
    };
    
    const handleSelectDisk = (disk) => {
        Helper.toConsole("DISK SELECTED", disk);
        setSelectedDisk(disk);
    };
    
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
    const closeButtonNames = ["Cancel", "Cancel", "Abort", "Close", "Close"];
    const isBackAllowed = activeStep === 1;
    const isNextDisabled = selectedDisk === null || (activeStep === 1 && !options.errors.isValid) || activeStep === 2;
    const areNextBackButtonsVisible = activeStep <= 1;
    const getColorOfSelectedDisk = (disk) => disk.freeSpace > 0 ? "primary" : "secondary";


    const applyError = (text) => {
        setError(text);
        setErrorAnchor(text ? errorElement.current : null);
    };
    const errorOpened = Boolean(errorAnchor);
    const errorId = errorOpened ? "error-popper-next2" : null;

    React.useEffect(() => {
        if (error === null) {
            setTimeout(() => {
                // applyError("NO ERROR YET");
                // console.log("POPPER ATCIVAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAATED");
            }, 2000);
        }
    });

    let formatBytes = Helper.Common.formatBytes; 
    let renderDiskSize = () => selectedDisk.freeSpace ? `${formatBytes(selectedDisk.freeSpace)} of ${formatBytes(selectedDisk.totalSize)} free` : `${formatBytes(selectedDisk.totalSize)}`;
    const renderDialogHeader = () =>
        selectedDisk == null ? <span>Benchmark a local or network disk</span> : (
            <span>
                Benchmark <b className="nowrap">{selectedDisk.mountEntry.mountPath}</b>{' '}
                ({selectedDisk.mountEntry.fileSystem})
                <span className={classNames(activeStep >= 2 && "hidden")}>
                    {', '}
                    <span className={classNames("nowrap")}>{renderDiskSize()}</span>
                </span>
            </span>
        );


        

    return (
        <div>
            
            <Button variant="contained" color="secondary" onClick={handleClickOpen} className={classNames(open && "hidden")} style={{marginBottom:20}}>
                {WizardIcon(34,'white')} Start disk benchmark
            </Button>
            
            <DiskBenchmarkHistory trigger={historyTrigger}/>

            <Popper id={errorId} open={errorOpened} anchorEl={errorAnchor} placement="bottom">
                <React.Fragment>
                    {Enumerable.Range(1,10).ToArray().map(i => (
                        <React.Fragment key={i}>
                        <br/>
                        <Paper >
                                <Typography style={{padding:16,color: "darkred"}}>{i}: The content of the Popper.</Typography>
                        </Paper>
                        </React.Fragment>
                    ))}
                </React.Fragment>
            </Popper>
            
            <Dialog open={open} onClose={handleClose} aria-labelledby="form-dialog-title" fullWidth={true} maxWidth={"sm"}>
                <DialogTitle id="form-dialog-title" style={{textAlign:"center"}}>
                    <div style={{minHeight:50}} className="Aligner">
                        {renderDialogHeader()}
                    </div>
                </DialogTitle>
                <DialogContent style={{textAlign: "center"}} >
                    {/*<Button variant="contained" ref={errorElement}>WTH</Button>*/}
                    <Stepper activeStep={activeStep} alternativeLabel style={styles.root}  id="wizard" ref={errorElement}>
                        {steps.map(label => (
                            <Step key={label}>
                                <StepLabel>{label}</StepLabel> 
                            </Step>
                        ))}
                    </Stepper>
                    
                    {/*<Typography className={classes.instructions}>{getStepContent(activeStep)}</Typography>*/}
                    {getStepContent(activeStep)}
                </DialogContent>
                <DialogActions id="disk-benchmark-actions" >
                    <React.Fragment>
                    {activeStep === steps.length ? (
                        <div style={styles.actions}>
                            <Button id={"BTN_DISK_BENCHMARK_ANOTHER"} onClick={handleReset} style={classes.wizardReset}>New Benchmark</Button>
                        </div>
                    ) : (
                        <div style={ activeStep === 2 ? styles.actions : styles.actions}>
                            
                            <Button id={"BTN_DISK_BENCHMARK_BACK"}
                                    variant="contained"
                                    disabled={!isBackAllowed}
                                    onClick={handleBack}
                                    style={classes.wizardButton}
                                    className={classNames(!areNextBackButtonsVisible && "hidden")}
                            >
                                « Back
                            </Button>
                            
                            <Button id={"BTN_DISK_BENCHMARK_NEXT"}
                                    variant="contained" 
                                    disabled={isNextDisabled}
                                    color="primary" 
                                    onClick={handleNext} 
                                    style={classes.wizardButton}
                                    className={classNames(!areNextBackButtonsVisible && "hidden")}
                            >
                                {nextButtonNames[activeStep]}
                            </Button>
                            
                            <Button id={"BTN_DISK_BENCHMARK_CLOSE"}
                                    variant="contained"
                                    disabled={activeStep === 0 && false}
                                    onClick={handleCancel}
                                    style={classes.wizardButton}
                            >
                                {closeButtonNames[activeStep]}
                            </Button>

                            <Button id={"BTN_DISK_BENCHMARK_BACKGROUND"}
                                    variant="contained"
                                    disabled={activeStep === 0 && false}
                                    onClick={handleClose}
                                    style={classes.wizardButton}
                                    className={classNames(activeStep < 2 && "hidden")}
                            >
                                Background
                            </Button>
                        </div>
                    )}
                    </React.Fragment>
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default DiskBenchmarkDialog;

