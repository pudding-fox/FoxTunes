//This should not be enabled for release.
//#define DISABLE_CERTIFICATE_VALIDATION

using System.Net;
#if DISABLE_CERTIFICATE_VALIDATION
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#endif

namespace FoxTunes
{
    public static class WebRequestFactory
    {
#if DISABLE_CERTIFICATE_VALIDATION
        static WebRequestFactory()
        {
#if NET40
            ServicePointManager.ServerCertificateValidationCallback += OnServerCertificateValidationCallback;
#else
            //Handled by HttpWebRequest.ServerCertificateValidationCallback
#endif
        }
#endif

        public static HttpWebRequest Create(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
#if DISABLE_CERTIFICATE_VALIDATION
#if NET40
            //Handled by ServicePointManager.ServerCertificateValidationCallback
#else
            request.ServerCertificateValidationCallback += OnServerCertificateValidationCallback;
#endif
#endif
            return request;
        }

#if DISABLE_CERTIFICATE_VALIDATION
        private static bool OnServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
#endif
    }
}
