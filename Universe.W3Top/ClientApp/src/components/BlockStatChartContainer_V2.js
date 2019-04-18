import React, { Component } from 'react';
import { BlockStatChart } from './BlockStatChart';
import { NetDevChartHeader } from './NetDevChartHeader';
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";
import * as Enumerable from "linq-es2015";
import {NetDevChart} from "./NetDevChart";
import {BlockStatDevChartHeader} from "./BlockStatChartHeader";

export class BlockChartContainer_V2 extends Component {
    static displayName = BlockChartContainer_V2.name;

    timerId = null;

    constructor(props) {
        super(props);

        this.tryInitBlockChartList = this.tryInitBlockChartList.bind(this);
        this.tryBuildBlockChartList = this.tryBuildBlockChartList.bind(this);

        this.state = {
            visibleDeviceNames: [],
            netChartList: this.tryBuildBlockChartList(),
        };
        
        console.log(`BlockChartContainer_V2::ctor. this.state.netChartList is [${this.state.netChartList}]`);
    }

    componentDidMount() {
        let isAlreadyBound = this.state.netChartList !== null;
        if (!isAlreadyBound)
            this.timerId = setInterval(this.waiterTick.bind(this));
    }

    componentWillUnmount() {
        if (this.timerId !== null)
            clearInterval(this.timerId);
    }

    waiterTick()
    {
        this.tryInitBlockChartList();
    }

    // used by timer callback
    tryInitBlockChartList()
    {
        if (this.state.netChartList === null)
        {
            let netChartList = this.tryBuildBlockChartList();
            if (netChartList !== null) {
                this.setState({
                    netChartList: netChartList,
                });
            }
        }
    }
    
    tryBuildBlockChartList()
    {
        let globalDataSource = dataSourceStore.getDataSource();
        return ["/dev/hdd1", "/dev/loop42"].map((x) => {
            return {name: x};
        });
        
        this.setState({
            visibleDeviceNames: ["/dev/hdd1", "/dev/loop42"],
            netChartList: ["/dev/hdd1", "/dev/loop42"],
        });
    }


    renderLoading() {
        return (
            <h6 id="BlockCharts_is_Waiting_for_connection" style={{textAlign: "center", paddingTop: 80}}>
                Waiting for connection
            </h6>
        )
    }

    render () {
        return (
            <div id="BlockChartContainer_V2">
                {this.state.netChartList === null ? this.renderLoading() : this.renderNormal()}
            </div>
        );
    }

    renderNormal() {
        return (
            <div id="NetCharts">
                {this.state.netChartList.map(netChart =>
                    <div className="CHART" key={netChart.name}>
                        <BlockStatDevChartHeader name={netChart.name}/>
                        <BlockStatChart name={netChart.name} />
                        <br/>
                    </div>
                )}
            </div>
        );
    }



}
