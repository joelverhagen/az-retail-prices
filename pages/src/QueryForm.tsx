import { Component, FormEvent } from 'react'
import QueryResults, { QueryResultsProps } from './QueryResults';
import { execute } from './QueryRunner'
import * as queryString from 'query-string'

interface QueryFormProps {
  query: string
}

interface QueryFormState {
  loading: boolean
  query: string
  results?: QueryResultsProps
  errorReason?: any
}

class QueryForm extends Component<QueryFormProps, QueryFormState> {
  constructor(props: QueryFormProps) {
    super(props);

    this.state = {
      loading: false,
      query: props.query
    }

    this.handleChange = this.handleChange.bind(this)
    this.handleSubmit = this.handleSubmit.bind(this)
  }

  handleSubmit(event: FormEvent) {
    event.preventDefault()
    this.executeQuery()
  }

  executeQuery() {
    if (this.state.loading) {
      return
    }

    this.setState({ loading: true })
    this.setQueryString()

    execute(this.state.query).then(allResults => {

      this.setState({
        query: this.state.query,
        results: allResults[0],
        errorReason: undefined
      })
    }).catch(reason => {
      this.setState({ results: undefined, errorReason: reason })
    }).finally(() => this.setState({ loading: false }))
  }

  setQueryString() {
    const parsedQueryString = queryString.parse(document.location.search);
    parsedQueryString.query = this.state.query;
    const newQueryString = queryString.stringify(parsedQueryString);
    window.history.replaceState('', '', '?' + newQueryString);
  }

  handleChange(event: FormEvent<HTMLTextAreaElement>) {
    this.setState({ query: event.currentTarget.value })
  }

  renderLoading() {
    return (
      <span className="loading">
        {this.state.loading ? "⌛" : ""}
      </span>
    )
  }

  renderResults() {
    if (this.state.results) {
      return (<QueryResults columns={this.state.results.columns} values={this.state.results.values} />)
    }
  }

  renderError() {
    if (this.state.errorReason) {
      return (<div>⚠️ {this.state.errorReason.toString()}</div>)
    }
  }

  componentDidMount() {
    if (this.state.query) {
      this.executeQuery()
    }
  }

  render() {
    return (
      <>
        <form onSubmit={this.handleSubmit}>
          <textarea name="name" value={this.state.query} onChange={this.handleChange} />
          <br />
          <input type="submit" value="Execute" disabled={this.state.loading} />
          {this.renderLoading()}
        </form>
        {this.renderResults()}
        {this.renderError()}
      </>
    )
  }
}

export default QueryForm
