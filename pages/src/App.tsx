import React from 'react';
import './App.css';
import QueryForm from './QueryForm';
import * as queryString from 'query-string'

function App() {
  let initialQuery = queryString.parse(document.location.search).query
  if (Array.isArray(initialQuery)) {
    initialQuery = initialQuery[0]
  }

  return (
    <div className="app">
      <section className="app-body">
        <QueryForm query={initialQuery ?? ""} />
      </section>
    </div>
  );
}

export default App;
