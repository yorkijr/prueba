using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Text;
using System.Net;

/// <summary>
/// Descripción breve de clsWSSabre
/// </summary>
/// 
namespace servicio_emision.App_Data
{
    class clsWSSabre
    {
        public clsWSSabre()
        {
            //
            // TODO: Agregar aquí la lógica del constructor
            //
            //cambio a protocolo TLS 1.2 DIOD
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        }

        /// <summary>
        /// Función que trae la comisión base para cierta linea aérea, clase y origen
        /// </summary>
        /// <param name="strReserva"></param>
        /// <param name="strLineaAerea"></param>
        /// <param name="strClase"></param>
        /// <param name="strOrigen"></param>
        /// <returns></returns>
        public decimal decTraeComisionBase(string strReserva, string strLineaAerea, string strClase, string strOrigen)
        {
            SqlConnection SqlConn = new SqlConnection(ConfigurationManager.AppSettings["StringConexion"].ToString());
            StringBuilder sb = new StringBuilder();
            SqlDataReader read;
            decimal decRetorno = 0;

            try
            {
                sb.Append("EXEC DBO.PRO_EMI_TRAE_COMISION_BASE ");
                sb.Append("'" + strLineaAerea + "',");
                sb.Append("'" + strClase + "',");
                sb.Append("'" + strOrigen + "'");

                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), SqlConn);

                SqlConn.Open();
                read = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                if (read.Read())
                {
                    decRetorno = Convert.ToDecimal(read[0].ToString());
                }
            }
            catch (Exception ex)
            {
                Log.grabaLog("clsWSSabre_decTraeComisionBase", strReserva, "Err: " + ex.Message);
            }
            finally
            {
                if (SqlConn.State == ConnectionState.Open)
                {
                    SqlConn.Close();
                }
            }

            return decRetorno;
        }

        /// <summary>
        /// Función que trae el over agencia para cierta linea aérea, clase y origen
        /// </summary>
        /// <param name="strReserva"></param>
        /// <param name="strLineaAerea"></param>
        /// <param name="strClase"></param>
        /// <param name="strOrigen"></param>
        /// <returns></returns>
        public DataSet dsTraeOverAgencia(string strReserva, string strLineaAerea, string strClase, string strOrigen,
            string strDestino, DateTime dteFechaInicio, DateTime dteFechaTermino)
        {
            SqlConnection SqlConn = new SqlConnection(ConfigurationManager.AppSettings["StringConexion"].ToString());
            StringBuilder sb = new StringBuilder();
            DataSet ds = new DataSet();

            try
            {
                sb.Append("EXEC DBO.PRO_EMI_TRAE_OVER_AGENCIA ");
                sb.Append("'" + strLineaAerea + "',");
                sb.Append("'" + strClase + "',");
                sb.Append("'" + strOrigen + "',");
                sb.Append("'" + strDestino + "',");
                sb.Append("'" + dteFechaInicio.ToString("yyyyMMdd") + "',");
                sb.Append("'" + dteFechaTermino.ToString("yyyyMMdd") + "'");


                SqlDataAdapter da = new SqlDataAdapter(sb.ToString(), SqlConn);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                Log.grabaLog("clsWSSabre_decTraeOverAgencia", strReserva, "Err: " + ex.Message);
            }
            return ds;
        }





        /// <summary>
        /// Funcion que devuelve verdadero si una linea aerea permite emisión de infantes
        /// </summary>
        /// <param name="strLineaAerea"></param>
        /// <returns></returns>
        public bool bolLineaAereaEmisionInf(string strLineaAerea)
        {
            SqlConnection SqlConn = new SqlConnection(ConfigurationManager.AppSettings["StringConexion"].ToString());
            StringBuilder sb = new StringBuilder();
            SqlDataReader read;
            bool bolEmiteInf = false;

            try
            {
                sb.Append("EXEC DBO.PRO_TRAE_LINEA_AEREA_EMISION_INF ");
                sb.Append("'" + strLineaAerea + "'");

                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), SqlConn);

                SqlConn.Open();
                read = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                if (read.Read())
                {
                    bolEmiteInf = Convert.ToBoolean(read["EMITE_INF"].ToString());
                }
            }
            catch (Exception ex)
            {
                Log.grabaLog("clsWSSabre_decTraeOverAgencia", strLineaAerea, "Err: " + ex.Message);
            }
            finally
            {
                if (SqlConn.State == ConnectionState.Open)
                {
                    SqlConn.Close();
                }
            }

            return bolEmiteInf;
        }


        /// <summary>
        /// Funcion que devuelve verdadero si una linea aerea permite emisión automatica
        /// </summary>
        /// <param name="strLineaAerea"></param>
        /// <returns></returns>
        public bool bolLineaAereaEmiteAutomaticamente(string strLineaAerea)
        {
            SqlConnection SqlConn = new SqlConnection(ConfigurationManager.AppSettings["StringConexion"].ToString());
            StringBuilder sb = new StringBuilder();
            SqlDataReader read;
            bool bolEmiteAutomaticamente = false;

            try
            {
                sb.Append("EXEC DBO.PRO_EMI_EMITE_LINEA_AEREA ");
                sb.Append("'" + strLineaAerea + "'");

                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), SqlConn);

                SqlConn.Open();
                read = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                if (read.Read())
                {
                    if (read["CUENTA"].ToString() != "0")
                    {
                        bolEmiteAutomaticamente = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.grabaLog("clsWSSabre_decTraeOverAgencia", strLineaAerea, "Err: " + ex.Message);
            }
            finally
            {
                if (SqlConn.State == ConnectionState.Open)
                {
                    SqlConn.Close();
                }
            }

            return bolEmiteAutomaticamente;
        }


        public string strTraeValorParametro(string strReserva, string strParametro)
        {
            SqlConnection SqlConn = new SqlConnection(ConfigurationManager.AppSettings["StringConexion"].ToString());
            StringBuilder sb = new StringBuilder();
            SqlDataReader read;
            string strRetorno = "";

            try
            {
                sb.Append("EXEC DBO.PRO_EMI_TRAE_PARAMETRO ");
                sb.Append("'" + strParametro + "'");

                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), SqlConn);

                SqlConn.Open();
                read = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                if (read.Read())
                {
                    strRetorno = read[0].ToString();
                }
            }
            catch (Exception ex)
            {
                Log.grabaLog("clsWSSabre_strTraeValorParametro", strReserva, "Err: " + ex.Message);
            }
            finally
            {
                if (SqlConn.State == ConnectionState.Open)
                {
                    SqlConn.Close();
                }
            }

            return strRetorno;
        }


        public bool bolExisteMsjError(string strReserva, string strMensaje)
        {
            SqlConnection SqlConn = new SqlConnection(ConfigurationManager.AppSettings["StringConexion"].ToString());
            StringBuilder sb = new StringBuilder();
            SqlDataReader read;
            bool bolRetorno = false;

            try
            {
                sb.Append("EXEC DBO.PRO_EMI_BUSCA_MSJ_ERROR ");
                sb.Append("'" + strMensaje + "'");

                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), SqlConn);

                SqlConn.Open();
                read = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                if (read.Read())
                {
                    if (read[0].ToString() != "0")
                    {
                        bolRetorno = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.grabaLog("clsWSSabre_bolExisteMsjError", strReserva, "Err: " + ex.Message);
            }
            finally
            {
                if (SqlConn.State == ConnectionState.Open)
                {
                    SqlConn.Close();
                }
            }

            return bolRetorno;
        }

    }
}
