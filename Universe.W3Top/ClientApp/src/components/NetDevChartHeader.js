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


export class NetDevChartHeader extends Component {
    
    constructor(props) {
        super(props);
        
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
    }
    
    componentDidMount() {
        let x = dataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
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
            marginBottom: "9px",
            marginTop: "15px",
        },
        center: {
            position: "absolute",
            left: 0, top: 0,
            textAlign: "center",
            width: "100%",
        },
        left: {
            position: "absolute",
            left: 0, top: 0,
            textAlign: "left",
            width: "100%",
            paddingLeft: NetDevChart.Padding - 26,
            paddingRight: NetDevChart.Padding,
        },
        right: {
            position: "absolute",
            left: 0, top: 0,
            textAlign: "right",
            width: "100%",
            paddingLeft: NetDevChart.Padding,
            paddingRight: NetDevChart.Padding - 26,
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
                <div style={this.dd.left}><FontAwesomeIcon icon={faArrowAltCircleDown} /> {format(totals.rxBytes)}</div>
                <div style={this.dd.center}>interface <b>{this.props.name}</b></div>
                <div style={this.dd.right}>{format(totals.txBytes)} <FontAwesomeIcon icon={faArrowAltCircleUp} /></div>
            </div>
        )
    };

}

NetDevChartHeader.propTypes = {
    name: PropTypes.string.isRequired,
};
