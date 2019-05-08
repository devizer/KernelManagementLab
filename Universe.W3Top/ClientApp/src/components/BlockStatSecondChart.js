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

    domId = nextUniqueId("chart_block_V2");
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

    _initChart()
    {
        var formatX = x => {
            if (x === 0) return "60";
            if (x === 60) return "";
            return Math.round(60-x);
        };

        let jsonChart = this.jsonChart;
        let {limitQueue, limitBusy} = this.computeAxisLimits(jsonChart);
        this.prevLimits = {limitQueue, limitBusy};

        let formatQueue = num => Math.round(num).toLocaleString(undefined, {useGrouping: true});
        let formatBusy = num => (Math.round(num) / 10).toLocaleString(undefined, {useGrouping: true});

        this.chart = c3.generate({
            bindto: `#${this.domId}`,
            data: {
                // columns: columns,
                json : this.jsonChart,
                type: 'area-spline',
                axes : {
                    queue : "y",
                    busy : "y2",
                },
                names : {
                    queue: "Queue",
                    busy: "Busy",
                }

            },
            transition: {
                duration: null
            },
            size: NetDevChart.ChartSize,
            axis: {
                x: {
                    padding: 0,
                    label: {
                        text: '60 seconds',
                        position: 'inner-left'
                    },
                    tick: {
                        count: 7,
                        format: formatX,
                        // values: [1, 2, 4, 8, 16, 32]
                    }
                },
                y: {
                    min: 0,
                    max: limitQueue,
                    padding: 0,
                    label: {
                        text: 'Queue',
                        position: 'outer-middle'
                    },
                    tick: {
                        count: 5,
                        format: formatQueue,
                        // values: [1, 2, 4, 8, 16, 32]
                    }
                },
                y2 : {
                    max: limitBusy,
                    min: 0,
                    padding: 0,
                    show: true,
                    tick: {
                        count: 5,
                        format: formatBusy,
                        // values: [1, 2, 4, 8, 16, 32]
                    },
                    label: {
                        text: 'Busy, %%',
                        position: 'outer-middle'
                    },

                }

            },
            point: {
                r: 1.5,
                select: {r:8},
                focus: {
                    expand: {
                        r: 4
                    }
                }
            },

            grid: {
                y: {
                    lines: [
                        {value: limitQueue/2, text: '50%'},
                        {value: limitQueue, text: '100%', class: 'label-200', position: 'middle'},
                    ]
                },
                x : {
                    lines: [
                        {value: 60, text: ''},
                    ]
                }
            },
            // fix tick&labels withs for Y and Y2 axises
            padding: {
                left: NetDevChart.Padding,
                right: NetDevChart.Padding,
            },

        });

    }

    updateGlobalDataSource() {
        let inactiveChartClassName = "inactive-chart";
        if (Helper.isDocumentHidden()) {
            if (this.refChart.current) DomClass.addClass(this.refChart.current, inactiveChartClassName);
            Helper.log(`chart ${this.domId} is on hidden page. chart update postponed ${new Date()}`)
            return;
        }
        else{
            // Helper.toConsole("this.refChart", this.refChart);
            if (this.refChart.current) DomClass.removeClass(this.refChart.current, inactiveChartClassName);
        }

        let jsonChart = this.buildLocalJsonChart();
        this.jsonChart = jsonChart;
        this._updateChart();
    }



    render () {
        return <div id={this.domId} ref={this.refChart} />
    }

}


