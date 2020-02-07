using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerNewsScrapper.ConsoleApp
{
	public class HackerNewsScrapper
	{
		private readonly HttpClient client = new HttpClient
		{
			BaseAddress = new Uri("https://news.ycombinator.com/"),
		};

		public async Task<string> DownloadPosts(int count)
		{
			var toReturn = new List<Post>();
			var page = 0;
			while (count > 0)
			{
				var posts = (await GetPosts(++page)).ToList();
				var toTake = Math.Min(posts.Count, count);
				toReturn.AddRange(posts.Take(toTake));
				count -= toTake;
			}

			return Serialize(toReturn);
		}

		private async Task<IEnumerable<Post>> GetPosts(int pageNumber) =>
			ParsePosts(GetHtmlNodes(await this.DownloadContent(++pageNumber)));

		private async Task<string> DownloadContent(int page) =>
			await client.GetStringAsync($"/news?p={page}");

		private static HtmlNodeCollection GetHtmlNodes(string content)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);
			return htmlDoc.DocumentNode
				.SelectNodes("//body/center/table/tr")
				.Skip(2).First()
				.SelectNodes("./td/table/tr");
		}

		private static IEnumerable<Post> ParsePosts(HtmlNodeCollection nodes)
		{
			for (int i = 0; i < nodes.Count - 2; i++)
			{
				var main = nodes[i].SelectNodes("./td");
				var details = nodes[++i].SelectNodes("./td").Last();
				++i;
				var rank = Int32.Parse(main.First().SelectSingleNode("./span").InnerText.TrimEnd('.'));
				var title = main.Last().SelectSingleNode("./a").InnerText;
				var url = main.Last().SelectSingleNode("./a").Attributes["href"].Value;
				var points = Int32.Parse(details.SelectNodes("./span").First().InnerText.Split(' ')[0]);
				var author = details.SelectNodes("./a").First().InnerText;
				var anyComments = Int32.TryParse(details.SelectNodes("./a").Last().InnerText.Split('&')[0], out var comments);
				yield return new Post(title, author, url, rank, points, anyComments ? comments : default(int?));
			}
		}

		private static string Serialize(List<Post> toReturn) =>
			JsonSerializer.Serialize<IEnumerable<Post>>(
				toReturn,
				new JsonSerializerOptions
				{
					IgnoreNullValues = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					WriteIndented = true,
				});
	}
}
