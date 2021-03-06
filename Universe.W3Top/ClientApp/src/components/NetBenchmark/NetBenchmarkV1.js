import React from 'react';
import PropTypes from 'prop-types';
import deburr from 'lodash/deburr';
import Autosuggest from 'react-autosuggest';
import match from 'autosuggest-highlight/match';
import parse from 'autosuggest-highlight/parse';
import TextField from '@material-ui/core/TextField';
import Paper from '@material-ui/core/Paper';
import MenuItem from '@material-ui/core/MenuItem';
import Popper from '@material-ui/core/Popper';
import { withStyles } from '@material-ui/core/styles';
// import suggestions_v1 from "./net-benchmark-suggestions";
import * as Helper from "../../Helper";

let suggestions = [];

function renderInputComponent(inputProps) {
    const { classes, inputRef = () => {}, ref, ...other } = inputProps;

    return (
        <TextField
            fullWidth
            InputProps={{
                inputRef: node => {
                    ref(node);
                    inputRef(node);
                },
                classes: {
                    input: classes.input,
                },
            }}
            {...other}
        />
    );
}

function renderSuggestion(suggestion, { query, isHighlighted }) {
    const matches = match(suggestion.label, query);
    const parts = parse(suggestion.label, matches);

    return (
        <MenuItem selected={isHighlighted} component="div">
            <div>
                {parts.map((part, index) =>
                        part.highlight ? (
                            <span key={String(index)} style={{ fontWeight: 500 }}>{part.text}</span>
                        ) : (
                            <span key={String(index)}>{part.text}</span>
                                
                            
                        ),
                )}
            </div>
        </MenuItem>
    );
}

function getSuggestions(value) {
    const inputValue = deburr(value.trim()).toLowerCase();
    const inputLength = inputValue.length;
    let count = 0;

    return inputLength === 0
        ? []
        : suggestions.filter(suggestion => {
            // const keep = count < 5 && suggestion.label.slice(0, inputLength).toLowerCase() === inputValue;
            const keep = count < 5 && 
                (
                    inputValue.length >= 2 
                        ? suggestion.label.toLowerCase().indexOf(inputValue) >= 0  
                        : suggestion.label.slice(0, inputLength).toLowerCase() === inputValue
                );
            
            const contains = suggestion.label.indexOf()
                

            if (keep) {
                count += 1;
            }

            return keep;
        });
}

function getSuggestionValue(suggestion) {
    return suggestion.label;
}

const styles = theme => ({
    root: {
        height: 250,
        flexGrow: 1,
    },
    container: {
        position: 'relative',
    },
    suggestionsContainerOpen: {
        position: 'absolute',
        zIndex: 1,
        marginTop: theme.spacing.unit,
        left: 0,
        right: 0,
    },
    suggestion: {
        display: 'block',
    },
    suggestionsList: {
        margin: 0,
        padding: 0,
        listStyleType: 'none',
    },
    divider: {
        height: theme.spacing.unit * 2,
    },
});

class IntegrationAutosuggest extends React.Component {
    state = {
        single: '',
        popper: '',
        suggestions: [],
    };
    
    componentDidMount() {
        let apiUrl = 'assets/proof-of-concept-net-benchmark-suggestions.json';
        try {
            fetch(apiUrl)
                .then(response => {
                    Helper.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    Helper.log(response);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(suggestionsRecieved => {
                    suggestions = suggestionsRecieved;
                    Helper.toConsole("suggestionsDataSource", suggestions);
                    this.setState({suggestionsDataSource: suggestions});
                    // Helper.toConsole("DISKS for benchmark", disks);
                })
                .catch(error => Helper.log(error));
        }
        catch(err)
        {
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }

    }

    handleSuggestionsFetchRequested = ({ value }) => {
        this.setState({
            suggestions: getSuggestions(value),
        });
    };

    handleSuggestionsClearRequested = () => {
        this.setState({
            suggestions: [],
        });
    };

    handleChange = name => (event, { newValue }) => {
        this.setState({
            [name]: newValue,
        });
    };

    render() {
        const { classes } = this.props;

        const autosuggestProps = {
            renderInputComponent,
            suggestions: this.state.suggestions,
            onSuggestionsFetchRequested: this.handleSuggestionsFetchRequested,
            onSuggestionsClearRequested: this.handleSuggestionsClearRequested,
            getSuggestionValue,
            renderSuggestion,
        };

        return (
            <div className={classes.root}>
                <Autosuggest
                    {...autosuggestProps}
                    inputProps={{
                        classes,
                        placeholder: 'Search a country (start with a)',
                        value: this.state.single,
                        onChange: this.handleChange('single'),
                    }}
                    theme={{
                        container: classes.container,
                        suggestionsContainerOpen: classes.suggestionsContainerOpen,
                        suggestionsList: classes.suggestionsList,
                        suggestion: classes.suggestion,
                    }}
                    renderSuggestionsContainer={options => (
                        <Paper {...options.containerProps} square>
                            {options.children}
                        </Paper>
                    )}
                />
                
                <div className={classes.divider} />
                
                <Autosuggest
                    {...autosuggestProps}
                    inputProps={{
                        classes,
                        label: 'Label',
                        placeholder: 'With Popper',
                        value: this.state.popper,
                        onChange: this.handleChange('popper'),
                        inputRef: node => {
                            this.popperNode = node;
                        },
                        InputLabelProps: {
                            shrink: true,
                        },
                    }}
                    theme={{
                        suggestionsList: classes.suggestionsList,
                        suggestion: classes.suggestion,
                    }}
                    renderSuggestionsContainer={options => (
                        <Popper anchorEl={this.popperNode} open={Boolean(options.children)}>
                            <Paper
                                square
                                {...options.containerProps}
                                style={{ width: this.popperNode ? this.popperNode.clientWidth : null }}
                            >
                                {options.children}
                            </Paper>
                        </Popper>
                    )}
                />
            </div>
        );
    }
}

IntegrationAutosuggest.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(IntegrationAutosuggest);
