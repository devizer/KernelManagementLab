import React, {Component} from 'react';
import { withStyles } from '@material-ui/core/styles';
import DialogContent from "@material-ui/core/DialogContent";

import processListStore from "./Store/ProcessListStore";
import * as ProcessListActions from "./Store/ProcessListActions"
import Switch from '@material-ui/core/Switch';
import {Checkbox, FormControlLabel, FormControl, TextField} from '@material-ui/core';
import ProcessColumnsDefinition from "./ProcessColumnsDefinition";

import * as Helper from "../../Helper";
import FormLabel from "@material-ui/core/FormLabel";
import RadioGroup from "@material-ui/core/RadioGroup";
import Radio from "@material-ui/core/Radio";
import {ProcessRowsFilters} from "./ProcessRowsFilters";

class NoWrap extends React.Component {
    render() {
        return (<span style={{whiteSpace: "nowrap", display: "inline-block"}}>{this.props.children}</span>);
    }
}

export class RowsFiltersComponent extends Component {
    static displayName = RowsFiltersComponent.name;

    constructor(props) {
        super(props);

        let rowsFilters = processListStore.getRowsFilters();
        let topValue = rowsFilters.TopFilter, customTop = "";
        if (topValue != 0 && topValue != 30 && topValue != 100)
        {
            customTop = topValue;
            topValue = -1;
        }
        this.state = {
            rowsFilters,
            topValue,  // only for custom (-1). rest options is the copy of rowsFilters.TopFilter 
            customTop, // only for custom
        };
    }
    
    render() {
        // validate only custom top 
        const getCustomTopError = (topValue, rawCustomTop) => {
            let customTopError = null;
            if (topValue === undefined) topValue = this.state.topValue; 
            if (`${topValue}` === "-1")
            {
                if (rawCustomTop === undefined) rawCustomTop = this.state.customTop;
                const isPositiveNumber = Helper.Numbers.isInt(rawCustomTop) && Helper.Numbers.greaterOrEqual(rawCustomTop,1);
                if (!isPositiveNumber) customTopError = "Should be a positive number";
            }
            return customTopError;
        };
        
        const asyncRaiseUpdate = (rowsFilters) => {
            Helper.runInBackground(() => {
                ProcessListActions.RowsFiltersUpdated(rowsFilters);
            });
        } 

        // should be called on each onChange
        const tryApplyRowsFilters = () => {
            return;
            if (getCustomTopError() == null)
            {
                const filters = new ProcessRowsFilters();
                filters.TopFilter = parseInt(this.state.topValue);
                if (filters.TopFilter === -1) filters.TopFilter = parseInt(this.state.customTop);
                filters.NeedContainers = this.state.NeedContainers;
                filters.NeedKernelThreads = this.state.NeedKernelThreads;
                filters.NeedNoFilter = this.state.NeedNoFilter;
                filters.NeedServices = this.state.NeedServices;
                asyncRaiseUpdate(filters);
            }
        };

        const onChangeTop = (event) => {
            const newTopValue = event.target.value;
            const st = this.state.rowsFilters;
            if (`${newTopValue}` != "-1") // fuzzy comparision
            {
                st.TopFilter = newTopValue;
                this.setState({rowsFilters: st, topValue: newTopValue});
                asyncRaiseUpdate(st);
            }
            else {
                if (getCustomTopError(newTopValue, this.state.customTop) === null) {
                    st.TopFilter = parseInt(this.state.customTop);
                    this.setState({rowsFilters: st, topValue: newTopValue});
                    asyncRaiseUpdate(st);
                }
                else {
                    this.setState({topValue: newTopValue});
                }
            }
            Helper.log(`NEW TOP for RowsFilters: ${newTopValue}`);
        };
        
        let customTopError = getCustomTopError();

        const onChangeCustomTop = (event) => {
            const newCustomTop = event.target.value;
            if (getCustomTopError(-1, newCustomTop) === null)
            {
                const newTopValue = parseInt(newCustomTop);
                const st = this.state.rowsFilters;
                
                st.TopFilter = newTopValue;
                this.setState({rowsFilters: st, topValue: -1, customTop: newCustomTop});
                asyncRaiseUpdate(st);
            }
            else {
                this.setState({customTop: newCustomTop});
            }
        }
        
        const onCustomTopFocus = (event) => {
            this.setState({topValue: -1});
        }
        
        const isTopChecked = (val) => {
            // does not work properly for custom
            return `${this.state.topValue}` === `${val}`;
        };
        
        const onChangedKind = (property) => (event) => {
            const checked = event.target.checked;
            const st = this.state.rowsFilters;
            st[property] = checked;
            if (property === "NeedNoFilter")
            {
                if (checked) {
                    st.NeedKernelThreads = false;
                    st.NeedServices = false;
                    st.NeedContainers = false;
                }
                else {
                    st.NeedKernelThreads = true;
                    st.NeedServices = true;
                    st.NeedContainers = true;
                }
            }
            else {
                if (checked) 
                    st.NeedNoFilter = false;
                else {
                    if (!st.NeedKernelThreads && !st.NeedServices && !st.NeedContainers)
                        st.NeedNoFilter = true;
                }
            }
            this.setState({rowsFilters: st});
            asyncRaiseUpdate(st);
        };
        
        return (
            <div className="row-2" style={{minWidth: 333}}>
                <div className="column-2">
                    <div className="column-header"><NoWrap>BY COUNT</NoWrap></div>
                    <NoWrap><Radio color="primary" value={0} onClick={onChangeTop} checked={isTopChecked(0)}/> ALL</NoWrap>
                    <br/>
                    <NoWrap><Radio color="primary" value={30} onClick={onChangeTop}  checked={isTopChecked(30)}/> Top 30</NoWrap>
                    <br/>
                    <NoWrap><Radio color="primary" value={100} onChange={onChangeTop} checked={isTopChecked(100)} /> Top 100</NoWrap>
                    <br/>
                    <NoWrap><Radio color="primary" value={-1} onChange={onChangeTop} checked={isTopChecked(-1)} style={{paddingTop:15}}/>
                    <TextField variant="outlined" label="Custom N" style={{width: 120}}
                               value={this.state.customTop}
                               onChange={onChangeCustomTop}
                               error={customTopError}
                               helperText={customTopError}
                               onFocus={onCustomTopFocus}
                    /></NoWrap>
                </div>
                <div className="column-2">
                    <div className="column-header"><NoWrap>BY KIND</NoWrap></div>
                    <NoWrap><Checkbox color="primary" checked={this.state.rowsFilters.NeedNoFilter} onChange={onChangedKind("NeedNoFilter")}/> Any Kind</NoWrap>
                    <br/>
                    <NoWrap><Checkbox color="primary" checked={this.state.rowsFilters.NeedServices} onChange={onChangedKind("NeedServices")} /> Services</NoWrap>
                    <br/>
                    <NoWrap><Checkbox color="primary" checked={this.state.rowsFilters.NeedContainers} onChange={onChangedKind("NeedContainers")} /> Containers</NoWrap>
                    <br/>
                    <NoWrap><Checkbox color="primary" checked={this.state.rowsFilters.NeedKernelThreads} onChange={onChangedKind("NeedKernelThreads")} /> Kernel Threads</NoWrap>
                </div>
            </div>

        );
    }
}
