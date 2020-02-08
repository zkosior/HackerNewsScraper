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
		private const string HackerNewsBaseUrl = "https://news.ycombinator.com/";
		private readonly HttpClient client = new HttpClient
		{
			BaseAddress = new Uri(HackerNewsBaseUrl),
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
			ParsePosts(GetHtmlNodes(await this.DownloadContent(pageNumber)));

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
			for (int i = 0; i < nodes.Count - 2; i+=3)
			{
				var main = nodes[i].SelectNodes("./td");
				var details = nodes[i+1].SelectNodes("./td").Last();

				if (!ParseTitle(main, out var title) ||
					!ParseUri(main, out var uri) ||
					!ParseRank(main, out var rank) ||
					!ParseAuthor(details, out var author) ||
					!ParsePoints(details, out var points)) continue;

				var anyComments = ParseComments(details, out var comments);
				yield return new Post(title, author, uri, rank, points, anyComments ? comments : default(int?));
			}
		}

		private static bool ParseTitle(
			HtmlNodeCollection nodes,
			out string author)
		{
			var text = nodes.Last().SelectSingleNode("./a").InnerText;
			if (string.IsNullOrWhiteSpace(text))
			{
				author = string.Empty;
				return false;
			}

			author = LimitString(text, 256);
			return true;
		}

		private static bool ParseAuthor(
			HtmlNode node,
			out string author)
		{
			var text = node.SelectNodes("./a").First().InnerText;
			if (string.IsNullOrWhiteSpace(text))
			{
				author = string.Empty;
				return false;
			}

			author = LimitString(text, 256);
			return true;
		}

		private static bool ParseUri(HtmlNodeCollection nodes, out Uri uri)
		{
			var url = nodes.Last().SelectSingleNode("./a").Attributes["href"].Value;
			if (string.IsNullOrWhiteSpace(url))
			{
				uri = new Uri(HackerNewsBaseUrl);
				return false;

			}
			try
			{
				uri = new Uri(
					url.StartsWith("item?")
					? HackerNewsBaseUrl + url
					: url);
				return true;
			}
			catch (UriFormatException)
			{
				uri = new Uri(HackerNewsBaseUrl);
				return false;
			}
		}

		private static bool ParseRank(HtmlNodeCollection nodes, out int rank) =>
			int.TryParse(
				nodes.First().SelectSingleNode("./span").InnerText.TrimEnd('.'),
				out rank);

		private static bool ParsePoints(HtmlNode node, out int points) =>
			int.TryParse(
				node.SelectNodes("./span").First().InnerText.Split(' ')[0],
				out points);

		private static bool ParseComments(HtmlNode node, out int comments) =>
			int.TryParse(
				node.SelectNodes("./a").Last().InnerText.Split('&')[0],
				out comments);

		private static string LimitString(string text, int length) =>
			text.Substring(0, Math.Min(text.Length, length));

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
