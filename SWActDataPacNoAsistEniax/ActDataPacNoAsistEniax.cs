using Newtonsoft.Json;
using SWActDataPacNoAsistEniax.Conexion;
using SWActDataPacNoAsistEniax.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataPacNoAsistEniax
{
    partial class ActDataPacNoAsistEniax : ServiceBase
    {
        public static string urlNotificacionCitas { get; set; }
        public static string urlCambioEstado { get; set; }
        public static string xUserEniax { get; set; }
        public static string xPasswordEniax { get; set; }
        public static string xAuthorizationToken { get; set; }
        public static string conexionOracle { get; set; }
        public static string rangoMin { get; set; }
        public static string Etiqueta { get; set; }
        public static string fechaIni { get; set; }
        public static string fechaFin { get; set; }
        public static string rangoIni { get; set; }
        public static string rangoFin { get; set; }
        public static string horaEjec { get; set; }
        public static int logEjecProcID { get; set; }

        private int TimerConsulta;
        bool blEnProceso = false;
        public ActDataPacNoAsistEniax()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            int TimerCons;
            ConexionSQL con = new ConexionSQL();
            SalParametros regParametros = new SalParametros();
            regParametros = con.ConsultaParametros(EventLog);
            horaEjec = regParametros.horaEjecucion;
            rangoMin = regParametros.minFrecuencia;
            TimerCons = Int32.Parse(rangoMin);
            // en duro el timer
            //TimerCons = 60;
            TimerCons = TimerCons * 60000;

            rangoIni = regParametros.horaInicio;
            rangoIni = rangoIni.Trim() + ":00";
            rangoFin = regParametros.horaTermino;
            rangoFin = rangoFin.Trim() + ":00";

            conexionOracle = regParametros.conexionOracle;
            urlNotificacionCitas = regParametros.urlNotificacionCitas;
            urlCambioEstado = regParametros.urlCambioEstado;
            xUserEniax = regParametros.xUserEniax;
            xPasswordEniax = regParametros.xPasswordEniax;
            xAuthorizationToken = regParametros.xAuthorizationToken;

            //TimerCons = ConfigurationManager.AppSettings["TimerConsulta"].ToString();
            //TimerConsulta = TimerCons;
            //stLapso.Interval = TimerConsulta;
            TimerConsulta = TimerCons;
            stLapso.Interval = TimerConsulta;
            stLapso.Enabled = true;
            stLapso.Start();// TODO: agregar código aquí para iniciar el servicio.
            // TODO: agregar código aquí para iniciar el servicio.
        }

        protected override void OnStop()
        {
            stLapso.Stop();// TODO: agregar código aquí para realizar cualquier anulación necesaria para detener el servicio.
            // TODO: agregar código aquí para realizar cualquier anulación necesaria para detener el servicio.
        }

        private void stLapso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            stLapso.Stop();
            stLapso.Enabled = false;
            EventLog.WriteEntry("Inicio - Proceso - Paciente No Asiste " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            if (blEnProceso) return;
            //INI - Rescate de Parametros
            //int TimerCons;
            //ConexionSQL con = new ConexionSQL();
            //SalParametros regParametros = new SalParametros();
            //regParametros = con.ConsultaParametros();
            //horaEjec = regParametros.horaEjecucion;
            //rangoMin = regParametros.minFrecuencia;
            ////TimerCons = Int32.Parse(rangoMin);
            //TimerCons = 60;
            //TimerCons = TimerCons * 60000;

            //rangoIni = regParametros.horaInicio;
            //rangoIni = rangoIni.Trim() + ":00";
            //rangoFin = regParametros.horaTermino;
            //rangoFin = rangoFin.Trim() + ":00";

            //conexionOracle = regParametros.conexionOracle;
            //urlNotificacionCitas = regParametros.urlNotificacionCitas;
            //urlCambioEstado = regParametros.urlCambioEstado;
            //xUserEniax = regParametros.xUserEniax;
            //xPasswordEniax = regParametros.xPasswordEniax;
            //xAuthorizationToken = regParametros.xAuthorizationToken;
            //FIN - Rescate de Parametros
            try
            {
                blEnProceso = true;
                ProcesoPrincipal(EventLog);

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Problemas en Servicio ActDataPacNoAsistEniax: " + ex.Message, EventLogEntryType.Error);
            }

            EventLog.WriteEntry("Fin - Proceso - Paciente No Asiste " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            blEnProceso = false;
            stLapso.Interval = TimerConsulta;
            stLapso.Enabled = true;
            stLapso.Start();
        }
        static async Task ProcesoPrincipal(EventLog log)
        {
            //Console.WriteLine("Inicio - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            //Console.WriteLine("Rescata Parametros - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            ConexionOracle cnn = new ConexionOracle();
            cnn.var_cadenaconexion = conexionOracle;
            cnn.var_min = rangoMin.Trim();
            cnn.horIni = rangoIni.Trim();
            cnn.horFin = rangoFin.Trim();

            //// RescataFechas
            //Console.WriteLine("Rescata Fechas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog log = new EventLog();
            //cnn.RescataFechas(log);
            cnn.RescataFechas(log);
            fechaIni = cnn.var_FechaRango;
            fechaFin = cnn.var_FechaActual;
            string[] fechasAct = fechaIni.Split(' ');
            string[] fechasRan = fechaFin.Split(' ');
            fechaIni = fechasRan[0] + " " + rangoIni.Trim();
            fechaFin = fechasAct[0] + " " + rangoFin.Trim();

            //fechaIni = "27/08/2022 " + rangoIni;
            //fechaFin = "27/08/2022 " + rangoFin;
            cnn.var_FechaRango = fechaIni;
            cnn.var_FechaActual = fechaFin;

            //log.WriteEntry("Fecha Actual:-" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "-", EventLogEntryType.Information);
            //log.WriteEntry("Fecha Inicio:-" + fechaIni + "-", EventLogEntryType.Information);
            //log.WriteEntry("Fecha Fin   :-" + fechaFin + "-", EventLogEntryType.Information);


            // El Paciente No Asistio a su cita (14)
            string Hora = DateTime.Now.ToString("HH:mm");
            string HoraProc = rangoFin.Substring(0, 2);
            //Descomentar para Pruebas con hora de prueba
            //rangoFin = "15";
            Hora = Hora.Substring(0, 2);
            //Hora = "21";
            if (Hora == HoraProc)
            {
                //Console.WriteLine("El Paciente No Asistio a su cita - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                ConexionSQL conf = new ConexionSQL();
                string fec = "";
                fec = conf.ConsultaPacNoAsiste(log);
                if (fec == "0")
                {
                    var dataProc = new LOGEjecProceso();
                    dataProc.logEjecProcNom = "Paciente No Asistio a su cita";
                    dataProc.logEjecFecIni = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var listRegPacNoAsis = new List<CambioEstCita>();
                    listRegPacNoAsis = cnn.ConsultaPacienteNoAsiste(log); // Paciente No Asistio a su cita (14)

                    ConexionSQL conn = new ConexionSQL();   // logEjecProcID
                    dataProc.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    logEjecProcID = conn.IngLOGEjecProceso(dataProc, log);

                    ConexionSQL conpn = new ConexionSQL();
                    foreach (var dataCon in listRegPacNoAsis)
                    {
                        var dataApi = new CambioEstCita();
                        dataApi.id_cita = dataCon.id_cita;
                        dataApi.estado = dataCon.estado;
                        dataApi.fecha = dataCon.fecha;
                        dataApi.responsable = dataCon.responsable;
                        dataApi.canal_estado = dataCon.canal_estado;
                        dataApi.motivo = dataCon.motivo;
                        dataApi.informacion_adicional = dataCon.informacion_adicional;
                        var Ind = conpn.ConsultaCita(Convert.ToInt32(dataApi.id_cita), "M", Convert.ToInt32(dataApi.estado), log);
                        if (Ind == 0)
                        {
                            // Llamadao a Api
                            var prog = new ActDataPacNoAsistEniax();
                            var resultado = await prog.LlamaApiCambioEstado(dataApi, log);

                        }

                    }
                    var dataProcAct = new ActLOGEjecProceso();
                    ConexionSQL conns = new ConexionSQL();   // logEjecProcID

                    dataProcAct.logEjecProcID = logEjecProcID;
                    dataProcAct.logEjecProcNom = dataProc.logEjecProcNom;
                    dataProcAct.logEjecFecIni = dataProc.logEjecFecIni;
                    dataProcAct.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    conns.ActLOGEjecProceso(dataProcAct, log);

                }

            }
        //Console.WriteLine("Fin - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

    }
        private async Task<bool> LlamaApiCambioEstado(CambioEstCita dataBody, EventLog log)
        {
            string url;
            string gloMetodo = "";
            switch (dataBody.estado)
            {
                case "0":
                    gloMetodo = "NOTI CITAS";
                    break;
                case "1":
                    gloMetodo = "CONF CITAS";
                    break;
                case "2":
                    gloMetodo = "ANU CITA";
                    break;
                case "3":
                    gloMetodo = "BLOQ CITAS";
                    break;
                case "5":
                    gloMetodo = "PAGO CITA";
                    break;
                case "14":
                    gloMetodo = "NO ASISTE";
                    break;
                default:
                    gloMetodo = "";
                    break;
            }

            url = urlCambioEstado + dataBody.id_cita.Trim();

            var jsonString = JsonConvert.SerializeObject(dataBody);
            HttpContent httpContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();

            //string contentType = "application/json";
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            client.DefaultRequestHeaders.Add("X-User-Eniax", xUserEniax);
            client.DefaultRequestHeaders.Add("X-Password-Eniax", xPasswordEniax);
            client.DefaultRequestHeaders.Add("X-Authorization-Token", xAuthorizationToken);
            Etiqueta = "[PutAsync]";
            //try
            //{

            var httpResponse = await client.PutAsync(url, httpContent);
            //var httpResponse = await client.PutAsJsonAsync() .PutAsync(url, httpContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                //var dataApi = JsonSerializer.Deserialize<ApiData>(content);
                var respApi = JsonConvert.DeserializeObject<RespApi>(content);

                ConexionSQL con = new ConexionSQL();
                var citaProcesada = new CitaProcesada();
                citaProcesada.id_cita = dataBody.id_cita;
                citaProcesada.accion = "M";
                citaProcesada.estado = dataBody.estado;
                citaProcesada.fecha_envio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                citaProcesada.fecha_cita = dataBody.fecha;
                citaProcesada.id_transaction = respApi.id_transaccion;
                con.IngCitaProcesada(citaProcesada, log); // Ingresar Cita Procesada

                var logApi = new LOGApi();
                logApi.codError = (int)httpResponse.StatusCode; ///200;
                logApi.gloError = httpResponse.ReasonPhrase;//respApi.message;
                logApi.logEjecProcID = logEjecProcID;
                logApi.logid_cita = dataBody.id_cita;
                logApi.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                logApi.fechaIni = fechaIni;
                logApi.fechaFin = fechaFin;
                logApi.tipoMetodo = gloMetodo; // "CAM ESTADO";
                logApi.urlMetodo = url;
                logApi.body = jsonString;
                con.IngLOGApi(logApi, log); // Ingresar LOG

            }
            else
            {
                ConexionSQL con = new ConexionSQL();
                var logApi = new LOGApi();
                logApi.codError = (int)httpResponse.StatusCode;  //500;
                logApi.gloError = httpResponse.ReasonPhrase;//"Error, al Llamar a API";
                logApi.logEjecProcID = logEjecProcID;
                logApi.logid_cita = dataBody.id_cita;
                logApi.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                logApi.fechaIni = fechaIni;
                logApi.fechaFin = fechaFin;
                logApi.tipoMetodo = gloMetodo; // "CAM ESTADO";
                logApi.urlMetodo = url;
                logApi.body = jsonString;
                con.IngLOGApi(logApi, log); // Ingresar LOG

            }
            //}
            //catch (HttpRequestException httpEx)
            //{
            //    var statusCodeString = httpEx.Message.Substring(ErrorStatusCodeStart.Length, 3);
            //    var statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCodeString);

            //    ConexionSQL con = new ConexionSQL();
            //    con.var_cadenaconexion = conexionSQL;
            //    var logApi = new LOGApi();
            //    logApi.codError = (int)statusCode;  //500;
            //    logApi.gloError = "Error, al Llamar a API";
            //    logApi.fecha = DateTime.Now.ToString();
            //    logApi.tipoMetodo = "CAM ESTADO";
            //    logApi.urlMetodo = url;
            //    logApi.body = jsonString;
            //    con.IngLOGApi(logApi); // Ingresar LOG

            //}
            return true;
        }
    }
}
