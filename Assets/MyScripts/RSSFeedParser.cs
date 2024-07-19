using System;
using System.Xml;
using System.Text.RegularExpressions;

public class RSSFeedParser
{
    public string ChannelTitle { get; private set; }
    public string ChannelLink { get; private set; }
    public string ChannelDescription { get; private set; }
    public string ChannelLanguage { get; private set; }
    public string ChannelPubDate { get; private set; }
    public string ChannelCopyright { get; private set; }
    public string ChannelImage { get; private set; }

    public class RSSItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Guid { get; set; }
        public string ContentEncoded { get; set; }

        // Additional fields extracted from Description
        public string PostedOn { get; set; }
        public string Category { get; set; }
        public string Skills { get; set; }
        public string Country { get; set; }

        public void ParseDescription()
        {
            // Regular expression patterns to match the required fields
            var postedOnPattern = @"<b>Posted On</b>:\s*(.*?)<br />";
            var categoryPattern = @"<b>Category</b>:\s*(.*?)<br />";
            var skillsPattern = @"<b>Skills</b>:(.*?)<br />";
            var countryPattern = @"<b>Country</b>:\s*(.*?)<br />";

            // Local variable to hold the description
            string description = Description;

            // Extracting and removing the fields from Description
            PostedOn = ExtractAndRemove(ref description, postedOnPattern);
            Category = ExtractAndRemove(ref description, categoryPattern);
            Skills = ExtractAndRemove(ref description, skillsPattern).Trim();
            Country = ExtractAndRemove(ref description, countryPattern).Trim();

            // Assign the cleaned description back to the property
            Description = description;
        }

        private string ExtractAndRemove(ref string input, string pattern)
        {
            var match = Regex.Match(input, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                input = input.Replace(match.Value, string.Empty);
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }

    public RSSItem[] Items { get; private set; }

    public void Parse(string rssFeed)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(rssFeed);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");

        XmlNode channelNode = xmlDoc.SelectSingleNode("rss/channel", nsmgr);
        ChannelTitle = channelNode.SelectSingleNode("title").InnerText;
        ChannelLink = channelNode.SelectSingleNode("link").InnerText;
        ChannelDescription = channelNode.SelectSingleNode("description").InnerText;
        ChannelLanguage = channelNode.SelectSingleNode("language").InnerText;
        ChannelPubDate = channelNode.SelectSingleNode("pubDate").InnerText;
        ChannelCopyright = channelNode.SelectSingleNode("copyright").InnerText;
        ChannelImage = channelNode.SelectSingleNode("image/url").InnerText;

        XmlNodeList itemNodes = channelNode.SelectNodes("item", nsmgr);
        Items = new RSSItem[itemNodes.Count];
        for (int i = 0; i < itemNodes.Count; i++)
        {
            XmlNode itemNode = itemNodes[i];
            RSSItem item = new RSSItem
            {
                Title = itemNode.SelectSingleNode("title").InnerText,
                Link = itemNode.SelectSingleNode("link").InnerText,
                Description = itemNode.SelectSingleNode("description").InnerText,
                PubDate = itemNode.SelectSingleNode("pubDate").InnerText,
                Guid = itemNode.SelectSingleNode("guid").InnerText,
                ContentEncoded = itemNode.SelectSingleNode("content:encoded", nsmgr)?.InnerText
            };

            item.ParseDescription();  // Parse additional fields from description
            Items[i] = item;
        }
    }
}
