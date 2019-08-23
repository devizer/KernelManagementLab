import React, { Component } from 'react';
import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import dataSourceStore from "../stores/DataSourceStore";
import { findLimit } from './LimitFinder'
import * as Helper from "../Helper";
import {NetDevChart} from "./NetDevChart";

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowAltCircleUp } from '@fortawesome/free-regular-svg-icons'
import { faArrowAltCircleDown } from '@fortawesome/free-regular-svg-icons'
import { faArrowUp } from '@fortawesome/free-solid-svg-icons'
import { faArrowDown } from '@fortawesome/free-solid-svg-icons'

const iconSent = faArrowUp, iconReceived = faArrowDown;

export class NetDevChartHeader extends Component {
    static displayName = NetDevChartHeader.name;
    
    constructor(props) {
        super(props);
        
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
    }
    
    componentDidMount() {
        let x = dataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
    }
    
    componentWillUnmount() {
        dataSourceStore.removeListener('storeUpdated', this.updateGlobalDataSource);
    }

    updateGlobalDataSource()
    {
        this.forceUpdate();
    }

    dd = {
        container: {
            position: "relative",
            width: NetDevChart.ChartSize.width,
            // width: "100%",
            marginBottom: 4,
            marginTop: "15px",
        },
        center: {
            paddingTop: 3,
            fontSize: 16,
            position: "absolute",
            left: 0, top: 0,
            textAlign: "center",
            width: "100%",
        },
        left: {
            paddingTop: 8,
            fontSize: 12,
            verticalAlign: "bottom",
            position: "absolute",
            left: 0, top: 0,
            textAlign: "left",
            width: "100%",
            paddingLeft: NetDevChart.Padding - 4,
            paddingRight: NetDevChart.Padding,
        },
        right: {
            paddingTop: 8,
            fontSize: 12,
            verticalAlign: "bottom",
            position: "absolute",
            left: 0, top: 0,
            textAlign: "right",
            width: "100%",
            paddingLeft: NetDevChart.Padding,
            paddingRight: NetDevChart.Padding - 4,
        },
    };
    
    render() {

        let totals;
        try {
            let glo = dataSourceStore.getDataSource();
            totals = glo.net.interfaceTotals[this.props.name];
        }
        catch{
            totals = {rxBytes: 0, txBytes: 0};
        }
        
        if (!Helper.Common.objectIsNotEmpty(totals))
            totals = {rxBytes: 0, txBytes: 0};
            

        let format = x => x > 0 ? Helper.Common.formatBytes(x) : "";
        // totals = {rxBytes: 0, txBytes: 0};


        return (
            <div style={this.dd.container}>&nbsp;
                <div style={this.dd.left}><span title={"TOTAL RECEIVED"}><FontAwesomeIcon icon={iconReceived} /> {format(totals.rxBytes)}</span></div>
                <div style={this.dd.center}>interface <b>{this.props.name.toUpperCase()}</b></div>
                <div style={this.dd.right}><span title={"TOTAL SENT"}>{format(totals.txBytes)} <FontAwesomeIcon icon={iconSent} /></span></div>
            </div>
        )
    };

}

NetDevChartHeader.propTypes = {
    name: PropTypes.string.isRequired,
};
