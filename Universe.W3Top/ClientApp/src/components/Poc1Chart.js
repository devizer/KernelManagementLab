import React, { Component } from 'react';
import { SingleAxisChart } from './SingleAxisChart';

export class Poc1Chart extends Component {
  static displayName = Poc1Chart.name;

  render () {
    return (
      <div>
          <table><tbody><tr><td style={{paddingRight: 40}}>
              <SingleAxisChart />
              <SingleAxisChart />
              <SingleAxisChart />
              <SingleAxisChart />
          </td><td>
              <SingleAxisChart />
              <SingleAxisChart />
              <SingleAxisChart />
              <SingleAxisChart />
          </td></tr></tbody>
          </table>
      </div>
    );
  }
}
