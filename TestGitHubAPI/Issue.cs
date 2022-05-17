using RestSharp;
using System.Threading.Tasks;

namespace TestGitHubAPI
{
    public class Issue
    {
        public int id { get; set; }
        public long number { get; set; }
        public string title { get; set; }
        public string body { get; set; }

    }
}