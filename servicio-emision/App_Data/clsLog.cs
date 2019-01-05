using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using servicio_emision;
using servicio_emision.App_Data;



/// <summary>
/// Descripción breve de clsLog
/// </summary>
namespace servicio_emision.App_Data
{
    class Log
    {
        public Log()
        {
            //
            // TODO: Agregar aquí la lógica del constructor
            //
        }

         public static void grabaLog(string strClase, string strReserva, string strDescripcion)
        {
            string linea;
            string strArchivo = "wsEmisionFacialSabre" + System.DateTime.Today.ToString().Replace("/", "") + ".txt";
            strArchivo = Convert.ToString(strArchivo).Replace(":", "");
            strArchivo = Convert.ToString(strArchivo).Replace("-", "");
            strArchivo = Convert.ToString(strArchivo).Replace(" ", "");



            bool boolEnabledLog = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnabledErrorLog"]);
            if (boolEnabledLog)
            {
                try
                {
                    System.IO.StreamWriter sw = System.IO.File.AppendText(System.Configuration.ConfigurationManager.AppSettings["CarpetaLogs"] + strArchivo);

                    linea = System.DateTime.Now + "; " + "; " + strReserva + "; " + strClase + "; " + strDescripcion;
                    sw.WriteLine(linea);
                    sw.Close();
                }
                catch
                {
                }
            }
        }

    }
}
