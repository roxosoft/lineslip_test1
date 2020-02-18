import React, { Component } from 'react';
import DatePicker from "react-datepicker";

export class BlockedIPs extends Component {
    static displayName = BlockedIPs.name;

    constructor(props) {
        super(props);
        this.state = { ips: [], loading: true };
    }

    componentDidMount() {
        this.populateipsData();
    }

    static renderipsable(ips) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>IP</th>
                    </tr>
                </thead>
                <tbody>
                    {ips.map(ip =>
                        <tr key={ip.date}>
                            <td>{BlockedIPs.convertUTCDateToLocalDate(ip.date)}</td>
                            <td>{ip.ip}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    static convertUTCDateToLocalDate(dateStr) {
        let date = new Date(dateStr);
        var newDate = new Date(date.getTime() + date.getTimezoneOffset() * 60 * 1000);

        var offset = date.getTimezoneOffset() / 60;
        var hours = date.getHours();

        newDate.setHours(hours - offset);

        return newDate.toLocaleString();
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : BlockedIPs.renderipsable(this.state.ips);

        return (
            <div>
                <h4 id="tabelLabel" >Blocked IPs</h4>
                {contents}
            </div>
        );
    }

    async populateipsData() {
        this.setState({ ips: [], loading: true });
        try {
            //(new Date).convertUTCDateToLocalDate
            console.log('loading', this.state.startDate.toISOString());
        }
        catch{ }

        const response = await fetch('api/ips');

        const data = await response.json();

        this.setState({ ips: data, loading: false });
    }

}
