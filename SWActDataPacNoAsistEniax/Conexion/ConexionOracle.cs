using Oracle.ManagedDataAccess.Client;
using SWActDataPacNoAsistEniax.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataPacNoAsistEniax.Conexion
{
    class ConexionOracle
    {
        public string var_cadenaconexion = "";
        public string var_min = "";
        public string var_FechaActual = "";
        public string var_FechaRango = "";

        public string horIni = "";
        public string horFin = "";

        OracleConnection var_conexion = new OracleConnection();

        private void abrirconexion()
        {
            var_conexion.ConnectionString = var_cadenaconexion;
            if (var_conexion.State == ConnectionState.Closed)
                var_conexion.Open();
        }
        private void cerrarconexion()
        {
            if (var_conexion.State == ConnectionState.Open)
                var_conexion.Close();
        }
        public void RescataFechas(EventLog log)
        {
            try
            {
                string queryString = "select ";
                queryString += "  to_char(sysdate, 'dd/mm/yyyy hh24:mi') FecActual ";
                queryString += ", to_char(sysdate - " + var_min + "/1440, 'dd/mm/yyyy hh24:mi') FecRango ";
                queryString += "from dual ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var_FechaActual = dr["FecActual"].ToString();
                    var_FechaRango = dr["FecRango"].ToString();
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(RescataFechas). " + ex.Message);
                //Console.ReadLine
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(RescataFechas): " + ex.Message, EventLogEntryType.Error);
                try
                {
                    ConexionSQL con = new ConexionSQL();
                    var dataSW = new SWLOG();
                    dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    dataSW.Proceso = "ActDataPacNoAsistEniax";
                    dataSW.GloError = "Hay error al acceder a la base de datos Oracle(RescataFechas): " + ex.Message;
                    con.IngSWLOG(dataSW, log);
                }
                catch (Exception err)
                {
                    log.WriteEntry("Hay error al acceder a la base de datos SQL(IngSWLOG): " + err.Message, EventLogEntryType.Error);
                }
                throw;
            }

        }
        public List<CambioEstCita> ConsultaPacienteNoAsiste(EventLog log)
        {
            List<CambioEstCita> listRegPacPag = new List<CambioEstCita>();
            string[] fechasAct = var_FechaActual.Split(' ');
            string[] fechasRan = var_FechaRango.Split(' ');

            string fecha1 = fechasRan[0];
            string fecha2 = fechasAct[0];
            //string hora1 = fechasAct[1];
            //string hora2 = fechasRan[1];

            try
            {
                string queryString = "select ";
                queryString += "r.correl_reserva   id_cita ";
                queryString += ", 14  estado "; //--- El Paciente No Asistio a su Cita
                queryString += ", to_Char( fecha_reserva ,'yyyy-mm-dd') Fecha";
                //queryString += ", fecha_reserva Fecha";
                queryString += ", resping_reserva responsable";
                queryString += ", case r.resping_reserva ";
                queryString += " when 'UINTERNET' then 'INTERNET'";
                queryString += " when 'UAVIRTUAL' then 'ALICIA'";
                queryString += " when 'TELEC_ENX' then 'TELECONSULTA'";
                queryString += " else ";
                queryString += "  decode ( nvl( r.resping_reserva,'X') ,'X','X',  decode ( (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and  cod_grupo=1 and id_usuario=r.resping_reserva),0,   decode (   (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and   cod_grupo in (172,163)   and id_usuario=r.resping_reserva)  , 0,   decode (  (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and  (cod_grupo<> 172 and cod_grupo<>163  and cod_grupo<>1) and id_usuario=r.resping_reserva ) , 0,'NN','PRESENCIAL'), 'TRUSTCORP' )   ,'CALL_CENTER'))";
                queryString += " end  Canal_estado";
                queryString += "  from ";
                queryString += "  am_Reserva r ";
                queryString += ", tab_paciente tp ";
                queryString += " where r.cod_empresa=8 ";
                queryString += " and r.cod_sucursal>=1 ";
                queryString += " and r.cod_unidad>=0 ";
                queryString += " and tp.cod_empresa=r.cod_empresa ";
                queryString += " and tp.id_ambulatorio=r.id_ambulatorio ";

                //queryString += " and to_date(fecha_reserva,'dd/mm/yyyy') BETWEEN TO_DATE(sysdate - " + var_min + "/1440, 'dd/mm/yyyy') AND TO_DATE(sysdate, 'dd/mm/yyyy') ";
                //queryString += " and hora_reserva >= to_char(sysdate - " + var_min + "/1440, 'hh24:mi') ";
                queryString += " and fecha_reserva >=to_date('" + fecha1 + "','dd/mm/yyyy') ";
                queryString += " and fecha_reserva <= to_date('" + fecha2 + "','dd/mm/yyyy hh24:mi') ";
                queryString += " and hora_reserva < '" + horFin + "' ";
                queryString += " and hora_reserva >= '" + horIni + "' ";
                //queryString += " and fecha_reserva >=to_date('07/07/2022','dd/mm/yyyy') ";
                //queryString += " and fecha_reserva <= to_date('07/07/2022','dd/mm/yyyy hh24:mi') ";
                //queryString += " and hora_reserva < to_char(sysdate,'HH24:MI') ";
                //queryString += " and hora_reserva >= to_char(sysdate - 15/1440,'HH24:MI') ";

                queryString += " and antr_reserva is  null ";
                queryString += " and nvl(rec_reserva,'N')='N' ";
                queryString += " and (r.modulo_reserva<>'KIN2I') ";

                queryString += " and tp.id_ambulatorio not in ";
                queryString += " ( ";
                queryString += " select ";
                queryString += " id_ambulatorio ";
                queryString += " from ";
                queryString += " tab_paciente p ";
                queryString += " where ";
                queryString += " p.cod_empresa=8 ";
                queryString += " and p.vigencia='S' ";
                queryString += " and  ( ";
                queryString += " p.apepat_paciente like '%NODAR%' ";
                queryString += " or p.apepat_paciente like '%NO DAR%' ";
                queryString += " or p.apepat_paciente like '%AGENDAR%' ";
                queryString += " or p.apepat_paciente like '%RESERV%' ";
                queryString += " or p.apepat_paciente like '%HORARIO%' ";
                queryString += " or p.apepat_paciente like '%MEDICO%' ";
                queryString += " or p.apepat_paciente like '%NINGUNA%' ";
                queryString += " or p.apepat_paciente like '%NO VIENE%' ";
                queryString += " or p.apepat_paciente like '%BLOQUE%' ";
                queryString += " or p.apepat_paciente like '%PACIENTE%' ";
                queryString += " or p.apepat_paciente like '%HOSPITA%' ";
                queryString += " or p.apepat_paciente like '%CONTROL%' ";
                queryString += " or p.apepat_paciente like '%HORA%' ";
                queryString += " or p.apepat_paciente like '%INTERCONS%' ";
                queryString += " or p.apepat_paciente like '%ANULA%' ";
                queryString += " or p.apepat_paciente like '%NO BORRAR%' ";
                queryString += " or p.apepat_paciente like '%NO ANOTAR%' ";
                queryString += " or p.apepat_paciente like '%NO ASISTE%' ";
                queryString += " or p.apepat_paciente like '%RES NO%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%REPETI%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '% RESPIRATORIO%' ";
                queryString += " or p.apepat_paciente like '%NO CITAR%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '%NO ESCRIBIR%' ";
                queryString += " ) ";
                queryString += " ) ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CambioEstCita regPacPag = new CambioEstCita();
                    regPacPag.id_cita = dr["id_cita"].ToString();
                    regPacPag.estado = dr["estado"].ToString();
                    regPacPag.fecha = dr["Fecha"].ToString();
                    regPacPag.responsable = dr["responsable"].ToString();
                    regPacPag.canal_estado = dr["Canal_Estado"].ToString();
                    regPacPag.motivo = "";
                    regPacPag.informacion_adicional = "";

                    listRegPacPag.Add(regPacPag);
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(ConsultaPacNoAsiste). " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(ConsultaPacienteNoAsiste): " + ex.Message, EventLogEntryType.Error);
                try
                {
                    ConexionSQL con = new ConexionSQL();
                    var dataSW = new SWLOG();
                    dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    dataSW.Proceso = "ActDataPacNoAsistEniax";
                    dataSW.GloError = "Hay error al acceder a la base de datos Oracle(ConsultaPacienteNoAsiste): " + ex.Message;
                    con.IngSWLOG(dataSW, log);
                }
                catch (Exception err)
                {
                    log.WriteEntry("Hay error al acceder a la base de datos SQL(IngSWLOG): " + err.Message, EventLogEntryType.Error);
                }
                throw;
            }
            return listRegPacPag;
        }
    }
}
