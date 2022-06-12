import { Component } from 'react'

export interface QueryResultsProps {
  columns: string[]
  values: any[][]
}

class QueryResults extends Component<QueryResultsProps> {
  renderRow(values: any[], index: number) {
    return (
      <tr key={index}>
        {values.map((x, i) => (<td key={i}>{x}</td>))}
      </tr>
    );
  }

  renderHeader(columns: string[]) {
    return (
      <tr>
        {columns.map((x, i) => (<th key={i}>{x}</th>))}
      </tr>
    );
  }

  render() {
    return (
      <table>
        <thead>
          {this.renderHeader(this.props.columns)}
        </thead>
        <tbody>
          {this.props.values.map(this.renderRow)}
        </tbody>
      </table>
    );
  }
}

export default QueryResults
