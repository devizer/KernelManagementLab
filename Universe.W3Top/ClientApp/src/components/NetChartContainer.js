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
        this.getNone = this.getNone.bind(this);
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

    getNone()
    {
        return {};
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
                <h2>Hardcoded interfaces (prototype with live data)</h2>
                <table><tbody><tr><td style={{paddingRight: 40}}>
                    <h4>ens33</h4>
                    <NetDevChart
                        name="ens33"
                        getLocalDataSource={ __ => this.getEns33() }
                        description="192.168.42.42"
                    />
                    <h4>lo</h4>
                    <NetDevChart
                        name="lo"
                        getLocalDataSource={ __ => this.getLo() }
                        description="192.168.42.42"
                    />
                    <h4>none</h4>
                    <NetDevChart
                        name="none"
                        getLocalDataSource={ __ => this.getNone() }
                        description="interface is absent"
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
