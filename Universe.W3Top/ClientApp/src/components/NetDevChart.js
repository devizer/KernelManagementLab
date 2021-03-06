import * as DomClass from "dom-helpers/class";
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import DataSourceStore from "../stores/DataSourceStore";
import { findLimit } from './LimitFinder'
import * as Helper from "../Helper";
import {Help} from "@material-ui/icons";

import MomentFormat from 'moment';


let c3 = require("c3");

const __EmptyJsonChart = {
    txBytes: [],
    rxBytes: [],
    txPackets: [],
    rxPackets: [],
};

// This component should never be re-rendered.
// All the state updates are managed by DataSourceStore.on('storeUpdated') callback

export class NetDevChart extends Component {
    static displayName = NetDevChart.name;

    static ChartSize = {
        width: 500,
        height: 160,
    };
    
    static Padding = 70;

    domId = nextUniqueId("chart_net_dev");
    chart = null;
    jsonChart = {};
    isInitCompleted = false;
    pointLimit = 60;
    updateInterval = 1000;
    timerId = null;
    
    prevLimits = {
        limitBytes: -1,
        limitPackets: -1
    };
    
    constructor (props) {
        super(props);
        
        this.refChart = React.createRef();
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
        this._updateChart = this._updateChart.bind(this);
        this._initChart = this._initChart.bind(this);
        this.buildLocalJsonChart = this.buildLocalJsonChart.bind(this);
        let x = DataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
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

    buildLocalJsonChart() {
        this.globalDataSource = DataSourceStore.activeDataSource;
        let localDataSource = this.props.getLocalDataSource(this.globalDataSource);
        if (!Helper.Common.objectIsNotEmpty(localDataSource))
            localDataSource = __EmptyJsonChart;

        let xValues = this.globalDataSource.x;
        // console.log(`[[${this.props.name}]] localDataSource`); console.log(localDataSource);
        // console.log(`[[${this.props.name}]] xValues`); console.log(xValues);

        let rxBytes = localDataSource.rxBytes;
        let txBytes = localDataSource.txBytes;
        let rxPackets = localDataSource.rxPackets;
        let txPackets = localDataSource.txPackets;
        let jsonChart = {
            // x: xValues,
            // totals: localDataSource.totals,
            rxBytes,
            txBytes,
            rxPackets,
            txPackets,
            
        };

        Helper.toConsole(`[[${this.props.name}]] jsonChart updated`, jsonChart);
        return jsonChart;
    }

    componentDidMount() {

        this.jsonChart = this.buildLocalJsonChart();
        this._initChart();

        this.timerId = setInterval(_ => {
            // TODO: Update chart every second if web-socket is disconnected
            // .... 
            // this.forceUpdate();
        }, this.updateInterval);
        
        console.log(`NetDevChart::componentDidMount COMPLETED SUCCESSFULLY for ${this.props.name}`);
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

    _initChart()
    {
        var formatX = x => {
            if (x === 0) return "60";
            if (x === 60) return "";
            return Math.round(60-x);
        };

        let jsonChart = this.jsonChart;
        let {limitBytes, limitPackets} = this.computeAxisLimits(jsonChart);
        this.prevLimits = {limitBytes, limitPackets};

        let formatTransfer = num => (Math.round(num / 1024.0 * 100.0) / 100).toLocaleString(undefined, {useGrouping: true});
        let formatPackets = num => Math.round(num).toLocaleString(undefined, {useGrouping: true});
        

        this.chart = c3.generate({
            bindto: `#${this.domId}`,
            data: {
                // columns: columns,
                json : this.jsonChart,
                type: 'area-spline',
                axes : {
                    rxPackets : "y2",
                    txPackets : "y2",
                },
                names : {
                    rxPackets: "RX:Packets",
                    txPackets: "TX:Packets",
                    rxBytes: "RX:KBytes",
                    txBytes: "TX:KBytes",
                }

            }, 
            tooltip: {
                format: { title: Helper.Chart.tooltipTitleFormat(1) }
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
                    max: limitBytes,
                    padding: 0,
                    label: {
                        text: 'Transfer, KB/s',
                        position: 'outer-middle'
                    },
                    tick: {
                        count: 5,
                        format: formatTransfer,
                        // values: [1, 2, 4, 8, 16, 32]
                    }
                },
                y2 : {
                    max: limitPackets,
                    min: 0,
                    padding: 0,
                    show: true,
                    tick: {
                        count: 5,
                        format: formatPackets,
                        // values: [1, 2, 4, 8, 16, 32]
                    },
                    label: {
                        text: 'Packets / s',
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
                        {value: limitBytes/2, text: '50%'},
                        {value: limitBytes, text: '100%', class: 'label-200', position: 'middle'},
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
        let {limitBytes, limitPackets} = this.computeAxisLimits(jsonChart);
        
        let isSameLimits = this.prevLimits.limitBytes === limitBytes && this.prevLimits.limitPackets === limitPackets;
        if (!isSameLimits) {
            console.log(`NEW NET/DEV LIMITS for ${this.props.name} are Bytes: ${limitBytes}, Packets: ${limitPackets}`);
            this.prevLimits = {limitBytes, limitPackets};
            this.chart.axis.max({
                y: limitBytes,
                y2: limitPackets,
            });

            this.chart.ygrids([
                {value: limitBytes / 2, text: '50%'},
                {value: limitBytes, text: '100%', class: 'label-200', position: 'middle'},
            ]);
        }

        this.chart.load({
            json: this.jsonChart,
            transition: {
                duration: 0
            },
            done: () => {Helper.log(`NET-CHART::UPDATED ${messageId} @ ${this.props.name}`)}
        });
    }

    computeAxisLimits(jsonChart) {
        let maxBytes = Math.max(this.getMaxOfArray(jsonChart.rxBytes), this.getMaxOfArray(jsonChart.txBytes));
        maxBytes = Math.floor((maxBytes + 1023) / 1024);
        let maxPackets = Math.max(this.getMaxOfArray(jsonChart.rxPackets), this.getMaxOfArray(jsonChart.txPackets));
        let limitBytes = findLimit(maxBytes);
        let limitPackets = findLimit(maxPackets);
        limitBytes = limitBytes * 1024;
        return {limitBytes, limitPackets};
    }

    render () {
        return <div id={this.domId} ref={this.refChart}/>
    }
}

NetDevChart.propTypes = {
    getLocalDataSource: PropTypes.func.isRequired,
    name: PropTypes.string.isRequired,
    description: PropTypes.string.isRequired,
};
