using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestGitHubAPI
{
    public class GitHubAPITests
    {
        private RestClient client;
        private RestRequest request;
        private string baseURL = "https://api.github.com"; 
        private string allIssuesURL = "/repos/steliyanbenev/postmantests/issues";
        private string singleIssueURL = "/repos/steliyanbenev/postmantests/issues/{id}";
        private string user = "PUT_YOUR_USER_HERE";
        private string token = "PUT_YOUR_TOKEN_HERE";


        [SetUp]
        public void Setup()
        {
            this.client = new RestClient(baseURL);
        }

        [Test]
        public async Task Test_Get_AllIssues()
        {
            //Arrange / Act
            this.request = new RestRequest(allIssuesURL);

            var response = await this.client.ExecuteAsync(this.request, Method.Get);

            var issues = JsonSerializer.Deserialize<List<Issue>>(response.Content);

            //Assert
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.That(issues.Count >= 1);

            foreach (var issue in issues)
            {
                Assert.That(issue.number > 0);
                Assert.That(issue.id > 0);
                Assert.IsNotEmpty(issue.title);
            }
        }

        [Test]
        public async Task Test_Get_Issue_By_Valid_Number()
        {
            // Arrange / Act
            this.request = new RestRequest(singleIssueURL);
            this.request.AddUrlSegment("id", 1);

            var response = await this.client.ExecuteAsync(this.request, Method.Get);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, issue.number);
            Assert.That(issue.id > 1);
            Assert.IsNotNull(issue.title);
        }

        [Test]
        public async Task Test_Get_Issue_By_Invalid_Number()
        {
            // Arrange / Act
            this.request = new RestRequest(singleIssueURL);
            this.request.AddUrlSegment("id", 16541515);

            var response = await this.client.ExecuteAsync(this.request, Method.Get);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_Post_With_Valid_Issue()
        {
            // Arrange 
            this.client.Authenticator = new HttpBasicAuthenticator(user, token);
            var request = new RestRequest(allIssuesURL);

            string title = "New issue from Restsharp";
            string body = "Some text here";

            // Act
            request.AddBody(new { body, title });
            var response = await this.client.ExecuteAsync(request, Method.Post);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.That(issue.number > 0);
            Assert.That(issue.id > 0);
            Assert.IsNotNull(issue.title);
        }

        [Test]
        public async Task Test_Post_With_Missing_Title_Issue()
        {
            // Arrange 
            this.client.Authenticator = new HttpBasicAuthenticator(user, token);
            var request = new RestRequest(allIssuesURL);

            
            string body = "Some text here";

            // Act
            request.AddBody(new { body });
            var response = await this.client.ExecuteAsync(request, Method.Post);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Test_Post_With_Missing_Authenticator()
        {
            // Arrange 
            var request = new RestRequest(allIssuesURL);

            string body = "Some text here";
            string title = "New issue from Restsharp";

            // Act
            request.AddBody(new { body });
            var response = await this.client.ExecuteAsync(request, Method.Post);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_Delete_With_Missing_Authenticator()
        {
            // Arrange 
            var request = new RestRequest(singleIssueURL);
            request.AddUrlSegment("id", 7);

            // Act
            var response = await this.client.ExecuteAsync(request, Method.Delete);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_Delete_With_Authenticator()
        {
            // Arrange 
            this.client.Authenticator = new HttpBasicAuthenticator(user, token);
            var request = new RestRequest(singleIssueURL);
            request.AddUrlSegment("id", 7);

            // Act
            var response = await this.client.ExecuteAsync(request, Method.Delete);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            //The issues cannot be deleted via GitHub API
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_Patch_With_Missing_Authenticator()
        {
            // Arrange 
            var request = new RestRequest(singleIssueURL);
            request.AddUrlSegment("id", 7);

            var newTitle = "Changed title from RestSharp";

            // Act
            request.AddBody(new { newTitle });
            var response = await this.client.ExecuteAsync(request, Method.Patch);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Test_Patch_With_Authenticator()
        {
            // Arrange 
            this.client.Authenticator = new HttpBasicAuthenticator(user, token);

            var request = new RestRequest(singleIssueURL);
            request.AddUrlSegment("id", 7);

            var newTitle = "Changed title from RestSharp";

            // Act
            request.AddJsonBody(new { title = "Changed title from RestSharp", body = "Body" });

            var response = await this.client.ExecuteAsync(request, Method.Patch);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(newTitle, issue.title);
        }
    }
}