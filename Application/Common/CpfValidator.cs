namespace Application.Common
{
    public static class CpfValidator
    {
        public static string Normalize(string? cpf) =>
            new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());

        public static bool IsValid(string cpf)
        {
            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            if (cpf.Distinct().Count() == 1)
                return false;

            var sum = 0;

            for (var i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);

            var firstDigit = sum % 11 < 2 ? 0 : 11 - sum % 11;

            if (cpf[9] - '0' != firstDigit)
                return false;

            sum = 0;

            for (var i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            var secondDigit = sum % 11 < 2 ? 0 : 11 - sum % 11;

            return cpf[10] - '0' == secondDigit;
        }
    }
}
