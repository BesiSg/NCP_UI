using HtmlAgilityPack;

namespace Utility.Lib.Wiki
{
    public class WikiParser : BaseUtility
    {
        public static List<CustomerAbbreviation> ParseCustomerTableList(string storageHtml)
        {
            var result = new List<CustomerAbbreviation>();

            var doc = new HtmlDocument();
            doc.LoadHtml(storageHtml);

            var table = doc.DocumentNode.SelectSingleNode("//table");
            if (table == null)
                return result;

            var rows = table.SelectNodes(".//tr");
            if (rows == null)
                return result;

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                var cells = row.SelectNodes("./td");
                if (cells == null || cells.Count < 6)
                    continue;

                string GetText(HtmlNode node)
                {
                    return HtmlEntity.DeEntitize(node.InnerText).Trim();
                }

                result.Add(new CustomerAbbreviation
                {
                    Customer = GetText(cells[0]),
                    City = GetText(cells[1]),
                    Country = GetText(cells[2]),
                    Abbreviation = GetText(cells[3]),
                    PatchBranch = GetText(cells[4]),
                    Notes = GetText(cells[5])
                });
            }

            return result;
        }

        public static Dictionary<string, CustomerAbbreviation> ParseCustomerTableDict(
            string storageHtml)
        {
            var result = new Dictionary<string, CustomerAbbreviation>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in ParseCustomerTableList(storageHtml))
            {
                if (!string.IsNullOrWhiteSpace(item.Abbreviation))
                {
                    result.TryAdd(item.Abbreviation, item);
                }

                if (!string.IsNullOrWhiteSpace(item.PatchBranch))
                {
                    result.TryAdd(item.PatchBranch, item);
                }
            }

            return result;
        }
    }
}