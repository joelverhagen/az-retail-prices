import './App.css';
import QueryForm from './QueryForm';
import * as queryString from 'query-string'
import { DbType } from './QueryRunner';

function App() {
  let parsedQueryString = queryString.parse(document.location.search)
  let initialQuery = parsedQueryString.query
  if (Array.isArray(initialQuery)) {
    initialQuery = initialQuery[0]
  }

  let denormalized = parsedQueryString.denormalized
  if (Array.isArray(denormalized)) {
    denormalized = denormalized[0]
  }

  return (
    <div className="app">
      <section className="app-body">
        <QueryForm query={initialQuery ?? ""} dbType={denormalized == 'true' ? DbType.Denormalized : DbType.Normalized} />
      </section>
    </div>
  );
}

export default App;
