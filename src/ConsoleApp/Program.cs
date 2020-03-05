using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace HackerNewsScraper.ConsoleApp
{
	internal class Program
	{
		private static async Task Main(params string[] args)
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
				new Option(
					new string[] { "--address", "-a" },
					"Base address to Hacker News.")
				{
					Argument = new Argument<string>(),
					Required = false,
				},
			};

			root.Handler = CommandHandler.Create<string, int>(DownloadAndPrint);
			await root.InvokeAsync(args);
		}

		private static async Task DownloadAndPrint(string? address, int posts)
		{
			if (!Helpers.ValidateAddress(address, out var validatedAddress) ||
				!Helpers.ValidatePostsCount(posts, out var validatedPosts))
			{
				return;
			}

			using var client = new Client(validatedAddress!);
			var nodes = await Collector.Collect(
				client,
				new Scraper(validatedAddress!),
				validatedPosts);

			Console.WriteLine(nodes);
		}
	}
}
