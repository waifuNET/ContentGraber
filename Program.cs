using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json;

namespace ContentGraber
{
    class gelbooruJSON
    {
        public string file { get; set; }
        public string file_url { get; set; }
        public string tags { get; set; }
        public List<string> Artist = new List<string>();
        public List<string> Character = new List<string>();
        public List<string> Copyright = new List<string>();
        public string original { get; set; }
        public string website { get; set; }
        public int postID { get; set; }
    }

    class Program
    {
        static string startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string content = startupPath + "\\Content";

        static string websiteurl = "https://rule34.xxx/";
        static string user_agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36 OPR/78.0.4093.153";

        static string html = "";

        static string tag = "";

        static bool downlaod = true;

        static string url_to_upload = "";

        static List<gelbooruJSON> gelJSON = new List<gelbooruJSON>();

        //Console.WriteLine(getElement(html, "tags-search"));
        //Console.WriteLine(getText(getElement(html, "tags-search"), "value=\"", "\""));

        //getAllElement(html, "<a class=\"js-pop\""); //<a class="js-pop"

        static void Main(string[] args)
        {
            string[] splitter = new string[0];
            List<string> total_elements = new List<string>();
            bool posts = false;

            Console.ForegroundColor = ConsoleColor.White;
            if (!Directory.Exists(content))
                Directory.CreateDirectory(content);

            Console.WriteLine("input url to upload:");
            url_to_upload = Console.ReadLine();

            while (true)
            {
                posts = false;
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "file":
                        Console.WriteLine("Input filename");
                        string file = Console.ReadLine();
                        if (File.Exists(file))
                        {
                            using (StreamReader sr = new StreamReader(file))
                            {
                                splitter = sr.ReadToEnd().Split('*', StringSplitOptions.RemoveEmptyEntries);
                                sr.Close();
                            }
                        }
                        break;
                    case "auto":
                        Console.WriteLine("Input url");
                        string url = Console.ReadLine();
                        Console.WriteLine("Input ot");
                        int mod_ot = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("Input do");
                        int mod_do = Int32.Parse(Console.ReadLine());

                        splitter = new string[Math.Max(mod_ot, mod_do)];
                        if (mod_ot < mod_do)
                        {
                            for (int i = mod_ot; i < mod_do; i++)
                            {
                                splitter[i] = url + Math.Abs(getPid(i));
                            }
                        }
                        else
                        {
                            for (int i = mod_do; i < mod_ot; i++)
                            {
                                splitter[i] = url + Math.Abs(getPid(i));
                            }
                        }

                        if (mod_ot > mod_do) // 20 10 
                        {
                            Array.Reverse(splitter);
                        }
                        break;
                    case "posts":
                        posts = true;
                        Console.WriteLine("Input filename else returns urls");
                        string sfile = Console.ReadLine();
                        if (File.Exists(sfile))
                        {
                            using (StreamReader sr = new StreamReader(sfile))
                            {
                                splitter = sr.ReadToEnd().Split('*', StringSplitOptions.RemoveEmptyEntries);
                                sr.Close();
                            }

                            for (int i = 0; i < splitter.Length; i++)
                            {
                                if (splitter[i] != null && splitter[i] != "")
                                {
                                    Console.WriteLine(splitter[i]);
                                    total_elements.Add(splitter[i]);
                                }
                            }
                            splitter = new string[0];
                            break;
                        }

                        splitter = sfile.Split('*', StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < splitter.Length; i++)
                        {
                            if (splitter[i] != null && splitter[i] != "")
                            {
                                Console.WriteLine(splitter[i]);
                                total_elements.Add(splitter[i]);
                            }
                        }
                        splitter = new string[0];
                        break;
                    default:
                        Console.WriteLine("Input urls");
                        splitter = Console.ReadLine().Split('*', StringSplitOptions.RemoveEmptyEntries);
                        break;
                }

                for (int s = 0; s < splitter.Length; s++)
                {
                    if (splitter[s] == null || splitter[s] == "") continue;
                    getHTML(splitter[s]);

                    Console.WriteLine("Url: " + splitter[s]);
                    tag = getText(getElement(html, "name=\"tags\""), "value=\"", "\"").Trim();

                    List<string> elements = getAllElement(html, new string[] { "a id=\"" });
                    for (int i = elements.Count - 1; i > 0; i--)
                    {
                        total_elements.Add(elements[i]);
                    }

                    //string qdqwd = "";
                    //for (int i = 0; i < total_elements.Count; i++)
                    //{
                    //    qdqwd += total_elements[i] + "\n";
                    //}
                    //File.WriteAllText("html.txt", qdqwd);
                    //return;
                }

                Console.WriteLine("Download:");
                for (int i = 0; i < total_elements.Count; i++)
                {
                    string url;
                    if (!posts)
                        url = websiteurl + getText(total_elements[i], "href=\"", "\"").Replace("amp;", String.Empty);
                    else
                        url = websiteurl + total_elements[i];

                    if(url == "") continue;

                    Uri myUri = new Uri(url);
                    string post_id = HttpUtility.ParseQueryString(myUri.Query).Get("id");
                    int postid = Int32.Parse(post_id);

                    if (DataBase.GetPostsByIdAndWebsite(postid, myUri.Host).Length <= 0)
                    {
                        //Console.WriteLine(myUri.Host + ": " + postid);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Post: {0} is exist! Post skip..", myUri.Host + ": " + postid);
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    getHTML(url);

                    string original = url;

                    url = getText(getElement(html, "id=\"image\""), "src=\"", "\"");
                    if (url == "")
                        url = getText(getElement(html, "<source src=\""), "src=\"", "\"");
                    if (url == "") continue;
                    url = url.Replace("wimg.", String.Empty);
                    url = url.Replace("img.", String.Empty);
                    double file_size = Math.Round(GetFileSize(new Uri(url)), 1);
                    string file_str = (Math.Round(file_size, 1) + " MB").Replace(',', '.');
                    if (file_size > 19)
                    {
                        Console.WriteLine(url + " file is too big: " + file_str);
                        continue;
                    }
                    Console.WriteLine(url + " " + (i + 1) + " /" + total_elements.Count + " : " + file_str);

                    url = (websiteurl + (url.Remove(0, websiteurl.Length + 1)));

                    //SaveFile(url, tag);
                    gelbooruJSON gelbooru = new gelbooruJSON();
                    if (downlaod)
                        gelbooru.file = SaveFile(url, tag);
                    gelbooru.file_url = url;
                    gelbooru.tags = getText(html, "<img alt=\"", "\"");
                    gelbooru.Artist = getLeftGelbooru("<li class=\"tag-type-artist tag\">");
                    gelbooru.Character = getLeftGelbooru("<li class=\"tag-type-character tag\">");
                    gelbooru.Copyright = getLeftGelbooru("<li class=\"tag-type-copyright tag\">");
                    gelbooru.original = original;
                    gelbooru.website = myUri.Host;
                    gelbooru.postID = postid;

                    gelJSON.Add(gelbooru);

                    uploadOnServer();
                }

                total_elements.Clear();
                counter = 1;

                Console.WriteLine("");
                Console.WriteLine("Downloads finish.");
            }
        }

        private static void uploadOnServer()
        {

            for (int i = 0; i < gelJSON.Count; i++)
            {
                try
                {
                    string artist = "";
                    string character = "";
                    string copyright = "";

                    for (int j = 0; j < gelJSON[i].Artist.Count; j++) artist += gelJSON[i].Artist[j] + " ";
                    for (int j = 0; j < gelJSON[i].Character.Count; j++) character += gelJSON[i].Character[j] + " ";
                    for (int j = 0; j < gelJSON[i].Copyright.Count; j++) copyright += gelJSON[i].Copyright[j] + " ";

                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add("tags", gelJSON[i].tags);
                    nvc.Add("user_auth", "true");
                    nvc.Add("upd", "rkbot");
                    nvc.Add("original", gelJSON[i].original);
                    nvc.Add("artist", artist);
                    nvc.Add("character", character);
                    nvc.Add("copyright", copyright);
                    nvc.Add("parse_file_url", gelJSON[i].file_url);
                    if (File.Exists(gelJSON[i].file) || !downlaod)
                    {
                        string result = "";
                        result = HttpUploadFile(url_to_upload, gelJSON[i].file, "photo", "image/" + Path.GetExtension(gelJSON[i].file).Remove(0, 1), nvc);
                        //result = HttpUploadFile("https://xenbooru-server-0.enginl.ru/php/************.php", gelJSON[i].file, "photo", "image/" + Path.GetExtension(gelJSON[i].file).Remove(0, 1), nvc);

                        //try
                        //{
                        //    using (WebClient webClient = new WebClient())
                        //    {
                        //        byte[] data = webClient.UploadValues("http://xenbooru/php/uploadfile-parse.php", "POST", nvc);
                        //        result = UnicodeEncoding.UTF8.GetString(data);
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.ForegroundColor = ConsoleColor.Red;
                        //    Console.WriteLine(ex.Message);
                        //    Console.ForegroundColor = ConsoleColor.White;
                        //}

                        if (result.IndexOf("Success") >= 0)
                        {
                            DataBase.addPost(new posts { postID = gelJSON[i].postID, website = gelJSON[i].website });
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
            }
            gelJSON.Clear();
        }

        private static double GetFileSize(Uri uriPath)
        {
            try
            {
                var webRequest = HttpWebRequest.Create(uriPath);
                webRequest.Method = "HEAD";

                using (var webResponse = webRequest.GetResponse())
                {
                    string fileSize = webResponse.Headers.Get("Content-Length");
                    double fileSizeInMegaByte = Convert.ToDouble(fileSize) / 1024 / 1024;
                    return fileSizeInMegaByte;
                }
            }
            catch
            {
                return Int32.MaxValue;
            }
        }

        static int counter = 1;
        public static string HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string result = "";
            Console.Write(string.Format("Uploading {0}: {1}, {2}", counter, Path.GetFileName(file), contentType));
            counter++;
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                result = reader2.ReadToEnd();
                Console.Write(": " + result);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                result = " Error uploading file: " + ex.Message;
                Console.WriteLine(result);
                Console.ForegroundColor = ConsoleColor.White;
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
                Console.WriteLine();
            }
            return result;
        }

        private static void setTrust(bool trust = false)
        {
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => false);
            if (trust)
                ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        public static string PostRequest(string url, string Request = "null=null", bool Trust = false, string ContentType = "application/x-www-form-urlencoded")
        {
            setTrust(Trust);
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST"; // для отправки используется метод Post
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(Request); // преобразуем данные в массив байтов
            request.ContentType = ContentType;  // устанавливаем тип содержимого - параметр ContentType
            request.ContentLength = byteArray.Length;   // Устанавливаем заголовок Content-Length запроса - свойство ContentLength

            using (Stream dataStream = request.GetRequestStream())  //записываем данные в поток запроса
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            WebResponse response = request.GetResponse();
            string temp = default;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    temp = reader.ReadToEnd();
                }
            }
            response.Close();

            return temp;
        }

        static List<string> getLeftGelbooru(string fn)
        {
            List<string> result = new List<string>();
            string glob = getText(html, "<ul id=\"tag-sidebar\"", "/ul>");
            glob = glob.Split("General")[0];

            List<string> el = getAllElement(glob, new string[] { fn });
            string[] tmp = glob.Split(fn);
            string new_glob = "";
            for (int j = 1; j < tmp.Length; j++)
            {
                new_glob += tmp[j];
            }
            glob = new_glob;
            for (int x = 0; x < el.Count; x++)
            {
                string frag = getText(glob, "<a href=\"index.php?", "a>");
                string name = getText(frag, "\">", "<");

                result.Add(name.Replace(' ', '_'));

                if (glob.Split(name).Length > 1)
                    glob = glob.Split(name)[1];
            }
            return result;
        }

        static int getPage(int pid)
        {
            return (pid / 42) + 1;
        }

        static int getPid(int page)
        {
            return page * 42;
        }

        static string SaveFile(string url, string directoryName)
        {
            if (!Directory.Exists(content + "\\" + DateTime.Now.Year))
                Directory.CreateDirectory(content + "\\" + DateTime.Now.Year);

            if (!Directory.Exists(content + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month))
                Directory.CreateDirectory(content + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month);

            if (!Directory.Exists(content + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month + "\\" + (DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year) + "\\" + directoryName))
                Directory.CreateDirectory(content + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month + "\\" + (DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year) + "\\" + directoryName);

            string path = content + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month + "\\" + (DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year) + "\\" + directoryName + "\\" + Path.GetFileName(url.Split('?')[0]);
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", user_agent);
                    webClient.DownloadFile(url, path);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            return path;
        }

        static void getHTML(string url)
        {
            try
            {
                string htmlCode = "NULL";
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
                {
                    client.Headers.Add("user-agent", user_agent);
                    htmlCode = client.DownloadString(url);
                }
                html = htmlCode;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("gH: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static List<string> getAllElement(string text, string[] rgx)
        {
            string tmp_s = "";

            int firstIndex = 0;
            int lastIndex = 0;

            bool skip = false;

            List<string> elements = new List<string>();

            for (int rg = 0; rg < rgx.Length; rg++)
            {
                for (int t = lastIndex; t < text.Length; t++)
                {
                    skip = false;

                    for (int i = 0; i < rgx[rg].Length; i++)
                    {
                        if (text[t] == rgx[rg][i])
                        {
                            tmp_s += text[t];
                            t++;

                            if (tmp_s == rgx[rg])
                            {
                                for (int first = t; first > 0; first--)
                                {
                                    if (text[first] == '<')
                                    {
                                        firstIndex = first;
                                        for (int last = t; last < text.Length; last++)
                                        {
                                            if (text[last] == '>')
                                            {
                                                lastIndex = last;

                                                string textOnly = "";
                                                for (int r = firstIndex; r < lastIndex; r++)
                                                {
                                                    textOnly += text[r];
                                                }

                                                //return textOnly + ">";
                                                elements.Add(textOnly + ">");
                                                skip = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (skip)
                                        break;
                                }
                            }
                        }
                        else
                        {
                            tmp_s = "";
                            break;
                        }
                        if (skip)
                            break;
                    }
                    tmp_s = "";
                }
            }

            return elements;
        }

        static string getElement(string text, string rgx)
        {
            string tmp_s = "";

            int firstIndex = 0;
            int lastIndex = 0;

            for(int t = 0; t < text.Length; t++)
            {
                for(int i = 0; i < rgx.Length; i++)
                {
                    if(text[t] == rgx[i])
                    {
                        tmp_s += text[t];
                        t++;

                        if (tmp_s == rgx)
                        {
                            for (int first = t; first > 0; first--)
                            {
                                if (text[first] == '<')
                                {
                                    firstIndex = first;
                                    for (int last = t; last < text.Length; last++)
                                    {
                                        if (text[last] == '>')
                                        {
                                            lastIndex = last;

                                            string textOnly = "";
                                            for (int r = firstIndex; r < lastIndex; r++)
                                            {
                                                textOnly += text[r];
                                            }

                                            return textOnly + ">";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        tmp_s = "";
                        break;
                    }
                }
            }
            return "< NULL >";
        }

        static string getText(string text, string start, string stop)
        {
            string tmp_s = ""; // value="
            string tmp_st = ""; // "

            bool _start = false;

            int firstIndex = 0;
            int lastIndex = 0;

            for (int t = 0; t < text.Length; t++)
            {
                if (!_start) {
                    for (int s = 0; s < start.Length; s++)
                    {
                        if (text[t] == start[s])
                        {
                            tmp_s += text[t];
                            t++;

                            if (tmp_s == start)
                            {
                                firstIndex = t;
                                _start = true;
                                break;
                            }
                        }
                        else
                        {
                            tmp_s = "";
                            break;
                        }
                    }
                }

                if (_start)
                {
                    for (int st = 0; st < stop.Length; st++)
                    {
                        if (text[t] == stop[st])
                        {
                            tmp_st += text[t];
                            t++;
                        }
                        else
                        {
                            tmp_st = "";
                        }
                    }

                    if(tmp_st == stop)
                    {
                        lastIndex = t - 1;
                        break;
                    }
                }
            }

            string result = "";
            for(int i = firstIndex; i < lastIndex; i++)
            {
                result += text[i];
            }

            return result;
        }
        static string getText(string text, string start, string[] stop)
        {
            string tmp_s = ""; // value="
            string tmp_st = ""; // "

            bool _start = false;

            int firstIndex = 0;
            int lastIndex = 0;

            for (int t = 0; t < text.Length; t++)
            {
                if (!_start)
                {
                    for (int s = 0; s < start.Length; s++)
                    {
                        if (text[t] == start[s])
                        {
                            tmp_s += text[t];
                            t++;

                            if (tmp_s == start)
                            {
                                firstIndex = t;
                                _start = true;
                                break;
                            }
                        }
                        else
                        {
                            tmp_s = "";
                            break;
                        }
                    }
                }

                if (_start)
                {
                    for (int stop_c = 0; stop_c < stop.Length; stop_c++)
                    {
                        for (int st = 0; st < stop[stop_c].Length; st++)
                        {
                            if (text[t] == stop[stop_c][st])
                            {
                                tmp_st += text[t];
                                t++;
                            }
                            else
                            {
                                tmp_st = "";
                            }
                        }

                        if (tmp_st == stop[stop_c])
                        {
                            lastIndex = t - 1;
                            break;
                        }

                        tmp_st = "";
                    }
                }
            }

            string result = "";
            for (int i = firstIndex; i < lastIndex; i++)
            {
                result += text[i];
            }

            return result;
        }
    }
}
