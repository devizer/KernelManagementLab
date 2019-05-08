import * as DomClass from "dom-helpers/class";
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import DataSourceStore from "../stores/DataSourceStore";
import { findLimit } from './LimitFinder'
import * as Helper from "../Helper";
import {NetDevChart} from "./NetDevChart";
let c3 = require("c3");

const __EmptyJsonChart = {
    queue: [],
    busy: [],
};

export class BlockStatSecondChart extends React.Component {
    static displayName = BlockStatSecondChart.name;

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
        limitQueue: -1,
        limitBusy: -1
    };

    constructor (props) {
        super(props);

        this.refChart = React.createRef();
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
        this._updateChart = this._updateChart.bind(this);
        this._initChart = this._initChart.bind(this);
        // this.buildLocalJsonChart = this.buildLocalJsonChart.bind(this);
        let x = DataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
    }

    componentDidMount() {

        this.jsonChart = this.buildLocalJsonChart();
        this._initChart();

        this.timerId = setInterval(_ => {
            // TODO: Update chart every second if web-socket is disconnected
            // .... 
            // this.forceUpdate();
        }, this.updateInterval);

        console.log(`BlockStatSecondChart::componentDidMount COMPLETED SUCCESSFULLY for ${this.props.name}`);
    }

    componentWillUnmount() {
        DataSourceStore.removeListener('storeUpdated', this.updateGlobalDataSource);

        if (this.timerId)
        {
            clearInterval(this.timerId);
            this.timerId = 0;
        }

        let destroy = () => {
            if (this.chart !== null) {
                let c = this.chart;
                this.chart = null;
                c.destroy();
                console.log(`chart #${this.domId} destroyed`);
            }
        };

        Helper.runInBackground(destroy);
    }

    buildLocalJsonChart() {
        this.globalDataSource = DataSourceStore.activeDataSource;
        let [hasBlock, block] = Helper.Common.tryGetProperty(this.globalDataSource, "block");
        let localDataSource = __EmptyJsonChart;
        if (hasBlock) localDataSource = block.blocks[this.props.name];

        let queue = localDataSource.inFlightRequests;
        let busy = localDataSource.ioMilliseconds;
        let jsonChart = {
            queue,
            busy,
        };

        Helper.toConsole(`BLOCK [[${this.props.name}]] jsonChart updated`, jsonChart);
        return jsonChart;
    }

    // PURE
    getMaxOfArray(numArray) {
        return Math.max.apply(null, numArray);
    }

    _updateChart() {
        if (this.chart === null) return;

        let jsonChart = this.jsonChart;

        let [hasMessageId, messageId] = Helper.Common.tryGetProperty(DataSourceStore.getDataSource(), "messageId");
        messageId = messageId || "<unknown-message>";

        // limitBytes & limitPackets
        let {limitQueue, limitBusy} = this.computeAxisLimits(jsonChart);

        let isSameLimits = this.prevLimits.limitBusy === limitBusy && this.prevLimits.limitQueue === limitQueue;
        if (!isSameLimits) {
            console.log(`NEW BLOCK-2nd LIMITS for ${this.props.name} are Queue: ${limitQueue}, Busy: ${limitBusy}`);
            this.prevLimits = {limitQueue, limitBusy};
            this.chart.axis.max({
                y: limitQueue,
                y2: limitBusy,
            });

            this.chart.ygrids([
                {value: limitQueue / 2, text: '50%'},
                {value: limitQueue, text: '100%', class: 'label-200', position: 'middle'},
            ]);
        }

        this.chart.load({
            json: this.jsonChart,
            transition: {
                duration: 0
            },
            done: () => {Helper.log(`BLOCK-2nd-CHART::UPDATED ${messageId} @ ${this.props.name}`)}
        });
    }

    computeAxisLimits(jsonChart) {
        let maxQueue = Math.max(this.getMaxOfArray(jsonChart.queue));
        let limitQueue = findLimit(maxQueue);
        return {limitQueue, limitBusy: 1000};
    }






}


