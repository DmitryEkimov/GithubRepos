using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GithubRepos
{
    /// <summary>
    /// API получения списка репозиторий Github
    /// </summary>
    public class ReposLists : IReposLists
    {
        const string UriString = "https://api.github.com/";
        const string AcceptValue = "application/vnd.github.v3+json";
        const string UserAgent = "GithubRepos-APIClient";
        const string orgUserError = "Первый параметр должен быть типа enum OrgOrUser";
        const string nameError = "Второй параметр не должен быть пустым";
        const string optParamWrongType = "Опциональный параметр должен быть одного из разрешенных типов";
        const string optParamDuplicate = "Параметр нельзя передавать дважды";

        #region конструктор
        readonly IHttpClientFactory _clientFactory;

        /// <summary>
        ///  IHttpClientFactory чтобы преодолевать проблемы HttpClient и обеспечить мокабельность
        /// </summary>
        /// <param name="clientFactory"></param>
        public ReposLists(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory)); 
        }
        #endregion

        #region необязательные параметры вывода данных

        static HashSet<Type> AllowedTypes = new HashSet<Type>() { typeof(direction), typeof(type), typeof(sort) };

        #endregion

        public Task<IEnumerable<Repo>> GetRepoList(Enum orgOrUser, string name, params Enum[] optParams) => GetFilteredRepoList(orgOrUser, name, string.Empty, optParams);

        public async Task<IEnumerable<Repo>> GetFilteredRepoList(Enum orgOrUser,string name, string filter, params Enum[] optParams)
        {
            #region разбор обязательных параметров
            if (!(orgOrUser is OrgOrUser repoType))
                throw new ArgumentException(nameof(orgOrUser), orgUserError);
            var repoTypeString = repoType.ToString();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), nameError);
            #endregion

            #region разбор необязательных параметров
            var parameters = new List<KeyValuePair<string, string>>();
            HashSet<Type> settedparams = new HashSet<Type>();
            for (int i = 0; i < optParams.Length; i++)
            {
                var optParam = optParams[i];
                var optParamType = optParam.GetType();
                if (!AllowedTypes.Contains(optParamType))
                    throw new ArgumentException(nameof(optParams), optParamWrongType);
                if(settedparams.Contains(optParamType))
                    throw new ArgumentException(nameof(optParams), optParamDuplicate);
                parameters.Add(new KeyValuePair<string, string>(optParamType.ToString(), optParam.ToString()));
                settedparams.Add(optParamType);
            }
            var paramString = parameters.Count==0? string.Empty:"?"+await(new FormUrlEncodedContent(parameters)).ReadAsStringAsync();
            #endregion

            #region формируем запрос и вызываем
            IEnumerable<Repo> repos = null;
            using (var client = _clientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(UriString);
                client.Timeout = TimeSpan.FromMinutes(1);
                // Github API versioning
                client.DefaultRequestHeaders.Add("Accept", AcceptValue);
                // Github requires a user-agent
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

                var response = await client.GetAsync($" /{repoTypeString}/{name}/repos{paramString}");
                response.EnsureSuccessStatusCode();
                repos = await response.Content.ReadAsJsonAsync<Repo[]>();
            }
            #endregion

            // в постановке задачи не уточнялось по каким именам фильтровать репозитарии
            return filter==string.Empty? repos : repos.Where(r=>r.name==filter);
        }
    }
}
