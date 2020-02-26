using HackerNewsScraper.ConsoleApp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HackerNewsScraper.ConsoleAppTests
{
	public class ScrapperTests
	{
		private const string Mappings = @"..\..\..\..\..\test\wiremock.net\mappings\";
		private const string BaseAddress = "https://news.ycombinator.com/"; // any address, doesn't have to be HN

		[Fact]
		public async Task ConvertsHtmlToObjects()
		{
			var nodes = await LoadPage1();

			Assert.NotEmpty(nodes);
			Assert.Equal(30, nodes.Count);
		}

		[Fact]
		public async Task LoadsRelativeAsBaseAddress() =>
			Assert.StartsWith(
				BaseAddress,
				(await LoadPage1()).Single(p => p.Rank == 29).Uri.AbsoluteUri,
				System.StringComparison.Ordinal);

		[Fact]
		public async Task IgnoresOnMissingUri() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 1);

		[Fact]
		public async Task IgnoresOnInvalidUri() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 2);

		[Fact]
		public async Task IgnoresOnMissingRank() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 3);

		[Fact]
		public async Task IgnoresOnMissingAuthor() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 4);

		[Fact]
		public async Task IgnoresOnMissingTitle() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 5);

		[Fact]
		public async Task IgnoresOnMissingPoints() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 6);

		[Fact]
		public async Task IgnoresOnInvalidPoints() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 7);

		[Fact]
		public async Task IgnoresOnInvalidRank() =>
			Assert.DoesNotContain(await LoadInvalidData(), p => p.Rank == 8);

		[Fact]
		public async Task IgnoresMissingComments() =>
			Assert.Null((await LoadInvalidData()).Single(p => p.Rank == 9).Comments);

		[Fact]
		public async Task IgnoresInvalidComments() =>
			Assert.Null((await LoadInvalidData()).Single(p => p.Rank == 10).Comments);

		[Fact]
		public async Task ShortensTitle() =>
			Assert.Equal(256, (await LoadInvalidData()).Single(p => p.Rank == 11).Title.Length);

		[Fact]
		public async Task ShortensAuthor() =>
			Assert.Equal(256, (await LoadInvalidData()).Single(p => p.Rank == 12).Author.Length);

		private static async Task<List<Post>> LoadPage1() => await LoadPage("Page1.html");

		private static async Task<List<Post>> LoadInvalidData() => await LoadPage("InvalidData.html");

		private static async Task<List<Post>> LoadPage(string file) =>
			(await new Scraper(BaseAddress)
			.GetPosts(File.ReadAllText($"{Mappings}{file}")))
			.ToList();
	}
}
