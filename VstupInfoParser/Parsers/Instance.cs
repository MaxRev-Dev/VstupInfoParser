using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VstupInfoParser.ModelsJSON
{
    public abstract partial class Instance
    {
        public Instance(string name, string url, int year)
        {
            Name = name; Url = url; Year = year;
        }
        public override string ToString()
        {
            return Name + ": " + Url;
        }
        protected abstract void Parse(HtmlDocument doc);
        public async Task FetchAsync()
        {
            if (Fetched != default) return;
            Fetched = DateTime.Now;
            try
            {
                HttpClient client = new HttpClient();
                var docStream = await client.GetStreamAsync(Url);

                HtmlDocument doc = new HtmlDocument();
                doc.Load(docStream);
                Parse(doc);
            }
            catch (Exception ex)
            {
                OnProcessError = ex;
            }
        }
    }
}
