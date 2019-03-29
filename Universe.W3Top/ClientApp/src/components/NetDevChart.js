import React, { Component } from 'react';
import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import DataSourceStore from "../stores/DataSourceStore";
import { findLimit } from './LimitFinder'
let c3 = require("c3");

const __EmptyJsonChart = {
    txBytes: [],
    rxBytes: [],
    txPackets: [],
    rxPackets: [],
};

export class NetDevChart extends Component {
    static displayName = NetDevChart.name;
    
    domId = nextUniqueId("chart_net_dev");
    chart = null;
    jsonChart = {};
    isInitCompleted = false;
    pointLimit = 60;
    updateInterval = 1000;
    timerId = null;
    
    constructor (props) {
        super(props);
        
        // this.buildLimits();
        // this.findLimit = this.findLimit.bind(this);
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
        this._updateChart = this._updateChart.bind(this);
        this._initChart = this._initChart.bind(this);
        let x = DataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
    }

    updateGlobalDataSource() {
        this.globalDataSource = DataSourceStore.activeDataSource;
        let localDataSource = this.props.getLocalDataSource(this.globalDataSource);
        if (Object.keys(localDataSource).length === 0)
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
            rxBytes,
            txBytes,
            rxPackets,
            txPackets,
        };

        console.log(`[[${this.props.name}]] jsonChart updated`); console.log(jsonChart);
        
        this.jsonChart = jsonChart;
        this._updateChart();
        
    }
    
    componentDidMount() {
        
        this._initChart();

        this.timerId = setInterval(_ => {
            // TODO: Update chart if web-secket is disconnected
            // .... 
            // this.forceUpdate();
        }, this.updateInterval);
        
        console.log("NetDevChart::componentDidMount COMPLETED SUCCESSFULLY");
    }

    componentWillUnmount() {
        if (this.timerId)
        {
            clearInterval(this.timerId);
            this.timerId = 0;
        }
        
        let destroy = () => {
            let c = this.chart;
            this.chart = null;
            c.destroy();
            console.log(`chart #${this.domId} destroyed`);
        };
        
        if (window.requestIdleCallback)
            window.requestIdleCallback(destroy.bind(this));
        else 
            destroy.bind(this);
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
                    rxBytes: "RX:Bytes",
                    txBytes: "TX:Bytes",
                }

            },
            transition: {
                duration: 1
            },
            size: {
                width: 500,
                height: 190,
            },
            axis: {
                x: {
                    padding: 0,
                    label: {
                        text: '60 seconds',
                        position: 'inner-left'
                    },
                    tick: {
                        count: 5,
                        format: formatX,
                        // values: [1, 2, 4, 8, 16, 32]
                    }
                },
                y: {
                    min: 0,
                    max: limitBytes,
                    padding: 0,
                    label: {
                        text: 'Transfer, Kb / s',
                        position: 'outer-left'
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
                        position: 'outer-right'
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
                        {value: 4000/2, text: '50%'},
                        {value: 4000, text: '100%', class: 'label-200', position: 'middle'},
                    ]
                },
                x : {
                    lines: [
                        {value: 60, text: ''},
                    ]

                }
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

        // limitBytes & limitPackets
        let {limitBytes, limitPackets} = this.computeAxisLimits(jsonChart);

        console.log(`NEW NET/DEV LIMITS are Bytes: ${limitBytes}, Packets: ${limitPackets}`);
        this.chart.axis.max({
            y: limitBytes,
            y2: limitPackets,
        });

        this.chart.ygrids([
            {value: limitBytes/2, text: '50%'},
            {value: limitBytes, text: '100%', class: 'label-200', position: 'middle'},
        ]);

        this.chart.load({
            json: this.jsonChart,
            transition: {
                duration: 1
            },
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
        return <div id={this.domId} />
        
    }
}

NetDevChart.propTypes = {
    getLocalDataSource: PropTypes.func.isRequired,
    name: PropTypes.string.isRequired,
    description: PropTypes.string.isRequired,
};
