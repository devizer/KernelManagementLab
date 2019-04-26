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
            blockChartList: this.tryBuildBlockChartList(),
        };
        
        Helper.toConsole(`BlockChartContainer_V2::ctor. this.state.blockChartList`, this.state.blockChartList);
    }

    componentDidMount() {
        let isAlreadyBound = this.state.blockChartList !== null;
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
        if (this.state.blockChartList === null)
        {
            let blockChartList = this.tryBuildBlockChartList();
            if (blockChartList !== null) {
                this.setState({
                    blockChartList: blockChartList,
                });
            }
        }
    }
    
    tryBuildBlockChartList()
    {
        let globalDataSource = dataSourceStore.getDataSource();
        let [hasBlock, block] = Helper.Common.tryGetProperty(globalDataSource, "block");
        if (hasBlock) {
            let blockNames = globalDataSource.block.blockNames.map((x) => {
                return {name: x};
            });

            return blockNames;
        }
        
        return null;
    }


    renderLoading() {
        let style = {textAlign: "center", paddingBottom: NetDevChart.ChartSize.height/2, paddingTop: NetDevChart.ChartSize.height/2, width: NetDevChart.ChartSize.width};
        return (
            <h6 id="BlockCharts_is_Waiting_for_connection" style={style}>
                Waiting for curved lines
            </h6>
        )
    }

    render () {
        return (
            <div id="BlockChartContainer_V2">
                {this.state.blockChartList === null ? this.renderLoading() : this.renderNormal()}
            </div>
        );
    }

    renderNormal() {
        return (
            <div id="NetCharts">
                {this.state.blockChartList.map(netChart =>
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
