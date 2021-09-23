using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;
using System.Threading.Tasks;
using System.Drawing;

namespace JH.Applications
{
    public partial class FormMain : Form
    {
        public class SearchCondition
        {
            public bool check;
            public string value;
            public string searchValue;
        }

        public class BatchInput
        {
            public BatchInput(string query, string hyperLink)
            {
                this.query = query;
                this.hyperLink = hyperLink;
            }

            public string query;
            public string hyperLink;
        }

        void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            string path = Path.GetFullPath(".");
            settings.RemoteDebuggingPort = 8080;
            settings.CachePath = path;

            //Initialize Cef with the provided settings
            Cef.Initialize(settings);
            settings.PersistSessionCookies = true;

            //Create a browser component
            webBrowser = new ChromiumWebBrowser("");
            Controls.Add(webBrowser);
            SetupWebbrowser();
        }

        void SetupWebbrowser()
        {
            webBrowser.Size = new Size(1000, 555);
            webBrowser.Location = new Point(30, 171);
            webBrowser.Dock = DockStyle.None;
            webBrowser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
            webBrowser.LoadingStateChanged += OnLoadingStateChanged;
            webBrowser.ConsoleMessage += OnBrowserConsoleMessage;
            webBrowser.StatusMessage += OnBrowserStatusMessage;
            webBrowser.TitleChanged += OnBrowserTitleChanged;
            webBrowser.AddressChanged += OnBrowserAddressChanged;
        }

        void SetupTooltips()
        {
            label3toolTip.SetToolTip(label3, "Hit tæller, finsøgning");
            label4toolTip.SetToolTip(label4, "Hit indikator");
            label5toolTip.SetToolTip(label5, "Total hit tæller");
            label6toolTip.SetToolTip(label6, "Antallet af of KB hits, max 75");
            label7toolTip.SetToolTip(label7, "Total antal hits");
            label8toolTip.SetToolTip(label8, "Dataforsynig hit tæller");
            label9toolTip.SetToolTip(label9, "Cirkel værdi");
            label10toolTip.SetToolTip(label10, "Batch tæller");
            linkLabel3toolTip.SetToolTip(linkLabel3, "Vis resultat folder");
            linkLabel1toolTip.SetToolTip(linkLabel1, "Vis sorteret resultatfil");
            linkLabel4toolTip.SetToolTip(linkLabel4, "Brugsanvisning");
            textBox5toolTip.SetToolTip(textBox5, "Max cikelværdi");
            comboBox1toolTip.SetToolTip(comboBox1, "Filnavngivnings vælger");
            button1toolTip.SetToolTip(button1, "Status indikator");
            button2toolTip.SetToolTip(button2, "Download hits med billeder");
            button3toolTip.SetToolTip(button3, "Download hits uden billeder");
            button4toolTip.SetToolTip(button4, "Stop download");
            button5toolTip.SetToolTip(button5, "Browse tibage");
            button6toolTip.SetToolTip(button6, "Gem token");
            button7toolTip.SetToolTip(button7, "Nord-Øst");
            button8toolTip.SetToolTip(button8, "Syd-Vest");
            button9toolTip.SetToolTip(button9, "Finsøgnings kriterier");
            button10toolTip.SetToolTip(button10, "Batch med billeder");
            button11toolTip.SetToolTip(button11, "Batch uden billeder");
            button12toolTip.SetToolTip(button12, "Stop batch");
            button13toolTip.SetToolTip(button13, "Vis batchkort");
            button14toolTip.SetToolTip(button14, "Browse frem");
        }

        void InitializeLabels()
        {
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";
            label11.Text = "";
            button5.Enabled = false;
            button14.Enabled = false;
        }

        void SetupFilePaths()
        {
            projectFolder = @"..";
            Trace.Listeners.Add((new MyTraceListener(new StreamWriter(projectFolder + @"\log.log"))));

            tokenPath = projectFolder + @"\dataforsyningToken.txt";
            if (File.Exists(tokenPath))
            {
                StreamReader reader = new StreamReader(tokenPath);
                token = reader.ReadLine();
                reader.Close();
                textBox1.Text = token;
            }

            lastUrlPath = projectFolder + @"\lastUrl.txt";
            if (File.Exists(lastUrlPath))
            {
                StreamReader reader = new StreamReader(lastUrlPath);
                lastUrlString = reader.ReadLine();
                reader.Close();
            }
            else
                lastUrlString = "http://www5.kb.dk/danmarksetfraluften/?q_fritekst=S%c3%B8nderladevej&q_stednavn=&q_bygningsnavn=&q_person=&notBefore=1890&notAfter=2015&category=subject203&itemType=all&thumbnailSize=&correctness=&thumbnailSize=&sortby=&sortorder=#zoom=9&lat=57.34&lng=9.27";

            mapCalibrationPath = projectFolder + @"\MapCalibration_" + Environment.GetEnvironmentVariable("ComputerName") + ".txt";
            if (File.Exists(mapCalibrationPath))
            {
                StreamReader reader = new StreamReader(mapCalibrationPath);
                deltaLat = double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
                deltaLng = double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
                reader.Close();
                Trace.WriteLine("Map calibration:");
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat));
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng));

            }
            else
            {
                Trace.WriteLine("Map not calibrated");
                MessageBox.Show("Kortet er ikke kalibreret.\n\nSe brugsanvisningen.");
            }

            resultFolder = projectFolder + @"\" + results;
            if (!Directory.Exists(resultFolder))
                Directory.CreateDirectory(resultFolder);
            photoPath = resultFolder + @"\" + luftfoto;
            IEnumerable<string> files = Directory.EnumerateDirectories(resultFolder + "\\");
            foreach (string file in files)
            {
                int count;
                try
                {
                    count = int.Parse(file.Substring(file.Length - 4, 4));
                }
                catch
                {
                    count = 0;
                }
                if (count > startExt)
                    startExt = count;
            }
            ext = string.Format("{0:0000}", startExt);
            exttxt = ext + ".txt";
            linkLabel3.Text = luftfoto + ext;
            linkLabel1.Text = sortedResult + exttxt;

            if (File.Exists(projectFolder + @"\verbose.log"))
                Trace.Listeners.Add((new MyTraceListener(new StreamWriter(projectFolder + @"\verbose.log"))));
        }

        void FormMain_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                Size size = this.Size;
                Trace.WriteLine("Form width: " + size.Width.ToString());
                Trace.WriteLine("Form height: " + size.Height.ToString());
                Size sizeWebBrowserAfter = new Size(size.Width - webBrowser.Location.X - 50, size.Height - webBrowser.Location.Y - 50);
                Trace.WriteLine("sizeWebBrowserAfter width: " + sizeWebBrowserAfter.Width.ToString());
                Trace.WriteLine("sizeWebBrowserAfter height: " + sizeWebBrowserAfter.Height.ToString());
                webBrowser.Size = sizeWebBrowserAfter;
                deltaLat *= (double)sizeWebBrowserAfter.Height / sizeWebBrowserBefore.Height;
                deltaLng *= (double)sizeWebBrowserAfter.Width / sizeWebBrowserBefore.Width;
                sizeWebBrowserBefore = sizeWebBrowserAfter;
                int zoom;
                double lat;
                double lng;
                BrowserCenter(out zoom, out lat, out lng);
                Trace.WriteLine("Form size changed");
                Trace.WriteLine(string.Format("zoom: {0}", zoom));
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "lat: {0}", lat));
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "lng: {0}", lng));
                Trace.WriteLine(string.Format(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat)));
                Trace.WriteLine(string.Format(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng)));
            }
            catch
            {

            }
        }

        void Navigated(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
                return;

            string url = webBrowser.GetMainFrame().Url;
            if (url.Contains("kb.dk"))
            {
                StreamWriter writer = new StreamWriter(lastUrlPath);
                writer.Write(url);
                writer.Close();
            }

            if (addToUrlHistory)
            {
                if (urlHistory.Count == 0 || url != urlHistory[urlHistoryPointer])
                {
                    if (button14.Enabled)
                    {
                        for (int i = urlHistory.Count - 1; i > urlHistoryPointer; i--)
                        {
                            urlHistory.RemoveAt(i);
                        }
                    }
                    urlHistory.Add(url);
                    urlHistoryPointer++;
                    if (urlHistoryPointer > 0)
                        Invoke(new Action(() => button5.Enabled = true));

                    Invoke(new Action(() => button14.Enabled = false));
                }
            }
            addToUrlHistory = true;
            Trace.WriteLine("KB server up and running");
        }

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

        void InitSearchCondition()
        {
            searchCondition[0].check = searchConditionShadow[0].check;
            searchCondition[0].searchValue = searchConditionShadow[0].searchValue;
            searchCondition[1].check = searchConditionShadow[1].check;
            searchCondition[1].searchValue = searchConditionShadow[1].searchValue;
            searchCondition[2].check = searchConditionShadow[2].check;
            searchCondition[2].searchValue = searchConditionShadow[2].searchValue;
            searchCondition[3].check = searchConditionShadow[3].check;
            searchCondition[3].searchValue = searchConditionShadow[3].searchValue;
            searchCondition[4].check = searchConditionShadow[4].check;
            searchCondition[4].searchValue = searchConditionShadow[4].searchValue;
            searchCondition[5].check = searchConditionShadow[5].check;
            searchCondition[5].searchValue = searchConditionShadow[5].searchValue;
            searchCondition[6].check = searchConditionShadow[6].check;
            searchCondition[6].searchValue = searchConditionShadow[6].searchValue;
            for (int i = 0; i < searchCondition.Length; i++)
                if (searchCondition[i].searchValue != null)
                    Trace.WriteLine("verbose", i.ToString() + "  " + searchCondition[i].check.ToString() + "  " + searchCondition[i].searchValue.ToString());

            maxCircle = int.Parse(textBox5.Text);
            if (maxCircle < 4)
            {
                maxCircle = 4;
                Invoke(new Action(() => textBox5.Text = "4"));
            }
            if (maxCircle > 1000)
            {
                maxCircle = 1000;
                Invoke(new Action(() => textBox5.Text = "1000"));
            }

            StreamWriter stream1 = new StreamWriter(projectFolder + @"\setup.txt");
            stream1.WriteLine("maxCircle = " + textBox5.Text.ToString());
            stream1.Close();

        }

        string GetQuery()
        {
            HtmlElementCollection f = null;
            bool queryFailure = true;
            int cnt = 0;
            while (cnt < 10 && queryFailure)
            {
                queryFailure = false;
                try
                {
                    var task = HtmlString();
                    Task.WaitAll(task);
                    string htmlString = task.Result;
                    HtmlDocument doc = null;
                    Invoke(new Action(() =>
                    {
                        WebBrowser browserOld = new WebBrowser();
                        browserOld.ScriptErrorsSuppressed = true;
                        browserOld.DocumentText = htmlString;
                        browserOld.Document.OpenNew(true);
                        browserOld.Document.Write(htmlString);
                        browserOld.Refresh();
                        doc = browserOld.Document;
                    }));


                    freetext = "";
                    location = "";
                    building = "";
                    person = "";
                    notBefore = "";
                    notAfter = "";

                    HtmlElement ele = doc.GetElementById("search-form");
                    f = ele.GetElementsByTagName("input");
                }
                catch (Exception e)
                {
                    queryFailure = true;
                    Trace.WriteLine("Trying to get query, cnt: " + cnt.ToString());
                }
                cnt++;
            }
            if (queryFailure)
            {
                Trace.WriteLine("Give up get query");
                throw new InvalidProgramException("Give up get query");
            }

            foreach (HtmlElement f1 in f)
            {
                if (f1.Name == "q_fritekst")
                    freetext = f1.GetAttribute("value");
                if (f1.Name == "q_stednavn")
                    location = f1.GetAttribute("value");
                if (f1.Name == "q_bygningsnavn")
                    building = f1.GetAttribute("value");
                if (f1.Name == "q_person")
                    person = f1.GetAttribute("value");
                if (f1.Name == "notBefore")
                    notBefore = f1.GetAttribute("value");
                if (f1.Name == "notAfter")
                    notAfter = f1.GetAttribute("value");
            }
            string query = "";
            if (freetext != "")
            {
                query = freetext;

                if (location != "")
                    query += "%26location%3A" + location;
                if (building != "")
                    query += "%26building%3A" + building;
                if (person != "")
                    query += "%26person%3A" + person;
            }
            else
            {
                if (location != "")
                {
                    query = "location%3A" + location;

                    if (building != "")
                        query += "%26building%3A" + building;

                    if (person != "")
                        query += "%26person%3A" + person;
                }
                else
                {
                    if (building != "")
                    {
                        query = "building%3A" + building;

                        if (person != "")
                            query += "%26person%3A" + person;
                    }
                    else
                        if (person != "")
                        query = "person%3A" + person;
                }

            }

            return query;
        }

        string SearchKbDb(string xml, string key)
        {
            int i0 = xml.IndexOf(key + ":");
            xml = xml.Substring(i0);
            int i1 = xml.IndexOf("col-xs-8") + 8 + 2;
            xml = xml.Substring(i1);
            int i2 = xml.IndexOf("<");
            xml = xml.Substring(0, i2).Replace("\r\n", "");
            xml = RemoveLeadingAndTrailingBlanks(xml);
            return xml;
        }

        string RemoveLeadingAndTrailingBlanks(string s)
        {
            while (s.StartsWith(" "))
                s = s.Substring(1);
            while (s.EndsWith(" "))
                s = s.Substring(0, s.Length - 1);
            return s;
        }

        string PictureFileName(int counter, string vej, string husnummer, string postnummer, string by, string aar, string id, string bygning)
        {
            int selectedIndex = 0;
            Invoke(new Action(() => selectedIndex = comboBox1.SelectedIndex));
            switch (selectedIndex)
            {

                default:
                case 0:
                    return id.Substring(0, id.Length - 4) + ".jpg";
                case 1:
                    return vej + " " + husnummer + ", " + postnummer + " " + by + " - " + aar + " - " + id.Substring(0, id.Length - 4) + ".jpg";
                case 2:
                    return vej + " " + husnummer + ", " + postnummer + " - " + aar + " - " + id.Substring(0, id.Length - 4) + ".jpg";
                case 3:
                    return vej + " " + husnummer + " - " + aar + " - " + id.Substring(0, id.Length - 4) + ".jpg";
                case 4:
                    return vej + " " + husnummer + " - " + aar + " - " + bygning + " - " + id.Substring(0, id.Length - 4) + ".jpg";
            }

        }

        void DownloadPicture(string link, string file, string fileName)
        {
            if (!downloadPictures)
                return;
            WebClient client = new WebClient();
            client.DownloadFile(link, file + @"\" + fileName);
            client.Dispose();
        }

        void WriteSearchResult(string key, string data)
        {
            resultFileWriter.WriteLine("{0,-20}\t{1}", key, data);
            string s = string.Format("{0,-20}\t{1}", key, data);
            itemList.Add(s);
        }

        void WriteSortedResult()
        {
            foreach (ItemList l in resultList)
            {
                foreach (string s in l)
                {
                    resultFileSortedWriter.WriteLine(s);
                }
            }
        }

        void BrowserCenter(out int zoom, out double lat, out double lng)
        {
            string docUrl = webBrowser.GetMainFrame().Url;

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
            WriteHeader(resultFileWriter, dateTime);
            WriteHeader(resultFileSortedWriter, dateTime);
            resultFileWriter.Flush();
            resultFileSortedWriter.Flush();
        }

        void WriteHeader(StreamWriter writer, string dateTime)
        {
            writer.WriteLine(this.Text);
            writer.WriteLine();

            if (writer == resultFileWriter)
                writer.WriteLine(searchResult + exttxt);
            else
                writer.WriteLine(sortedResult + exttxt);
            writer.WriteLine();
            writer.WriteLine("Search date-time: " + dateTime);
            writer.WriteLine();
            string docUrl = "";
            Invoke(new Action(() => docUrl = webBrowser.GetMainFrame().Url));
            writer.WriteLine("URL: " + docUrl);
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

        class ItemList : List<string>, IComparer
        {
            public static int count;
            static public int Compare(ItemList l0, ItemList l1)
            {
                return string.Compare(l0[count], l1[count]);
            }
            public int Compare(object l0, object l1)
            {

                return string.Compare(((List<string>)l0)[count], ((List<string>)l1)[count]);
            }
        }

        class MyTraceListener : TraceListener
        {
            String fileName = "";
            DateTime start;
            TextWriter writer;
            public MyTraceListener(TextWriter writer)
            {
                start = DateTime.Now;
                this.writer = writer;
                FileStream fileStream = (writer as StreamWriter).BaseStream as FileStream;
                if (fileStream != null)
                    fileName = Path.GetFileName(fileStream.Name).ToLower();
                writer.WriteLine(DateTime.Now.ToString());
            }
            public override void WriteLine(string message)
            {
                writer.WriteLine(string.Format("{0,10:0.000}  {1}", (DateTime.Now - start).TotalSeconds, message));
                writer.Flush();
            }
            public override void Write(string message)
            {
                writer.Write(string.Format("{0,10:0.000}  {1}", (DateTime.Now - start).TotalSeconds, message));
                writer.Flush();
            }
            public override void WriteLine(string type, string message)
            {
                if (type == "verbose" && fileName == "verbose.log")
                {
                    writer.WriteLine(string.Format("{0,10:0.000}  {1}", (DateTime.Now - start).TotalSeconds, message));
                    writer.Flush();
                }
            }
            public override void Write(string type, string message)
            {
                if (type == "verbose" && fileName == "verbose.log")
                {
                    writer.Write(string.Format("{0,10:0.000}  {1}", (DateTime.Now - start).TotalSeconds, message));
                    writer.Flush();
                }
            }
        }

        string GetCurrentWebSource()
        {
            var task = HtmlString();
            Task.WaitAll(task);
            return task.Result;
        }

        async Task<string> HtmlString()
        {
            return await webBrowser.GetMainFrame().GetSourceAsync();
        }

    }
}
