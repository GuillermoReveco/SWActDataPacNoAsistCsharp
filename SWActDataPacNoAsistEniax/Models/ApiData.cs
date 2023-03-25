using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataPacNoAsistEniax.Models
{
    class ApiData
    {
        public CambioEstCita cambioEstadoCita { get; set; }
    }
    public class CambioEstCita
    {
        public string id_cita { get; set; }
        public string estado { get; set; }
        public string fecha { get; set; }
        public string responsable { get; set; }
        public string canal_estado { get; set; }
        public string motivo { get; set; }
        public string informacion_adicional { get; set; }
    }
    public class RespApi
    {
        public string id_transaccion { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }
    public class CitaProcesada
    {
        public string id_cita { get; set; }
        public string accion { get; set; }
        public string estado { get; set; }
        public string fecha_envio { get; set; }
        public string fecha_cita { get; set; }
        public string id_transaction { get; set; }
    }
    public class LOGApi
    {
        public int codError { get; set; }
        public string gloError { get; set; }
        public int logEjecProcID { get; set; }
        public string logid_cita { get; set; }
        public string fecha { get; set; }
        public string fechaIni { get; set; }
        public string fechaFin { get; set; }
        public string tipoMetodo { get; set; }
        public string urlMetodo { get; set; }
        public string body { get; set; }
    }
    public class SalParametros
    {
        public string horaEjecucion { get; set; }
        public string minFrecuencia { get; set; }
        public string horaInicio { get; set; }
        public string horaTermino { get; set; }
        public string conexionOracle { get; set; }
        public string urlNotificacionCitas { get; set; }
        public string urlCambioEstado { get; set; }
        public string xUserEniax { get; set; }
        public string xPasswordEniax { get; set; }
        public string xAuthorizationToken { get; set; }

    }
    public class LOGEjecProceso
    {
        public string logEjecProcNom { get; set; }
        public string logEjecFecIni { get; set; }
        public string logEjecFecFin { get; set; }

    }
    public class ActLOGEjecProceso
    {
        public int logEjecProcID { get; set; }
        public string logEjecProcNom { get; set; }
        public string logEjecFecIni { get; set; }
        public string logEjecFecFin { get; set; }

    }
    public class SWLOG
    {
        public string fecha { get; set; }
        public string Proceso { get; set; }
        public string GloError { get; set; }

    }

}
