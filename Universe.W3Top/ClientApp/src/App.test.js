import React from 'react';
import ReactDOM from 'react-dom';
import { MemoryRouter } from 'react-router-dom';
import App from './App';

it('default route renders without crashing', () => {
  const div1 = document.createElement('div');
  ReactDOM.render(
    <MemoryRouter>
      <App />
    </MemoryRouter>, div1);
});

for (let AnComponent of App.GetMenuComponents()) {
    const name = AnComponent.displayName || AnComponent.name;

    it(`RENDER {${name}}`, () => {
        console.log(`---===*** Component: [${name}], type: [${typeof AnComponent}] ***===--- `);
        const div2 = document.createElement('div');
        ReactDOM.render(<MemoryRouter><AnComponent/></MemoryRouter>, div2);
    });
}
    