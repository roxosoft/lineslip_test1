import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { NotesList } from './components/NotesList';
import { BlockedIPs } from './components/BlockedIPs';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Route exact path='/' component={Home} />
                <Route path='/BlockedIPs' component={BlockedIPs} />
                <Route path='/notes-list' component={NotesList} />
            </Layout>
        );
    }
}
