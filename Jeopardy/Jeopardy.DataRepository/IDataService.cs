using Jeopardy.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jeopardy.DataAccess
{
    public interface IDataService
    {
        JeopardyEpisode GetJeopardyEpisodeByShowID(int showID);
        IEnumerable<JeopardyEpisode> GetAllEpisodes();
    }
}
