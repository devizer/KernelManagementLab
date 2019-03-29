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
        this.connection.on("ReceiveDataSource", dataSource => {
            // var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            let isProd = process.env.NODE_ENV === "production";
            if (!isProd || true) {
                console.log('DataSource RECEIVED ' + (new Date().toLocaleTimeString()));
                console.log(dataSource);
            }
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
        this.tryToConnect();
        this.needConnection = true;
    }

    stop()
    {
        this.connection.stop();
        console.log("Closing SignalR connection");
        this.needConnection = false;
        DataSourceActions.ConnectionStatusUpdated(false);
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
        this.connection.start().then(() => {
            DataSourceActions.ConnectionStatusUpdated(true);
            me.markConnectionState(true);
            console.log("SignalR connection established");
        }).catch(err => {
            me.markConnectionState(false);
            console.warn("SignalR connection error. " + err.toString());
            DataSourceActions.ConnectionStatusUpdated(false);
        });
    }

    // available for callbacks
    watchdog()
    {
        let isProd = process.env.NODE_ENV === "production";
        if (!isProd || true)
            console.log(`watchdog. isConnected: ${this.isConnected}. needConnection: ${this.needConnection}, state: ${this.connection.state}`);
        
        var me = this;
        if (this.needConnection) {
            if (!this.isConnected) {
                if (this.connection.state === 0) {
                    me.tryToConnect();
                }
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
