import React, { Component } from 'react';
// import PropTypes from 'prop-types';
import nextUniqueId from "../NextUniqueId"
import DataSourceStore from "../stores/DataSourceStore";
// import c3 from 'c3';
let c3 = require("c3");

const EnptyLocalDataSource = {
    x: [],
    interfaces: [],
    interfaceNames: [],
};

export class NetDevChart extends Component {
    static displayName = NetDevChart.name;
    
    // static domIdCounter = 0;
    domId = nextUniqueId("chart_net_dev");
    chart = null;
    jsonChart = {};
    // jsonData = {}; - replaced by this.state.localDataSource 
    pointLimit = 60;
    updateInterval = 1000;
    timerId = null;

    constructor (props) {
        super(props);
        
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
        let x = DataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
        
    }

    updateGlobalDataSource() {
        this.globalDataSource = DataSourceStore.activeDataSource;
        let localDataSource = this.props.getLocalDataSource(this.globalDataSource);
        let xValues = this.globalDataSource.x;
        // console.log(`[[${this.props.name}]] localDataSource`); console.log(localDataSource);
        // console.log(`[[${this.props.name}]] xValues`); console.log(xValues);
        
        let rxBytes = localDataSource.rxBytes;
        let txBytes = localDataSource.txBytes;
        let rxPackets = localDataSource.rxPackets;
        let txPackets = localDataSource.txPackets;
        let jsonChart = {
            x: xValues,
            rxBytes,
            txBytes,
            rxPackets,
            txPackets,
        };
        
        console.log(`[[${this.props.name}]] jsonChart updated`); console.log(jsonChart);
        
        this._updateChart();
        this.jsonChart = jsonChart;
        
        // this.setState({
        //     jsonChart: jsonChart, 
        // });
    }
    
    componentDidMount() {
        
        this._initChart();

        this.timerId = setInterval(_ => {
            // TODO: Update chart if web-secket is disconnected
            // .... 
            // this.forceUpdate();
        }, this.updateInterval);


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
        
        this.chart = c3.generate({
            bindto: `#${this.domId}`,
            data: {
                // columns: columns,
                json : this.jsonChart,
                type: 'area-spline'
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
                    // min: 0,
                    max: 100,
                    padding: 0,
                    label: {
                        text: 'Busy',
                        position: 'outer-left'
                    },
                    tick: {
                        count: 5,
                        format: val => Math.round(val),
                        // values: [1, 2, 4, 8, 16, 32]
                    }
                },

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
                        {value: 50, text: '50%'},
                        {value: 100, text: '100%', class: 'label-200', position: 'middle'},
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

    _updateChart() {
        if (this.chart === null) return;
        this.chart.load({
            json: this.jsonChart,
            transition: {
                duration: 1
            },
        });
    }
    
    render () {
        return <div id={this.domId} />
        
    }
}

// NetDevChart.propTypes = {
//     getLocalDataSource: PropTypes.func.isRequired,
//     name: PropTypes.string.isRequired,
//     description: PropTypes.string.isRequired,
// };
