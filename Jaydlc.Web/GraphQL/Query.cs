using System.Collections.Generic;
using HotChocolate;
using Jaydlc.Core;
using Jaydlc.Core.Models;

namespace Jaydlc.Web.GraphQL
{
    public class Query
    {
        public IEnumerable<VideoInfo>? GetVideos([Service] VideoManager videoManager) =>
            videoManager.Videos;
    }
}