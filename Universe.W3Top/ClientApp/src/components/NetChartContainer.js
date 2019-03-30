import React, { Component } from 'react';
import { AnotherChart2 } from './AnotherChart2';
import { NetDevChart } from './NetDevChart';
import dataSourceStore from "../stores/DataSourceStore";

export class NetChartContainer extends Component {
    static displayName = NetChartContainer.name;

    hasProperty = (obj, name) => typeof(obj) !== 'undefined'  && typeof(obj[name]) !== 'undefined';
    
    constructor(props) {
        super(props);

        this.getEns33 = this.getEns33.bind(this);
        this.getLo = this.getLo.bind(this);
    }
    
    getEns33(ds)
    {
        
        try {
            return dataSourceStore.getDataSource().interfaces.ens33;
        }
        catch(err)
        {
            return {};
        }
    }

    getLo(ds)
    {

        try {
            return dataSourceStore.getDataSource().interfaces.lo;
        }
        catch(err)
        {
            return {};
        }
    }


    render () {
        return (
            <div>
                <table><tbody><tr><td style={{paddingRight: 40}}>
                    <NetDevChart
                        name="ens33"
                        getLocalDataSource={ __ => this.getEns33() }
                        description="192.168.42.42"
                    />
                    <NetDevChart
                        name="lo"
                        getLocalDataSource={ __ => this.getLo() }
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
