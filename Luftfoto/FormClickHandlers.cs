using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace JH.Applications
{
    public partial class FormMain : Form
    {
        void linkLabel3_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Trace.WriteLine(string.Format("Fullsize path clicked"));
            try
            {
                this.linkLabel3.LinkVisited = true;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    string s = Environment.CurrentDirectory + @"\" + photoPath + ext;
                    Clipboard.SetText(s);
                }
                else
                {
                    string s = photoPath + ext;
                    System.Diagnostics.Process.Start(s);
                }
            }
            catch
            {

            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Trace.WriteLine(string.Format("Sorted path clicked"));
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    string s = Environment.CurrentDirectory + @"\" + photoPath + ext + @"\" + sortedResult + exttxt;
                    Clipboard.SetText(s);
                }
                else
                {
                    this.linkLabel3.LinkVisited = true;
                    string s = photoPath + ext + @"\" + sortedResult + exttxt;
                    System.Diagnostics.Process.Start(s);
                }
            }
            catch
            {

            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Trace.WriteLine(string.Format("Manual clicked"));
            this.linkLabel3.LinkVisited = true;

            System.Diagnostics.Process.Start(@"..\theBadger.pdf");

        }

        private void button2_Click(object sender, EventArgs e)  // download with pictures
        {
            Trace.WriteLine(string.Format("Download with pictures clicked"));
            downloadPictures = true;
            Thread thread = new Thread(new ThreadStart(DownloadInit));
            thread.Start();
        }

        private void button3_Click(object sender, EventArgs e)  // download without pictures
        {
            Trace.WriteLine(string.Format("Download without pictures clicked"));
            downloadPictures = false;
            Thread thread = new Thread(new ThreadStart(DownloadInit));
            thread.Start();
        }

        private void button4_Click(object sender, EventArgs e)  // stop download
        {
            Trace.WriteLine(string.Format("Stop download clicked"));
            searching = false;
        }

        private void button1_Click(object sender, EventArgs e)  // stop download
        {
            Trace.WriteLine("Colored button clicked");
            if (button1.BackColor == Color.Green)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    Trace.WriteLine(string.Format("Download without pictures"));
                    downloadPictures = false;
                }
                else
                {
                    Trace.WriteLine(string.Format("Download with pictures"));
                    downloadPictures = true;
                }
                Thread thread = new Thread(new ThreadStart(DownloadInit));
                thread.Start();
            }
            else
            {
                Trace.WriteLine(string.Format("Stop download"));
                searching = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)  // browser back
        {
            Trace.WriteLine(string.Format("Browse back clicked"));
            urlHistoryPointer--;
            if (urlHistoryPointer < 0)
                urlHistoryPointer = 0;
            else
            {
                string src = urlHistory[urlHistoryPointer];
                string dst = ToUTF8(src);

                webBrowser.Load(dst);
                addToUrlHistory = false;
                button14.Enabled = true;
            }
            if (urlHistoryPointer == 0)
                button5.Enabled = false;
        }

        private void button14_Click(object sender, EventArgs e)  // browser forward
        {
            Trace.WriteLine(string.Format("Browse back clicked"));     
            if (urlHistoryPointer== urlHistory.Count-1)
            {
                button14.Enabled = false;
            }
            string src = urlHistory[urlHistory.Count-1];
            string dst = ToUTF8(src);

            webBrowser.Load(dst);
            addToUrlHistory = false;
            if (urlHistoryPointer > 0)
                button5.Enabled = true;
        }

        private void button6_Click(object sender, EventArgs e)  // save token
        {
            Trace.WriteLine("Save token clicked");
            token = textBox1.Text;
            StreamWriter writer = new StreamWriter(tokenPath);
            writer.WriteLine(token);
            writer.Close();

        }

        private void button7_Click(object sender, EventArgs e)  // calibrate ne
        {
            Trace.WriteLine("NE clicked");
            BrowserCenter(out neZoom, out neLat, out neLng);

            deltaLat = swLat * (1 << neZoom) - neLat * (1 << swZoom);
            deltaLng = swLng * (1 << neZoom) - neLng * (1 << swZoom);

            StreamWriter writer = new StreamWriter(mapCalibrationPath);
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat));
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng));
            writer.Close();
            Trace.WriteLine(string.Format(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat)));
            Trace.WriteLine(string.Format(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng)));
        }

        private void button8_Click(object sender, EventArgs e)  // calibrate sw
        {
            Trace.WriteLine("SW clicked");
            BrowserCenter(out swZoom, out swLat, out swLng);

            deltaLat = swLat * (1 << neZoom) - neLat * (1 << swZoom);
            deltaLng = swLng * (1 << neZoom) - neLng * (1 << swZoom);

            StreamWriter writer = new StreamWriter(mapCalibrationPath);
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat));
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng));
            writer.Close();
            Trace.WriteLine(string.Format(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat)));
            Trace.WriteLine(string.Format(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng)));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("More clicked");
            if (searchDialog.IsDisposed)
                searchDialog = new SearchDialog(this);
            DisplayDialog(searchDialog);

        }

        private void DisplayDialog(Form form)
        {
            searchDialog.checkBox1.Checked = searchConditionShadow[0].check;
            searchDialog.checkBox6.Checked = searchConditionShadow[1].check;
            searchDialog.checkBox7.Checked = searchConditionShadow[2].check;
            searchDialog.checkBox2.Checked = searchConditionShadow[3].check;
            searchDialog.checkBox3.Checked = searchConditionShadow[4].check;
            searchDialog.checkBox4.Checked = searchConditionShadow[5].check;
            searchDialog.checkBox5.Checked = searchConditionShadow[6].check;
            searchDialog.textBox1.Text = searchConditionShadow[0].searchValue;
            searchDialog.textBox6.Text = searchConditionShadow[1].searchValue;
            searchDialog.textBox7.Text = searchConditionShadow[2].searchValue;
            searchDialog.textBox2.Text = searchConditionShadow[3].searchValue;
            searchDialog.textBox3.Text = searchConditionShadow[4].searchValue;
            searchDialog.textBox4.Text = searchConditionShadow[5].searchValue;
            searchDialog.textBox5.Text = searchConditionShadow[6].searchValue;
            form.Show();
            form.WindowState = FormWindowState.Normal;
            form.BringToFront();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                circle = int.Parse(textBox5.Text);
                Trace.WriteLine("Circle changed, circle: " + circle.ToString());
            }
            catch
            {
                Trace.WriteLine("Parse error when parsing max circle");
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Batch processing begin");
            if (!searchStopped)
                return;
            label10.Text = "";
            searchStopped = false;
            batchProcessing = true;
            downloadPictures = true;
            Thread thread = new Thread(new ThreadStart(Batch));
            thread.Start();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Batch processing begin");
            if (!searchStopped)
                return;
            label10.Text = "";
            searchStopped = false;
            batchProcessing = true;
            downloadPictures = false;
            Thread thread = new Thread(new ThreadStart(Batch));
            thread.Start();
        }

        void Batch()
        {
            string batchInputFile = projectFolder + @"\batchInput.txt";
            StreamReader batchInputStream = new StreamReader(batchInputFile);
            batchInput = new List<BatchInput>();
            batchCoordinates = new List<double[]>();
            Trace.WriteLine("Reading batchInput.txt");
            while (!batchInputStream.EndOfStream)
            {
                string s = batchInputStream.ReadLine();
                string[] split = s.Split(new char[] { '\t' });
                if (split.Length == 1)
                {
                    batchInput.Add(new BatchInput(split[0], ""));
                    Trace.WriteLine(string.Format("Query and maybe link: {0}", s));
                }
                else if (split.Length == 2)
                {
                    batchInput.Add(new BatchInput(split[0], split[1]));
                    Trace.WriteLine(string.Format("Query and maybe link: {0}", s));
                }
                else
                {
                    batchInput.Add(new BatchInput(split[0], split[1]));
                    Trace.WriteLine(string.Format("Query and maybe link: {0}", s));
                }
            }
            batchInputStream.Close();
            Trace.WriteLine("Processing batch");
            int count = 0;
            foreach (BatchInput input in batchInput)
            {
                if (!batchProcessing)
                    break;
                Trace.WriteLine(string.Format("Downloading query: {0}", input.query));
                Invoke(new Action(() => label10.Text = string.Format("{0} af {1}", count + 1, batchInput.Count)));
                Download(input.query);
                count++;
                Trace.WriteLine("Downloading done");
                batchCoordinates.Add(avgCoordinate);
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Avg. longitude: {0} Avg. latitude: {1}", avgCoordinate[0], avgCoordinate[1]));
                searchStopped = false;
            }
            batchProcessing = false;
            searchStopped = true;
            string header = "<html><head><title></title><script src = \"okapi.js\"></script> </head> <body> <div  id = \"map\"  class=\"geomap\"  data-token=\"" + token + "\"data-zoom = \"5\"></div>";
            string middle = "";
            for (int i = 0; i < batchCoordinates.Count; i++)
            {
                middle += string.Format(CultureInfo.InvariantCulture, "<span class=\"geomarker\" data-lon=\"{2}\" data-lat=\"{3}\"data-description=\"<a href = '{1}' target='_blank'>{0}</a>\"></span>\r\n", batchInput[i].query, batchInput[i].hyperLink, batchCoordinates[i][0], batchCoordinates[i][1]);
            }
            string tail = "<script>var map = new okapi.Initialize({});</script></ body ></html>";
            StreamWriter sw = new StreamWriter("../batchMap.html");
            sw.WriteLine(header);
            sw.Write(middle);
            sw.WriteLine(tail);
            sw.Close();
            if (!File.Exists("../batchMapTotal.html"))
                File.Copy("../batchMap.html", "../batchMapTotal.html");
            else
            {
                List<string> lst = new List<string>();
                StreamReader srt = new StreamReader("../batchMapTotal.html");
                while (!srt.EndOfStream)
                {
                    string s = srt.ReadLine();
                    lst.Add(s);
                }
                srt.Close();
                StreamWriter swt = new StreamWriter("../tmp.html");
                swt.WriteLine(header);
                for (int i = 1; i < lst.Count - 1; i++)
                    swt.WriteLine(lst[i]);
                swt.Write(middle);
                swt.WriteLine(tail);
                swt.Close();
                File.Copy("../tmp.html", "../batchMapTotal.html",true);
                File.Delete("../tmp.html");
            }
            Trace.WriteLine("Batch processing end");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (!batchProcessing)
                return;
            batchProcessing = false;
            searching = false;
            Trace.WriteLine("Batch processing stopped");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Show map");
            webBrowser.Load("file:///" + Environment.CurrentDirectory + "/../batchMap.html");
            addToUrlHistory = true;
        }
        private void button15_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Show total map");
            webBrowser.Load("file:///" + Environment.CurrentDirectory + "/../batchMapTotal.html");
            addToUrlHistory = true;

        }

    }
}

