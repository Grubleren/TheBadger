using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace JH.Applications
{
    public partial class FormMain : Form
    {
        void linkLabel3_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.linkLabel3.LinkVisited = true;

                System.Diagnostics.Process.Start(badgerResultFile);
            }
            catch
            {

            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.linkLabel3.LinkVisited = true;

                System.Diagnostics.Process.Start(badgerResultSortedFile);
            }
            catch
            {

            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel3.LinkVisited = true;

            System.Diagnostics.Process.Start("explorer.exe", resultFolder);

        }

        private void button2_Click(object sender, EventArgs e)  // download with pictures
        {
            downloadPictures = true;
            Download();
        }

        private void button3_Click(object sender, EventArgs e)  // download without pictures
        {
            downloadPictures = false;
            Download();
        }

        private void button4_Click(object sender, EventArgs e)  // stop download
        {
            searching = false;
        }

        private void button1_Click(object sender, EventArgs e)  // stop download
        {
            searching = false;
        }

        private void button5_Click(object sender, EventArgs e)  // browser back
        {
            urlHistory.RemoveAt(urlHistory.Count - 1);
            string src = urlHistory[urlHistory.Count - 1];
            string dst = ToUTF8(src);

            webBrowser.Navigate(dst);
        }

        private void button6_Click(object sender, EventArgs e)  // save token
        {
            token = textBox1.Text;
            StreamWriter writer = new StreamWriter(tokenPath);
            writer.WriteLine(token);
            writer.Close();

        }

        private void button7_Click(object sender, EventArgs e)  // calibrate ne
        {
            BrowserCenter(out neZoom, out neLat, out neLng);

            deltaLat = swLat * (1 << neZoom) - neLat * (1 << swZoom);
            deltaLng = swLng * (1 << neZoom) - neLng * (1 << swZoom);

            StreamWriter writer = new StreamWriter(mapCalibrationPath);
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat));
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng));
            writer.Close();
        }

        private void button8_Click(object sender, EventArgs e)  // calibrate sw
        {
            BrowserCenter(out swZoom, out swLat, out swLng);

            deltaLat = swLat * (1 << neZoom) - neLat * (1 << swZoom);
            deltaLng = swLng * (1 << neZoom) - neLng * (1 << swZoom);

            StreamWriter writer = new StreamWriter(mapCalibrationPath);
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLat));
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", deltaLng));
            writer.Close();

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (searchDialog.IsDisposed)
                searchDialog = new SearchDialog(this);
            DisplayDialog(searchDialog);

        }

        private void DisplayDialog(Form form)
        {
            searchDialog.checkBox2.Checked = searchConditionShadow[3].check;
            searchDialog.checkBox3.Checked = searchConditionShadow[4].check;
            searchDialog.checkBox4.Checked = searchConditionShadow[5].check;
            searchDialog.checkBox5.Checked = searchConditionShadow[6].check;
            searchDialog.textBox2.Text = searchConditionShadow[3].searchValue;
            searchDialog.textBox3.Text = searchConditionShadow[4].searchValue;
            searchDialog.textBox4.Text = searchConditionShadow[5].searchValue;
            searchDialog.textBox5.Text = searchConditionShadow[6].searchValue;
            form.Show();
            form.WindowState = FormWindowState.Normal;
            form.BringToFront();
        }


    }
}
