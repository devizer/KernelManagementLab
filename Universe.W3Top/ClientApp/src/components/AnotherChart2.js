import React, { Component } from 'react';
import nextUniqueId from "../NextUniqueId"
import c3 from 'c3';

export class AnotherChart2 extends Component {
    static displayName = AnotherChart2.name;

    // static domIdCounter = 0;
    domId = nextUniqueId("cart2y");
    chart = null;
    jsonData = {};
    pointCurrent = Math.round(Math.random()*1000);
    pointLimit = 60;
    updateInterval = 1000;
    timerId = null;

    calcNextSceneDataSource() {
        this.pointCurrent = (this.pointCurrent + 1) % 2000000000;
        let arrx = [];
        let arr1Busy = [];
        let arr2Busy = [];
        let arr1Transfer = [];
        let arr2Transfer = [];
        let round = n => Math.round(n * 10) / 10.0;
        for(let x = 0; x <= 60; x += 1)
        {
            let y1Busy = Math.sin(((this.pointCurrent+x) / this.pointLimit) * Math.PI)*40+50 + Math.sin(20*(this.pointCurrent+x))*2;
            let y2Busy = Math.sin(((this.pointCurrent*3+x*1.8+17) / this.pointLimit) * Math.PI)*40+50 + Math.cos(20*(this.pointCurrent+x))*2;
            let y1Transfer = ((this.pointCurrent+x) % 30)*((this.pointCurrent+x) % 30) / 30;
            let y2Transfer = ((this.pointCurrent+x+12) % 30)*((this.pointCurrent+x+12) % 30) / 30;
            // console.log(`x = ${x}, y1 = ${y1}, y2 = ${y2}`);
            arrx.push(x);
            arr1Busy.push(round(y1Busy)*40);
            arr2Busy.push(round(y2Busy)*40);
            arr1Transfer.push(y1Transfer*80);
            arr2Transfer.push(y2Transfer*80);
        }

        this.jsonData = { 
            microSD_Busy: arr2Busy, 
            eMMC_Busy: arr1Busy,
            microSD_Transfer: arr1Transfer,
            eMMC_Transfer: arr2Transfer,
        };
    }

    constructor (props) {
        super(props);
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
                json : this.jsonData,
                type: 'area-spline',
                axes : {
                    microSD_Transfer : "y2",
                    eMMC_Transfer : "y2",
                },
                names : {
                    microSD_Transfer: "Rd IOPS",
                    eMMC_Transfer: "Wr IOPS",
                    microSD_Busy: "Rd Mb/s",
                    eMMC_Busy: "Wr Mb/s",
                }
            },
            transition: {
                duration: 1
            },
            size: {
                width: 450,
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
                    min: 0,
                    max: 4000,
                    padding: 0,
                    label: {
                        text: 'Transfer, Mb/s',
                        position: 'outer-left'
                    },
                    tick: {
                        count: 5,
                        format: val => Math.round(val),
                        // values: [1, 2, 4, 8, 16, 32]
                    }
                },
                y2 : {
                    max: 2400,
                    // min: -42,
                    padding: 0,
                    show: true,
                    tick: {
                        count: 5,
                        format: val => Math.round(val),
                        // values: [1, 2, 4, 8, 16, 32]
                    },
                    label: {
                        text: 'IOPS',
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

    _updateChart() {
        if (this.chart === null) return;
        this.chart.load({
            json: this.jsonData,
            // transition: {
            //     duration: 1000
            // },
            // size: {
            //     width: 450,
            //     height: 100,
            // },
        });
    }

    render () {
        return <div id={this.domId} />

    }
}
