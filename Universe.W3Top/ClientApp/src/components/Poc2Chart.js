import React, { Component } from 'react';
import { AnotherChart2 } from './AnotherChart2';

export class Poc2Chart extends Component {
    static displayName = Poc2Chart.name;

    render () {
        return (
            <div>
                <table><tbody><tr><td style={{paddingRight: 40}}>
                    <AnotherChart2 />
                    <br/>
                    <AnotherChart2 />
                    <br/>
                    <AnotherChart2 />
                    <br/>
                    <AnotherChart2 />
                </td><td>
                    <AnotherChart2 />
                    <br/>
                    <AnotherChart2 />
                    <br/>
                    <AnotherChart2 />
                    <br/>
                    <AnotherChart2 />
                </td></tr></tbody>
                </table>
            </div>
        );
    }
}
