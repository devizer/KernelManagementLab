import React, { Component } from 'react';
import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import DataSourceStore from "../stores/DataSourceStore";
import { findLimit } from './LimitFinder'
import * as Helper from "../Helper";
import {NetDevChart} from "./NetDevChart";
let c3 = require("c3");

const __EmptyBlockJsonChart = {
    readBytes: [],
    readOps: [],
    writeBytes: [],
    writeOps: [],
    busy: [],
    queue: [],
};


export class BlockStatChart extends React.Component {
    static displayName = BlockStatChart.name;

    static ChartSize = {
        width: 500,
        height: 160,
    };

    static Padding = 70;

    domId = nextUniqueId("chart_block_stat");
    chart = null;
    pointLimit = 60;
    updateInterval = 1000;
    timerId = null;

    prevLimits = {
        limitBytes: -1,
        limitPackets: -1
    };

    constructor (props) {
        super(props);

        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
        // this.buildLocalJsonChart = this.buildLocalJsonChart.bind(this);
        let x = DataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
    }

    updateGlobalDataSource() {
        // let jsonChart = this.buildLocalJsonChart();

        // this.jsonChart = jsonChart;
        // this._updateChart();

    }
    
    render()
    {
        let tempStyle = {
            border: "1px solid red", 
            width: BlockStatChart.ChartSize.width - 70*2, 
            height: BlockStatChart.ChartSize.height,
            paddingTop: BlockStatChart.ChartSize.height/3,
            textAlign: "center",
            marginLeft: "70px",
            verticalAlign: "middle",
            color: "grey",
        };
        return (
            <div style={tempStyle} >
                CHART placeholder for<br/>{this.props.name}
            </div>
        );
    }


}

BlockStatChart.propTypes = {
    name: PropTypes.string.isRequired,
};
