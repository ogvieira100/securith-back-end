using System.Text;
using System.Xml.Linq;

namespace Portal.Fornecedor.Api.Services
{
    public abstract class SoapClientBase
    {

        readonly HttpClient _httpClient;
        public SoapClientBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<string> CallSoapWebServiceAsync(string soapAction, string soapEnvelope)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, (string?)null);
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Trate exceções conforme necessário
                throw new Exception("Erro ao chamar o serviço SOAP", ex);
            }
        }

        protected string CreateSoapEnvelope(string bodyContent)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            var soapEnvelope = new XElement(soapEnv + "Envelope",
                new XElement(soapEnv + "Body", XElement.Parse(bodyContent))
            );

            return soapEnvelope.ToString();
        }

    }
}
