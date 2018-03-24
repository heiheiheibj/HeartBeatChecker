using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
 
using System.Net.Mail;
using System.Threading;
using System.IO;
using System.Net;

namespace HeartBeatChecker
{
    public partial class HeartBeatChecker : Form
    {
       
        string[] arrUrl = new string[20];
        //string[] arrUrlDesc = new string[20];
        //string strFileContent = "";
        public HeartBeatChecker()
        {
            InitializeComponent();

        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (txtSMTP.Text.Trim().Length < 3 || txtMail.Text.Trim().Length < 3 || txtPassword.Text.Trim().Length < 3 || txtToMail.Text.Trim().Length < 3)
            {
                MessageBox.Show("请完整填写界面中的内容项!");
                return;
                
            }
            button1.Text = "监控中";
            int j = ReadText();
            if (j <=1 )
            {
                MessageBox.Show("在当前目录下未找到site.ini文件或文件中没有有效网址!");
                return;
            }


            int intT = 1000;
            try
            {
                intT = Convert.ToInt32(txtTime.Text) * intT;
                if (intT < 3000)
                {
                    MessageBox.Show("最小时间间隔为3秒" + intT.ToString());
                    txtTime.Focus();
                    return;
                }
                timer1.Interval = intT;
                StartCheck();
            }
            catch
            {
                MessageBox.Show("最小时间间隔为3秒!" + intT.ToString());
                txtTime.Focus();
                return;            
            }
            

        }

        private void StartCheck()
        {
            string v = "";
            int intError = 0;
            timer1.Stop();
            for (int i = 1; i <= arrUrl.Length; i++)
            {
                try
                {                    
                    v = CheckServer(arrUrl[i].ToString());
                    if (v != "")
                    {
                        intError = 1;
                        if (CheckServer("http://www.baidu.com", "baidu") == "")
                        {
                            SendMail(v);
                        }
                        else
                        {
                            intError = 0;   //如果BAIDU也出错说明网络问题，不发送邮件
                        }
                    }
                }
                catch (Exception e)
                {
                    break;
                }
            }
            if (intError != 0)
            {
                timer1.Stop();
                Thread.Sleep(3 * 60 * 1000);   //发过邮件后，过3分钟再继续测
                //MessageBox.Show("ok");
                StartCheck();
            }
            else
            {
                timer1.Start();
            }
        }

        private string CheckServer(string strUrlV,string strV ="")
        {
            string strTitle = "";
            string strUrl="";
            string strKey = "err";
            try
            {
                /*objXmlHTTP.open("GET", strUrl, false, null, null);
                objXmlHTTP.send("");
                if (objXmlHTTP.readyState == 4)
                {
                    //MessageBox.Show(objXmlHTTP.status.ToString());
                    //Byte[] b = (Byte[])objXmlHTTP.responseBody;
                    //string andy = System.Text.Encoding.GetEncoding("GB2312").GetString(b).Trim();
                    if (objXmlHTTP.status.ToString() != "200")
                    {
                        strTitle = strUrl + "发生 HTTP" + objXmlHTTP.status.ToString() + " 错误!";
                    }
                    else
                    {
                        if (objXmlHTTP.responseText.IndexOf("ok") < 0 && objXmlHTTP.responseText.IndexOf("title") <= 0)
                        {
                            strTitle = strUrl + "运行环境可能发生错误!";
                        }
                    }
                }*/
                if (strV == "") //如果是带第二个KEY参数，说明是效验BAIDU能不能通过
                {
                    string[] arrUrlDesc = strUrlV.Split('|');

                    strUrl = arrUrlDesc[0];
                    strV = arrUrlDesc[1];
                }
                else
                {
                    strUrl = strUrlV;
                }
                string a = GetSiteHtml(strUrl);
                if (a.IndexOf(strV) <= 0)
                        {
                            strTitle = strUrl + "运行环境可能发生错误!";
                            if (a.IndexOf("siteerror") > 0)
                            {
                                WriteFile("errlog.txt", a, "");
                            }
                        }
                //objXmlHTTP.abort();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.TargetSite + strUrl);
                //SendMail(arrUrl[1], strTitle);
                strTitle = strUrlV + " 发生异常!";
            }
            finally {
                //objXmlHTTP.abort();
            }
            return strTitle;
            
        }

        //获取某个页面。以返回流的编码方式，接收返回的信息
        public string GetSiteHtml(string url)
        {
            //string tempurl = System.Web.HttpUtility.UrlEncode(url, System.Text.Encoding.GetEncoding("gb2312")); //url
            string tempurl = url;
            try
            {
                HttpWebRequest webr = (HttpWebRequest)WebRequest.Create(tempurl);
                webr.Timeout = 60000;

                //创建请求
                HttpWebResponse wb = (HttpWebResponse)webr.GetResponse();
                Stream sr = wb.GetResponseStream(); //得到返回数据流
                Encoding encode;
                switch (wb.CharacterSet.ToLower()) //返回数据流的编码方式
                {
                    case "utf-8":
                        encode = System.Text.Encoding.GetEncoding("utf-8");
                        break;
                    case "gb2312":
                        encode = System.Text.Encoding.GetEncoding("gb2312");
                        break;
                    default:
                        encode = System.Text.Encoding.GetEncoding("utf-8"); //默认为中文
                        break;
                }
                StreamReader sr1 = new StreamReader(sr, encode);
                string zz = sr1.ReadToEnd(); //读取完成
                sr1.Close();
                wb.Close();//关闭
                return zz;
            }
            catch (Exception x4)
            {
                return " siteerror:" + x4.Message;
            }
        }

        private void SendMail(string strTitle)
        {
            if (txtSMTP.Text.Trim() != "" && txtMail.Text.Trim() != "")
            {
                
                MailAddress objFrom = new MailAddress(txtMail.Text.Trim(), txtMail.Text.Trim());
                MailAddress objTo = new MailAddress(txtToMail.Text, txtToMail.Text);
                MailMessage objMail = new MailMessage(objFrom,objTo);
                //objMail.CC.Add("13056870@qq.com");
                //objMail.CC.Add("66014196@qq.com");
                if (IsEmail(textCC1.Text))
                {
                    objMail.CC.Add(textCC1.Text);
                }
                if (IsEmail(textCC2.Text))
                {
                    objMail.CC.Add(textCC2.Text);
                }
                if (IsEmail(textCC3.Text))
                {
                    objMail.CC.Add(textCC3.Text);
                }
                if (IsEmail(textCC4.Text))
                {
                    objMail.CC.Add(textCC4.Text);
                }
                //objMail.CC.Add("guoshaobo@ciic.com.cn");
                //objMail.CC.Add("guoshaobo@ciic.com.cn");            
                
                objMail.Subject = strTitle;
                //objMail.Body=strUrl + "中好的";
                objMail.Priority = MailPriority.High;
                SmtpClient MySmtpClient = new SmtpClient(txtSMTP.Text,25);
                MySmtpClient.UseDefaultCredentials = false;
                //MySmtpClient.EnableSsl = true;
                MySmtpClient.Credentials = new System.Net.NetworkCredential(txtSMTPUserName.Text, txtPassword.Text);
                MySmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                MySmtpClient.EnableSsl = false;
                try
                {
                    MySmtpClient.Send(objMail);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    System.Environment.Exit(0); 
                }
                objMail.Dispose();
                    

            }
        }

        private int ReadText()
        {
            try
            {
                FileStream aFile = new FileStream(Application.StartupPath + "\\Site.ini", FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                //strFileContent = sr.ReadToEnd();
                string s = "";
                //sr.BaseStream.Seek(0, SeekOrigin.Begin);  //读一下，看看文件内有没有内容，为下一步循环 提供判断依据  
                //sr.ReadLine() 这里是 StreamReader的方法 可不是 console 中的~   string str = sr.ReadLine();//如果 文件有内容   while (str != null)  {      //输出字符串，str 在上面已经定义了 读入一行字符       Console.WriteLine("{0}", str);      //这里我的理解是 当输出一行后，指针移动到下一行~      //下面这句话就是 判断 指针所指这行是否有内容~      str = sr.ReadLine();}  //C#读取TXT文件之关闭文件，注意顺序，先对文件内部进行关闭，然后才是文件~  sr.Close();  fs.Close();      }  
                int j = 1;
                int n = 1;
                s = sr.ReadLine();
                while (s != null && j<20)
                {
                    if (((s.ToString()).ToLower()).IndexOf("http://") >= 0)
                    {
                        arrUrl[j] = s.ToString();
                        j++;
                    }
                    n++;
                    s = sr.ReadLine();
                }
                sr.Close();
                sr.Dispose();
                aFile.Close();
                aFile.Dispose();
                return j;
                //MessageBox.Show(strFileContent);
                //MessageBox.Show(strFileContent.IndexOf("\n").ToString());
            }
            catch(Exception e)
            {
                return -1;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReadText();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            StartCheck();
        }

        //文件名，内容，放在哪个子目录下，文件内容中是否加在尾上加上时间信息
        public void WriteFile(string strLogFileName, string LogStr, string strSiteFolder = "", bool bolNeedDateTime = true)
        {
            StreamWriter sw = null;
            try
            {
                if (bolNeedDateTime)
                {
                    LogStr = DateTime.Now.ToLocalTime().ToString() + "\n" + LogStr;
                }
                if (strSiteFolder != "")
                {
                    strLogFileName = Application.StartupPath.ToString()  + "\\" + strLogFileName;
                }
                sw = new StreamWriter(strLogFileName, true, Encoding.Default);
                sw.WriteLine(LogStr);
            }
            catch (Exception exf)
            {
                string v = exf.Message.ToString();
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
            sw.Dispose();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }

        public bool IsEmail(string str)
        {
            //string res = string.Empty;

            string expression = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            if (System.Text.RegularExpressions.Regex.IsMatch(str, expression, System.Text.RegularExpressions.RegexOptions.Compiled))
            {                
                return true;
            }
            return false;
        }

        private void HeartBeatChecker_Load(object sender, EventArgs e)
        {

        }

    }
}
