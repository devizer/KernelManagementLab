import React from 'react';
import ReactDOM from 'react-dom';
import { MemoryRouter } from 'react-router-dom';
import App from './App';

// https://github.com/c3js/c3/issues/2129
var JSDOM = require('jsdom').JSDOM; global.window = new JSDOM().window; window.SVGPathElement = function () {};
// console.log(require('c3')); // => no error!

it('sums numbers', () => {
    expect(1+2).toEqual(3);
    expect(2+2).toEqual(4);
});

it('renders without crashing', () => {
  const div = document.createElement('div');
  ReactDOM.render(
    <MemoryRouter>
      <App />
    </MemoryRouter>, div);
});
