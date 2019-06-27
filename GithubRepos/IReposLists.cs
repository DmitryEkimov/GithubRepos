using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GithubRepos
{
    public enum OrgOrUser { orgs, users }

    public enum sort { created, updated, pushed, full_name }

    public enum direction { asc, desc }

    public enum type { all, @public, @private, forks, sources, member }

    /// <summary>
    /// полезно для инверсии зависимостей
    /// </summary>
    public interface IReposLists
    {
        Task<IEnumerable<Repo>> GetRepoList(Enum orgOrUser, string name, params Enum[] optParams);
        Task<IEnumerable<Repo>> GetFilteredRepoList(Enum orgOrUser, string name, string filter, params Enum[] optParams);
    }
}
