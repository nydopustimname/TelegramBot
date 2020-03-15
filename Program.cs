using System;
using System.Linq;
//using Microsoft.SmallBasic.Library;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Xml.Linq;

namespace bot
{
    public class Program
    {
        private static string Weather(string city)
        {
            string url = "https://xml.meteoservice.ru/export/gismeteo/point/";

            if (city == "minsk") url += "34.xml";
            else if (city == "msk") url += "37.xml";
            else if (city == "spb") url += "69.xml";
            else if (city == "brest") url += "2895.xml";
            else if (city == "vitebsk") url += "2897.xml";
            else if (city == "gomel") url += "2900.xml";
            else if (city == "grodno") url += "2896.xml";
            else if (city == "mogilev") url += "35.xml";
            else return "wrong city. mb /minsk /msk /spb /brest /vitebsk /gomel /grodno /mogilev";
            WebClient wwc = new WebClient();
            string date = wwc.DownloadString(url);

            var weatherarr = XDocument.Parse(date)
                .Descendants("MMWEATHER")
                .Descendants("REPORT")
                .Descendants("TOWN")
                .Descendants("FORECAST").ToArray();

            string textweahter = String.Empty;
            foreach(var i in weatherarr)
            {
                textweahter += string.Format
                    (
                    "{0}-{1}-{2}  {3}:00  {4}°,  {5} mm.rt.st.\n",
                    i.Attribute("day").Value,
                    i.Attribute("month").Value,
                    i.Attribute("year").Value,
                    i.Attribute("hour").Value,
                    i.Element("TEMPERATURE").Attribute("max").Value,
                    i.Element("PRESSURE").Attribute("max").Value
                    );
            }
            return textweahter ;
        }
        static void Main()
        {

            //Turtle.Speed = 10;
            int update_id = 0;
            string mess_fr_id = "";
            string mess_text = "";
            string f_name = "";
            string token = "734175944:AAEcUyI9yqqUUcI6LtgfmuxUpN6KitlCWD0";
            System.IO.StreamWriter log = new System.IO.StreamWriter(@"d:\log.txt", true);

            WebClient wc = new WebClient();

            string start_url = $"https://api.telegram.org/bot{token}";

            while (true)
            {
                string url = $"{ start_url}/getUpdates?offset={update_id + 1}";
                string resp = wc.DownloadString(url);

                var message = JObject.Parse(resp)["result"].ToArray();

                foreach (var curr_mess in message)
                {
                    update_id = Convert.ToInt32(curr_mess["update_id"]);
                    try
                    {
                        f_name = curr_mess["message"]["from"]["first_name"].ToString();
                        mess_fr_id = curr_mess["message"]["from"]["id"].ToString();
                        mess_text = curr_mess["message"]["text"].ToString();

                        Console.WriteLine($"{f_name} {mess_fr_id} {mess_text}");

                        log.WriteLine($"{DateTime.Now} {f_name} {mess_fr_id} {mess_text}");
                        log.Flush();

                        mess_text = Weather(mess_text);
                       
                        url = $"{start_url}/sendMessage?chat_id={mess_fr_id}&text={mess_text}";
                        wc.DownloadString(url);
                    }
                    catch { }
                }
                Thread.Sleep(100); 
            }
        }
    }
}
