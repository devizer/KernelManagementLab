import * as DataSourceActions from './DataSourceStore'
import * as signalR from '@aspnet/signalr'
import dispatcher from "./DataSourceDispatcher"; 


class DataSourceListener {
    
    start() {

        this.connection = new signalR.HubConnectionBuilder().withUrl("/dataSourceHub").build();

        this.connection.on("ReceiveDataSource", function (dataSource) {
            // var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            console.log('MESSAGE RECEIVED');
            console.log(dataSource);
        });

        this.connection.start().then(function(){
           console.log("SignalR connection established");
        }).catch(function (err) {
            return console.error("SignalR connection error. " + err.toString());
        });
    }
    
    stop()
    {
        console.log("Closing SignalR connection");
        this.connection.stop();
    }
}

const dataSourceListener = new DataSourceListener();
export default dataSourceListener;
