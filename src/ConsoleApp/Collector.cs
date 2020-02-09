using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	public static class Collector
	{
		public static async Task<string> Collect(
			Client client,
			Scraper scraper,
			int count)
		{
			var toReturn = new List<Post>();
			var page = 0;
			while (count > 0)
			{
				List<Post> posts;
				try
				{
					// pages are numbered sequentially
					var content = await client.DownloadContent(++page);
					posts = scraper.GetPosts(content).ToList();
				}
				catch (ApplicationException e)
				{
					return e.Message;
				}

				var toTake = Math.Min(posts.Count, count);
				toReturn.AddRange(posts.Take(toTake));
				count -= toTake;
			}

			return Helpers.Serialize(toReturn);
		}
	}
}
