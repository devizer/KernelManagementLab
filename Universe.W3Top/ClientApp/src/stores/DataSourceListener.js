import * as DataSourceActions from './DataSourceActions'
import * as signalR from '@aspnet/signalr'

class DataSourceListener {
    
    constructor()
    {
        this.watchdog = this.watchdog.bind(this);
        this.markConnectionState = this.markConnectionState.bind(this);
        this.tryToConnect = this.tryToConnect.bind(this);
        
        this.isConnected = false;
        this.needConnection = false;

        this.connection = new signalR.HubConnectionBuilder().withUrl("/dataSourceHub").build();
        this.connection.on("ReceiveDataSource", function (dataSource) {
            // var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            console.log('DataSource RECEIVED ' + (new Date().toLocaleTimeString()));
            console.log(dataSource);
            DataSourceActions.DataSourceUpdated(dataSource);
        });

        this.connection.onclose(error => {
            // 1006: server terminated
            console.warn("SignalR connection closed" + (error ? " with error" : ""));
            this.markConnectionState(false);
            DataSourceActions.ConnectionStatusUpdated(false);
        });

        this.timerId = setInterval(this.watchdog, 1000);
    }

    start() {
        this.markConnectionState(true);
        this.tryToConnect();
    }

    stop()
    {
        console.log("Closing SignalR connection");
        this.markConnectionState(false);
        this.connection.stop();
        // clearInterval(this.timerId);
    }


    // available for callbacks
    markConnectionState(newIsConencted)
    {
        this.isConnected = newIsConencted;
    }

    // available for callbacks
    tryToConnect()
    {
        let me = this;
        this.connection.start().then(function () {
            me.markConnectionState(true);
            console.log("SignalR connection established");
            DataSourceActions.ConnectionStatusUpdated(true);
        }).catch(function (err) {
            me.markConnectionState(false);
            console.warn("SignalR connection error. " + err.toString());
            DataSourceActions.ConnectionStatusUpdated(false);
        });
    }

    // available for callbacks
    watchdog()
    {
        console.log(`watchdog. isConnected: ${this.isConnected}. needConnection: ${this.needConnection}`);
        var me = this;
        if (this.needConnection) {
            if (!this.isConnected) {
                me.tryToConnect();
            }
        }
        else
        {
            if (this.isConnected) {
                this.markConnectionState(false);
                this.connection.stop();
            }
        }
    }
    
}

const dataSourceListener = new DataSourceListener();
export default dataSourceListener;
