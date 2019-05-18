using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SimpleTokenRetrievers
{
    /// <summary>
    /// Builder for creating different token retrievers.
    /// </summary>
    public class TokenRetrieverBuilder
    {
        /// <summary>
        /// Gets the name of the query parameter.
        /// </summary>
        public string QueryParameter { get; private set; } = "access_token";

        /// <summary>
        /// Gets the name of the scheme.
        /// </summary>
        public string Scheme { get; private set; } = "bearer";

        /// <summary>
        /// Gets a value indicating whether the token retriever accepts tokens from the authentication header.
        /// </summary>
        public bool AcceptsFromAuthenticationHeader { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether the token retriever accepts tokens from the query string.
        /// </summary>
        public bool AcceptsFromQueryString { get; private set; } = false;

        /// <summary>
        /// Sets the scheme name being used for authentication.
        /// </summary>
        /// <param name="scheme">The scheme name.</param>
        /// <returns>The token retriever builder.</returns>
        public TokenRetrieverBuilder WithScheme(string scheme)
        {
            Scheme = scheme;
            return this;
        }

        /// <summary>
        /// Sets the query parameter name used to retrieve the token from the query string.
        /// </summary>
        /// <param name="parameter">The query parameter name.</param>
        /// <returns>The token retriever builder.</returns>
        public TokenRetrieverBuilder WithQueryParameter(string parameter)
        {
            QueryParameter = parameter;
            return this;
        }

        /// <summary>
        /// Indicates that the access token may be retrieved from the query string.
        /// </summary>
        /// <returns>The token retriever builder.</returns>
        public TokenRetrieverBuilder FromQueryString()
        {
            AcceptsFromQueryString = true;
            return this;
        }

        /// <summary>
        /// Indicates that the access token may be retrieved from the authentication header.
        /// </summary>
        /// <returns>The token retriever builder.</returns>
        public TokenRetrieverBuilder FromAuthenticationHeader()
        {
            AcceptsFromAuthenticationHeader = true;
            return this;
        }

        /// <summary>
        /// Builds the token retriever.
        /// </summary>
        /// <returns>The token retriever.</returns>
        public Func<HttpRequest, string> Build()
        {
            return (request) =>
            {
                if (AcceptsFromAuthenticationHeader && TryGetHeaderValue(request, out string headerValue))
                {
                    return headerValue;
                }

                if (AcceptsFromQueryString && TryGetQueryValue(request, out string queryValue))
                {
                    return queryValue;
                }

                return null;
            };
        }

        /// <summary>
        /// Tries to get the access token from the authentication header.
        /// </summary>
        /// <param name="request">The request to get the heades value from.</param>
        /// <param name="headerValue">The variable to write the result to.</param>
        /// <returns>True if the header value existed and could be returned.</returns>
        private bool TryGetHeaderValue(HttpRequest request, out string headerValue)
        {
            if (request.Headers.TryGetValue("Authorization", out StringValues values) && values.Count > 0)
            {
                string value = values[0];

                if (value.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    headerValue = value.Substring(Scheme.Length).Trim();
                    return true;
                }
            }

            headerValue = null;
            return false;
        }

        /// <summary>
        /// Tries to get the access token from the authentication header.
        /// </summary>
        /// <param name="request">The request to get the heades value from.</param>
        /// <param name="queryValue">The variable to write the result to.</param>
        /// <returns>True if the header value existed and could be returned.</returns>
        private bool TryGetQueryValue(HttpRequest request, out string queryValue)
        {
            if (request.Query.TryGetValue(QueryParameter, out StringValues values) && values.Count > 0)
            {
                queryValue = values[0];
                return true;
            }

            queryValue = null;
            return false;
        }
    }
}
