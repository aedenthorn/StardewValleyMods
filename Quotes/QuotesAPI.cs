using System.Collections.Generic;

namespace Quotes
{
    public class QuotesAPI
    {
        public string[] GetRandomQuoteAndAuthor(bool makeLines = false)
        {
            var quote = ModEntry.GetAQuote(true);
            if (!makeLines)
            {
                return new string[] { quote.quote, quote.author };
            }
            List<string> result = new List<string>(quote.quoteLines);
            result.Add(quote.author);
            return result.ToArray();
        }
    }
}