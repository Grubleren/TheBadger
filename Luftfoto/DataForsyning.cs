using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace JH.Applications
{
    public class Dataforsyning
    {
        protected string[] key;
        public List<string> dataList;

        public Dataforsyning(string type, string koordinat, int circle, string token, WebClient client)
        {
            try
            {
                string uri = "https://api.dataforsyningen.dk/" + type + "/?cirkel=" + koordinat + "," + circle.ToString() + "&format=csv&token=" + token;
                key = new string[0];


                string database = "";
                int ntry = 10;
                bool responseFailure = true;
                while (ntry > 0 && responseFailure)
                {
                    responseFailure = false;
                    try
                    {

                        database = client.DownloadString(uri);
                    }
                    catch
                    {
                        ntry--;
                        database = "";
                        responseFailure = true;
                        Trace.WriteLine(string.Format("Dataforsyning response failure {0}", ntry));
                        Trace.WriteLine(string.Format("URI: {0}", uri));
                        Thread.Sleep(200);
                    }
                }
                if (responseFailure)
                {
                    Trace.WriteLine("Dataforsyning fatal response failure - Give up on: " + uri);
                }
                database = database.Replace("\n", "");
                string[] split = database.Split(new char[] { '\r' });
                dataList = new List<string>();
                if (split == null || split.Length == 2)
                    return;
                key = split[0].Split(new char[] { ',' });
                string[] data = split[split.Length - 2].Split(new char[] { '\"' });
                for (int i = 0; i < data.Length; i++)
                {
                    if ((i & 1) == 1)
                    {
                        dataList.Add(data[i]);
                    }
                    else
                    {
                        string[] ss = data[i].Split(new char[] { ',' });
                        for (int j = (i == 0 ? 0 : 1); j < ss.Length - 1; j++)
                            dataList.Add(ss[j]);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }

    public class DataforsyningAdresser : Dataforsyning
    {
        public DataforsyningAdresser(string type, string koordinat, int circle, string token, WebClient client)
            : base(type, koordinat, circle, token, client)
        {
        }

        public string Vejnavn
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "vejnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Husnr
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "husnr")
                        return dataList[i];
                return "";
            }
        }
        public string Etage
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "etage")
                        return dataList[i];
                return "";
            }
        }
        public string Kommune
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "kommunenavn")
                        return dataList[i];
                return "";
            }
        }

        public string Ejerlav
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "ejerlavnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Matrikelnr
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "matrikelnr")
                        return dataList[i];
                return "";
            }
        }
        public string Region
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "regionsnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Politikreds
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "politikredsnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Retskreds
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "retskredsnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Zone
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "zone")
                        return dataList[i];
                return "";
            }
        }
        public string Betegnelse
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "betegnelse")
                        return dataList[i];
                return "";
            }
        }
        public string Lokalitet
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "ejerlavnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Postnummer
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "postnr")
                        return dataList[i];
                return "";
            }
        }
        public string By
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "postnrnavn")
                        return dataList[i];
                return "";
            }
        }
        public string Sogn
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "sognenavn")
                        return dataList[i];
                return "";
            }
        }
        public string Id
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "id")
                        return dataList[i];
                return "";
            }
        }
    }

    public class DataforsyningSteder : Dataforsyning
    {
        public DataforsyningSteder(string type, string koordinat, int circle, string token, WebClient client)
            : base(type, koordinat, circle, token, client)
        {
        }

        public string PrimNavn
        {
            get
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] == "primærtnavn")
                        return dataList[i];
                return "";
            }
        }
    }

}
