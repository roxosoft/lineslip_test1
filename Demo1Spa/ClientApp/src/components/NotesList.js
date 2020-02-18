import React, { Component } from 'react';
import DayPickerInput from 'react-day-picker/DayPickerInput';
import 'react-day-picker/lib/style.css';


export class NotesList extends Component {
    static displayName = NotesList.name;

  constructor(props) {
    super(props);
      this.state = { notes: [], loading: true, startDate: null, endDate: null };

      this.handleChangeStart = this.handleChangeStart.bind(this);
      this.handleChangeEnd = this.handleChangeEnd.bind(this);
  }

  componentDidMount() {
    this.populateNotesData();
    }

    async handleChangeStart(date) {
        await this.setState({
            startDate: date
        });
        console.log('start', date, this.state);
        this.populateNotesData();
    };

    async handleChangeEnd(date) {
        await this.setState({
            endDate: date
        });
        console.log('end', date, this.state);
        this.populateNotesData();
    };


  static renderNotesTable(notes) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Date</th>
            <th>Note</th>
          </tr>
        </thead>
        <tbody>
          {notes.map(note =>
            <tr key={note.date}>
                  <td>{NotesList.convertUTCDateToLocalDate(note.date)}</td>
                <td>{note.note}</td>
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
        : NotesList.renderNotesTable(this.state.notes);

    return (
      <div>
            <h4 id="tabelLabel" >Notes</h4>
            <div>
                Start date: <DayPickerInput selected={this.state.startDate}
                    onDayChange={this.handleChangeStart}
                />&nbsp;&nbsp;&nbsp;
                End date: <DayPickerInput selected={this.state.endDate}
                    onDayChange={this.handleChangeEnd}
                    />
            </div>
        {contents}
      </div>
    );
  }

    async populateNotesData() {
        this.setState({ notes: [], loading: true });
        try {
            //(new Date).convertUTCDateToLocalDate
            console.log('loading', this.state.startDate.toISOString());
        }
        catch{ }

        const response = await fetch(
            'api/notes?startDate=' + this.stringifyDate(this.state.startDate) +
            "&endDate=" + this.stringifyDate(this.state.endDate));

        const data = await response.json();

        this.setState({ notes: data, loading: false });
    }

    stringifyDate(date) {
        return (date === null ? '' : (new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()))).toISOString());
    }
}
