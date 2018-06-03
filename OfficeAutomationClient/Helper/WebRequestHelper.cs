using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace OfficeAutomationClient.Helper
{
    public class WebRequestHelper
    {
        private HttpWebRequest client;
        private MemoryStream postStream = new MemoryStream();

        private WebRequestHelper(string url)
        {
            client = WebRequest.Create(url) as HttpWebRequest;
            client.CookieContainer = new CookieContainer();
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0";
        }

        public static WebRequestHelper Create(string url)
        {
            return new WebRequestHelper(url);
        }

        public WebRequestHelper WithProxy(IWebProxy webProxy)
        {
            client.Proxy = webProxy;

            return this;
        }

        public WebRequestHelper WithCookies(CookieContainer cookieContainer)
        {
            client.CookieContainer = cookieContainer;

            return this;
        }

        public WebRequestHelper WithParamters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return WithParamters(parameters, Encoding.UTF8);
        }

        public WebRequestHelper WithParamters(IEnumerable<KeyValuePair<string, string>> parameters, Encoding encoding)
        {
            CachePostData(parameters, postStream);

            return this;
        }

        private async void CachePostData(IEnumerable<KeyValuePair<string, string>> parameters, MemoryStream postStream)
        {
            using (var content = new FormUrlEncodedContent(parameters))
            {
                await content.CopyToAsync(postStream);
            }
        }

        private void DoPostData()
        {
            if (postStream.Length > 0)
            {
                client.Method = "POST";
                client.ContentType = "application/x-www-form-urlencoded";
                client.ContentLength = postStream.Length;

                postStream.Position = 0;

                client.ContentLength = postStream.Length;
                using (var req = client.GetRequestStream())
                {
                    postStream.CopyTo(req);
                }
            }
        }

        public Stream GetResponseStream()
        {
            DoPostData();

            var response = client.GetResponse() as HttpWebResponse;
            client.CookieContainer.Add(response.Cookies);

            return response.GetResponseStream();
        }

        public string GetResponseString()
        {
            return GetResponseString(Encoding.UTF8);
        }

        public string GetResponseString(Encoding encoding)
        {
            using (var response = GetResponseStream())
            using (var reader = new StreamReader(response, encoding))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
