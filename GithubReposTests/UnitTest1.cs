using GithubRepos;
using System;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using System.Linq;

namespace GithubReposTests
{

    internal class DefaultHttpClientFactory : IHttpClientFactory {

        HttpClient IHttpClientFactory.CreateClient(string name)
        {
            return new HttpClient();
        }
    }

    public class UnitTest1 
    {
        ReposLists _ReposLists = null;

        public UnitTest1()
        {
            var httpClientFactory = new DefaultHttpClientFactory();
            _ReposLists = new ReposLists(httpClientFactory);
        }

        [Fact]
        public async Task Test1()
        {
            var res = await _ReposLists.GetRepoList(OrgOrUser.users, "DmitryEkimov");
            Assert.NotNull(res);
            Assert.NotEmpty(res);
            Assert.Equal<int>(4, res.Count());
        }

        [Fact]
        public async Task Test2()
        {
            var reposLists = A.Fake<IReposLists>();
            A.CallTo(() => reposLists.GetRepoList(direction.asc, "DmitryEkimov"))
             .ThrowsAsync(new ArgumentException("orgOrUser", "Первый параметр должен быть типа enum OrgOrUser"));
        }


        [Fact]
        public async Task Test3()
        {
            var reposLists = A.Fake<IReposLists>();
            A.CallTo(() => reposLists.GetRepoList(OrgOrUser.users, null))
             .ThrowsAsync(new ArgumentNullException("name", "Второй параметр не должен быть пустым"));
        }

        [Fact]
        public async Task Test4()
        {
            var res = await _ReposLists.GetFilteredRepoList(OrgOrUser.users, "DmitryEkimov", "nlog-exception-dialog");
            Assert.NotNull(res);
            Assert.NotEmpty(res);
            Assert.Single(res);
        }
    }
}
