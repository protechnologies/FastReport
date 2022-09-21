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
using FastReport;
using FastReport.Export.Image;
using FastReport.Utils;

namespace WinFormsOpenSource
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadReportList();
            data.ReadXml(reportsPath + "nwind.xml"); //Загружаем в источник данных базу
            pictureBox1.Visible = false;
            //pictureBox1.Width = 793;
            //pictureBox1.Height = 1122;
        }

        public Report Report { get; set; } = new Report();
        public int width;
        public int height;
        public ImageExport exp = new ImageExport(); //Создаем экспорт отчета в формат изображения
        public string reportsPath = Config.ApplicationFolder + @"..\..\Reports\";
        public int CurrentPage = 0;
        public List<Image> pages = new List<Image>();
        private DataSet data = new DataSet(); //Создаем источник данных


        public void LoadReportList()
        {
            List<string> filesname = Directory.GetFiles(reportsPath, "*.frx").ToList<string>();

            foreach (string file in filesname)
            {
                ReportsList.Items.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private void CenterReport()
        {
            var left = (panel1.ClientSize.Width - pictureBox1.Width /*- panel1.Padding.Horizontal*/) / 2;
            pictureBox1.Left = left < panel1.Padding.Left ? panel1.Padding.Left : left;
        }

        private void ZoomReport(int factor)
        {
            using (MemoryStream stream = new MemoryStream())
            {

                exp.ImageFormat = ImageExportFormat.Png; //Устанавливаем для изображения формат png
                exp.Resolution += factor;
                exp.PageNumbers = (CurrentPage + 1).ToString();
                exp.Export(Report, stream); //Выполняем экспорт отчета в файл
                pictureBox1.Image = Image.FromStream(stream);
                CenterReport();
            }
        }

        private void LoadReport(string fileName)
        {
            Report.Load(fileName); //Загружаем шаблон отчета
            Report.RegisterData(data, "NorthWind"); //Регистрируем источник данных в отчете 
            Report.Prepare(); //Выполняем предварительное построение отчета
            ReportExport();
        }

        private void LoadPreparedReport(string fileName)
        {
            Report.LoadPrepared(fileName);
            ReportExport();
        }

        public void ReportExport()
        {
            DeleteTempFiles();
            exp.ImageFormat = ImageExportFormat.Png; //Устанавливаем для изображения формат png
            exp.Export(Report, Directory.GetCurrentDirectory() + "/" + System.Guid.NewGuid() + ".png"); //Выполняем экспорт отчета в файл
            foreach (string fileName in exp.GeneratedFiles)
            {
                using (var img = File.OpenRead(fileName))
                {
                    pages.Add(Image.FromStream(img));
                }
            }
            CurrentPage = 0;
            ShowReport();
        }

        public void DeleteTempFiles()
        {
            pages.Clear();
            pictureBox1.Visible = false;
            pictureBox1.Image = null;
            pictureBox1.Invalidate();
            FileInfo[] path = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.png", SearchOption.AllDirectories);
            foreach (FileInfo file in path)
            {

                File.Delete(file.FullName);
            }

        }

        public void ShowReport()
        {
            if (CurrentPage >= 0 && CurrentPage < pages.Count)
            {
                pictureBox1.Image = pages[CurrentPage]; //Устанавливаем изображение
                toolStripPageNum.Text = (CurrentPage + 1).ToString();
                toolStripPagesCount.Text = $"/ {pages.Count}";
                CenterReport();
                pictureBox1.Visible = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ReportsList.SelectedItem != null)
            {
                LoadReport(reportsPath + ReportsList.SelectedItem.ToString() + ".frx");
            }
        }

        private void toolStripOpnBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Report file (*.frx)|*.frx|Prapared report (*.fpx)|*.fpx";
                dialog.Multiselect = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    switch (dialog.FilterIndex)
                    {
                        case 1:
                            LoadReport(dialog.FileName);
                            break;

                        case 2:
                            LoadPreparedReport(dialog.FileName);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void toolStripFirstBtn_Click(object sender, EventArgs e)
        {
            CurrentPage = 0;
            ShowReport();
        }

        private void toolStripPrewBtn_Click(object sender, EventArgs e)
        {
            CurrentPage--;
            ShowReport();
        }

        private void toolStripNextBtn_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            ShowReport();
        }

        private void toolStripLastBtn_Click(object sender, EventArgs e)
        {
            CurrentPage = pages.Count - 1;
            ShowReport();
        }

        private void toolStripPageNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int pageNum = Convert.ToInt16(toolStripPageNum.Text) - 1;
                if (pageNum >= 0 && pageNum < pages.Count)
                {
                    CurrentPage = pageNum;
                    ShowReport();
                }
            }
        }


        private void toolStripZoomIn_Click(object sender, EventArgs e)
        {
            ZoomReport(25);
        }

        private void toolStripZoomOut_Click(object sender, EventArgs e)
        {
            ZoomReport(-25);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            CenterReport();
        }
    }
}
