namespace Portal.Fornecedor.Api.Util
{
    public static class CommonsExtension
    {
        #region " String "

        public static string removeAccents(this string texto) => Commons.RemoveAccents(texto);

        public static string GetQueryString<T>(this T obj, bool usingEncode = false, IEnumerable<string> propsExcluded = null) where T : class => Commons.GetQueryString(obj, usingEncode, propsExcluded);

        public static bool IsGuid(this string numero) => Commons.IsGuid(numero);

        /// <summary>
        /// Formatar uma string CNPJ
        /// </summary>
        /// <param name="CNPJ">string CNPJ sem formatacao</param>
        /// <returns>string CNPJ formatada</returns>
        /// <example>Recebe '99999999999999' Devolve '99.999.999/9999-99'</example>

        public static string FormatCNPJ(this string CNPJ) => Commons.FormatCNPJ(CNPJ);

        /// <summary>
        /// Formatar uma string CPF
        /// </summary>
        /// <param name="CPF">string CPF sem formatacao</param>
        /// <returns>string CPF formatada</returns>
        /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

        public static string FormatCPF(this string CPF) => Commons.FormatCPF(CPF);

        public static bool IsValidEmail(this string email) => Commons.IsValidEmail(email);

        public static bool IsCnpj(this string cnpj) => Commons.IsCnpj(cnpj);

        public static bool IsCpf(this string cpf) => Commons.IsCpf(cpf);

        public static string FormatRG(this string texto) => Commons.FormatRG(texto);

        public static string OnlyNumbers(this string numeros) => Commons.OnlyNumbers(numeros);

        #endregion
    }
}
