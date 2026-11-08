using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class EnrollmentRequestDTO
    {
        [Required(ErrorMessage = "Aluno é obrigatório")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Instrumento é obrigatório")]
        public Guid InstrumentId { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Mensalidade é obrigatória")]
        [Range(0.01, 9999999999, ErrorMessage = "Mensalidade deve ser maior que zero")]
        public decimal MonthlyFee { get; set; }

        [Required(ErrorMessage = "Dia de vencimento é obrigatório")]
        [Range(1, 28, ErrorMessage = "Dia de vencimento deve estar entre 1 e 28")]
        public int DueDay { get; set; }
    }
}
