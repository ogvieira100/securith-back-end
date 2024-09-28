namespace Portal.Fornecedor.Api.Services
{
    public class TempConvertClient : SoapClientBase
    {

        public TempConvertClient(HttpClient httpClient) : base(httpClient)
        {

        }
        public async Task<object> CelsiusToFahrenheitAsync(string celsius)
        {
            string soapAction = "https://www.w3schools.com/xml/CelsiusToFahrenheit";
            string bodyContent = $@"
            <CelsiusToFahrenheit xmlns='https://www.w3schools.com/xml/'>
                <Celsius>{celsius}</Celsius>
            </CelsiusToFahrenheit>";

            string soapEnvelope = CreateSoapEnvelope(bodyContent);

            string response = await CallSoapWebServiceAsync(soapAction, soapEnvelope);

            // Aqui você pode processar a resposta para extrair o valor desejado.
            return new { resposta = response } ;
        }

    }
}
