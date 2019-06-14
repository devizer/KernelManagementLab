import "c3/c3.css"
import "./App.css"
import AppGitInfo from './AppGitInfo'

import 'babel-polyfill';
import React, { Component } from 'react';
import { Route, Switch, Router } from 'react-router';
import { Layout } from './components/Layout';
import { Poc1Chart } from './components/Poc1Chart';
import { Poc2Chart } from './components/Poc2Chart';
import MNav from "./components/MaterialNav"
import { NetChartContainer } from './components/NetChartContainer';
import { NetChartContainer_V2 } from './components/NetChartContainer_V2';
import { BlockChartContainer_V2 } from './components/BlockStatChartContainer_V2';
import { MountsList } from './components/MountsList';
import dataSourceListener from './stores/DataSourceListener';
import "./components/MyC3.css"
import DiskBenchmarkDialog from "./components/DiskBenchmarkDialog";
import PopperLab from "./components/Popper-Lab"


require('typeface-roboto')

// fetch for IE11
require('es6-promise').polyfill();
require('isomorphic-fetch');

export default class App extends Component {
    static displayName = App.name;

    constructor (props) {
        super(props);

        this.logVersionInfo();
        console.log(`window.jQuery is ${typeof window.$}`)
    }
    
    logVersionInfo()
    {
        AppGitInfo.CommitAt = new Date(AppGitInfo.CommitDate*1000).toLocaleString();
        AppGitInfo.BuildAt = new Date(AppGitInfo.BuildDate*1000).toLocaleString();
        console.log(AppGitInfo);
    }
    
    componentDidMount() {
        dataSourceListener.start();
    }
    
    componentWillUnmount() {
        dataSourceListener.stop();
    }
    
    static _404 = () => (
        <h6 style={{textAlign: "center"}}>
            <img src="https://cdnjs.cloudflare.com/ajax/libs/emojione/2.2.7/assets/png/2611.png" width={24}/>&nbsp;
            Oops! 404 had been happened
        </h6>
    );
    
    render () {
        return (
            <Layout>
                    <Switch>
                        <Route exact path='/' component={NetChartContainer_V2} />
                        <Route exact path='/disks' component={BlockChartContainer_V2} />
                        <Route exact path='/material-nav' component={MNav} />
                        <Route exact path='/1-axis' component={Poc1Chart} />
                        <Route exact path='/2-axis' component={Poc2Chart} />
                        <Route exact path='/net-v1' component={NetChartContainer} />
                        <Route exact path='/net-v2' component={NetChartContainer_V2} />
                        <Route exact path='/mounts' component={MountsList} />
                        <Route exact path='/disk-benchmark-lab' component={DiskBenchmarkDialog} />
                        <Route exact path='/popper-lab' component={PopperLab} />
                        <Route path="*" component={App._404} />
                    </Switch>
            </Layout>
        );
    }
}
