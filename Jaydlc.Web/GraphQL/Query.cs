using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HotChocolate;
using Jaydlc.Core;
using Jaydlc.Core.Models;

namespace Jaydlc.Web.GraphQL
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "CA1822")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Query
    {
        public IEnumerable<VideoInfo> GetVideos(
            [Service] VideoManager videoManager) =>
            videoManager.Videos;
    }
}