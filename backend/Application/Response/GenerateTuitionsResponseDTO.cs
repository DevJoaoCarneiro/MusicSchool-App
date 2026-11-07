using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class GenerateTuitionsResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public GenerateTuitionsData? Data { get; set; }
    }

    public class GenerateTuitionsData
    {
        public string ReferenceMonth { get; set; } = string.Empty;

        public int Created { get; set; }

        public int Skipped { get; set; }
    }
}