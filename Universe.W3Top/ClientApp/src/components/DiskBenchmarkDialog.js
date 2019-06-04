import React from 'react';
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
import Typography from '@material-ui/core/Typography';

import Avatar from '@material-ui/core/Avatar';
import Chip from '@material-ui/core/Chip';
import FaceIcon from '@material-ui/icons/Face';
import DoneIcon from '@material-ui/icons/Done';


import * as Enumerable from "linq-es2015";
import * as DataSourceActions from "../stores/DataSourceActions";
import * as Helper from "../Helper";




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

function getSteps() {
    return ['Select', 'Tune', 'Perform', "Done"];
}


function DiskBenchmarkDialog() {
    const [open, setOpen] = React.useState(true);
    const [activeStep, setActiveStep] = React.useState(0);
    const [disks, setDisks] = React.useState(null);
    const [selectedDisk, setSelectedDisk] = React.useState(null);

    React.useEffect(() => {
        if (disks === null) initDisksSource(); 
    });

    function getStepContent(stepIndex) {
        switch (stepIndex) {
            case 0:
                return renderStepSelectDisk();
            case 1:
                return 'Tune benchmark options...';
            case 2:
                return 'Perform...';
            case 3:
                return 'Welldone';
            default:
                return 'Unknown stepIndex';
        }
    }
    
    function initDisksSource() {
        setDisks(null);
        try {
            let apiUrl = 'api/benchmark/disk/get-list';
            fetch(apiUrl)
                .then(response => {
                    console.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    console.log(response);
                    console.log(response.body);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(disks => {
                    setDisks(disks);
                    Helper.toConsole("DISKS for benchmark", disks);
                })
                .catch(error => console.log(error));
        }
        catch(err)
        {
            console.log('FETCH failed. ' + err);
        }
    }
    
    function renderStepSelectDisk() {
        if (disks === null)
            return (<div>waiting for actual disks info ...</div>);
        
        return (
            <React.Fragment>
                {/* <Typography>Choose disk:</Typography> */}   
                {disks.map(disk => (
                <React.Fragment>
                    <Chip label={disk.mountEntry.mountPath} style={styles.diskChips} 
                        color={disk === selectedDisk ? "primary" : ""}
                          onClick={() => handleSelectDisk(disk)}
                    />{' '}
                </React.Fragment>
            ))}
            </React.Fragment>
        )
    }

    function handleClickOpen() {
        setOpen(true);
        initDisksSource();
    }

    function handleClose() {
        setOpen(false);
    }
    
    let handleSelectDisk = (disk) => setSelectedDisk(disk);

    let handleNext = () => setActiveStep(activeStep + 1);
    let handleBack = () => setActiveStep(activeStep - 1);
    let handleReset = () => { 
        setSelectedDisk(null);
        setActiveStep(0); 
    };

    const steps = getSteps();
    const classes = styles;
    const fakeContent = (<Typography className={classes.instructions}>{getStepContent(activeStep)}</Typography>);
    
    // (Enumerable.Range(1,100).ToArray().map(
    const longFakeContent = () => (
        <React.Fragment>
            {Enumerable.Range(1,100).ToArray().map( x=> (
                <React.Fragment>
                    <Typography className={classes.instructions}>{101-x}: {getStepContent(activeStep)}</Typography>
                </React.Fragment>
            ))}
        </React.Fragment>
    );
    

    return (
        <div>
            <Button variant="outlined" color="primary" onClick={handleClickOpen}>
                Open form dialog
            </Button>
            <Dialog open={open} onClose={handleClose} aria-labelledby="form-dialog-title" fullWidth={true} maxWidth={"sm"}>
                <DialogTitle id="form-dialog-title">Benchmark a local or network disk</DialogTitle>
                <DialogContent>
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
                                    disabled={activeStep === 0}
                                    onClick={handleBack}
                                    style={classes.wizardButton}
                            >
                                « Back
                            </Button>
                            
                            <Button variant="contained" 
                                    disabled={selectedDisk === null}
                                    color="primary" 
                                    onClick={handleNext} 
                                    style={classes.wizardButton}>
                                {activeStep === steps.length - 1 ? 'Finish ' : 'Next »'}
                            </Button>

                            <Button variant="contained"
                                    disabled={activeStep === 0 && false}
                                    onClick={handleClose}
                                    style={classes.wizardButton}
                            >
                                Cancel
                            </Button>
                        </div>
                    )}
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default DiskBenchmarkDialog;

