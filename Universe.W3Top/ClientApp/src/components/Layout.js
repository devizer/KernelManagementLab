import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import MaterialNav from './MaterialNav';


export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div>
        {/*<NavMenu />*/}
        <MaterialNav />
        {/*<Container>*/}
        <div style={{padding: "0px 24px"}}>
          {this.props.children}
        {/*</Container>*/}
        </div>
      </div>
    );
  }
}
