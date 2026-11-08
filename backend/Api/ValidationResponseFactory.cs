using System.Text.RegularExpressions;
using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api
{
    public static partial class ValidationResponseFactory
    {
        private static readonly Dictionary<string, string> FieldLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "nome",
            ["email"] = "e-mail",
            ["password"] = "senha",
            ["currentPassword"] = "senha atual",
            ["newPassword"] = "nova senha",
            ["role"] = "perfil",
            ["phone"] = "telefone",
            ["cpf"] = "CPF",
            ["birthDate"] = "data de nascimento",
            ["address"] = "endereço",
            ["street"] = "rua",
            ["number"] = "número",
            ["city"] = "cidade",
            ["state"] = "UF",
            ["zipCode"] = "CEP",
            ["guardian"] = "responsável",
            ["specialties"] = "especialidades",
            ["description"] = "descrição",
            ["studentId"] = "aluno",
            ["instrumentId"] = "instrumento",
            ["teacherId"] = "professor",
            ["enrollmentId"] = "matrícula",
            ["startDate"] = "data de início",
            ["endDate"] = "data de término",
            ["monthlyFee"] = "mensalidade",
            ["dueDay"] = "dia de vencimento",
            ["startsAt"] = "data da aula",
            ["newStartsAt"] = "nova data da aula",
            ["durationMinutes"] = "duração",
            ["room"] = "sala",
            ["weekday"] = "dia da semana",
            ["time"] = "horário",
            ["reason"] = "motivo",
            ["referenceMonth"] = "mês de referência",
            ["paidAt"] = "data de pagamento",
            ["amount"] = "valor",
            ["method"] = "forma de pagamento",
        };

        public static IActionResult Create(ActionContext context)
        {
            var entries = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToList();

            var hasJsonErrors = entries.Any(entry => entry.Key.StartsWith("$."));

            var errors = entries
                .Where(entry => entry.Key.StartsWith("$.") || !hasJsonErrors)
                .GroupBy(entry => FieldKey(entry.Key))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(entry => entry.Value!.Errors.Select(error => Translate(group.Key, error)))
                        .Distinct()
                        .ToArray());

            var firstError = errors.Values.SelectMany(messages => messages).FirstOrDefault();

            return new BadRequestObjectResult(new
            {
                Message = firstError ?? "Parâmetros da requisição inválidos",
                Status = ResponseStatus.InvalidArgument,
                Data = errors
            });
        }

        private static string FieldKey(string key)
        {
            var name = key.StartsWith("$.") ? key[2..] : key;
            return string.Join('.', name.Split('.').Select(part =>
                part.Length > 0 ? char.ToLowerInvariant(part[0]) + part[1..] : part));
        }

        private static string Label(string fieldKey)
        {
            if (string.IsNullOrEmpty(fieldKey))
                return "requisição";

            var last = fieldKey.Split('.').Last();
            return FieldLabels.TryGetValue(last, out var label) ? label : last;
        }

        private static string Translate(string fieldKey, ModelError error)
        {
            var message = error.ErrorMessage;
            var label = Label(fieldKey);

            if (error.Exception != null ||
                string.IsNullOrWhiteSpace(message) ||
                message.Contains("could not be converted", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("JSON", StringComparison.Ordinal))
            {
                return string.IsNullOrEmpty(fieldKey)
                    ? "Corpo da requisição inválido"
                    : $"Valor inválido para {label}";
            }

            if (RequiredPattern().IsMatch(message))
                return $"O campo {label} é obrigatório";

            var range = RangePattern().Match(message);
            if (range.Success)
                return $"O campo {label} deve estar entre {range.Groups[1].Value} e {range.Groups[2].Value}";

            var minLength = MinLengthPattern().Match(message);
            if (minLength.Success)
                return $"O campo {label} deve ter pelo menos {minLength.Groups[1].Value} caracteres";

            var maxLength = MaxLengthPattern().Match(message);
            if (maxLength.Success)
                return $"O campo {label} deve ter no máximo {maxLength.Groups[1].Value} caracteres";

            if (EmailPattern().IsMatch(message))
                return $"O campo {label} não é um e-mail válido";

            return message;
        }

        [GeneratedRegex(@"^The .+ field is required\.?$")]
        private static partial Regex RequiredPattern();

        [GeneratedRegex(@"^The field .+ must be between (.+) and (.+)\.$")]
        private static partial Regex RangePattern();

        [GeneratedRegex(@"minimum length of '(\d+)'")]
        private static partial Regex MinLengthPattern();

        [GeneratedRegex(@"maximum length of '(\d+)'")]
        private static partial Regex MaxLengthPattern();

        [GeneratedRegex(@"is not a valid e-mail address")]
        private static partial Regex EmailPattern();
    }
}
