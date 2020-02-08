using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	public class HackerNewsScraper
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
				List<Post> posts;
				try
				{
					posts = (await GetPosts(++page)).ToList();
				}
				catch (ApplicationException e)
				{
					return e.Message;
				}

				var toTake = Math.Min(posts.Count, count);
				toReturn.AddRange(posts.Take(toTake));
				count -= toTake;
			}

			return Serialize(toReturn);
		}

		private async Task<IEnumerable<Post>> GetPosts(int pageNumber) =>
			ParsePosts(GetHtmlNodes(await this.DownloadContent(pageNumber)));

		private async Task<string> DownloadContent(int page)
		{
			try
			{
				return await client.GetStringAsync($"/news?p={page}");
			}
			catch (HttpRequestException)
			{
				// checking status code might work better here
				throw new ApplicationException("Could not download posts.");
			}
		}

		private static HtmlNodeCollection GetHtmlNodes(string content)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);
			// this code is not resilient to layout changes
			// as an example skipping first two rows
			var nodes = htmlDoc.DocumentNode
				.SelectNodes("//body/center/table/tr")
				.Skip(2).First()
				.SelectNodes("./td/table/tr");

			if (nodes == null)
			{
				// todo: there should be more checking for failures in parsing
				// either as logging or additional status,
				// but that could get in the way of clean json output
				throw new ApplicationException("Could not parse content.");
			}

			return nodes;
		}

		private static IEnumerable<Post> ParsePosts(HtmlNodeCollection nodes)
		{
			// currently Hacker News lists 30 posts per page,
			// and there is some site formatting that needs to be handled
			var skipLastRows = 2;
			for (int i = 0; i < nodes.Count - skipLastRows; i += 3)
			{
				var main = nodes[i].SelectNodes("./td");
				var details = nodes[i + 1].SelectNodes("./td").Last();

				if (main == null || details == null ||
					!TryParseTitle(main, out var title) ||
					!TryParseUri(main, out var uri) ||
					!TryParseRank(main, out var rank) ||
					!TryParseAuthor(details, out var author) ||
					!TryParsePoints(details, out var points)) continue;

				var anyComments = TryParseComments(details, out var comments);
				yield return new Post(title, author, uri, rank, points, anyComments ? comments : default(int?));
			}
		}

		private static bool TryParseTitle(
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

		private static bool TryParseAuthor(
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

		private static bool TryParseUri(HtmlNodeCollection nodes, out Uri uri)
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

		private static bool TryParseRank(HtmlNodeCollection nodes, out int rank) =>
			int.TryParse(
				nodes.First().SelectSingleNode("./span").InnerText.TrimEnd('.'),
				out rank);

		private static bool TryParsePoints(HtmlNode node, out int points) =>
			int.TryParse(
				node.SelectNodes("./span").First().InnerText.Split(' ')[0],
				out points);

		// doesn't differentiate between not found node and no comments
		private static bool TryParseComments(HtmlNode node, out int comments) =>
			int.TryParse(
				node.SelectNodes("./a").Last().InnerText.Split('&')[0],
				out comments);

		// nicer option would be to replace last characters with '...' in case of too long strings
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