using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace HackerNewsScrapper.ConsoleApp
{
	class Program
	{
		static async Task Main(params string[] args)
		{
			RootCommand root = new RootCommand("Scrapps Hacker News page for posts metadata.")
			{
				new Option(
					new string[] { "--posts", "-p" },
					"How many posts to print. A positive integer <= 100.")
				{
					Argument = new Argument<int>(),
				},
			};

			root.Handler = CommandHandler.Create<int>(DownloadAndPrint);
			await root.InvokeAsync(args);
		}

		private static async Task DownloadAndPrint(int posts = 0)
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

			var nodes = await new HackerNewsScrapper().DownloadPosts(posts);

			Console.WriteLine(nodes);
		}
	}
}
