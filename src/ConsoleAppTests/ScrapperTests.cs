using HackerNewsScraper.ConsoleApp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace HackerNewsScraper.ConsoleAppTests
{
	public class ScrapperTests
	{
		private const string Mappings = @"..\..\..\..\..\test\wiremock.net\mappings\";
		private const string BaseAddress = "https://news.ycombinator.com/"; // any address, doesn't have to be HN

		[Fact]
		public void ConvertsHtmlToObjects()
		{
			var nodes = LoadPage1();

			Assert.NotEmpty(nodes);
			Assert.Equal(30, nodes.Count);
		}

		[Fact]
		public void LoadsRelativeAsBaseAddress() =>
			Assert.StartsWith(
				BaseAddress,
				LoadPage1().Single(p => p.Rank == 29).Uri.AbsoluteUri,
				System.StringComparison.Ordinal);

		[Fact]
		public void IgnoresOnMissingUri() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 1);

		[Fact]
		public void IgnoresOnInvalidUri() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 2);

		[Fact]
		public void IgnoresOnMissingRank() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 3);

		[Fact]
		public void IgnoresOnMissingAuthor() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 4);

		[Fact]
		public void IgnoresOnMissingTitle() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 5);

		[Fact]
		public void IgnoresOnMissingPoints() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 6);

		[Fact]
		public void IgnoresOnInvalidPoints() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 7);

		[Fact]
		public void IgnoresOnInvalidRank() =>
			Assert.DoesNotContain(LoadInvalidData(), p => p.Rank == 8);

		[Fact]
		public void IgnoresMissingComments() =>
			Assert.Null(LoadInvalidData().Single(p => p.Rank == 9).Comments);

		[Fact]
		public void IgnoresInvalidComments() =>
			Assert.Null(LoadInvalidData().Single(p => p.Rank == 10).Comments);

		[Fact]
		public void ShortensTitle() =>
			Assert.Equal(256, LoadInvalidData().Single(p => p.Rank == 11).Title.Length);

		[Fact]
		public void ShortensAuthor() =>
			Assert.Equal(256, LoadInvalidData().Single(p => p.Rank == 12).Author.Length);

		private static List<Post> LoadPage1() => LoadPage("Page1.html");

		private static List<Post> LoadInvalidData() => LoadPage("InvalidData.html");

		private static List<Post> LoadPage(string file) =>
			new Scraper(BaseAddress)
			.GetPosts(File.ReadAllText($"{Mappings}{file}"))
			.ToList();
	}
}
