using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	public class Scraper
	{
		private readonly string hackerNewsBaseUrl;

		public Scraper(string baseAddress)
		{
			this.hackerNewsBaseUrl = baseAddress;
		}

		public async Task<IEnumerable<Post>> GetPosts(string content) => this.ParsePosts(await GetDocument(content));

		private static async Task<IHtmlCollection<IElement>> GetDocument(string content)
		{
			var doc = await BrowsingContext.New(Configuration.Default.WithDefaultLoader())
				.OpenAsync(req => req.Content(content));
			var nodes = doc.QuerySelectorAll("table.itemlist tr");

			if (nodes == null)
			{
				// todo: there should be more checking for failures in parsing
				// either as logging or additional status,
				// but that could get in the way of clean json output
				throw new ApplicationException("Could not parse content.");
			}

			return nodes;
		}

		private static bool TryParseTitle(
			IHtmlCollection<IElement> nodes,
			out string author)
		{
			var text = nodes.Last().QuerySelector("a").InnerHtml;
			if (string.IsNullOrWhiteSpace(text))
			{
				author = string.Empty;
				return false;
			}

			author = LimitString(text, 256);
			return true;
		}

		private static bool TryParseAuthor(
			IElement node,
			out string author)
		{
			var text = node.QuerySelectorAll("a").First().InnerHtml;
			if (string.IsNullOrWhiteSpace(text))
			{
				author = string.Empty;
				return false;
			}

			author = LimitString(text, 256);
			return true;
		}

		private static bool TryParseRank(IHtmlCollection<IElement> nodes, out int rank) =>
			int.TryParse(
				nodes.First().QuerySelector("span").InnerHtml.TrimEnd('.'),
				out rank);

		private static bool TryParsePoints(IElement node, out int points) =>
			int.TryParse(
				node.QuerySelectorAll("span").First().InnerHtml.Split(' ')[0],
				out points);

		// doesn't differentiate between not found node and no comments
		private static bool TryParseComments(IElement node, out int comments)
		{
			var commentsElement = node.QuerySelectorAll("a").Last().InnerHtml.Split('&');
			if (commentsElement.Length != 2)
			{
				comments = 0;
				return false;
			}

			return int.TryParse(
				node.QuerySelectorAll("a").Last().InnerHtml.Split('&')[0],
				out comments);
		}

		// nicer option would be to replace last characters with '...' in case of too long strings
		private static string LimitString(string text, int length) =>
			text.Substring(0, Math.Min(text.Length, length));

		private IEnumerable<Post> ParsePosts(IHtmlCollection<IElement> nodes)
		{
			// currently Hacker News lists 30 posts per page,
			// and there is some site formatting that needs to be handled
			var skipLastRows = 2;
			for (int i = 0; i < nodes.Length - skipLastRows; i += 3)
			{
				var main = nodes[i].QuerySelectorAll("td");
				var details = nodes[i + 1].QuerySelectorAll("td").Last();

				if (main == null || details == null ||
					!TryParseTitle(main, out var title) ||
					!this.TryParseUri(main, out var uri) ||
					!TryParseRank(main, out var rank) ||
					!TryParseAuthor(details, out var author) ||
					!TryParsePoints(details, out var points)) continue;

				var anyComments = TryParseComments(details, out var comments);
				yield return new Post(title, author, uri, rank, points, anyComments ? comments : default(int?));
			}
		}

		private bool TryParseUri(IHtmlCollection<IElement> nodes, out Uri uri)
		{
			var url = nodes.Last().QuerySelector("a").Attributes["href"].Value;
			if (Helpers.IsValidUri(url))
			{
				uri = new Uri(url);
				return true;
			}

			if (url.StartsWith("item?", StringComparison.OrdinalIgnoreCase) &&
				this.TryFromRelative(url, out var absolute))
			{
				uri = new Uri(absolute);
				return true;
			}

			uri = new Uri(this.hackerNewsBaseUrl);
			return false;
		}

		private bool TryFromRelative(string relative, out string absolute)
		{
			if (Helpers.IsValidUri(this.hackerNewsBaseUrl + relative))
			{
				absolute = this.hackerNewsBaseUrl + relative;
				return true;
			}
			else
			{
				absolute = this.hackerNewsBaseUrl;
				return false;
			}
		}
	}
}