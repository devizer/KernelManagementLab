import React, { Component } from 'react';
import "c3/c3.css"
import "./MyC3.css"
let c3 = require('c3');

export class Counter_kill extends Component {
    static displayName = Counter.name;

    chart = null;
    jsonData = {};
    pointCurrent = -1;
    pointLimit = 60;
    updateInterval = 1000;
    timerId = null;
    
    calcNextSceneDataSource() {
        this.pointCurrent = (this.pointCurrent + 1) % 2000000000;
        let arrx = [];
        let arr1 = [];
        let arr2 = [];
        let round = n => Math.round(n * 10) / 10.0;
        for(let x = 0; x <= 60; x += 1)
        {
            let y1 = Math.sin(((this.pointCurrent+x) / this.pointLimit) * Math.PI)*40+50 + Math.sin(20*(this.pointCurrent+x))*2;
            let y2 = Math.sin(((this.pointCurrent*3+x*1.8+17) / this.pointLimit) * Math.PI)*40+50 + Math.cos(20*(this.pointCurrent+x))*2;
            // console.log(`x = ${x}, y1 = ${y1}, y2 = ${y2}`);
            arrx.push(x);
            arr1.push(round(y1));
            arr2.push(round(y2));
        }
        
        this.jsonData = { microSD: arr2, eMMC: arr1, }; 
    }

    constructor (props) {
        super(props);
        this.state = { currentCount: 0 };
        this.incrementCounter = this.incrementCounter.bind(this);
    }

    incrementCounter () {
        this.setState({
            currentCount: this.state.currentCount + 1
        });
    }
    
    componentDidMount() {
        
        this.calcNextSceneDataSource();
        this._initChart();
        
        // return;
        this.timerId = setInterval(_ => {
            this.calcNextSceneDataSource();
            this._updateChart();
            // this.forceUpdate();
        }, this.updateInterval);
    }
    
    componentWillUnmount() {
        if (this.timerId)
        {
            clearInterval(this.timerId);
            this.timerId = 0;
        }
    }


    _initChart()
    {
        var formatX = x => {
            if (x === 0) return "60";
            if (x === 60) return "";
            return Math.round(60-x);
        };
        
        this.chart = c3.generate({
            bindto: '#chart',
            data: {
                // columns: columns,
                json : this.jsonData,
                type: 'area-spline'
            },
            transition: {
                duration: 1
            },
            size: {
                width: 350,
                height: 130,
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
        this.chart.load({
           json: this.jsonData,
            transition: {
                duration: 1000
            },
            size: {
                width: 350,
                height: 100,
            },
        });
    }

    render () {
        return <div id='chart' />

    }
}
