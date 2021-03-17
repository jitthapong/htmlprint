using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HtmlPrint
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                comboPrinter.Items.Add(printer);
            }

            try
            {
                txtHtml.Text = File.ReadAllText("bill.html");
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var printerName = comboPrinter.SelectedItem?.ToString() ?? "EPSON TM-T82 Receipt";
            if (string.IsNullOrEmpty(printerName))
                return;

            var html = txtHtml.Text;
            if (string.IsNullOrEmpty(html))
                return;

            try
            {
                HtmlPrintLib.HtmlPrint htmlPrint = new HtmlPrintLib.HtmlPrint(html, printerName);
                htmlPrint.ResizeRatio = Convert.ToInt32(txtAdj.Text ?? "0");
                htmlPrint.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
