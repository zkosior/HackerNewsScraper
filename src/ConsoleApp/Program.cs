using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	class Program
	{
		static async Task Main(params string[] args)
		{
			RootCommand root = new RootCommand("Scraps Hacker News page for posts metadata.")
			{
				new Option(
					new string[] { "--posts", "-p" },
					"How many posts to print. A positive integer <= 100.")
				{
					Argument = new Argument<int>(),
					Required = true,
				},
			};

			root.Handler = CommandHandler.Create<int>(DownloadAndPrint);
			await root.InvokeAsync(args);
		}

		private static async Task DownloadAndPrint(int posts)
		{
			if (posts <= 0)
			{
				Console.WriteLine("Nothing to download.");
				return;
			}

			if (posts > 100)
			{
				Console.WriteLine("Too much to download. Limiting to 100.");
				posts = 100;
			}

			var baseAddress = "https://news.ycombinator.com/";
			var client = new HNClient(baseAddress);
			var scraper = new HackerNewsScraper(baseAddress);
			var nodes = await DownloadPosts(client, scraper, posts);

			Console.WriteLine(nodes);
		}

		public static async Task<string> DownloadPosts(
			HNClient client,
			HackerNewsScraper scraper,
			int count)
		{
			var toReturn = new List<Post>();
			var page = 0;
			while (count > 0)
			{
				List<Post> posts;
				try
				{
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

			return Serialize(toReturn);
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
