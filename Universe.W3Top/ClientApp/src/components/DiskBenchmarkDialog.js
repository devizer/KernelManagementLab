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

const styles = {
    root: {
        width: '90%',
    },
    wizardButton: {
        marginRight: 24,
        marginBottom: 8,
    },
    wizardReset: {
        marginRight: 24,
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
    }
};

function getSteps() {
    return ['Select', 'Tune', 'Perform', "Done"];
}

function getStepContent(stepIndex) {
    switch (stepIndex) {
        case 0:
            return 'Select local/network disk...';
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

function DiskBenchmarkDialog() {
    const [open, setOpen] = React.useState(true);
    const [activeStep, setActiveStep] = React.useState(0);

    function handleClickOpen() {
        setOpen(true);
    }

    function handleClose() {
        setOpen(false);
    }

    let handleNext = () => setActiveStep(activeStep + 1);
    let handleBack = () => setActiveStep(activeStep - 1);
    let handleReset = () => setActiveStep(0);

    const steps = getSteps();
    const classes = styles;

    return (
        <div>
            <Button variant="outlined" color="primary" onClick={handleClickOpen}>
                Open form dialog
            </Button>
            <Dialog open={open} onClose={handleClose} aria-labelledby="form-dialog-title" fullWidth={true} maxWidth={"md"}>
                <DialogTitle id="form-dialog-title">Benchmark a local or network disk</DialogTitle>
                <DialogContent>
                    <Stepper activeStep={activeStep} alternativeLabel >
                        {steps.map(label => (
                            <Step key={label}>
                                <StepLabel>{label}</StepLabel>
                            </Step>
                        ))}
                    </Stepper>
                    <Typography className={classes.instructions}>{getStepContent(activeStep)}</Typography>
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

