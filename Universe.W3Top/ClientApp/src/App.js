import "c3/c3.css"
import "./components/MyC3.css"

import 'babel-polyfill';
import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Poc1Chart } from './components/Poc1Chart';
import { Poc2Chart } from './components/Poc2Chart';
import { NetChartContainer } from './components/NetChartContainer';
import dataSourceListener from './stores/DataSourceListener';

export default class App extends Component {
    static displayName = App.name;

    constructor (props) {
        super(props);

        fetch('api/Test1/BeCrazy')
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
            </Layout>
        );
    }
}
