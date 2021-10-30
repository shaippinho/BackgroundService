using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Application.DTO
{
    public class ResultadoMonitoramento
    {
        public string Horario { get; set; }
        public string Host { get; set; }
        public string Status { get; set; }
        public string Exception { get; set; }
    }
}
