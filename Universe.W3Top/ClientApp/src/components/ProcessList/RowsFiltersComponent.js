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
            topValue,
            customTop,
            NeedNoFilter: rowsFilters.NeedNoFilter === true,
            NeedKernelThreads: rowsFilters.NeedKernelThreads === true,
            NeedServices: rowsFilters.NeedServices === true,
            NeedContainers: rowsFilters.NeedContainers === true,
        };
    }
    
    render() {
        // validate only custom top 
        const getCustomTopError = () => {
            let customTopError = null;
            if (`${this.state.topValue}` === "-1")
            {
                const isPositiveNumber = Helper.Numbers.isInt(this.state.customTop) && Helper.Numbers.greaterOrEqual(this.state.customTop,1);
                if (!isPositiveNumber) customTopError = "Should be a positive number";
            }
            return customTopError;
        };

        // should be called on each onChange
        const tryApplyRowsFilters = () => {
            if (getCustomTopError() == null)
            {
                const filters = new ProcessRowsFilters();
                filters.TopFilter = parseInt(this.state.topValue);
                if (filters.TopFilter === -1) filters.TopFilter = parseInt(this.state.customTop);
                filters.NeedContainers = this.state.NeedContainers;
                filters.NeedKernelThreads = this.state.NeedKernelThreads;
                filters.NeedNoFilter = this.state.NeedNoFilter;
                filters.NeedServices = this.state.NeedServices;
                ProcessListActions.RowsFiltersUpdated(filters);
            }
        };

        const onChangeTop = (event) => {
            const newTopValue = event.target.value;
            this.setState({topValue: newTopValue});
            Helper.log(`NEW TOP for RowsFilters: ${newTopValue}`);
            tryApplyRowsFilters();
        };
        
        let customTopError = getCustomTopError();

        const onChangeCustomTop = (event) => {
            this.setState({customTop: event.target.value});
            tryApplyRowsFilters();
        }
        
        const onCustomTopFocus = (event) => {
            this.setState({topValue: -1});
        }
        
        const isTopChecked = (val) => {
            return `${this.state.topValue}` === `${val}`;
        };
        
        const onChangedKind = (property) => (event) => {
            const checked = event.target.checked;
            const st = {...this.state};
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
            this.setState(st);
            tryApplyRowsFilters();
        };
        
        return (
            <div className="row-2">
                <div className="column-2">
                    <div className="column-header">BY COUNT</div>
                    <Radio color="primary" value={0} onClick={onChangeTop} checked={isTopChecked(0)}/> ALL
                    <br/>
                    <Radio color="primary" value={30} onClick={onChangeTop}  checked={isTopChecked(30)}/> Top 30
                    <br/>
                    <Radio color="primary" value={100} onChange={onChangeTop} checked={isTopChecked(100)} /> Top 100
                    <br/>
                    <Radio color="primary" value={-1} onChange={onChangeTop} checked={isTopChecked(-1)} style={{paddingTop:15}}/>
                    <TextField variant="outlined" label="Custom" style={{width: 120}}
                               value={this.state.customTop}
                               onChange={onChangeCustomTop}
                               error={customTopError}
                               helperText={customTopError}
                               onFocus={onCustomTopFocus}
                    />
                </div>
                <div className="column-2">
                    <div className="column-header">BY KIND</div>
                    <Checkbox color="primary" checked={this.state.NeedNoFilter} onChange={onChangedKind("NeedNoFilter")}/> Any Kind
                    <br/>
                    <Checkbox color="primary" checked={this.state.NeedServices} onChange={onChangedKind("NeedServices")} /> Services
                    <br/>
                    <Checkbox color="primary" checked={this.state.NeedContainers} onChange={onChangedKind("NeedContainers")} /> Containers
                    <br/>
                    <Checkbox color="primary" checked={this.state.NeedKernelThreads} onChange={onChangedKind("NeedKernelThreads")} /> Kernel Threads
                </div>
            </div>

        );
    }
}
