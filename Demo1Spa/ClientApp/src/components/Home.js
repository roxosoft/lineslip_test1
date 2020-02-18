import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

    constructor(props) {
        super(props);
        this.state = { value: '' };

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(event) {
        this.setState({ value: event.target.value });
    }

    async handleSubmit(event) {
        console.log('Creating note...', this.state.value);
        event.preventDefault();

        await fetch('api/CreateNote', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(
                this.state.value
            )
        })

        console.log('Creating note... done');
        this.setState({ value: '' });
    }

  render () {
    return (
      <div>
        <h4>Create note</h4>
        Note: <input type="text" value={this.state.value} onChange={this.handleChange} /> <button className="btn btn-primary" onClick={this.handleSubmit}>Create</button>
      </div>
    );
  }
}
