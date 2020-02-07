namespace HackerNewsScrapper.ConsoleApp
{
	public class Post
	{
		public Post(
			string title,
			string author,
			string uri,
			int rank,
			int points,
			int? comments)
		{
			this.Title = title;
			this.Author = author;
			this.Uri = uri;
			this.Rank = rank;
			this.Points = points;
			this.Comments = comments;
		}

		public string Title { get; }

		public string Author { get; }

		public string Uri { get; }

		public int Rank { get; }

		public int Points { get; }

		public int? Comments { get; }
	}
}
