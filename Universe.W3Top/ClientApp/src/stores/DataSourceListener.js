import * as DataSourceActions from './DataSourceActions'
import * as signalR from '@aspnet/signalr'
import * as Helper from "../Helper";

class DataSourceListener {
    
    constructor()
    {
        this.watchdogTick = this.watchdogTick.bind(this);
        this.markConnectionState = this.markConnectionState.bind(this);
        this.tryToConnect = this.tryToConnect.bind(this);
        this.applyDocumentTitle = this.applyDocumentTitle.bind(this);
        
        this.isConnected = false;
        this.needConnection = false;

        this.connection = new signalR.HubConnectionBuilder().withUrl("/dataSourceHub").build();
        this.connection.on("ReceiveDataSource", dataSource => {
            Helper.toConsole('DataSource RECEIVED at ' + (new Date().toLocaleTimeString()), dataSource);
            try {
                DataSourceActions.DataSourceUpdated(dataSource);
                this.applyDocumentTitle(dataSource);
            } catch(err){
                console.error(err);
            }
        });

        this.connection.onclose(error => {
            // 1006: server terminated
            console.warn("SignalR connection closed" + (error ? " with error" : ""));
            this.markConnectionState(false);
            DataSourceActions.ConnectionStatusUpdated(false);
        });

        this.timerId = setInterval(this.watchdogTick, 1000);
    }
    
    applyDocumentTitle(globalDataSource)
    {
        let [hasHostname, hostname] = Helper.Common.tryGetProperty(globalDataSource, "hostname");
        document.title = `W3 Top` + (hasHostname? ` (${hostname})` : "");
    }

    start() {
        this.tryToConnect();
        this.needConnection = true;
    }

    stop()
    {
        console.log("Closing SignalR connection");
        this.connection.stop();
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
        this.connection.start().then(() => {
            DataSourceActions.ConnectionStatusUpdated(true);
            this.markConnectionState(true);
            console.log("SignalR connection established");
        }).catch(err => {
            this.markConnectionState(false);
            console.warn("SignalR connection error. " + err.toString());
            DataSourceActions.ConnectionStatusUpdated(false);
        });
    }

    // available for callbacks
    watchdogTick()
    {
        Helper.log(`[watchdog] isConnected: ${this.isConnected}. needConnection: ${this.needConnection}, state: ${this.connection.state}`);
        
        if (this.needConnection) {
            if (!this.isConnected) {
                if (this.connection.state === 0) {
                    this.tryToConnect();
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
