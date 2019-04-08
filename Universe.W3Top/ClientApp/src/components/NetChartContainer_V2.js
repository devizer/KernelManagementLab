import React, { Component } from 'react';
import { AnotherChart2 } from './AnotherChart2';
import { NetDevChart } from './NetDevChart';
import { NetDevChartHeader } from './NetDevChartHeader';
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";

export class NetChartContainer_V2 extends Component {
    static displayName = NetChartContainer_V2.name;

    hasProperty = (obj, name) => typeof obj != 'undefined'  && typeof obj[name] != 'undefined';
    
    timerId = null;
    
    constructor(props) {
        super(props);

        this.tryInitNetChartList = this.tryInitNetChartList.bind(this);
        this.tryBuildNetChartList = this.tryBuildNetChartList.bind(this);
        
        this.state = {
            visibleInterfaceNames: [],
            netChartList: this.tryBuildNetChartList(),
        };
        
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
        this.tryInitNetChartList();
    }

    tryBuildNetChartList()
    {
        let globalDataSource = dataSourceStore.getDataSource();
        let interfaces = Helper.NetDev.getOptionalInterfacesProperty(globalDataSource);
        
        // activeInterfaceNames.map(interfaceName => {return {}});
        if (interfaces !== null) {
            let activeInterfaceNames = Helper.NetDev.getActiveInterfaceList(globalDataSource);
            // just simple map to this.jsonChart 
            return activeInterfaceNames.map(interfaceName => {
                return {
                    name: interfaceName,
                    description: `the ${interfaceName} description is not yet completed`,
                    getLocalDataSource: () => {
                        try {
                            let glo = dataSourceStore.getDataSource();
                            let ret = Helper.NetDev.getOptionalInterfacesProperty(glo)[interfaceName];
                            ret.totals = glo.net.interfaceTotals[interfaceName];
                            return ret;
                        } catch {
                            return {};
                        }
                    },
                    getTotals: () => {
                        try {
                            let glo = dataSourceStore.getDataSource();
                            return glo.net.interfaceTotals[interfaceName];
                        } catch {
                            return {};
                        }
                    },
                }
            });
        }
        
        return null;
    }

    // used by timer callback
    tryInitNetChartList()
    {
        if (this.state.netChartList === null)
        {
            let netChartList = this.tryBuildNetChartList();
            if (netChartList !== null) {
                this.setState({
                    netChartList: netChartList,
                });
            }
        }
    }
    

    renderLoading() {
        return (
            <h1 id="NetCharts_is_Waiting_for_connection" style={{textAlign: "center"}}>
                Network History is Waiting for connection
            </h1>
        )
    }
    
    renderNormal() {
        return (
            <div id="NetCharts">
                {this.state.netChartList.map(netChart =>
                    <div className="CHART" key={netChart.name}>
                        <NetDevChartHeader name={netChart.name}/>
                        
                        <NetDevChart
                            name={netChart.name}
                            getLocalDataSource={netChart.getLocalDataSource}
                            description={netChart.description}
                        />
                    <br/>
                    </div>
                )}
            </div>
        );
    }

    render () {
        return (
            <div id="NetChartContainer_V2">
                {this.state.netChartList === null ? this.renderLoading() : this.renderNormal()}
            </div>
        );
    }
}
