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
using System.Collections;

namespace JH.Applications
{
    public partial class FormMain : Form
    {
        string version = "The Badger ver. 2.6";
        string path;
        string token;
        string tokenPath;
        string mapCalibrationPath;
        string projectFolder;
        string resultFolder;
        string badgerResultPath;
        string badgerResultSortedPath;
        string badgerResultFile;
        string badgerResultSortedFile;
        string badgerThumbnailPath;
        string badgerFullsizePath;
        StreamWriter badgerResultFileWriter;
        StreamWriter badgerResultFileSortedWriter;
        bool searching;
        bool searchStopped = true;
        bool downloadPictures;
        string kbObjets;
        List<string> urlHistory;
        double neLat;
        double neLng;
        double swLat;
        double swLng;
        int neZoom;
        int swZoom;
        double deltaLat = 300;
        double deltaLng = 1360;
        Size sizeWebBrowserBefore;
        List<ItemList> resultList;
        ItemList itemList;
        string lastUrlPath;
        string lastUrlString;
        SearchDialog searchDialog;
        public SearchCondition[] searchCondition;
        public SearchCondition[] searchConditionShadow;
        string freetext = "";
        string location = "";
        string building = "";
        string person = "";
        string notBefore = "";
        string notAfter = "";
        int max;
        int circle;
        int maxCircle;
        string results = "Results";
        string searchResult = "SearchResult";
        string sortedResult = "SortedResult";
        string ext;
        string exttxt;

        public class SearchCondition
        {
            public bool check;
            public string value;
            public string searchValue;
        }
        public FormMain()
        {
            InitializeComponent();
            urlHistory = new List<string>();
            resultList = new List<ItemList>();
            itemList = new ItemList();
            searchCondition = new SearchCondition[7];
            searchConditionShadow = new SearchCondition[7];
            for (int i = 0; i < searchCondition.Length; i++)
                searchCondition[i] = new SearchCondition();
            for (int i = 0; i < searchConditionShadow.Length; i++)
                searchConditionShadow[i] = new SearchCondition();
            searchDialog = new SearchDialog(this);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Text = version;
            label3toolTip.SetToolTip(label3, "Hit tæller, finsøgning");
            label4toolTip.SetToolTip(label4, "Hit titel");
            label5toolTip.SetToolTip(label5, "Total hit tæller");
            label6toolTip.SetToolTip(label6, "Antallet af of KB hits, max 75");
            label7toolTip.SetToolTip(label7, "Total antal hits");
            label8toolTip.SetToolTip(label8, "Dataforsynig hit tæller");
            label9toolTip.SetToolTip(label9, "Cirkel værdi");
            linkLabel3toolTip.SetToolTip(linkLabel3, "Vis kb_FullSize folder");
            linkLabel1toolTip.SetToolTip(linkLabel1, "Vis sortet resultatfil");
            linkLabel4toolTip.SetToolTip(linkLabel4, "Brugsanvisning");
            comboBox1toolTip.SetToolTip(comboBox1, "Filnavngivnings vælger");
            button1toolTip.SetToolTip(button1, "Status indikator");
            button2toolTip.SetToolTip(button2, "Download hits med billeder");
            button3toolTip.SetToolTip(button3, "Download hits uden billeder");
            button4toolTip.SetToolTip(button4, "Stop download");
            button5toolTip.SetToolTip(button5, "Browse tibage");
            button6toolTip.SetToolTip(button6, "Gem token");
            button7toolTip.SetToolTip(button7, "Nord-Øst");
            button8toolTip.SetToolTip(button8, "Syd-Vest");
            button9toolTip.SetToolTip(button9, "Flere  finsøgnings kriterier");

            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";

            projectFolder = @"..";
            resultFolder = projectFolder + @"\" + results;
            if (!Directory.Exists(resultFolder))
                Directory.CreateDirectory(resultFolder);
            badgerResultPath = resultFolder + @"\" + searchResult;
            badgerResultSortedPath = resultFolder + @"\" + sortedResult;
            badgerThumbnailPath = resultFolder + @"\kb_tumbnails";
            badgerFullsizePath = resultFolder + @"\kb_Fullsize";
            tokenPath = projectFolder + @"\DataForsyningToken.txt";
            lastUrlPath = projectFolder + @"\LastUrl.txt";
            mapCalibrationPath = projectFolder + @"\MapCalibration.txt";
            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles(resultFolder + "\\");
                foreach (string file in files)
                {
                    int count = int.Parse(file.Substring(file.Length - 8, 4));
                    if (count > max)
                        max = count;
                }
            }
            catch
            {
                MessageBox.Show("Please remove files from the " + resultFolder + " folder not created by this program");
                searching = false;
                searchStopped = true;
                button1.BackColor = Color.Green;
                return;
            }
            ext = string.Format("{0:0000}", max);
            exttxt = ext + ".txt";
            badgerResultFile = badgerResultPath + exttxt;
            badgerResultSortedFile = badgerResultSortedPath + exttxt;
            Invoke(new Action(() => linkLabel3.Text = "kb_FullSize" + ext));
            Invoke(new Action(() => linkLabel1.Text = sortedResult + exttxt));

            StreamReader reader;
            if (File.Exists(tokenPath))
            {
                reader = new StreamReader(tokenPath);
                token = reader.ReadLine();
                reader.Close();
                textBox1.Text = token;
            }

            if (File.Exists(mapCalibrationPath))
            {
                reader = new StreamReader(mapCalibrationPath);
                deltaLat = double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
                deltaLng = double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
                reader.Close();
            }
            else
                MessageBox.Show("Map not calibrated");

            if (File.Exists(lastUrlPath))
            {
                reader = new StreamReader(lastUrlPath);
                lastUrlString = reader.ReadLine();
                reader.Close();
            }
            else
                lastUrlString = "http://www5.kb.dk/danmarksetfraluften/?q_fritekst=S%c3%B8nderladevej&q_stednavn=&q_bygningsnavn=&q_person=&notBefore=1890&notAfter=2015&category=subject203&itemType=all&thumbnailSize=&correctness=&thumbnailSize=&sortby=&sortorder=#zoom=9&lat=57.34&lng=9.27";

            webBrowser.Navigated += new WebBrowserNavigatedEventHandler(Navigated);
            webBrowser.Navigate(lastUrlString);
            webBrowser.Show();
            urlHistory.Add(lastUrlString);
            sizeWebBrowserBefore = webBrowser.Size;
        }

        void FormMain_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                Size size = this.Size;
                webBrowser.Width = size.Width - webBrowser.Location.X - 50;
                webBrowser.Height = size.Height - webBrowser.Location.Y - 50;
                Size sizeWebBrowserAfter = webBrowser.Size;
                deltaLat *= (double)sizeWebBrowserAfter.Height / sizeWebBrowserBefore.Height;
                deltaLng *= (double)sizeWebBrowserAfter.Width / sizeWebBrowserBefore.Width;
                int zoom;
                double lat;
                double lng;
                BrowserCenter(out zoom, out lat, out lng);
            }
            catch
            {

            }
        }

        void Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            urlHistory.Add(webBrowser.Url.ToString());
        }

        void Download()
        {
            try
            {
                if (!searchStopped)
                    return;
                searchStopped = false;

                searchCondition[0].check = checkBox1.Checked;
                searchCondition[0].searchValue = textBox2.Text;
                searchCondition[1].check = checkBox2.Checked;
                searchCondition[1].searchValue = textBox3.Text;
                searchCondition[2].check = checkBox2.Checked;
                searchCondition[2].searchValue = textBox4.Text;
                searchCondition[3].check = searchConditionShadow[3].check;
                searchCondition[3].searchValue = searchConditionShadow[3].searchValue;
                searchCondition[4].check = searchConditionShadow[4].check;
                searchCondition[4].searchValue = searchConditionShadow[4].searchValue;
                searchCondition[5].check = searchConditionShadow[5].check;
                searchCondition[5].searchValue = searchConditionShadow[5].searchValue;
                searchCondition[6].check = searchConditionShadow[6].check;
                searchCondition[6].searchValue = searchConditionShadow[6].searchValue;

                HtmlDocument doc = webBrowser.Document;
                HtmlElementCollection f = doc.GetElementById("search-form").GetElementsByTagName("input");
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

                maxCircle = int.Parse(textBox5.Text);
                if (maxCircle < 4)
                {
                    maxCircle = 4;
                    textBox5.Text = "4";
                }
                if (maxCircle > 200)
                {
                    maxCircle = 200;
                    textBox5.Text = "200";
                }
                button1.BackColor = Color.Blue;
                label3.Text = "";
                label5.Text = "";
                label6.Text = "";
                label7.Text = "";
                label8.Text = "";
                label9.Text = "";
                int zoom;
                double lat;
                double lng;
                BrowserCenter(out zoom, out lat, out lng);

                string coordinates = (lng + deltaLng / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture) + "," + (lat + deltaLat / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture) + "," + (lng - deltaLng / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture) + "," + (lat - deltaLat / (1 << zoom) / 2).ToString(CultureInfo.InvariantCulture);
                string uri = "http://www.kb.dk/cop/syndication/images/luftfo/2011/maj/luftfoto/subject203/?format=kml&type=all&bbo=";
                uri += coordinates + "&notBefore=" + notBefore + "-01-01" + "&notAfter=" + notAfter + "-01-01";
                uri += "&itemsPerPage=5000&random=0.0&query=" + query;

                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                path = projectFolder + @"\temp.xml";
                client.DownloadFileAsync(new Uri(uri), path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                searchStopped = true;
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            searching = true;
            button1.BackColor = Color.Red;
            HtmlDocument doc = webBrowser.Document;
            string leftMenu = doc.GetElementById("leftmenu").InnerHtml;
            int len;
            kbObjets = KbObjects(leftMenu, out len);
            label3.Text = "";
            label4.Text = "";
            label8.Text = "";
            label9.Text = "";
            label6.Text = len.ToString();
            Thread thread = new Thread(new ThreadStart(ProcessDownload));
            thread.Start();
        }

        void ProcessDownload()
        {
            try
            {
                resultList.Clear();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                List<List<string>> items = new List<List<string>>();
                int searchCounter = 0;
                max++;
                ext = string.Format("{0:0000}", max);
                exttxt = ext + ".txt";
                badgerResultFile = badgerResultPath + exttxt;
                badgerResultSortedFile = badgerResultSortedPath + exttxt;
                badgerResultFileWriter = new StreamWriter(badgerResultFile);
                badgerResultFileSortedWriter = new StreamWriter(badgerResultSortedFile);
                Invoke(new Action(() => linkLabel3.Text = "kb_FullSize" + ext));
                Invoke(new Action(() => linkLabel1.Text = sortedResult + exttxt)); ;
                WriteHeaders(DateTime.Now.ToString());

                string badgerThumbnailFile = badgerThumbnailPath + string.Format("{0:0000}", max);
                string badgerFullsizeFile = badgerFullsizePath + string.Format("{0:0000}", max);
                Directory.CreateDirectory(badgerThumbnailFile);
                Directory.CreateDirectory(badgerFullsizeFile);
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(stream);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);
                stream.Close();
                XmlNode node = xmlDoc.DocumentElement.FirstChild.FirstChild;
                int counter = 0;

                while (node != null && searching)
                {
                    if (node.Name == "Placemark")
                        counter++;
                    node = node.NextSibling;
                }
                Invoke(new Action(() => label7.Text = counter.ToString()));
                node = xmlDoc.DocumentElement.FirstChild.FirstChild;
                counter = 0;
                int dfCounter = 0;


                while (node != null && searching)
                {
                    if (node.Name == "Placemark")
                    {

                        XmlNodeList lst = node.ChildNodes;
                        string kbdb = lst[2].Attributes[0].Value;
                        string[] kbdbSplit = kbdb.Split(new char[] { '/' });
                        string kbObject = kbdbSplit[kbdbSplit.Length - 1];
                        //if (!kbObjets.Contains(kbObject))
                        //{
                        //    node = node.NextSibling;
                        //    counter++;
                        //    Invoke(new Action(() => label5.Text = counter.ToString()));
                        //    continue;

                        //}

                        string vejnavn = "";
                        string husnummer = "";
                        string postnummer = "";
                        string betegnelse = "";
                        string lokalitet = "";
                        string by = "";
                        string sogn = "";
                        string kommune = "";
                        string politikreds = "";
                        string retskreds = "";
                        string region = "";
                        string ejerlav = "";
                        string matrikelnr = "";
                        string zone = "";
                        string primnavn = "";
                        string koordinat = lst[3].FirstChild.InnerText;
                        DataforsyningAdresser dataforsyningAdresser = null;
                        DataforsyningSteder dataforsyningSteder = null;

                        searchCondition[0].value = "";
                        searchCondition[1].value = "";
                        searchCondition[2].value = "";
                        searchCondition[3].value = "";
                        searchCondition[4].value = "";
                        searchCondition[5].value = "";
                        searchCondition[6].value = "";

                        string kbdbResponse = client.DownloadString(kbdb);
                        string vejnavnKB = SearchKbDb(kbdbResponse, "Vejnavn");
                        string husnummerKB = SearchKbDb(kbdbResponse, "Husnummer");
                        string postnummerKB = SearchKbDb(kbdbResponse, "Postnummer");
                        string matrikelnummerKB = SearchKbDb(kbdbResponse, "Matrikelnummer");
                        string ejerlavKB = "";
                        string kommuneKB = "";
                        string sognKB = SearchKbDb(kbdbResponse, "Sogn");
                        double circ = 4;
                        while (circ <= maxCircle)
                        {
                            circle = (int)Math.Round(circ);
                            dataforsyningAdresser = new DataforsyningAdresser("adresser", koordinat, circle, token, client);
                            string vejn = dataforsyningAdresser.Vejnavn;
                            if (vejn != "")
                            {
                                dfCounter++;
                                break;
                            }
                            circ *= Math.Pow(2, 0.5);
                        }
                        circle = circ > maxCircle ? maxCircle : circle;

                        if (dataforsyningAdresser.dataList.Count != 0)
                        {
                            searchCondition[0].value = vejnavnKB == "" ? dataforsyningAdresser.Vejnavn : vejnavnKB;
                            searchCondition[1].value = husnummerKB == "" ? dataforsyningAdresser.Husnr : husnummerKB;
                            searchCondition[2].value = postnummerKB == "" ? dataforsyningAdresser.Postnummer : postnummerKB;
                            searchCondition[3].value = matrikelnummerKB == "" ? dataforsyningAdresser.Matrikelnr : matrikelnummerKB;
                            searchCondition[4].value = ejerlavKB == "" ? dataforsyningAdresser.Ejerlav : ejerlavKB;
                            searchCondition[5].value = kommuneKB == "" ? dataforsyningAdresser.Kommune : kommuneKB;
                            searchCondition[6].value = sognKB == "" ? dataforsyningAdresser.Sogn : sognKB;
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

                        }
                        if (NotFoundSearchChriteria(searchCondition))
                        {
                            node = node.NextSibling;
                            counter++;
                            Invoke(new Action(() => label5.Text = counter.ToString()));
                            continue;
                        }

                        dataforsyningSteder = new DataforsyningSteder("steder", koordinat, circle, token, client);
                        if (dataforsyningSteder.dataList.Count != 0)
                            primnavn = dataforsyningSteder.PrimNavn;

                        if (dataforsyningAdresser.dataList.Count != 0)
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
                        string bygningsnavnKB = SearchKbDb(kbdbResponse, "Bygningsnavn");
                        string stedKB = SearchKbDb(kbdbResponse, "Sted");
                        string byKB = SearchKbDb(kbdbResponse, "By");
                        string ophavKB = SearchKbDb(kbdbResponse, "Ophav");
                        string aarKB = SearchKbDb(kbdbResponse, "År");
                        string noteKB = SearchKbDb(kbdbResponse, "Note");
                        string idKB = SearchKbDb(kbdbResponse, "Id");

                        searchCounter++;
                        counter++;
                        string title = lst[0].InnerText;
                        Invoke(new Action(() => { label3.Text = searchCounter.ToString(); label5.Text = counter.ToString(); label8.Text = dfCounter.ToString(); label9.Text = circle.ToString(); label4.Text = title; }));

                        itemList = new ItemList();
                        WriteSearchResult("________________________________________________________________________________", "");
                        WriteSearchResult("","");
                        WriteSearchResult("Item nr:", searchCounter.ToString());
                        WriteSearchResult("Cirkel:", circle.ToString());
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
                        badgerResultFileWriter.Flush();
                        resultList.Add(itemList);

                        DownloadPicture(lst[4].ChildNodes[8].FirstChild.InnerText, badgerThumbnailFile, searchCounter, vejnavnKB, husnummerKB, postnummerKB, byKB,aarKB, idKB, bygningsnavnKB);
                        DownloadPicture(lst[4].ChildNodes[7].FirstChild.InnerText, badgerFullsizeFile, searchCounter, vejnavnKB, husnummerKB, postnummerKB, byKB, aarKB, idKB, bygningsnavnKB);

                    }
                    node = node.NextSibling;
                }
                badgerResultFileWriter.Close();
                resultList.Sort(ItemList.Compare);
                WriteSortedResult();
                badgerResultFileSortedWriter.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Huston, we have a problem");
                MessageBox.Show(e.Message);
            }
            finally
            {
                searching = false;
                searchStopped = true;
                button1.BackColor = Color.Green;
            }
        }
        class ItemList : List<string>, IComparer
        {
            static public int Compare(ItemList l0, ItemList l1)
            {

                return string.Compare(l0[15], l1[15]);
            }
            public int Compare(object l0, object l1)
            {

                return string.Compare(((List<string>)l0)[15], ((List<string>)l1)[15]);
            }
        }


    }
}