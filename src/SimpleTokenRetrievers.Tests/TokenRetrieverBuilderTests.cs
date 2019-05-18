using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using static AssertNet.Moq.Assertions;
using static AssertNet.Xunit.Assertions;

namespace SimpleTokenRetrievers.Tests
{
    /// <summary>
    /// Test class for the <see cref="TokenRetrieverBuilder"/> class.
    /// </summary>
    public class TokenRetrieverBuilderTests
    {
        private const string QueryToken = "abc";
        private const string HeaderToken = "def";

        private readonly TokenRetrieverBuilder builder = new TokenRetrieverBuilder();
        private readonly Mock<HttpRequest> requestMock = new Mock<HttpRequest>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRetrieverBuilderTests"/> class.
        /// </summary>
        public TokenRetrieverBuilderTests()
        {
            Mock<IQueryCollection> queryMock = new Mock<IQueryCollection>();
            Mock<IHeaderDictionary> headerMock = new Mock<IHeaderDictionary>();

            requestMock.Setup(x => x.Query).Returns(queryMock.Object);
            requestMock.Setup(x => x.Headers).Returns(headerMock.Object);

            StringValues values;
            queryMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out values))
                .Callback(new TryGetValueDelegate((string key, out StringValues values) => values = QueryToken))
                .Returns(true);

            headerMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out values))
                .Callback(new TryGetValueDelegate((string key, out StringValues values) => values = $"{builder.Scheme} {HeaderToken}"))
                .Returns(true);
        }

        private delegate void TryGetValueDelegate(string key, out StringValues values);

        /// <summary>
        /// Checks that the default values are correct.
        /// </summary>
        [Fact]
        public void DefaultValues()
        {
            AssertThat(builder.AcceptsFromAuthenticationHeader).IsFalse();
            AssertThat(builder.AcceptsFromQueryString).IsFalse();
            AssertThat(builder.Scheme).IsEqualToIgnoringCase("bearer");
            AssertThat(builder.QueryParameter).IsEqualTo("access_token");
        }

        /// <summary>
        /// Checks that we can enable query string retrieval.
        /// </summary>
        [Fact]
        public void EnableQueryString()
        {
            builder.FromQueryString();
            AssertThat(builder.AcceptsFromQueryString).IsTrue();
        }

        /// <summary>
        /// Checks that we can enable authentication header retrieval.
        /// </summary>
        [Fact]
        public void EnableAuthenticationHeader()
        {
            builder.FromAuthenticationHeader();
            AssertThat(builder.AcceptsFromAuthenticationHeader).IsTrue();
        }

        /// <summary>
        /// Checks that we can correctly set the scheme.
        /// </summary>
        [Fact]
        public void ChangeScheme()
        {
            builder.WithScheme("helloscheme");
            AssertThat(builder.Scheme).IsEqualTo("helloscheme");
        }

        /// <summary>
        /// Checks that we can correctly set the query parameter.
        /// </summary>
        [Fact]
        public void ChangeParameter()
        {
            builder.WithQueryParameter("helloparameter");
            AssertThat(builder.QueryParameter).IsEqualTo("helloparameter");
        }

        /// <summary>
        /// Checks that we can't retrieve a token if nothing is enabled.
        /// </summary>
        [Fact]
        public void NoneEnabled()
        {
            AssertThat(builder.Build()(requestMock.Object)).IsNullOrEmpty();
        }

        /// <summary>
        /// Checks that we can get the token from the query string.
        /// </summary>
        [Fact]
        public void QueryEnabled()
        {
            AssertThat(builder.FromQueryString().Build()(requestMock.Object)).IsEqualTo(QueryToken);
        }

        /// <summary>
        /// Checks that we can get the token from the query string.
        /// </summary>
        [Fact]
        public void HeaderEnabled()
        {
            AssertThat(builder.FromAuthenticationHeader().Build()(requestMock.Object)).IsEqualTo(HeaderToken);
        }
    }
}
