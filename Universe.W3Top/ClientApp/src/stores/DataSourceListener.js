import * as DataSourceActions from './DataSourceActions'
import * as signalR from '@microsoft/signalr'
import * as Helper from "../Helper";
import DataSourceStore from "./DataSourceStore";

class DataSourceListener {
    
    constructor()
    {
        this.watchdogTick = this.watchdogTick.bind(this);
        this.markConnectionState = this.markConnectionState.bind(this);
        this.tryToConnect = this.tryToConnect.bind(this);
        this.applyDocumentTitle = this.applyDocumentTitle.bind(this);
        
        this.isConnected = false;
        this.needConnection = false;
        this.isConnecting = false;

        let hub = new signalR.HubConnectionBuilder().withUrl("/api-hubs/dataSourceHub");
        // hub.build().state
        this.connection = hub.build();
        this.connection.on("ReceiveDataSource", dataSource => {
            Helper.notifyTrigger("MetricsArriving", "true");

            let [hasMessageId, messageId] = Helper.Common.tryGetProperty(dataSource, "messageId");
            messageId = messageId || "<unknown-message>";

            Helper.toConsole(`DataSource RECEIVED [${messageId}] at ` + (new Date().toLocaleTimeString()), dataSource);
            try {
                DataSourceActions.DataSourceUpdated(dataSource);
                this.applyDocumentTitle(dataSource);
                Helper.notifyTrigger("MetricsArrived", "true");
            } catch(err){
                console.error(err);
            }
        });

        this.connection.onclose(error => {
            // 1006: server terminated
            console.warn("SignalR connection closed" + (error ? " with error" : ""));
            this.markConnectionState(false);
            this.isConnecting = false;
            DataSourceActions.ConnectionStatusUpdated(false);
        });

        this.timerId = setInterval(this.watchdogTick, 1000);

        let apiUrl = 'api/BriefInfo';
        try {
            fetch(apiUrl)
                .then(response => {
                    console.log(`Response.Status for ${apiUrl} obtained: ${response.status}`);
                    console.log(response);
                    console.log(response.body);
                    return response.ok ? response.json() : {error: response.status, details: response.json()}
                })
                .then(briefInfo => {
                    this.applyDocumentTitle(briefInfo);
                    DataSourceActions.BriefUpdated(briefInfo);
                    Helper.notifyTrigger("BriefInfoArrived", "wow!");
                    Helper.toConsole("BRIEF INFO", briefInfo);
                })
                .catch(error => console.log(error));
        }
        catch(err)
        {
            console.error(`FETCH failed for ${apiUrl}. ${err}`);
        }


    }
    
    first = true;
    applyDocumentTitle(message)
    {
        if (this.first) {
            try {
                let hostname = Helper.System.getHostName(message);
                if (hostname !== null) {
                    document.title = `W3 Top (${hostname})`;
                    this.first = false;
                }
            }catch{}
        }
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
        // Helper.toConsole("tryToConnect()...", "sure");
        this.isConnecting = true;
        this.connection.start().then(() => {
            DataSourceActions.ConnectionStatusUpdated(true);
            this.markConnectionState(true);
            console.log("SignalR connection established");
        }).catch(err => {
            this.isConnecting = false;
            this.markConnectionState(false);
            console.warn("SignalR connection error. " + err.toString());
            DataSourceActions.ConnectionStatusUpdated(false);
        });
    }

    // available for callbacks
    watchdogTick()
    {
        this.needConnection = !Helper.isDocumentHidden(); 
        if (this.isConnected !== this.needConnection)
            console.log(`%c[watchdog] isConnected: ${this.isConnected},
           needConnection: ${this.needConnection},
           isConnecting: ${this.isConnecting},
           state: ${this.connection.state}`, 'color:#4615b2');
        
        if (this.needConnection) {
            if (!this.isConnected && !this.isConnecting) {
                if (this.connection.state === "Disconnected" || this.connection.state === 0) {
                    this.tryToConnect();
                }
            }
        }
        else
        {
            if (this.isConnected) {
                this.markConnectionState(false);
                this.isConnecting = false;
                this.connection.stop();
            }
        }
    }
    
}

const dataSourceListener = new DataSourceListener();
export default dataSourceListener;
