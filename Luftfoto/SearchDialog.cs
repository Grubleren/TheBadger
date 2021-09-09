using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JH.Applications
{
    public partial class SearchDialog : Form
    {
        FormMain form;

        public SearchDialog(FormMain form)
        {
            InitializeComponent();
            this.form = form;
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[0].searchValue = ((TextBox)sender).Text;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[1].searchValue = ((TextBox)sender).Text;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[2].searchValue = ((TextBox)sender).Text;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[3].searchValue = ((TextBox)sender).Text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[4].searchValue = ((TextBox)sender).Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[5].searchValue = ((TextBox)sender).Text;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[6].searchValue = ((TextBox)sender).Text;
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[0].check = checkBox1.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[1].check = checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[2].check = checkBox7.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[3].check = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[4].check = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[5].check = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            form.searchConditionShadow[6].check = checkBox5.Checked;
        }

    }
}
