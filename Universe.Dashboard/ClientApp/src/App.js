import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Poc1Chart } from './components/Poc1Chart';
import { Poc2Chart } from './components/Poc2Chart';
import { SingleAxisChart } from './components/SingleAxisChart';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
          <Route exact path='/' component={Poc1Chart} />
          <Route exact path='/proof2' component={Poc2Chart} />
      </Layout>
    );
  }
}
