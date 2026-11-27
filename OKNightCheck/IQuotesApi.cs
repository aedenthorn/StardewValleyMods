namespace OKNightCheck
{
    public interface IQuotesApi
    {
        public string[] GetRandomQuoteAndAuthor(bool makeLines = false);
    }
}