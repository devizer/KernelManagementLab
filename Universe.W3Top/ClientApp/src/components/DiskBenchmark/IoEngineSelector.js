import React from 'react';
import ReactDOM from 'react-dom';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import Input from '@material-ui/core/Input';
import OutlinedInput from '@material-ui/core/OutlinedInput';
import FilledInput from '@material-ui/core/FilledInput';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormHelperText from '@material-ui/core/FormHelperText';
import FormControl from '@material-ui/core/FormControl';
import Select from '@material-ui/core/Select';

const styles = theme => ({
    root: {
        display: 'flex',
        flexWrap: 'wrap',
    },
    formControl: {
        margin: theme.spacing.unit,
        minWidth: 120,
    },
    selectEmpty: {
        marginTop: theme.spacing.unit * 2,
    },
});

export class IoEngineSelector extends React.Component {
    static displayName = IoEngineSelector.name;

    constructor(props) {
        super(props);

        this.state = {selected: null}
        this.handleChange = this.handleChange.bind(this);
    }

    handleChange = event => {
        this.setState({ selected: event.target.value });
    };
    
    render() {
        const classes  = styles;

        return (
        <FormControl className={classes.formControl}>
            <InputLabel htmlFor="age-helper">Age</InputLabel>
            <Select
                value={this.state.age}
                onChange={this.handleChange}
                input={<Input name="age" id="age-helper" />}
            >
                <MenuItem value="">
                    <em>None</em>
                </MenuItem>
                <MenuItem value={10}>Ten</MenuItem>
                <MenuItem value={20}>Twenty</MenuItem>
                <MenuItem value={30}>Thirty</MenuItem>
            </Select>
            <FormHelperText>Some important helper text</FormHelperText>
        </FormControl>
        );
    }
}

