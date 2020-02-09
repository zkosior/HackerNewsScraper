using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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
			if (address == null)
			{
				address = "https://news.ycombinator.com/";
			}
			else
			{
				if (!Helpers.IsValidUri(address))
				{
					Console.WriteLine("Not valid Uri.");
					return;
				}
			}

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

			var nodes = await Collector.Collect(
				new Client(address!),
				new Scraper(address!),
				posts);

			Console.WriteLine(nodes);
		}
	}
}
