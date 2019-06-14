import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import Popper from '@material-ui/core/Popper';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import Paper from '@material-ui/core/Paper';

function PopperLab(props) {
    const [errorAnchor, setAnchorError] = React.useState(null);

    function handleClick(event) {
        // setAnchorEl(anchorEl ? null : event.currentTarget);
    }

    const errorElement = React.useRef();
    React.useEffect(() => {
        setTimeout(() => {
            setAnchorError(errorElement.current);
            console.log("POPPER OPENED");
        }, 2000);
    });

    const errorOpened = Boolean(errorAnchor);
    const errorId = errorOpened ? "error-popper" : null;

    return (
        <div>
            <img src="https://via.placeholder.com/350x150" />
            <button
                aria-describedby={errorId}
                type="button"
                onClick={handleClick}
                ref={errorElement}
            >
                Toggle Popper
            </button>

            <br/>
            <Paper>
                <Typography style={{padding:16}}>a paper.</Typography>
            </Paper>
            <br/>
            <Paper>
                <Typography style={{padding:16}}>a paper.</Typography>
            </Paper>


            <Popper id={errorId} open={errorOpened} anchorEl={errorAnchor} placement="top">
                <Paper >
                    <Typography style={{padding:16,color: "darkred"}}>The content of the Popper.</Typography>
                </Paper>
            </Popper>
        </div>
    );
}

export default PopperLab;

