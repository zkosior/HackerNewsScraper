using System;
using System.Threading.Tasks;

namespace HackerNewsScrapper.ConsoleApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var nodes = await new HackerNewsScrapper().DownloadPosts(100);

			Console.WriteLine(nodes);
		}
	}
}
