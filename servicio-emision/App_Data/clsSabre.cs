using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EmisionVOL.Entidades;
using System.Collections.Generic;
using servicio_emision;
/// <summary>
/// Descripción breve de clsSabre
/// </summary>
/// 

namespace servicio_emision.App_Data
{
    class ClsSabre
    {

        private string username, password, ipcc;
        private string _archivoSesionSabre;
        private Log log;

        public string ArchivoSesionSabre
        {
            get { return _archivoSesionSabre; }
            set { _archivoSesionSabre = value; }
        }
        public ClsSabre(string strUser, string strPassword, string strPCC)
        {
            //cambio a protocolo TLS 1.2 DIOD
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            username = strUser;
            password = strPassword;
            ipcc = strPCC;
        }

        public string Message;


        #region Crea Sesion sabre
        public bool CrearSesion()
        {
            string Resultado = "";
            bool flujoExitoso = true;

            try
            {
                // Set user information
                //string username = ConfigurationSettings.AppSettings.Get("UserSabre");
                //string password = ConfigurationSettings.AppSettings.Get("PasswordSabre");
                //string ipcc = ConfigurationSettings.AppSettings.Get("PccSabre");

                string domain = "DEFAULT";
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";

                wsSabreSessionCreate.MessageHeader msgHeader = new wsSabreSessionCreate.MessageHeader();

                msgHeader.ConversationId = "TestSession";       // Set the ConversationId

                wsSabreSessionCreate.From from = new wsSabreSessionCreate.From();
                wsSabreSessionCreate.PartyId fromPartyId = new wsSabreSessionCreate.PartyId();
                wsSabreSessionCreate.PartyId[] fromPartyIdArr = new wsSabreSessionCreate.PartyId[1];
                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                wsSabreSessionCreate.To to = new wsSabreSessionCreate.To();
                wsSabreSessionCreate.PartyId toPartyId = new wsSabreSessionCreate.PartyId();
                wsSabreSessionCreate.PartyId[] toPartyIdArr = new wsSabreSessionCreate.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = ipcc;
                msgHeader.Action = "SessionCreateRQ";
                wsSabreSessionCreate.Service service = new wsSabreSessionCreate.Service();
                service.Value = "SessionCreate";
                msgHeader.Service = service;

                wsSabreSessionCreate.MessageData msgData = new wsSabreSessionCreate.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;

                wsSabreSessionCreate.Security security = new wsSabreSessionCreate.Security();
                wsSabreSessionCreate.SecurityUsernameToken securityUserToken = new wsSabreSessionCreate.SecurityUsernameToken();
                securityUserToken.Username = username;
                securityUserToken.Password = password;
                securityUserToken.Organization = ipcc;
                securityUserToken.Domain = domain;
                security.UsernameToken = securityUserToken;

                wsSabreSessionCreate.SessionCreateRQ req = new wsSabreSessionCreate.SessionCreateRQ();
                wsSabreSessionCreate.SessionCreateRQPOS pos = new wsSabreSessionCreate.SessionCreateRQPOS();
                wsSabreSessionCreate.SessionCreateRQPOSSource source = new wsSabreSessionCreate.SessionCreateRQPOSSource();
                source.PseudoCityCode = ipcc;
                pos.Source = source;
                req.POS = pos;

                wsSabreSessionCreate.SessionCreateRQService serviceObj = new wsSabreSessionCreate.SessionCreateRQService();
                serviceObj.MessageHeaderValue = msgHeader;
                serviceObj.SecurityValue = security;

                wsSabreSessionCreate.SessionCreateRS resp = serviceObj.SessionCreateRQ(req);   // Send the request

                if (resp.Errors != null && resp.Errors.Error != null)
                {
                    Console.WriteLine("Error : " + resp.Errors.Error.ErrorInfo.Message);
                    flujoExitoso = false;
                }
                else
                {
                    string temp = Environment.GetEnvironmentVariable("tmp");    // Get temp directory
                    bool archivoDisponible = false;
                    string strCarpetaArchivoSesion = temp + "/";
                    string strNombreArchivoSesion = "";
                    string PropsFileName = "";


                    while (!archivoDisponible)
                    {
                        Random random = new Random();
                        int randomNumber1 = random.Next(0, 10000);
                        int randomNumber2 = random.Next(0, 10000);

                        strNombreArchivoSesion = "S" + randomNumber1.ToString() + randomNumber2.ToString() + ".properties";
                        //string strNombreArchivoSesion = "session" + randomNumber.ToString() + ".properties";
                        //string strNombreArchivoSesion = HttpContext.Current.Session.SessionID + ".properties"; ;

                        PropsFileName = strCarpetaArchivoSesion + strNombreArchivoSesion;       // Define dir and file name
                        if (!File.Exists(PropsFileName))
                        {
                            archivoDisponible = true;
                        }
                    }

                    _archivoSesionSabre = PropsFileName;

                    msgHeader = serviceObj.MessageHeaderValue;
                    security = serviceObj.SecurityValue;

                    Resultado = "**********************************************" + "\n";
                    Resultado += "Response of SessionCreateRQ service" + "\n";
                    Resultado += "BinarySecurityToken returned : " + security.BinarySecurityToken + "\n";
                    Resultado += "**********************************************" + "\n";
                    string ConvIdLine = "convid=" + msgHeader.ConversationId;   // ConversationId to a string
                    string TokenLine = "securitytoken=" + security.BinarySecurityToken; // BinarySecurityToken to a string
                    string ipccLine = "ipcc=" + ipcc;   // IPCC to a string

                    //File.Delete(PropsFileName);		// Clean up
                    TextWriter tw = new StreamWriter(PropsFileName);    // Create & open the file
                    tw.WriteLine(DateTime.Now);     // Write the date for reference
                    tw.WriteLine(TokenLine);        // Write the BinarySecurityToken
                    tw.WriteLine(ConvIdLine);       // Write the ConversationId
                    tw.WriteLine(ipccLine);     // Write the IPCC
                    tw.Close();

                    //Console.Read();
                }
            }
            catch (Exception e)
            {
                Resultado = "Exception Message : " + e.Message;
                Resultado += "Exception Stack Trace : " + e.StackTrace;
                Log.grabaLog("clsSabre_CrearSesion", "", "Exception Message : " + e.Message);
                Log.grabaLog("clsSabre_CrearSesion", "", "Exception Message : " + e.StackTrace);
                flujoExitoso = false;
            }

            Message = Resultado;
            return flujoExitoso;
        }
        #endregion

        #region Cierra Sesion Sabre
        public bool CerrarSesion()
        {
            string Resultado = "";
            try
            {
                //string temp = Environment.GetEnvironmentVariable("tmp");	// Get temp directory
                //string PropsFileName = temp + "/session.properties";		// Define dir and file name
                string PropsFileName = _archivoSesionSabre;
                string securitytoken = null;
                string convid = null;
                string ipcc = null;
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";

                try
                {
                    using (StreamReader sr = new StreamReader(PropsFileName)) // Open the file
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Regex r1 = new Regex("securitytoken="); // Setup search
                            Regex r2 = new Regex("convid=");        // Setup search
                            Regex r3 = new Regex("ipcc=");          // Setup search
                            Match m1 = r1.Match(line);              // Looking for securitytoken
                            Match m2 = r2.Match(line);              // Looking for convid
                            Match m3 = r3.Match(line);              // Looking for ipcc
                            if (m1.Success)
                            {
                                securitytoken = line.Substring(m1.Length);  // Put BinarySecurityToken in a string
                            }
                            else if (m2.Success)
                            {
                                convid = line.Substring(m2.Length);         // Put ConversationId in a string
                            }
                            else if (m3.Success)
                            {
                                ipcc = line.Substring(m3.Length);               // Put IPCC in a string
                            }
                        }
                    }
                    //return Resultado;
                }
                catch (Exception e)
                {
                    Resultado = "The file could not be read:" + "\n";
                    Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                    Log.grabaLog("clsSabre_CerrarSesion", "", "Exception Message : " + e.Message);
                    Log.grabaLog("clsSabre_CerrarSesion", "", "Exception Message : " + e.StackTrace);
                    Message = Resultado;
                    return false;
                }


                wsSabreSessionClose.MessageHeader msgHeader = new wsSabreSessionClose.MessageHeader();
                msgHeader.ConversationId = convid;          // Put ConversationId in req header

                wsSabreSessionClose.From from = new wsSabreSessionClose.From();
                wsSabreSessionClose.PartyId fromPartyId = new wsSabreSessionClose.PartyId();
                wsSabreSessionClose.PartyId[] fromPartyIdArr = new wsSabreSessionClose.PartyId[1];
                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                wsSabreSessionClose.To to = new wsSabreSessionClose.To();
                wsSabreSessionClose.PartyId toPartyId = new wsSabreSessionClose.PartyId();
                wsSabreSessionClose.PartyId[] toPartyIdArr = new wsSabreSessionClose.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = ipcc;
                msgHeader.Action = "SessionCloseRQ";
                wsSabreSessionClose.Service service = new wsSabreSessionClose.Service();
                service.Value = "SessionClose";
                msgHeader.Service = service;

                wsSabreSessionClose.MessageData msgData = new wsSabreSessionClose.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;
                wsSabreSessionClose.Security security = new wsSabreSessionClose.Security();
                security.BinarySecurityToken = securitytoken;       // Put BinarySecurityToken in req header


                wsSabreSessionClose.SessionCloseRQ req = new wsSabreSessionClose.SessionCloseRQ();
                wsSabreSessionClose.SessionCloseRQPOS pos = new wsSabreSessionClose.SessionCloseRQPOS();
                wsSabreSessionClose.SessionCloseRQPOSSource source = new wsSabreSessionClose.SessionCloseRQPOSSource();
                source.PseudoCityCode = ipcc;
                pos.Source = source;
                req.POS = pos;

                wsSabreSessionClose.SessionCloseRQService serviceObj = new wsSabreSessionClose.SessionCloseRQService();
                serviceObj.MessageHeaderValue = msgHeader;
                serviceObj.SecurityValue = security;

                wsSabreSessionClose.SessionCloseRS resp = serviceObj.SessionCloseRQ(req);  // Send the request
                Resultado = "******************************************" + "\n";
                Resultado += "Session close Status : " + resp.status + "\n";
                Resultado += "******************************************" + "\n";
                //Console.Read();

                //Eliminamos el archivo de la sesion para no generar mucha basura
                try
                {
                    File.Delete(PropsFileName);
                }
                catch { }
            }
            catch (Exception e)
            {
                Resultado = "Exception Message : " + e.Message + "\n";
                Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                Log.grabaLog("clsSabre_CerrarSesion", "", "Exception Message : " + e.Message);
                Log.grabaLog("clsSabre_CerrarSesion", "", "Exception Message : " + e.StackTrace);
                Message = Resultado;
                return false;
                //Console.Read();
            }
            Message = Resultado;
            return true;
        }
        #endregion

        #region Consulta comando Sabre
        public string ConsultarComando(string Comando)
        {
            string Resultado = "";
            try
            {
                string Pcc = ipcc;
                //string temp = Environment.GetEnvironmentVariable("tmp");	// Get temp directory
                //string PropsFileName = temp + "/session.properties";		// Define dir and file name
                string PropsFileName = _archivoSesionSabre;
                string securitytoken = null;
                string convid = null;
                ipcc = null;
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";

                // This assumes the information returned by the SessionCreateRQ has been
                //   stored in a file named "session.properties", located in the temp
                //   directory defined in the system environment.  This is merely an example
                //   of how this can be done.  Several other methods are available which may
                //   instead be used to accomplish the storing and retrieval of the data.
                try
                {
                    using (StreamReader sr = new StreamReader(PropsFileName)) // Open the file
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Regex r1 = new Regex("securitytoken="); // Setup search
                            Regex r2 = new Regex("convid=");
                            Regex r3 = new Regex("ipcc=");
                            Match m1 = r1.Match(line);              // Looking for securitytoken
                            Match m2 = r2.Match(line);              // Looking for convid
                            Match m3 = r3.Match(line);              // Looking for ipcc
                            if (m1.Success)
                            {
                                securitytoken = line.Substring(m1.Length);  // Get the BinarySecurityToken
                            }
                            else if (m2.Success)
                            {
                                convid = line.Substring(m2.Length);         // Get the ConversationId
                            }
                            else if (m3.Success)
                            {
                                ipcc = line.Substring(m3.Length);               // Get the IPCC
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Resultado = "The file could not be read:" + "\n";
                    //Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                    Log.grabaLog("clsSabre_ConsultarComando", "", "Exception Message : " + e.Message);
                    Log.grabaLog("clsSabre_ConsultarComando", "", "Exception Message : " + e.StackTrace);
                    return Resultado;
                }


                wsSabreCommand.MessageHeader msgHeader = new wsSabreCommand.MessageHeader();
                msgHeader.ConversationId = convid;          // Put ConversationId in req header

                wsSabreCommand.From from = new wsSabreCommand.From();
                wsSabreCommand.PartyId fromPartyId = new wsSabreCommand.PartyId();
                wsSabreCommand.PartyId[] fromPartyIdArr = new wsSabreCommand.PartyId[1];
                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                wsSabreCommand.To to = new wsSabreCommand.To();
                wsSabreCommand.PartyId toPartyId = new wsSabreCommand.PartyId();
                wsSabreCommand.PartyId[] toPartyIdArr = new wsSabreCommand.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = Pcc;
                msgHeader.Action = "SabreCommandLLSRQ";
                wsSabreCommand.Service service = new wsSabreCommand.Service();
                service.Value = "SabreCommandLLSService";
                msgHeader.Service = service;


                wsSabreCommand.MessageData msgData = new wsSabreCommand.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;
                wsSabreCommand.Security security = new wsSabreCommand.Security();
                security.BinarySecurityToken = securitytoken;       // Put BinarySecurityToken in req header

                /***************************************************************************************************/
                /***************************************************************************************************/

                wsSabreCommand.SabreCommandLLSRQRequest Req1 = new wsSabreCommand.SabreCommandLLSRQRequest();
                Req1.CDATA = true;
                Req1.HostCommand = Comando;


                //if (bolXML)
                //{
                //    Req1.Output = wsSabreCommand.SabreCommandLLSRQRequestOutput.DATABAHN;
                //    Req1.CDATA = false;
                //}

                wsSabreCommand.SabreCommandLLSRQ Req2 = new wsSabreCommand.SabreCommandLLSRQ();
                Req2.PrimaryLangID = "en-us";
                Req2.AltLangID = "en-us";
                Req2.EchoToken = securitytoken;
                Req2.Request = Req1;
                Req2.TimeStamp = "2007-12-17T10:40:47-05:00";
                Req2.Version = "2003A.TsabreXML1.5.1";
                Req2.SequenceNmbr = "1";

                wsSabreCommand.SabreCommandLLSService Serv = new wsSabreCommand.SabreCommandLLSService();
                Serv.MessageHeaderValue = msgHeader;
                Serv.SecurityValue = security;

                wsSabreCommand.SabreCommandLLSRS RS = Serv.SabreCommandLLSRQ(Req2);

                //if (Tipo == 1)
                //{
                //    return RS.XML_Content.Any[0].InnerXml;
                //}
                //else
                //{
                //    return RS.Response;
                //}

                return RS.Response;

            }
            catch (Exception e)
            {
                //Resultado += "Exception Message : " + e.Message + "\n";
                //Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                Log.grabaLog("clsSabre_ConsultarComando", "", "Exception Message : " + e.Message);
                Log.grabaLog("clsSabre_ConsultarComando", "", "Exception Message : " + e.StackTrace);
                return Resultado;
            }
        }


        #endregion

        #region ConsultarPNR

        #region OLD
        public wsSabreReadPNR.TravelItineraryReadRS ConsultarPNR(string strPNR)
        {
            string Resultado = "";
            wsSabreReadPNR.TravelItineraryReadRS resp = new wsSabreReadPNR.TravelItineraryReadRS();
            try
            {
                //string temp = Environment.GetEnvironmentVariable("tmp");	// Get temp directory
                //string PropsFileName = temp + "/session.properties";		// Define dir and file name
                string PropsFileName = _archivoSesionSabre;
                string securitytoken = null;
                string convid = null;
                string ipcc = null;
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";

                try
                {
                    using (StreamReader sr = new StreamReader(PropsFileName)) // Open the file
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Regex r1 = new Regex("securitytoken="); // Setup search
                            Regex r2 = new Regex("convid=");        // Setup search
                            Regex r3 = new Regex("ipcc=");          // Setup search
                            Match m1 = r1.Match(line);              // Looking for securitytoken
                            Match m2 = r2.Match(line);              // Looking for convid
                            Match m3 = r3.Match(line);              // Looking for ipcc
                            if (m1.Success)
                            {
                                securitytoken = line.Substring(m1.Length);  // Put BinarySecurityToken in a string
                            }
                            else if (m2.Success)
                            {
                                convid = line.Substring(m2.Length);         // Put ConversationId in a string
                            }
                            else if (m3.Success)
                            {
                                ipcc = line.Substring(m3.Length);               // Put IPCC in a string
                            }
                        }
                    }
                    //return Resultado;
                }
                catch (Exception e)
                {
                    Resultado = "The file could not be read:" + "\n";
                    Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                    Log.grabaLog("clsSabre_ConsultarPNR", "", "Exception Message : " + e.Message);
                    Log.grabaLog("clsSabre_ConsultarPNR", "", "Exception Message : " + e.StackTrace);
                    Message = Resultado;
                    return resp;
                }



                wsSabreReadPNR.MessageHeader msgHeader = new wsSabreReadPNR.MessageHeader();
                msgHeader.ConversationId = convid;          // Put ConversationId in req header

                wsSabreReadPNR.From from = new wsSabreReadPNR.From();
                wsSabreReadPNR.PartyId fromPartyId = new wsSabreReadPNR.PartyId();
                wsSabreReadPNR.PartyId[] fromPartyIdArr = new wsSabreReadPNR.PartyId[1];
                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                wsSabreReadPNR.To to = new wsSabreReadPNR.To();
                wsSabreReadPNR.PartyId toPartyId = new wsSabreReadPNR.PartyId();
                wsSabreReadPNR.PartyId[] toPartyIdArr = new wsSabreReadPNR.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = ipcc;
                msgHeader.Action = "TravelItineraryReadLLSRQ";
                wsSabreReadPNR.Service service = new wsSabreReadPNR.Service();
                service.Value = "TravelItineraryReadService";
                msgHeader.Service = service;

                wsSabreReadPNR.MessageData msgData = new wsSabreReadPNR.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;
                wsSabreReadPNR.Security security = new wsSabreReadPNR.Security();
                security.BinarySecurityToken = securitytoken;       // Put BinarySecurityToken in req header


                wsSabreReadPNR.TravelItineraryReadRQ req = new wsSabreReadPNR.TravelItineraryReadRQ();
                wsSabreReadPNR.TravelItineraryReadRQPOS pos = new wsSabreReadPNR.TravelItineraryReadRQPOS();
                wsSabreReadPNR.TravelItineraryReadRQPOSSource source = new wsSabreReadPNR.TravelItineraryReadRQPOSSource();

                wsSabreReadPNR.TravelItineraryReadRQUniqueID uid = new wsSabreReadPNR.TravelItineraryReadRQUniqueID();
                wsSabreReadPNR.TravelItineraryReadRQMessagingDetails det = new wsSabreReadPNR.TravelItineraryReadRQMessagingDetails();
                wsSabreReadPNR.TravelItineraryReadRQMessagingDetailsTransaction trans = new wsSabreReadPNR.TravelItineraryReadRQMessagingDetailsTransaction();

                source.PseudoCityCode = ipcc;
                pos.Source = source;
                req.POS = pos;
                req.Version = "1.0.1";
                req.TimeStamp = "2001-12-17T09:30:47-05:00";

                uid.ID = strPNR;

                trans.Code = "PNR";
                //det.ApplicationID = "DEF456";

                det.Transaction = new wsSabreReadPNR.TravelItineraryReadRQMessagingDetailsTransaction[1];

                req.UniqueID = uid;
                det.Transaction[0] = trans;
                req.MessagingDetails = det;

                wsSabreReadPNR.TravelItineraryReadService serviceObj = new wsSabreReadPNR.TravelItineraryReadService();
                serviceObj.MessageHeaderValue = msgHeader;
                serviceObj.SecurityValue = security;

                resp = serviceObj.TravelItineraryReadRQ(req);   // Send the request
                                                                //Resultado = "******************************************" + "\n";
                                                                //Resultado += "Session close Status : " + resp.status + "\n";
                                                                //Resultado += "******************************************" + "\n";
                                                                //Console.Read();
            }
            catch (Exception e)
            {
                Resultado = "Exception Message : " + e.Message + "\n";
                Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                Log.grabaLog("clsSabre_ConsultarPNR", "", "Exception Message : " + e.Message);
                Log.grabaLog("clsSabre_ConsultarPNR", "", "Exception Message : " + e.StackTrace);
                Message = Resultado;
                return resp;
                //Console.Read();
            }
            Message = Resultado;
            return resp;
        }
        #endregion
        #region New
        public wsSabreReadPNR_RQ.TravelItineraryReadRS ConsultarPNR_New(string strPNR)
        {
            string Resultado = "";
            wsSabreReadPNR_RQ.TravelItineraryReadRS resp = new wsSabreReadPNR_RQ.TravelItineraryReadRS();
            try
            {
                //string temp = Environment.GetEnvironmentVariable("tmp");	// Get temp directory
                //string PropsFileName = temp + "/session.properties";		// Define dir and file name
                string PropsFileName = _archivoSesionSabre;
                string securitytoken = null;
                string convid = null;
                string ipcc = null;
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";

                try
                {
                    using (StreamReader sr = new StreamReader(PropsFileName)) // Open the file
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Regex r1 = new Regex("securitytoken=");	// Setup search
                            Regex r2 = new Regex("convid=");		// Setup search
                            Regex r3 = new Regex("ipcc=");			// Setup search
                            Match m1 = r1.Match(line);				// Looking for securitytoken
                            Match m2 = r2.Match(line);				// Looking for convid
                            Match m3 = r3.Match(line);				// Looking for ipcc
                            if (m1.Success)
                            {
                                securitytoken = line.Substring(m1.Length);	// Put BinarySecurityToken in a string
                            }
                            else if (m2.Success)
                            {
                                convid = line.Substring(m2.Length);			// Put ConversationId in a string
                            }
                            else if (m3.Success)
                            {
                                ipcc = line.Substring(m3.Length);				// Put IPCC in a string
                            }
                        }
                    }
                    //return Resultado;
                }
                catch (Exception e)
                {
                    Resultado = "The file could not be read:" + "\n";
                    Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                    Message = Resultado;
                    return resp;
                }


                wsSabreReadPNR_RQ.MessageHeader msgHeader = new wsSabreReadPNR_RQ.MessageHeader();
                msgHeader.ConversationId = convid;			// Put ConversationId in req header

                wsSabreReadPNR_RQ.From from = new wsSabreReadPNR_RQ.From();
                wsSabreReadPNR_RQ.PartyId fromPartyId = new wsSabreReadPNR_RQ.PartyId();
                wsSabreReadPNR_RQ.PartyId[] fromPartyIdArr = new wsSabreReadPNR_RQ.PartyId[1];

                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                wsSabreReadPNR_RQ.To to = new wsSabreReadPNR_RQ.To();
                wsSabreReadPNR_RQ.PartyId toPartyId = new wsSabreReadPNR_RQ.PartyId();
                wsSabreReadPNR_RQ.PartyId[] toPartyIdArr = new wsSabreReadPNR_RQ.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = ipcc;
                msgHeader.Action = "TravelItineraryReadRQ";

                //msgHeader.Action = "TravelItineraryReadLLSRQ";
                wsSabreReadPNR_RQ.Service service = new wsSabreReadPNR_RQ.Service();
                service.Value = "TravelItineraryReadService";
                msgHeader.Service = service;

                wsSabreReadPNR_RQ.MessageData msgData = new wsSabreReadPNR_RQ.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;


                wsSabreReadPNR_RQ.Security1 security = new wsSabreReadPNR_RQ.Security1();
                security.BinarySecurityToken = securitytoken;		// Put BinarySecurityToken in req header


                wsSabreReadPNR_RQ.TravelItineraryReadRQ req = new wsSabreReadPNR_RQ.TravelItineraryReadRQ();

                // wsSabreReadPNR_RQ.TravelItineraryReadRQPOS pos = new wsSabreReadPNR_RQ.TravelItineraryReadRQPOS();
                // wsSabreReadPNR_RQ.TravelItineraryReadRQPOSSource source = new wsSabreReadPNR_RQ.TravelItineraryReadRQPOSSource();

                wsSabreReadPNR_RQ.TravelItineraryReadRQUniqueID uid = new wsSabreReadPNR_RQ.TravelItineraryReadRQUniqueID();
                wsSabreReadPNR_RQ.TravelItineraryReadRQMessagingDetails det = new wsSabreReadPNR_RQ.TravelItineraryReadRQMessagingDetails();
                // wsSabreReadPNR_RQ.TravelItineraryReadRQMessagingDetailsTransaction trans = new wsSabreReadPNR_RQ.TravelItineraryReadRQMessagingDetailsTransaction();

                //source.PseudoCityCode = ipcc;
                //pos.Source = source;
                //req.POS = pos;
                req.Version = "3.6.0";
                //req.Version = "1.0.1";
                req.TimeStamp = Convert.ToDateTime("2001-12-17T09:30:47-05:00");

                uid.ID = strPNR;

                //trans.Code = "PNR";
                //det.ApplicationID = "DEF456";

                //det. = new wsSabreReadPNR_RQ.TravelItineraryReadRQMessagingDetailsTransaction[1];

                req.UniqueID = uid;
                //det.Transaction[0] = trans;
                req.MessagingDetails = det;

                wsSabreReadPNR_RQ.TravelItineraryReadService read = new wsSabreReadPNR_RQ.TravelItineraryReadService();


                read.MessageHeaderValue = msgHeader;
                read.Security = security;


                //    wsSabreReadPNR_RQ.resp2 = new wsSabreReadPNR_RQ.TravelItineraryReadRS();

                resp = read.TravelItineraryReadRQ(req);	// Send the request
                //Resultado = "******************************************" + "\n";
                //Resultado += "Session close Status : " + resp.status + "\n";
                //Resultado += "******************************************" + "\n";
                //Console.Read();
            }
            catch (Exception e)
            {
                Resultado = "Exception Message : " + e.Message + "\n";
                Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                Message = Resultado;
                return resp;
                //Console.Read();
            }
            Message = Resultado;
            return resp;
        }
        #endregion
        #endregion

        #region ConsultarPQ

#pragma warning disable CS0246 // El nombre del tipo o del espacio de nombres 'ItinerarioPQ' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
        public ItinerarioPQ GedataPQ(string strPNR)
#pragma warning restore CS0246 // El nombre del tipo o del espacio de nombres 'ItinerarioPQ' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
        {
            ItinerarioPQ colspq = new ItinerarioPQ();
            string Resultado = "";
            GetReservationOperation.GetReservationRS resp = new GetReservationOperation.GetReservationRS();
            GetReservationOperation.GetReservationRQ req = new GetReservationOperation.GetReservationRQ();
            try
            {
                //string temp = Environment.GetEnvironmentVariable("tmp");	// Get temp directory
                //string PropsFileName = temp + "/session.properties";		// Define dir and file name
                string PropsFileName = _archivoSesionSabre;
                string securitytoken = null;
                string convid = null;
                string ipcc = null;
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";
                try
                {
                    using (StreamReader sr = new StreamReader(PropsFileName)) // Open the file
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Regex r1 = new Regex("securitytoken="); // Setup search
                            Regex r2 = new Regex("convid=");        // Setup search
                            Regex r3 = new Regex("ipcc=");          // Setup search
                            Match m1 = r1.Match(line);              // Looking for securitytoken
                            Match m2 = r2.Match(line);              // Looking for convid
                            Match m3 = r3.Match(line);              // Looking for ipcc
                            if (m1.Success)
                            {
                                securitytoken = line.Substring(m1.Length);  // Put BinarySecurityToken in a string
                            }
                            else if (m2.Success)
                            {
                                convid = line.Substring(m2.Length);         // Put ConversationId in a string
                            }
                            else if (m3.Success)
                            {
                                ipcc = line.Substring(m3.Length);               // Put IPCC in a string
                            }
                        }
                    }
                    //return Resultado;
                }
                catch (Exception e)
                {
                    Resultado = "The file could not be read:" + "\n";
                    Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                    Message = Resultado;
                    return colspq;
                }

                GetReservationOperation.MessageHeader msgHeader = new GetReservationOperation.MessageHeader();
                msgHeader.ConversationId = convid;          // Put ConversationId in req header

                GetReservationOperation.From from = new GetReservationOperation.From();
                GetReservationOperation.PartyId fromPartyId = new GetReservationOperation.PartyId();
                GetReservationOperation.PartyId[] fromPartyIdArr = new GetReservationOperation.PartyId[1];

                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                GetReservationOperation.To to = new GetReservationOperation.To();
                GetReservationOperation.PartyId toPartyId = new GetReservationOperation.PartyId();
                GetReservationOperation.PartyId[] toPartyIdArr = new GetReservationOperation.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = ipcc;
                msgHeader.Action = "getReservationRQ";

                GetReservationOperation.Service service = new GetReservationOperation.Service();
                service.Value = "getReservationRQ";
                msgHeader.Service = service;

                GetReservationOperation.MessageData msgData = new GetReservationOperation.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;

                GetReservationOperation.Security security = new GetReservationOperation.Security();
                security.BinarySecurityToken = securitytoken;       // Put BinarySecurityToken in req header



                req.Version = "1.18.0";
                req.Locator = strPNR;
                req.RequestType = "Stateful";

                GetReservationOperation.ReturnOptionsPNRB opciones = new GetReservationOperation.ReturnOptionsPNRB();
                opciones.ViewName = "Simple";
                opciones.ResponseFormat = "STL";

                opciones.SubjectAreas = new string[]{
            "PRICE_QUOTE"
            };

                req.ReturnOptions = opciones;

                GetReservationOperation.GetReservationService read = new GetReservationOperation.GetReservationService();
                read.MessageHeaderValue = msgHeader;
                read.SecurityValue = security;
                resp = read.GetReservationOperation(req);   // Send the request    

                string json = JsonConvert.SerializeObject(resp);

                var ObjectPricing = JObject.Parse(json);


                var PQsReserva = JObject.Parse(ObjectPricing.SelectToken("PriceQuote.Item.PriceQuoteInfo.Summary").ToString());
                List<PQ> pqs = new List<PQ>();
                PQ _pq = new PQ();
                colspq.colPQItinerarios = new System.Collections.Generic.List<PQ>();

                if (PQsReserva.SelectToken("NameAssociation") is JArray)
                {
                    for (int cantpqs = 0; cantpqs < JArray.Parse(PQsReserva.SelectToken("NameAssociation").ToString()).Count; cantpqs++)
                    {
                        _pq = new PQ();
                        if (PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote") is JArray)
                        {
                            _pq.Aerolinea = PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote[" + cantpqs + "].ValidatingCarrier").ToString();
                        }
                        else if (PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote") is JObject)
                        {
                            _pq.Aerolinea = PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote.ValidatingCarrier").ToString();
                        }
                       
                        if (PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote") is JArray)
                        {
                            _pq.tipoPasajero = PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote[" + cantpqs + "].Passenger.@type").ToString();
                        }
                        else if (PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote") is JObject)
                        {
                            _pq.tipoPasajero = PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote.Passenger.@type").ToString();
                        }
                        if (PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote") is JArray)
                        {
                            _pq.TotalTarifa = PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote[" + cantpqs + "].Amounts.Total.#text").ToString();
                        }
                        else if (PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote") is JObject)
                        {
                            _pq.TotalTarifa = PQsReserva.SelectToken("NameAssociation[" + cantpqs + "].PriceQuote.Amounts.Total.#text").ToString();
                        }
                        pqs.Add(_pq);
                    }
                    colspq.colPQItinerarios = pqs;
                }
                else
                {
                    colspq.colPQItinerarios = GetColPQ(PQsReserva.SelectToken("NameAssociation").ToString(), ObjectPricing);
                }
            }
            catch (Exception e)
            {
                Resultado = "Exception Message : " + e.Message + "\n";
                Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                Message = Resultado;
                return colspq;
                //Console.Read();
            }
            Message = Resultado;
            return colspq;
        }
      private List<PQ> GetColPQ(string jsonPqs, JObject ObjectPricing)
      {
            List<PQ> pqs = new List<PQ>();
            PQ _pq = new PQ();
            var ObjectPQ = JObject.Parse(jsonPqs);
            _pq.Aerolinea = ObjectPQ.SelectToken("PriceQuote.ValidatingCarrier").ToString();

            if (ObjectPricing.SelectToken("PriceQuote.Item.PriceQuoteInfo.Details.FareInfo.Commission.Percentage") != null)
            {
                _pq.PorComicion = ObjectPricing.SelectToken("PriceQuote.Item.PriceQuoteInfo.Details.FareInfo.Commission.Percentage").ToString();
            }
            _pq.tipoPasajero = ObjectPQ.SelectToken("PriceQuote.Passenger.@type").ToString();
            _pq.TotalTarifa = ObjectPQ.SelectToken("PriceQuote.Amounts.Total.#text").ToString();
            pqs.Add(_pq);
            return pqs;
        }

        #endregion

        #region CC
        public string CC(string strPNR)
        {
            string Resultado = "";
            try
            {
                string Pcc = ipcc;
                //string temp = Environment.GetEnvironmentVariable("tmp");	// Get temp directory
                //string PropsFileName = temp + "/session.properties";		// Define dir and file name
                string PropsFileName = _archivoSesionSabre;
                string securitytoken = null;
                string convid = null;
                ipcc = null;
                DateTime dt = DateTime.UtcNow;
                string tstamp = dt.ToString("s") + "Z";

                // This assumes the information returned by the SessionCreateRQ has been
                //   stored in a file named "session.properties", located in the temp
                //   directory defined in the system environment.  This is merely an example
                //   of how this can be done.  Several other methods are available which may
                //   instead be used to accomplish the storing and retrieval of the data.
                try
                {
                    using (StreamReader sr = new StreamReader(PropsFileName)) // Open the file
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Regex r1 = new Regex("securitytoken="); // Setup search
                            Regex r2 = new Regex("convid=");
                            Regex r3 = new Regex("ipcc=");
                            Match m1 = r1.Match(line);              // Looking for securitytoken
                            Match m2 = r2.Match(line);              // Looking for convid
                            Match m3 = r3.Match(line);              // Looking for ipcc
                            if (m1.Success)
                            {
                                securitytoken = line.Substring(m1.Length);  // Get the BinarySecurityToken
                            }
                            else if (m2.Success)
                            {
                                convid = line.Substring(m2.Length);         // Get the ConversationId
                            }
                            else if (m3.Success)
                            {
                                ipcc = line.Substring(m3.Length);               // Get the IPCC
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Resultado = "The file could not be read:" + "\n";
                    //Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                    Log.grabaLog("clsSabre_CC", "", "Exception Message : " + e.Message);
                    Log.grabaLog("clsSabre_CC", "", "Exception Message : " + e.StackTrace);
                    return Resultado;
                }


                wsSabreReadPNR.MessageHeader msgHeader = new wsSabreReadPNR.MessageHeader();
                msgHeader.ConversationId = convid;          // Put ConversationId in req header

                wsSabreReadPNR.From from = new wsSabreReadPNR.From();
                wsSabreReadPNR.PartyId fromPartyId = new wsSabreReadPNR.PartyId();
                wsSabreReadPNR.PartyId[] fromPartyIdArr = new wsSabreReadPNR.PartyId[1];
                fromPartyId.Value = "99999";
                fromPartyIdArr[0] = fromPartyId;
                from.PartyId = fromPartyIdArr;
                msgHeader.From = from;

                wsSabreReadPNR.To to = new wsSabreReadPNR.To();
                wsSabreReadPNR.PartyId toPartyId = new wsSabreReadPNR.PartyId();
                wsSabreReadPNR.PartyId[] toPartyIdArr = new wsSabreReadPNR.PartyId[1];
                toPartyId.Value = "123123";
                toPartyIdArr[0] = toPartyId;
                to.PartyId = toPartyIdArr;
                msgHeader.To = to;

                msgHeader.CPAId = Pcc;
                msgHeader.Action = "TravelItineraryReadRQ";
                wsSabreReadPNR.Service service = new wsSabreReadPNR.Service();
                service.Value = "TravelItineraryReadService";
                msgHeader.Service = service;


                wsSabreReadPNR.MessageData msgData = new wsSabreReadPNR.MessageData();
                msgData.MessageId = "mid:20001209-133003-2333@clientofsabre.com1";
                msgData.Timestamp = tstamp;
                msgHeader.MessageData = msgData;
                wsSabreReadPNR.Security security = new wsSabreReadPNR.Security();
                security.BinarySecurityToken = securitytoken;       // Put BinarySecurityToken in req header


                /***************************************************************************************************/
                /***************************************************************************************************/




                wsSabreReadPNR.TravelItineraryReadRQ Req2 = new wsSabreReadPNR.TravelItineraryReadRQ();
                wsSabreReadPNR.TravelItineraryReadRQPOS pos = new wsSabreReadPNR.TravelItineraryReadRQPOS();
                wsSabreReadPNR.TravelItineraryReadRQPOSSource source = new wsSabreReadPNR.TravelItineraryReadRQPOSSource();
                wsSabreReadPNR.TravelItineraryReadRQUniqueID UniqueId = new wsSabreReadPNR.TravelItineraryReadRQUniqueID();

                source.PseudoCityCode = Pcc;
                pos.Source = source;
                Req2.POS = pos;
                UniqueId.ID = strPNR;

                Req2.UniqueID = UniqueId;


                wsSabreReadPNR.TravelItineraryReadService Serv = new wsSabreReadPNR.TravelItineraryReadService();
                Serv.MessageHeaderValue = msgHeader;
                Serv.SecurityValue = security;



                wsSabreReadPNR.TravelItineraryReadRS RS = Serv.TravelItineraryReadRQ(Req2);

                return "";

            }
            catch (Exception e)
            {
                //Resultado += "Exception Message : " + e.Message + "\n";
                //Resultado += "Exception Stack Trace : " + e.StackTrace + "\n";
                Log.grabaLog("clsSabre_CC", "", "Exception Message : " + e.Message);
                Log.grabaLog("clsSabre_CC", "", "Exception Message : " + e.StackTrace);
                return Resultado;
            }
        }
    }
    #endregion
}

