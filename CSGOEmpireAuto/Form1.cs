using xNet;
using System.Diagnostics;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.IO;
using System.IO.Compression;

namespace CSGOEmpireAuto
{
    public partial class Form1 : Form
    {
        double all = 0;
        string tlogo = "https://csgoempire.com/img/coin-t.37ae39bf.png";
        string ctlogo = "https://csgoempire.com/img/coin-ct.d399ffd4.png";
        string dicelogo = "https://csgoempire.com/img/coin-bonus.806c9d88.png";
        int max = 0;
        int maxdice = 0;
        int unrecognize = 0;

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "0.01" + "\r\n";
            all = 0.01;
        }

        private static async Task<string> GetContentFromPageAsync(string page)
        {
            System.Net.ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(page))
            using (var content = response.Content)
            {
                var result = await content.ReadAsStringAsync();
                return result;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = 1;
            for (; ; )
            {
                double hientai = 0;
                if (all >= 0.5)
                {
                    hientai = Math.Round(all + 0.5, 2);
                }
                else
                {
                    hientai = Math.Round(all * 2, 2);
                }

                if (all >= 10)
                {
                    textBox1.Text += all.ToString() + " " + count.ToString();
                    break;
                }
                else
                {
                    count++;
                    textBox1.Text += hientai.ToString() + "\r\n";
                    all += hientai;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                //foreach (var process in Process.GetProcessesByName("msedgedriver"))
                //{
                //    process.Kill();
                //}
                var chromeOptions = new EdgeOptions();
                var chromeDriverService = EdgeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                IWebDriver driver = new EdgeDriver(chromeDriverService, chromeOptions);

                List<string> ls = new List<string>();
                ls.Add(tlogo);
                ls.Add(ctlogo);
                for (int date = Int32.Parse(textBox2.Text.ToString()); date >= 1000; date--)
                {
                    driver.Navigate().GoToUrl("https://csgoempire.com/history?seed=" + date.ToString());
                    System.Threading.Thread.Sleep(5000);
                    var all = driver.FindElements(By.ClassName("mb-1"));

                    //for (var i = 1; i < all.Count - 1; i++)
                    //    if (!ls.Contains(all[i].GetAttribute("src").ToString()))
                    //        ls.Add(all[i].GetAttribute("src").ToString());
                    int start = 1;
                    int countT = 0;
                    int countCT = 0;
                    int countDICE = 0;

                    unrecognize = 0;
                    maxdice = 0;
                    max = 0;
                    for (; ; )
                    {
                        if (!dicelogo.Equals(all[start].GetAttribute("src").ToString()))
                        {
                            if (all[start].GetAttribute("src").ToString().Equals(tlogo))
                            {
                                if (max < countT)
                                    max = countT;
                                countCT++;
                                countDICE++;
                                countT = 0;
                            }
                            else
                            {
                                if (max < countCT)
                                    max= countCT;
                                countT++;
                                countDICE++;
                                countCT = 0;
                            }
                            break;
                        }
                        countCT++;
                        countT++;
                        if (maxdice < countDICE)
                        {
                            maxdice = countDICE;
                        }
                        countDICE = 0;
                        start++;
                    }

                    for (var i = start+1; i < all.Count - 1; i++)
                    {
                        if (dicelogo.Equals(all[i].GetAttribute("src").ToString()))
                        {
                            countCT++;
                            countT++;
                            if (maxdice < countDICE)
                            {
                                maxdice = countDICE;
                            }
                            countDICE = 0;
                        }
                        else if (ls.Contains(all[i].GetAttribute("src").ToString()))
                        {
                            if (all[i].GetAttribute("src").ToString().Equals(tlogo))
                            {
                                if (max < countT)
                                    max = countT;
                                countCT++;
                                countDICE++;
                                countT = 0;
                            }
                            else
                            {
                                if (max < countCT)
                                    max = countCT;
                                countT++;
                                countDICE++;
                                countCT = 0;
                            }
                        }
                        else
                        {
                            unrecognize++;
                        }
                    }
                    if (max < countCT)
                        max = countCT;
                    if (max < countT)
                        max = countT;
                    if (maxdice < countDICE)
                    {
                        maxdice = countDICE;
                    }
                    try
                    {
                        using (StreamWriter sw = File.AppendText("result.txt"))
                        {
                            sw.WriteLine(date.ToString() + "|" + max.ToString() + "|" + maxdice.ToString() + "|" + unrecognize.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                        using (StreamWriter sw = File.AppendText("error.txt"))
                        {
                            sw.WriteLine(date.ToString() + "|" + max.ToString() + "|" + maxdice.ToString() + "|" + unrecognize.ToString());
                        }
                    }

                }
                MessageBox.Show("Done!");
            }
            catch (InvalidOperationException)
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName("msedgedriver"))
                    {
                        process.Kill();
                    }
                    MessageBox.Show("Phiên bản Microsoft Edge cần cập nhập!\nBấm ok và chờ một lát để cập nhập.", "Lỗi");
                    string version = "";
                    string path = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\";
                    foreach (string folder in Directory.GetDirectories(path))
                        if (Path.GetFileName(folder) != "SetupMetrics")
                            version = Path.GetFileName(folder);
                    if (version != "")
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile("https://msedgedriver.azureedge.net/" + version + "/edgedriver_win64.zip", @"edgedriver.zip");
                    }
                    File.Delete("msedgedriver.exe");
                    string startPath = @".\start";
                    string zipPath = @".\edgedriver.zip";
                    string extractPath = @".\";
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                    Directory.Delete("Driver_Notes", true);
                    File.Delete("edgedriver.zip");
                    MessageBox.Show("Đã cập nhập xong!", "Thông báoooooo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cập nhật thất bại!\n" + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //foreach (var process in Process.GetProcessesByName("msedgedriver"))
                //{
                //    process.Kill();
                //}
                var chromeOptions = new EdgeOptions();
                var chromeDriverService = EdgeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                IWebDriver driver = new EdgeDriver(chromeDriverService, chromeOptions);

                List<string> ls = new List<string>();
                ls.Add(tlogo);
                ls.Add(ctlogo);
                for (int date = Int32.Parse(textBox2.Text.ToString()); date >= 1000; date--)
                {
                    driver.Navigate().GoToUrl("https://csgoempire.com/history?seed=" + date.ToString());
                    System.Threading.Thread.Sleep(5000);
                    var all = driver.FindElements(By.ClassName("mb-1"));

                    //for (var i = 1; i < all.Count - 1; i++)
                    //    if (!ls.Contains(all[i].GetAttribute("src").ToString()))
                    //        ls.Add(all[i].GetAttribute("src").ToString());
                    int start = 1;
                    string result = "";

                    for (var i = start; i < all.Count - 1; i++)
                    {
                        string text = all[i].GetAttribute("src").ToString();
                        if (dicelogo.Equals(text))
                        {
                            try
                            {
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + ".txt"))
                                {
                                    sw.WriteLine("3");
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + "z.txt"))
                                {
                                    sw.WriteLine("3");
                                }
                            }
                        }
                        else if (all[i].GetAttribute("src").ToString().Equals(ctlogo))
                        {
                            try
                            {
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + ".txt"))
                                {
                                    sw.WriteLine("1");
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + "z.txt"))
                                {
                                    sw.WriteLine("1");
                                }
                            }
                        }
                        else if (all[i].GetAttribute("src").ToString().Equals(tlogo))
                        {
                            try
                            {
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + ".txt"))
                                {
                                    sw.WriteLine("2");
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + "z.txt"))
                                {
                                    sw.WriteLine("2");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + ".txt"))
                                {
                                    sw.WriteLine("4");
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.ToString());
                                using (StreamWriter sw = File.AppendText("ketqua/" + date.ToString() + "z.txt"))
                                {
                                    sw.WriteLine("4");
                                }
                            }
                        }
                    }

                }
                MessageBox.Show("Done!");
            }
            catch (InvalidOperationException)
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName("msedgedriver"))
                    {
                        process.Kill();
                    }
                    MessageBox.Show("Phiên bản Microsoft Edge cần cập nhập!\nBấm ok và chờ một lát để cập nhập.", "Lỗi");
                    string version = "";
                    string path = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\";
                    foreach (string folder in Directory.GetDirectories(path))
                        if (Path.GetFileName(folder) != "SetupMetrics")
                            version = Path.GetFileName(folder);
                    if (version != "")
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile("https://msedgedriver.azureedge.net/" + version + "/edgedriver_win64.zip", @"edgedriver.zip");
                    }
                    File.Delete("msedgedriver.exe");
                    string startPath = @".\start";
                    string zipPath = @".\edgedriver.zip";
                    string extractPath = @".\";
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                    Directory.Delete("Driver_Notes", true);
                    File.Delete("edgedriver.zip");
                    MessageBox.Show("Đã cập nhập xong!", "Thông báoooooo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cập nhật thất bại!\n" + ex.Message);
                }
            }
        }
    }
}