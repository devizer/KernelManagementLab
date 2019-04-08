import React, { Component } from 'react';
import { AnotherChart2 } from './AnotherChart2';
import { NetDevChart } from './NetDevChart';
import { NetDevChartHeader } from './NetDevChartHeader';
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";

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
            return Helper.NetDev.getOptionalInterfacesProperty(dataSourceStore.getDataSource()).ens33;
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
            return Helper.NetDev.getOptionalInterfacesProperty(dataSourceStore.getDataSource()).lo;
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
                    <NetDevChartHeader name={"ens33"}/>
                    <NetDevChart
                        name="ens33"
                        getLocalDataSource={ __ => this.getEns33() }
                        description="192.168.42.42"
                    />
                    <h4>lo</h4>
                    <NetDevChartHeader name={"lo"}/>
                    <NetDevChart
                        name="lo"
                        getLocalDataSource={ __ => this.getLo() }
                        description="192.168.42.42"
                    />
                    <h4>none</h4>
                    <NetDevChartHeader name={"none"}/>
                    <NetDevChart
                        name="none"
                        getLocalDataSource={ __ => this.getNone() }
                        description="interface is absent"
                    />
                </td><td>



                </td></tr></tbody>
                </table>
            </div>
        );
    }
}
