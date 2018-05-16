using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Jeopardy.Model;

namespace Jeopardy.DataAccess
{
    public class FileDataService : IDataService
    {
        public IEnumerable<JeopardyEpisode> GetAllEpisodes()
        {

            return null;

            string GetFileContents(int showID)
            {
                var asm = GetType().GetTypeInfo().Assembly;
                using (var stream = asm.GetManifestResourceStream($"Jeopardy.Tests.HtmlFiles.{showID}.html"))
                {
                    if (stream != null)
                    {
                        var reader = new StreamReader(stream);
                        return reader.ReadToEnd();
                    }
                }
                return string.Empty;
            }
        }

        public JeopardyEpisode GetJeopardyEpisodeByShowID(int showID)
        {
            JeopardyEpisode game = new JeopardyEpisode();            

            return game;
        }
       
    }
}
