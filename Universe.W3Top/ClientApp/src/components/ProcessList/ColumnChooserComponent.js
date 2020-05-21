import React, {Component} from 'react';
import processListStore from "./Store/ProcessListStore";
import DialogContent from "@material-ui/core/DialogContent";

export class ColumnChooserComponent extends Component {
    static displayName = ColumnChooserComponent.name;

    constructor(props) {
        super(props);

        this.state = {
            selectedColumns: processListStore.getSelectedColumns(),
        };
    }

    render () {
        return (
            <div>
                <p className="cs-group">Process:</p>
                <p className="cs-line">[X] PID, [X] Name, [X] Priority, [X] Uptime, [X] Command line</p>

                <p className="cs-group">IO Time:</p>
                <p className="cs-line">[X] Total, [X] Current</p>

                <p className="cs-group">IO Transfer:</p>
                <p className="cs-line">Logical Read: [X] Total, [X] Current. Logical Write [X] Total, [X] Current</p>
                <p className="cs-line">Calls Read: [X] Total, [X] Current. Calls Write: [X] Total, [X] Current</p>
                <p className="cs-line">Block Level Read: [X] Total, [X] Current. Block Level Write: [X] Total, [X] Current</p>

                <p className="cs-group">Memory:</p>
                <p className="cs-line">[X] RSS, [X] Shared, [X] Swapped</p>

                <p className="cs-group">Page Faults:</p>
                <p className="cs-line">Minor: [X] Total, [X] Current. Swapin: [X] Total, [X] Current</p>
                <p className="cs-line">Children Minor: [X] Total, [X] Current. Children Swapins: [X] Total, [X] Current</p>

                <p className="cs-group">CPU Usage:</p>
                <p className="cs-line">User: [X] Total, [X] Current. Kernel: [X] Total, [X] Current. Total: [X] Total, [X] Current</p>
                <p className="cs-line">Children User: [X] Total, [X] Current. Children Kernel: [X] Total, [X] Current. Children Total: [X] Total, [X] Current</p>

            </div>
        );
    }
}
