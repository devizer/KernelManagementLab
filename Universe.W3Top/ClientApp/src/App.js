import "c3/c3.css"
import "./components/MyC3.css"
import "./App.css"
import AppGitInfo from './AppGitInfo'

import 'babel-polyfill';
import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Poc1Chart } from './components/Poc1Chart';
import { Poc2Chart } from './components/Poc2Chart';
import { NetChartContainer } from './components/NetChartContainer';
import { NetChartContainer_V2 } from './components/NetChartContainer_V2';
import { MountsList } from './components/MountsList';
import dataSourceListener from './stores/DataSourceListener';

// fetch for IE11
require('es6-promise').polyfill();
require('isomorphic-fetch');

export default class App extends Component {
    static displayName = App.name;

    constructor (props) {
        super(props);

        this.logVersionInfo();

        try {
            let apiUrl = 'api/Health/PingDb';
            fetch(apiUrl)
                .then(response => {
                    console.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    console.log(response);
                    console.log(response.body);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(data => {
                    console.log(data);
                })
                .catch(error => console.log(error));
        }
        catch(err)
        {
            console.log('FETCH failed. ' + err);
        }

        
        return;
        fetch('api/Health/PingDb')
            .then(response => {
                console.log(`Response.Status: ${response.status}`);
                console.log(response);
                console.log(response.body);
                return response.ok ? response.json() : {error: response.status, details: response.json()}
            })
            .then(data => {
                console.log(data);
            })
            .catch(error => console.log(error));
    }
    
    logVersionInfo()
    {
        AppGitInfo.CommitAt = new Date(AppGitInfo.CommitDate*1000).toLocaleString();
        console.log(AppGitInfo);
    }
    
    componentDidMount() {
        dataSourceListener.start();
    }
    
    componentWillUnmount() {
        dataSourceListener.stop();
    }
    

    render () {
        return (
            <Layout>
                <Route exact path='/' component={Poc1Chart} />
                <Route exact path='/1axis' component={Poc1Chart} />
                <Route exact path='/2axis' component={Poc2Chart} />
                <Route exact path='/net' component={NetChartContainer} />
                <Route exact path='/net_v2' component={NetChartContainer_V2} />
                <Route exact path='/disk_v1' component={MountsList} />
            </Layout>
        );
    }
}
