using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HackerNewsScraper.ConsoleApp
{
	public class Scraper
	{
		private readonly string hackerNewsBaseUrl;

		public Scraper(string baseAddress)
		{
			this.hackerNewsBaseUrl = baseAddress;
		}

		public IEnumerable<Post> GetPosts(string content) => ParsePosts(GetHtmlNodes(content));

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

		private IEnumerable<Post> ParsePosts(HtmlNodeCollection nodes)
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

		private bool TryParseUri(HtmlNodeCollection nodes, out Uri uri)
		{
			var url = nodes.Last().SelectSingleNode("./a").Attributes["href"].Value;
			if (Helpers.IsValidUri(url))
			{
				uri = new Uri(url);
				return true;
			}

			if (url.StartsWith("item?") && TryFromRelative(url, out var absolute))
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

		private static bool TryParseRank(HtmlNodeCollection nodes, out int rank) =>
			int.TryParse(
				nodes.First().SelectSingleNode("./span").InnerText.TrimEnd('.'),
				out rank);

		private static bool TryParsePoints(HtmlNode node, out int points) =>
			int.TryParse(
				node.SelectNodes("./span").First().InnerText.Split(' ')[0],
				out points);

		// doesn't differentiate between not found node and no comments
		private static bool TryParseComments(HtmlNode node, out int comments)
		{
			var commentsElement = node.SelectNodes("./a").Last().InnerText.Split('&');
			if (commentsElement.Length != 2)
			{
				comments = 0;
				return false;
			}

			return int.TryParse(
				node.SelectNodes("./a").Last().InnerText.Split('&')[0],
				out comments);
		}

		// nicer option would be to replace last characters with '...' in case of too long strings
		private static string LimitString(string text, int length) =>
			text.Substring(0, Math.Min(text.Length, length));
	}
}