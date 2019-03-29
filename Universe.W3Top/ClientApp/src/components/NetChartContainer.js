import React, { Component } from 'react';
import { AnotherChart2 } from './AnotherChart2';
import { NetDevChart } from './NetDevChart';

export class NetChartContainer extends Component {
    static displayName = NetChartContainer.name;
    
    getEns33(ds)
    {
        return ds['interfaces']['ens33'];
    }

    render () {
        return (
            <div>
                <table><tbody><tr><td style={{paddingRight: 40}}>
                    <NetDevChart 
                        name="ens33"
                        getLocalDataSource={ ds => this.getEns33(ds) }
                        description="192.168.42.42"
                    />
                    {/* 
                    <br/>
                    <NetDevChart
                        name="lo"
                        getLocalDataSource={ glo => glo.Interfaces.lo }
                        description="localhost"
                    />
                    */}
                </td><td>



                </td></tr></tbody>
                </table>
            </div>
        );
    }
}
