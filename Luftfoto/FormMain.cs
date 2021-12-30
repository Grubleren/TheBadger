using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using CefSharp.WinForms;
using CefSharp;
using System.Threading.Tasks;

namespace JH.Applications
{
    public partial class FormMain : Form
    {
        string version = "The Badger ver. 5.15";

        public FormMain()
        {
            enableChangeSize = false;
            InitializeComponent();
            InitializeChromium();

            SetupTooltips();

            this.Text = version;

            Cef.EnableHighDPISupport();

            urlHistory = new List<string>();
            resultList = new List<ItemList>();
            searchCondition = new SearchCondition[9];
            searchConditionShadow = new SearchCondition[9];
            for (int i = 0; i < searchCondition.Length; i++)
                searchCondition[i] = new SearchCondition();
            for (int i = 0; i < searchConditionShadow.Length; i++)
                searchConditionShadow[i] = new SearchCondition();
            searchDialog = new SearchDialog(this);

            Trace.AutoFlush = true;

            InitializeLabels();
            label10.Text = "";

            SetupFilePaths();

            StreamReader stream = new StreamReader(projectFolder + @"\setup.txt");
            string s = stream.ReadLine();
            int i0 = s.IndexOf("=");
            maxCircle = int.Parse(s.Substring(i0 + 1));
            textBox5.Text = maxCircle.ToString();
            stream.Close();

            webClient = new WebClient();
            kbServerRunning = false;
            webClient.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(KBServerTestCompleted);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(KBServerTestCompleted);
            webClient.DownloadStringAsync(new Uri(lastUrlString));
            System.Windows.Forms.Timer kbServerRunningTimer = new System.Windows.Forms.Timer();
            kbServerRunningTimer.Interval = 5000;
            kbServerRunningTimer.Enabled = true;
            kbServerRunningTimer.Tick += timer_Tick;
            kbServerRunningTimer.Start();
        }

        void FormMain_Load(object sender, EventArgs e)
        {

        }

        void timer_Tick(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Timer)sender).Stop();
            if (!kbServerRunning)
            {
                webClient.CancelAsync();
            }
        }

        void KBServerTestCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            webClient.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(KBServerTestCompleted);
            webClient.Dispose();
            if (e.Cancelled)
            {
                webBrowser.Load("file:///" + Environment.CurrentDirectory + "/../kbServerDown.html");
                Trace.WriteLine("KB server not responding");
                return;
            }


            kbServerRunning = true;
            webBrowser.LoadingStateChanged -= new EventHandler<LoadingStateChangedEventArgs>(Navigated);
            webBrowser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>(Navigated);
            webBrowser.Load(lastUrlString);
            addToUrlHistory = true;
            Size size = this.Size;
            Trace.WriteLine("Form width: " + size.Width.ToString());
            Trace.WriteLine("Form height: " + size.Height.ToString());
            Size sizeWebBrowserAfter = new Size(size.Width - webBrowser.Location.X - 50, size.Height - webBrowser.Location.Y - 50);
            Trace.WriteLine("sizeWebBrowserAfter width: " + sizeWebBrowserAfter.Width.ToString());
            Trace.WriteLine("sizeWebBrowserAfter height: " + sizeWebBrowserAfter.Height.ToString());
            webBrowser.Size = sizeWebBrowserAfter;
            sizeWebBrowserBefore = sizeWebBrowserAfter;
            enableChangeSize = true;
            Trace.WriteLine("KBServerTestCompleted");
        }

        void DownloadInit()
        {
            if (textBox1.Text.Length != 32)
            {
                Trace.WriteLine("Invalid token");
                MessageBox.Show("Ugyldig token:\nProgrammet benytter dataforsyningen.dk til indhentning af oplysninger om f. eks. en adresse svarende til en bestemt geografisk koordinat. Det er derfor nødvendigt for at bruge programmet at oprette sig som bruger af dataforsyningen.dk https://dataforsyningen.dk/ og generere en token. Denne token kopieres til The Badger i øverste venstre hjørne og man gemmer token ved at trykke på knappen mærket ’G’. Herefter skal man ikke tænke mere p˚a token. Det er ikke nødvendigt at være tilmeldt som bruger af Det Kongelige Bibliotek.");
            }
            else
            {
                if (!searchStopped)
                    return;
                searchStopped = false;

                Trace.WriteLine(string.Format("Preparing fastsearch"));

                int downloadCounter = 0;
                bool downloadFailure = true;
                while (downloadCounter < 10 && downloadFailure)
                {
                    string query = GetQuery();
                    downloadFailure = !Download(query);
                    if (downloadFailure)
                    {
                        downloadCounter++;
                        string failDownload = string.Format("Failed to download fastsearch, cnt: {0}", downloadCounter);
                        Trace.WriteLine(failDownload);
                        Invoke(new Action(() => label4.Text = failDownload));
                    }
                }
                if (downloadFailure)
                {
                    string failDownloadGiveUp = "Give up download fastsearch";
                    Trace.WriteLine(failDownloadGiveUp);
                    searching = false;
                    searchStopped = true;
                    Invoke(new Action(() => { label4.Text = failDownloadGiveUp; button1.BackColor = Color.Green; button1.Text = ""; }));
                    MessageBox.Show(failDownloadGiveUp);
                }
            }
        }

        bool Download(string query)
        {
            try
            {
                this.queryString = query;
                InitSearchCondition();

                Invoke(new Action(() =>
                {
                    button1.BackColor = Color.Blue;
                    InitializeLabels();
                }));

                int zoom;
                double lat;
                double lng;
                BrowserCenter(out zoom, out lat, out lng);
                Trace.WriteLine(string.Format("zoom: {0}", zoom));
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "lat: {0}", lat));
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "lng: {0}", lng));
                string coordinates = (lng + deltaLng / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture) + "," + (lat + deltaLat / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture) + "," + (lng - deltaLng / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture) + "," + (lat - deltaLat / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture);
                Trace.WriteLine("Coordinates: " + coordinates);
                string uri = "http://www.kb.dk/cop/syndication/images/luftfo/2011/maj/luftfoto/subject203/?format=kml&type=all&bbo=";
                uri += coordinates + "&notBefore=" + notBefore + "-01-01" + "&notAfter=" + notAfter + "-01-01";
                uri += "&itemsPerPage=20000000&random=0.0&query=" + query;

                WebClient client = null;
                string tempString = null;
                try
                {
                    client = new WebClient();
                    Trace.WriteLine(string.Format("Downloading fastsearch with URI: {0}", uri));
                    tempString = client.DownloadString(uri);
                }
                catch
                {
                    client.Dispose();
                    string failDownload = string.Format("Failed to download fastsearch trying again");
                    Trace.WriteLine(failDownload);
                    Invoke(new Action(() => label4.Text = failDownload));
                    return false;
                }
                client.Dispose();
                StreamWriter streamTemp = new StreamWriter(projectFolder + @"\temp.xml");
                streamTemp.Write(tempString);
                streamTemp.Close();
                Trace.WriteLine(string.Format("Fastsearch download completed"));

                int counter = -1;
                int dfCounter = -1;
                int searchCounter = -1;
                List<List<string>> items = new List<List<string>>();
                resultList.Clear();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client = new WebClient();
                client.Encoding = Encoding.UTF8;
                avgCoordinate = new double[2];
                avgCircle = 0;
                long timeZero = DateTime.Now.Ticks;
                searching = true;
                Trace.WriteLine("Read Html string");

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

                Trace.WriteLine("Get Left menu");
                string leftMenu = doc.GetElementById("leftmenu").InnerHtml;
                int len;
                string kbObjets = KbObjects(leftMenu, out len); // only len is used
                Trace.WriteLine(String.Format("Leftmenu: {0}   len: {1}", leftMenu, len));
                Invoke(new Action(() =>
                {
                    button1.BackColor = Color.Red;
                    button1.Text = "STOP";
                    label6.Text = len.ToString();
                }));


                startExt++;
                ext = string.Format("{0:0000}", startExt);
                exttxt = ext + ".txt";
                string photoFile = photoPath + ext;
                try
                {
                    Directory.CreateDirectory(photoFile);
                }
                catch (Exception e1)
                {
                    Trace.WriteLine(string.Format("Could not create directory: {0}", photoFile));
                    Trace.WriteLine(string.Format("Error message: " + e1.Message));
                    Trace.WriteLine(string.Format("Stack trace:" + e1.StackTrace));
                    Trace.WriteLine(string.Format("Target site: " + e1.TargetSite));
                    client.Dispose();
                    searching = false;
                    searchStopped = true;
                    Invoke(new Action(() => { button1.BackColor = Color.Green; button1.Text = ""; }));
                    return true;
                }
                Trace.WriteLine("Setup file paths");
                resultFile = photoFile + @"\" + searchResult + exttxt;
                resultSortedFile = photoFile + @"\" + sortedResult + exttxt;
                resultFileWriter = new StreamWriter(resultFile);
                resultFileSortedWriter = new StreamWriter(resultSortedFile);
                Invoke(new Action(() => linkLabel3.Text = luftfoto + ext));
                Invoke(new Action(() => linkLabel1.Text = sortedResult + exttxt)); ;
                Trace.WriteLine(string.Format("Write headers"));
                WriteHeaders(DateTime.Now.ToString());

                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(tempString);

                    XmlNode node = xmlDoc.DocumentElement.FirstChild.FirstChild;
                    Trace.WriteLine(string.Format("Counting fastsearch hits"));
                    counter = 0;
                    while (node != null && searching)
                    {
                        if (node.Name == "Placemark")
                            counter++;
                        node = node.NextSibling;
                    }
                    Trace.WriteLine(string.Format("Fastsearch hits: {0}", counter));
                    Invoke(new Action(() => label7.Text = counter.ToString()));

                    node = xmlDoc.DocumentElement.FirstChild.FirstChild;
                    counter = 0;
                    dfCounter = 0;
                    searchCounter = 0;
                    Trace.WriteLine(string.Format("Finesearch started"));
                    while (node != null && searching)
                    {
                        if (node.Name == "Placemark")
                        {

                            XmlNodeList lst = node.ChildNodes;
                            string kbdb = lst[2].Attributes[0].Value;
                            Trace.WriteLine("verbose", kbdb);
                            string[] kbdbSplit = kbdb.Split(new char[] { '/' });
                            string kbObject = kbdbSplit[kbdbSplit.Length - 1];

                            if (!SearchCriteria(lst, client, kbdb, node, ref counter, ref dfCounter))
                                continue;

                            if (NotFoundSearchChriteria(searchCondition))
                            {
                                node = node.NextSibling;
                                counter++;
                                if ((counter % 100) == 0)
                                    Trace.WriteLine(string.Format("Counter: {0}", counter));
                                Trace.WriteLine("verbose", string.Format("Counter: {0}", counter));
                                Invoke(new Action(() => label5.Text = counter.ToString()));
                                continue;
                            }
                            string[] splitCoordinate = koordinat.Split(new char[] { ',' });
                            double longitude = double.Parse(splitCoordinate[0], CultureInfo.InvariantCulture);
                            double latitude = double.Parse(splitCoordinate[1], CultureInfo.InvariantCulture);
                            avgCoordinate[0] += longitude;
                            avgCoordinate[1] += latitude;
                            avgCircle += circle;
                            avgTimePerDownload = DateTime.Now.Ticks - timeZero;
                            dataforsyningSteder = new DataforsyningSteder("steder", koordinat, circle, circleCount, token, client);
                            if (dataforsyningSteder.dataList != null && dataforsyningSteder.dataList.Count != 0)
                                primnavn = dataforsyningSteder.PrimNavn;

                            if (dataforsyningAdresser.dataList != null && dataforsyningAdresser.dataList.Count != 0)
                            {
                                vejnavn = dataforsyningAdresser.Vejnavn;
                                husnummer = dataforsyningAdresser.Husnr;
                                lokalitet = dataforsyningAdresser.Lokalitet;
                                postnummer = dataforsyningAdresser.Postnummer;
                                by = dataforsyningAdresser.By;
                                sogn = dataforsyningAdresser.Sogn;
                                matrikelnr = dataforsyningAdresser.Matrikelnr;
                                ejerlav = dataforsyningAdresser.Ejerlav;
                                betegnelse = dataforsyningAdresser.Betegnelse;
                                kommune = dataforsyningAdresser.Kommune;
                                politikreds = dataforsyningAdresser.Politikreds;
                                retskreds = dataforsyningAdresser.Retskreds;
                                region = dataforsyningAdresser.Region;
                                zone = dataforsyningAdresser.Zone;
                            }
                            string lokalitetKB = SearchKbDb(kbdbResponse, "Lokalitet");
                            string titelKB = SearchKbDb(kbdbResponse, "Titel");
                            string personKB = SearchKbDb(kbdbResponse, "Person");
                            string stedKB = SearchKbDb(kbdbResponse, "Sted");
                            string ophavKB = SearchKbDb(kbdbResponse, "Ophav");
                            string aarKB = SearchKbDb(kbdbResponse, "År");
                            string noteKB = SearchKbDb(kbdbResponse, "Note");
                            string idKB = SearchKbDb(kbdbResponse, "Id");

                            searchCounter++;
                            counter++;
                            if ((counter % 100) == 0)
                                Trace.WriteLine(string.Format("Counter: {0}", counter));
                            Trace.WriteLine("verbose", string.Format("Counter: {0}", counter));
                            string title = lst[0].InnerText;
                            searchCondition[0].value = vejnavnKB == "" ? dataforsyningAdresser.Vejnavn : vejnavnKB;
                            searchCondition[1].value = husnummerKB == "" ? dataforsyningAdresser.Husnr : husnummerKB;
                            searchCondition[2].value = postnummerKB == "" ? dataforsyningAdresser.Postnummer : postnummerKB;
                            searchCondition[3].value = matrikelnummerKB == "" ? dataforsyningAdresser.Matrikelnr : matrikelnummerKB;
                            searchCondition[4].value = ejerlavKB == "" ? dataforsyningAdresser.Ejerlav : ejerlavKB;
                            searchCondition[5].value = kommuneKB == "" ? dataforsyningAdresser.Kommune : kommuneKB;
                            searchCondition[6].value = sognKB == "" ? dataforsyningAdresser.Sogn : sognKB;
                            searchCondition[7].value = bygningsnavnKB == "" ? dataforsyningSteder.PrimNavn : bygningsnavnKB;
                            searchCondition[8].value = byKB == "" ? dataforsyningAdresser.By : byKB;
                            string pictureFileName = PictureFileName(searchCounter, searchCondition[0].value, searchCondition[1].value, searchCondition[2].value, searchCondition[8].value, aarKB, idKB, searchCondition[7].value);
                            Trace.WriteLine("verbose", "Picture file name: " + pictureFileName);
                            Invoke(new Action(() => { label3.Text = searchCounter.ToString(); label5.Text = counter.ToString(); label8.Text = dfCounter.ToString(); label9.Text = circle.ToString(); label8.Text = circleCount.ToString(); label4.Text = pictureFileName; }));

                            itemList = new ItemList();
                            WriteSearchResult("________________________________________________________________________________", "");
                            WriteSearchResult("", "");
                            WriteSearchResult("Item nr:", searchCounter.ToString());
                            WriteSearchResult("Cirkel:", circle.ToString());
                            WriteSearchResult("Cirkel antal:", circleCount.ToString());
                            WriteSearchResult("Titel:", titelKB);
                            WriteSearchResult("Person:", personKB);
                            WriteSearchResult("Bygningsnavn:", bygningsnavnKB);
                            WriteSearchResult("Sted:", stedKB);
                            WriteSearchResult("Vejnavn:", vejnavnKB);
                            WriteSearchResult("Husnummer:", husnummerKB);
                            WriteSearchResult("Lokalitet:", lokalitetKB);
                            WriteSearchResult("Postnummer:", postnummerKB);
                            WriteSearchResult("By:", byKB);
                            WriteSearchResult("Sogn:", sognKB);
                            WriteSearchResult("Matrikelnummer:", matrikelnummerKB);
                            WriteSearchResult("Ophav:", ophavKB);
                            WriteSearchResult("År:", aarKB);
                            WriteSearchResult("Type:", lst[4].ChildNodes[4].FirstChild.InnerText);
                            ItemList.count = itemList.Count;
                            WriteSearchResult("Id:", idKB);
                            WriteSearchResult("Rigtighed:", lst[4].ChildNodes[11].FirstChild.InnerText);
                            WriteSearchResult("Interesse:", lst[4].ChildNodes[12].FirstChild.InnerText);
                            WriteSearchResult("Koordinat:", lst[3].FirstChild.InnerText);
                            WriteSearchResult("URL:", kbdb);
                            WriteSearchResult("Thumbnail:", lst[4].ChildNodes[8].FirstChild.InnerText);
                            WriteSearchResult("Foto:", lst[4].ChildNodes[7].FirstChild.InnerText);
                            WriteSearchResult("Note:", noteKB);

                            WriteSearchResult("", "");

                            WriteSearchResult("Bygningsnavn:.", primnavn);
                            WriteSearchResult("Sted:.", betegnelse);
                            WriteSearchResult("Vejnavn:.", vejnavn);
                            WriteSearchResult("Husnummer:.", husnummer);
                            WriteSearchResult("Lokalitet:.", lokalitet);
                            WriteSearchResult("Postnummer:.", postnummer);
                            WriteSearchResult("By:.", by);
                            WriteSearchResult("Sogn:.", sogn);
                            WriteSearchResult("Ejerlav:.", ejerlav);
                            WriteSearchResult("Matrikelnr:.", matrikelnr);
                            WriteSearchResult("Kommune:.", kommune);
                            WriteSearchResult("Politikreds:.", politikreds);
                            WriteSearchResult("Retskreds:.", retskreds);
                            WriteSearchResult("Region:.", region);
                            WriteSearchResult("Zome:.", zone);
                            WriteSearchResult("", "");
                            resultFileWriter.Flush();
                            resultList.Add(itemList);
                            Invoke(new Action(() => label11.Text = string.Format("{0:0.0} s/item", avgTimePerDownload / searchCounter / 10000000.0)));

                            DownloadPicture(lst[4].ChildNodes[7].FirstChild.InnerText, photoFile, pictureFileName);

                        }
                        node = node.NextSibling;
                    }
                    resultFileWriter.Close();
                    resultList.Sort(ItemList.Compare);
                    avgCoordinate[0] /= searchCounter;
                    avgCoordinate[1] /= searchCounter;
                    avgCircle /= searchCounter;
                    resultFileSortedWriter.WriteLine(string.Format(CultureInfo.InvariantCulture, "Center Koordinat: {0}, {1}", avgCoordinate[0], avgCoordinate[1]));
                    resultFileSortedWriter.WriteLine(string.Format("Middel cirkel: {0:0.0}", avgCircle));
                    resultFileSortedWriter.WriteLine();
                    WriteSortedResult();
                    resultFileSortedWriter.Close();
                }
                catch (Exception e3)
                {
                    Trace.WriteLine(string.Format("Error message: " + e3.Message));
                    Trace.WriteLine(string.Format("Stack trace:" + e3.StackTrace));
                    Trace.WriteLine(string.Format("Target site: " + e3.TargetSite));
                    MessageBox.Show("Huston, we have a problem");
                }
                finally
                {
                    Trace.WriteLine(string.Format("Counter: {0}", counter));
                    Trace.WriteLine(string.Format("Dataforsyningscounter: {0}", dfCounter));
                    Trace.WriteLine(string.Format("Search counter: {0}", searchCounter));
                    client.Dispose();
                    searching = false;
                    searchStopped = true;
                    Invoke(new Action(() => { button1.BackColor = Color.Green; button1.Text = ""; }));
                    Trace.WriteLine(string.Format("Finesearch completed"));
                    Trace.WriteLine("");
                }
                return true;
            }
            catch (Exception er)
            {
                Trace.WriteLine("Download error message: "+er.Message);
                Trace.WriteLine(er.StackTrace);
            }
            return true;
        }

        bool SearchCriteria(XmlNodeList lst, WebClient client, string kbdb, XmlNode node, ref int counter, ref int dfCounter)
        {
            vejnavn = "";
            husnummer = "";
            postnummer = "";
            betegnelse = "";
            lokalitet = "";
            by = "";
            sogn = "";
            kommune = "";
            politikreds = "";
            retskreds = "";
            region = "";
            ejerlav = "";
            matrikelnr = "";
            zone = "";
            primnavn = "";
            koordinat = lst[3].FirstChild.InnerText;
            dataforsyningAdresser = null;
            dataforsyningSteder = null;

            searchCondition[0].value = "";
            searchCondition[1].value = "";
            searchCondition[2].value = "";
            searchCondition[3].value = "";
            searchCondition[4].value = "";
            searchCondition[5].value = "";
            searchCondition[6].value = "";
            searchCondition[7].value = "";
            searchCondition[8].value = "";

            kbdbResponse = "";
            int ntry = 10;
            bool responseFailure = true;
            while (ntry > 0 && responseFailure)
            {
                responseFailure = false;
                try
                {

                    kbdbResponse = client.DownloadString(kbdb);
                }
                catch
                {
                    ntry--;
                    responseFailure = true;
                    string failReaponse = string.Format("KB response failure {0}", ntry);
                    Invoke(new Action(() => label4.Text = failReaponse));
                    Trace.WriteLine(failReaponse);
                    Trace.WriteLine(string.Format("URI: {0}", kbdb));
                    Thread.Sleep(200);
                }
            }
            if (responseFailure)
            {
                node = node.NextSibling;
                counter++;
                int cnt = counter;
                Trace.WriteLine(string.Format("Counter: {0}", counter));
                Invoke(new Action(() => { label4.Text = "Fatal response failure - Give up"; label5.Text = cnt.ToString(); }));
                Trace.WriteLine("Fatal response failure - Give up on: " + kbdb);
                return false;
            }
            vejnavnKB = SearchKbDb(kbdbResponse, "Vejnavn");
            husnummerKB = SearchKbDb(kbdbResponse, "Husnummer");
            postnummerKB = SearchKbDb(kbdbResponse, "Postnummer");
            matrikelnummerKB = SearchKbDb(kbdbResponse, "Matrikelnummer");
            ejerlavKB = "";
            kommuneKB = "";
            sognKB = SearchKbDb(kbdbResponse, "Sogn");
            bygningsnavnKB = SearchKbDb(kbdbResponse, "Bygningsnavn");
            byKB = SearchKbDb(kbdbResponse, "By");
            circle = maxCircle;
            dataforsyningAdresser = new DataforsyningAdresser("adresser", koordinat, ref circle, ref circleCount, token, client);
            dataforsyningSteder = new DataforsyningSteder("steder", koordinat, circle, circleCount, token, client);
            string vejn = dataforsyningAdresser.Vejnavn;

            Trace.WriteLine("verbose", "Max circle: " + maxCircle.ToString());
            Trace.WriteLine("verbose", "Circle: " + circle.ToString());
            if (dataforsyningAdresser.dataList != null &&  dataforsyningAdresser.dataList.Count != 0)
            {
                searchCondition[0].value = vejnavnKB == "" ? dataforsyningAdresser.Vejnavn : vejnavnKB;
                searchCondition[1].value = husnummerKB == "" ? dataforsyningAdresser.Husnr : husnummerKB;
                searchCondition[2].value = postnummerKB == "" ? dataforsyningAdresser.Postnummer : postnummerKB;
                searchCondition[3].value = matrikelnummerKB == "" ? dataforsyningAdresser.Matrikelnr : matrikelnummerKB;
                searchCondition[4].value = ejerlavKB == "" ? dataforsyningAdresser.Ejerlav : ejerlavKB;
                searchCondition[5].value = kommuneKB == "" ? dataforsyningAdresser.Kommune : kommuneKB;
                searchCondition[6].value = sognKB == "" ? dataforsyningAdresser.Sogn : sognKB;
                searchCondition[7].value = bygningsnavnKB == "" ? dataforsyningSteder.PrimNavn : bygningsnavnKB;
                searchCondition[8].value = byKB == "" ? dataforsyningAdresser.By : byKB;
            }
            else
            {
                searchCondition[0].value = vejnavnKB;
                searchCondition[1].value = husnummerKB;
                searchCondition[2].value = postnummerKB;
                searchCondition[3].value = matrikelnummerKB;
                searchCondition[4].value = ejerlavKB;
                searchCondition[5].value = kommuneKB;
                searchCondition[6].value = sognKB;
                searchCondition[7].value = bygningsnavnKB;
                searchCondition[8].value = byKB;

            }
            return true;
        }

        private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
        {
            var b = ((ChromiumWebBrowser)sender);
            ICookieManager man = webBrowser.GetCookieManager(null);


            //            this.InvokeOnUiThreadIfRequired(() => b.Focus());
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            //            Trace.WriteLine(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            //    this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            //SetCanGoBack(args.CanGoBack);
            //SetCanGoForward(args.CanGoForward);

            //this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            //      this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            //       this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

    }
}