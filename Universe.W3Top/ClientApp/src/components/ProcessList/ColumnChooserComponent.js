import React, {Component} from 'react';
import processListStore from "./Store/ProcessListStore";

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
                TODO: ColumnChooserComponent
            </div>
        );
    }
}
