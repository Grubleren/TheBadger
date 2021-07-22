using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace JH.Applications
{
    public partial class FormMain : Form
    {
        string KbObjects(string leftMenu, out int len)
        {
            string kbdbObjects;
            len = 0;
            try
            {
                string[] split = leftMenu.Split(new char[] { '\"' });
                len = split.Length / 28;
                kbdbObjects = "";
                for (int i = 0; i < len; i++)
                {
                    kbdbObjects += split[28 * i + 3];
                }
            }
            catch (Exception e)
            {
                kbdbObjects = null;
            }
            return kbdbObjects;
        }

        string SearchKbDb(string xml, string key)
        {
            int i0 = xml.IndexOf(key + ":");
            xml = xml.Substring(i0);
            int i1 = xml.IndexOf("col-xs-8") + 8 + 2;
            xml = xml.Substring(i1);
            int i2 = xml.IndexOf("<");
            return xml.Substring(0, i2).Replace("\r\n", "");
        }

        void DownloadPicture(string link, string file, int counter, string id)
        {
            if (!downloadPictures)
                return;
            WebClient client = new WebClient();
            client.DownloadFile(link, file + "/" + id.Substring(0, id.Length - 4) + ".jpg");
        }

        void WriteSearchResult(string key, string data)
        {
            badgerResultFileWriter.WriteLine("{0,-20}\t{1}", key, data);
            string s = string.Format("{0,-20}\t{1}", key, data);
            itemList.Add(s);
        }

        void WriteSortedResult()
        {
            foreach (ItemList l in resultList)
            {
                foreach (string s in l)
                {
                    badgerResultFileSortedWriter.WriteLine(s);
                }
            }
        }

        void BrowserCenter(out int zoom, out double lat, out double lng)
        {
            HtmlDocument doc = webBrowser.Document;
            string docUrl = doc.Url.ToString();
            string dst = ToUTF8(docUrl);
            StreamWriter writer = new StreamWriter(lastUrlPath);
            writer.Write(dst);
            writer.Close();

            int i0 = docUrl.IndexOf("zoom=") + 5;
            docUrl = docUrl.Substring(i0);
            int i1 = docUrl.IndexOf("&");
            zoom = int.Parse(docUrl.Substring(0, i1));
            i0 = docUrl.IndexOf("lat=") + 4;
            docUrl = docUrl.Substring(i0);
            i1 = docUrl.IndexOf("&");
            lat = double.Parse(docUrl.Substring(0, i1), CultureInfo.InvariantCulture);
            i0 = docUrl.IndexOf("lng=") + 4;
            lng = double.Parse(docUrl.Substring(i0), CultureInfo.InvariantCulture);

        }

        string ToUTF8(string src)
        {
            string dst = "";
            foreach (char c in src)
                if (c == 'æ')
                    dst += "%c3%a6";
                else if (c == 'Æ')
                    dst += "%c3%86";
                else if (c == 'ø')
                    dst += "%c3%b8";
                else if (c == 'Ø')
                    dst += "%c3%98";
                else if (c == 'å')
                    dst += "%c3%a5";
                else if (c == 'Å')
                    dst += "%c3%85";
                else
                    dst += c;

            return dst;
        }

        bool NotFoundSearchChriteria(SearchCondition[] searchCondition)
        {
            bool notFound = false;
            foreach (SearchCondition search in searchCondition)
                notFound |= search.check && search.value.ToLower() != search.searchValue.ToLower() && search.searchValue != "";

            return notFound;
        }

        void WriteHeaders(string dateTime)
        {
            WriteHeader(badgerResultFileWriter, dateTime);
            WriteHeader(badgerResultFileSortedWriter, dateTime);
            badgerResultFileWriter.Flush();
            badgerResultFileSortedWriter.Flush();
        }

        void WriteHeader(StreamWriter writer, string dateTime)
        {
            writer.WriteLine(this.Text);
            writer.WriteLine();

            if (writer == badgerResultFileWriter)
                writer.WriteLine(searchResult + ext);
            else
                writer.WriteLine(sortedResult + ext);
            writer.WriteLine();
            writer.WriteLine("Search date-time: " + dateTime);
            writer.WriteLine();

            writer.WriteLine("Initial search parameters:");

            writer.WriteLine("Fritekst: " + freetext);
            writer.WriteLine("Sted: " + location);
            writer.WriteLine("Bygningsnavn: " + building);
            writer.WriteLine("Person: " + person);
            writer.WriteLine("Not before: " + notBefore);
            writer.WriteLine("Not after: " + notAfter);
            writer.WriteLine();

            writer.WriteLine("Post search parameters:");

            writer.WriteLine("Vejnavn: " + (searchCondition[0].check ? searchCondition[0].searchValue : ""));
            writer.WriteLine("Husnummer: " + (searchCondition[1].check ? searchCondition[1].searchValue : ""));
            writer.WriteLine("Postnummer: " + (searchCondition[2].check ? searchCondition[2].searchValue : ""));
            writer.WriteLine("Matrikelnummer: " + (searchCondition[3].check ? searchCondition[3].searchValue : ""));
            writer.WriteLine("Ejerlav: " + (searchCondition[4].check ? searchCondition[4].searchValue : ""));
            writer.WriteLine("Kommune: " + (searchCondition[5].check ? searchCondition[5].searchValue : ""));
            writer.WriteLine("Sogn: " + (searchCondition[6].check ? searchCondition[6].searchValue : ""));
            writer.WriteLine();

        }
    }
}
