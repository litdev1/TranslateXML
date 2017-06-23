using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Xml;

namespace TranslateXML
{
    public class AdmAccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
    }

    class Translator
    {
        private string token = string.Empty;
        private DateTime tokenEnds = DateTime.Now;
        private string clientID;
        private string clientSecret;

        public Translator()
        {
            clientID = Settings.GetValue("CLIENTID");
            clientSecret = Settings.GetValue("CLIENTSECRET");
        }

        private void GetToken()
        {
            if (DateTime.Now < tokenEnds) return;

            String strTranslatorAccessURI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            String strRequestDetails = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientID), HttpUtility.UrlEncode(clientSecret));
            WebRequest webRequest = WebRequest.Create(strTranslatorAccessURI);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(strRequestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            WebResponse webResponse = webRequest.GetResponse();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
            AdmAccessToken admToken = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
            token = "Bearer " + admToken.access_token;
            tokenEnds = DateTime.Now.AddSeconds(double.Parse(admToken.expires_in) - 10);
        }

        public string Translate(string sourceText, string langFrom, string langTo)
        {
            try
            {
                GetToken();
                string txtToTranslate = sourceText;
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(txtToTranslate) + "&from=" + langFrom + "&to=" + langTo;
                WebRequest translationWebRequest = WebRequest.Create(uri);
                translationWebRequest.Headers.Add("Authorization", token);
                WebResponse response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader translatedStream = new StreamReader(stream, encode);
                XmlDocument xTranslation = new XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());
                return xTranslation.InnerText;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetLanguagesForTranslate()
        {
            try
            {
                GetToken();
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForTranslate";
                WebRequest languagesWebRequest = WebRequest.Create(uri);
                languagesWebRequest.Headers.Add("Authorization", token);
                WebResponse response = languagesWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                DataContractSerializer dcs = new DataContractSerializer(typeof(List<string>));
                return (List<string>)dcs.ReadObject(stream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetLanguageNames(List<string> languageList)
        {
            try
            {
                GetToken();
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale=" + CultureInfo.CurrentCulture.Name;
                WebRequest languagesWebRequest = WebRequest.Create(uri);
                languagesWebRequest.Headers.Add("Authorization", token);
                languagesWebRequest.ContentType = "text/xml";
                languagesWebRequest.Method = "POST";
                using (Stream stream = languagesWebRequest.GetRequestStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(List<string>));
                    dcs.WriteObject(stream, languageList);
                }
                WebResponse response = languagesWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(List<string>));
                    return (List<string>)dcs.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    class Translator2
    {
        private Dictionary<string, string> languageList = new Dictionary<string, string>();
        public Translator2()
        {
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("AFRIKAANS".ToLower()), "af");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ALBANIAN".ToLower()), "sq");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("AMHARIC".ToLower()), "am");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ARABIC".ToLower()), "ar");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ARMENIAN".ToLower()), "hy");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("AZERBAIJANI".ToLower()), "az");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("BASQUE".ToLower()), "eu");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("BELARUSIAN".ToLower()), "be");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("BENGALI".ToLower()), "bn");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("BIHARI".ToLower()), "bh");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("BULGARIAN".ToLower()), "bg");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("BURMESE".ToLower()), "my");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CATALAN".ToLower()), "ca");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CHEROKEE".ToLower()), "chr");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CHINESE".ToLower()), "zh");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CHINESE_SIMPLIFIED".ToLower()), "zh-CN");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CHINESE_TRADITIONAL".ToLower()), "zh-TW");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CROATIAN".ToLower()), "hr");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("CZECH".ToLower()), "cs");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("DANISH".ToLower()), "da");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("DHIVEHI".ToLower()), "dv");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("DUTCH".ToLower()), "nl");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ENGLISH".ToLower()), "en");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ESPERANTO".ToLower()), "eo");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ESTONIAN".ToLower()), "et");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("FILIPINO".ToLower()), "tl");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("FINNISH".ToLower()), "fi");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("FRENCH".ToLower()), "fr");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("GALICIAN".ToLower()), "gl");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("GEORGIAN".ToLower()), "ka");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("GERMAN".ToLower()), "de");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("GREEK".ToLower()), "el");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("GUARANI".ToLower()), "gn");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("GUJARATI".ToLower()), "gu");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("HEBREW".ToLower()), "iw");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("HINDI".ToLower()), "hi");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("HUNGARIAN".ToLower()), "hu");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ICELANDIC".ToLower()), "is");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("IRISH".ToLower()), "ga");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("INDONESIAN".ToLower()), "id");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("INUKTITUT".ToLower()), "iu");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ITALIAN".ToLower()), "it");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("JAPANESE".ToLower()), "ja");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("KANNADA".ToLower()), "kn");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("KAZAKH".ToLower()), "kk");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("KHMER".ToLower()), "km");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("KOREAN".ToLower()), "ko");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("KURDISH".ToLower()), "ku");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("KYRGYZ".ToLower()), "ky");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("LAOTHIAN".ToLower()), "lo");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("LATVIAN".ToLower()), "lv");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("LITHUANIAN".ToLower()), "lt");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("MACEDONIAN".ToLower()), "mk");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("MALAY".ToLower()), "ms");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("MALAYALAM".ToLower()), "ml");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("MALTESE".ToLower()), "mt");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("MARATHI".ToLower()), "mr");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("MONGOLIAN".ToLower()), "mn");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("NEPALI".ToLower()), "ne");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("NORWEGIAN".ToLower()), "no");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ORIYA".ToLower()), "or");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("PASHTO".ToLower()), "ps");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("PERSIAN".ToLower()), "fa");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("POLISH".ToLower()), "pl");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("PORTUGUESE".ToLower()), "pt-PT");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("PUNJABI".ToLower()), "pa");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("ROMANIAN".ToLower()), "ro");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("RUSSIAN".ToLower()), "ru");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SANSKRIT".ToLower()), "sa");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SERBIAN".ToLower()), "sr");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SINDHI".ToLower()), "sd");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SINHALESE".ToLower()), "si");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SLOVAK".ToLower()), "sk");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SLOVENIAN".ToLower()), "sl");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SPANISH".ToLower()), "es");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SWAHILI".ToLower()), "sw");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("SWEDISH".ToLower()), "sv");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("TAJIK".ToLower()), "tg");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("TAMIL".ToLower()), "ta");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("TAGALOG".ToLower()), "tl");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("TELUGU".ToLower()), "te");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("THAI".ToLower()), "th");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("TIBETAN".ToLower()), "bo");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("TURKISH".ToLower()), "tr");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("UKRAINIAN".ToLower()), "uk");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("URDU".ToLower()), "ur");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("UZBEK".ToLower()), "uz");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("UIGHUR".ToLower()), "ug");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("VIETNAMESE".ToLower()), "vi");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("WELSH".ToLower()), "cy");
            languageList.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase("YIDDISH".ToLower()), "yi");

            //foreach (EncodingInfo ei in Encoding.GetEncodings())
            //{
            //    string aa = ei.Name;
            //}
        }

        public string Translate(string sourceText, string langFrom, string langTo)
        {
            try
            {
                //string url = String.Format("https://translate.google.co.uk/#{0}/{1}/{2}", langFrom, langTo, HttpUtility.UrlEncode(sourceText));
                string url = String.Format("https://translate.google.com/?hl=en&eotf=1&sl={0}&tl={1}&q={2}", langFrom, langTo, HttpUtility.UrlEncode(sourceText));

                WebRequest translationWebRequest = WebRequest.Create(url);
                WebResponse response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader translatedStream = new StreamReader(stream, Encoding.Default);
                string result = translatedStream.ReadToEnd();
                byte[] bytes;

                int startIndex = result.IndexOf("charset=", StringComparison.Ordinal)+8;
                int endIndex = result.IndexOf("\"", startIndex, StringComparison.Ordinal);
                string charset = result.Substring(startIndex, endIndex - startIndex);

                startIndex = result.IndexOf("<span id=result_box", StringComparison.Ordinal);
                var sb = new StringBuilder();
                if (startIndex > 0)
                {
                    startIndex = result.IndexOf("<span title=", startIndex, StringComparison.Ordinal);
                    while (startIndex > 0)
                    {
                        startIndex = result.IndexOf("onmouseover", startIndex, StringComparison.Ordinal);
                        startIndex = result.IndexOf(">", startIndex, StringComparison.Ordinal);
                        if (startIndex > 0)
                        {
                            startIndex++;
                            endIndex = result.IndexOf("</span>", startIndex, StringComparison.Ordinal);
                            string translatedText = result.Substring(startIndex, endIndex - startIndex);
                            translatedText = HttpUtility.HtmlDecode(translatedText);
                            translatedText = translatedText.Replace((char)160, (char)32);
                            bytes = Encoding.Default.GetBytes(translatedText);
                            translatedText = Encoding.GetEncoding(charset).GetString(bytes);
                            sb.Append(translatedText);
                            startIndex = result.IndexOf("<span title=", startIndex, StringComparison.Ordinal);
                        }
                    }
                }
                return sb.ToString().Replace("<br>", Environment.NewLine);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetLanguagesForTranslate()
        {
            try
            {
                return languageList.Values.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetLanguageNames()
        {
            try
            {
                return languageList.Keys.ToList();

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
