using EmisionVOL.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using servicio_emision.App_Data;

namespace servicio_emision
{
    /// <summary>
    /// Descripción breve de servicio_emision
    /// </summary>
    [WebService(Namespace = "http://travel.cl/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class clsWSEmisionFacialSabre : System.Web.Services.WebService
    {
        [WebMethod]
        [SoapHeader("CustomSoapHeader")]
        public RespuestaDTO AnulaReserva(string strReserva)
        {
            bool bolEmitidoCorrectamente = false;
            //LVQ 20160404 se agrega detalle a la respuesta
            RespuestaDTO TheRespuesta = new RespuestaDTO();

            //Verificamos que el usuario este validado
            ServiceAuthHeaderValidation.Validate(CustomSoapHeader);

            Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Ingresando");

            if (String.IsNullOrEmpty(strReserva))
            {
                Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "No hay reserva.");
                TheRespuesta.Estado = "Error";
                TheRespuesta.Mensaje = "Error interno";
                
                return TheRespuesta;
            }

            string strRespuesta;

            string strUsuario;
            string strClave;
            string strPCC;
            string strTAEmision;

            //Asignamos credenciales Sabre
            strUsuario = System.Configuration.ConfigurationManager.AppSettings["UsuarioSabreVOL"];
            strClave = System.Configuration.ConfigurationManager.AppSettings["ClaveSabreVOL"];
            strPCC = System.Configuration.ConfigurationManager.AppSettings["PCCSabreVOL"];
            strTAEmision = System.Configuration.ConfigurationManager.AppSettings["TAEmisionVOL"];

            ClsSabre csSabre = new ClsSabre(strUsuario, strClave, strPCC);
            //wsSabreReadPNR.TravelItineraryReadRS RS = new wsSabreReadPNR.TravelItineraryReadRS();

            wsSabreReadPNR_RQ.TravelItineraryReadRS RS = new wsSabreReadPNR_RQ.TravelItineraryReadRS();
            bool bolContinuar = true;

            if (csSabre.CrearSesion())
            {
                try
                {
                    //Bitacora, Inicio de sesion correcta
                    Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Sesion creada");


                    RS = csSabre.ConsultarPNR_New(strReserva);
                    Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Obteniendo Reserva");


                    for (int i = 0; i < RS.TravelItinerary.ItineraryInfo.ReservationItems.Length; i++)
                    {
                        if (RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment != null)
                        {
                            if (RS.TravelItinerary.ItineraryInfo.Ticketing[0].TicketTimeLimit.Substring(0, 2) == "T-")
                            {
                                Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Reserva tiene items emitidos");
                                bolContinuar = false;
                                break;
                            }
                        }
                    }
                    if (bolContinuar)
                    {
                        //Para prevenir duplicidad de cambios
                        strRespuesta = csSabre.ConsultarComando("IR");
                        Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Respuesta IR: " + strRespuesta);

                        strRespuesta = csSabre.ConsultarComando("XI");
                        Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Respuesta XI: " + strRespuesta);

                        strRespuesta = csSabre.ConsultarComando("6VOLTC");
                        Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Respuesta 6: " + strRespuesta);

                        //Para prevenir duplicidad de cambios
                        strRespuesta = csSabre.ConsultarComando("E");
                        Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Respuesta E: " + strRespuesta);

                        if (strRespuesta.Substring(0, 2) == "OK")
                        {
                            bolEmitidoCorrectamente = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Error: " + ex.Message);
                }
                finally
                {
                    //Cerramos la sesion
                    Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Cerrando sesion");
                    csSabre.CerrarSesion();
                    Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Sesion cerrada");
                }
            }
        
            if (bolEmitidoCorrectamente)
            {
                Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Se entrega mensaje de anulacion OK");
                TheRespuesta.Estado = "Anulada";
                TheRespuesta.Mensaje = "Anulación OK";
            }
            else
            {
                Log.grabaLog("wsSabre_bolAnulaReserva", strReserva, "Se entrega mensaje de error de anulacion");
                TheRespuesta.Estado = "Error";
                TheRespuesta.Mensaje = "Error interno";
            }

            //return bolEmitidoCorrectamente;
            return TheRespuesta;
            //LVQ..
        }

        [WebMethod]
        [SoapHeader("CustomSoapHeader")]
        public RespuestaDTO EmiteReserva(string strReserva, string strTipoReserva, string strDestinoComision)
        {
            bool bolEmitidoCorrectamente = false;
            string responseTicketIssue = "";
            bool NoMsgexisteError = true;
            //Verificamos que el usuario este validado
            ServiceAuthHeaderValidation.Validate(CustomSoapHeader);
            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "////////////////////////////////////////////////////////////////////////////////");
            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "//////////////////               COMIENZA EMISION                ///////////////");
            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "////////////////////////////////////////////////////////////////////////////////");
            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Ingresando");
            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, string.Format("Datos de entrada.  strReserva:{0}, strTipoReserva:{1}, strDestinoComision:{2}", strReserva, strTipoReserva, strDestinoComision));

            try
            {
                string strUsuario;
                string strClave;
                string strPCC;
                string strTAEmision;

                strUsuario = System.Configuration.ConfigurationManager.AppSettings["UsuarioSabreVOL"];
                strClave = System.Configuration.ConfigurationManager.AppSettings["ClaveSabreVOL"];
                strPCC = System.Configuration.ConfigurationManager.AppSettings["PCCSabreVOL"];
                strTAEmision = System.Configuration.ConfigurationManager.AppSettings["TAEmisionVOL"];

                ClsSabre csSabre = new ClsSabre(strUsuario, strClave, strPCC);
                clsWSSabre csWSSabre = new clsWSSabre();



                wsSabreReadPNR_RQ.TravelItineraryReadRS RS = new wsSabreReadPNR_RQ.TravelItineraryReadRS();
                wsSabreReadPNR_RQ.TravelItineraryReadRS RSRespuesta = new wsSabreReadPNR_RQ.TravelItineraryReadRS();

                decimal decComisionBase;
                decimal decOverAgencia;
                decimal decComisionEjecutiva;
                decimal decComisionFinal;
                string strLineaAerea = "";
                string strLineaAereaOriginal = "";
                string strClase = "";
                string strOrigen = "";
                //string strDestino = "";
                string strTipoPax = "";
                DateTime dteFechaInicio = Convert.ToDateTime(null);
                DateTime dteFechaTermino = Convert.ToDateTime(null);
                string strLineaAereaSegmento;
                string strRespuesta;
                bool bolTieneKP = false;
                bool bolContinuar = true;

                DateTime datSalidaSegAnt = DateTime.Now;
                DateTime datLlegadaSegAnt = DateTime.Now;
                TimeSpan ts;

                StringBuilder stbComandoEmision = new StringBuilder();


                if (csSabre.CrearSesion())
                {
                    try
                    {


                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Sesion creada");

                        //Bitacora, Inicio de sesion correctastrDestinoComision
                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Sesion creada");

                        //Se cambia de metodo al migrado.
                        //DortizD 
                        //10-06-2018
                        RS = csSabre.ConsultarPNR_New(strReserva);
                        ItinerarioPQ DataPQ = new ItinerarioPQ();

                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Obteniendo Reserva");

                        //Para prevenir duplicidad de cambios
                        strRespuesta = csSabre.ConsultarComando("IR");

                        if (strRespuesta.IndexOf("WARNING - PNR MODIFICATION IN PROGRESS") > -1)
                        {
                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva con modificación en progreso");
                            bolContinuar = false;
                        }


                        if (RS.TravelItinerary == null)
                        {
                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Itinerario no encontrado. PNR inválido.");
                            bolContinuar = false;
                        }


                        //Revisamos si tiene items la reserva, si es asi, entonces guardamos linea aerea, clase, origen
                        //del primer tramo
                        if (bolContinuar)
                        {
                            //Si el itineraria es menor igual que cero esto quiere decir que no hay itinerario
                            //DortizD 
                            //10-06-2018
                            if (RS.TravelItinerary.ItineraryInfo.ReservationItems.Length <= 0)
                            {
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Itinerario en null, existe la reserva?");
                                bolContinuar = false;
                            }
                            else
                            {//Se obtiene data de pq
                             //DortizD 
                             //10-06-2018
                                DataPQ.colPQItinerarios = new List<PQ>();
                                DataPQ = csSabre.GedataPQ(strReserva);
                            }

                            if (bolContinuar)
                            {
                                //Cambio Obtencion PriceQuote Data
                                //DortizD 
                                //10-06-2018
                                if (RS.TravelItinerary.ItineraryInfo.ReservationItems.Length > 0)
                                {
                                    //DortizD 
                                    //10-06-2018
                                    if (DataPQ != null)
                                    {   //DortizD 
                                        //10-06-2018
                                        if (DataPQ.colPQItinerarios.Count > 0)
                                        {   //DortizD 
                                            //10-06-2018
                                            if (DataPQ.colPQItinerarios[0].TotalTarifa != null)
                                            {
                                                //DortizD 
                                                //10-06-2018
                                                if (Convert.ToDouble(DataPQ.colPQItinerarios[0].TotalTarifa) > 0)
                                                {
                                                    strLineaAerea = DataPQ.colPQItinerarios[0].Aerolinea;
                                                    strLineaAereaOriginal = strLineaAerea;
                                                }
                                                else
                                                {
                                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "No se puede obtener linea aerea del PQ (B)");
                                                }
                                            }
                                            else
                                            {
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "No se puede obtener linea aerea del PQ (C)");
                                                bolContinuar = false;
                                            }
                                        }
                                        else
                                        {
                                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "No se puede obtener linea aerea del PQ (A)");
                                            bolContinuar = false;
                                        }
                                    }
                                    else
                                    {
                                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "PQ en nulo");
                                        bolContinuar = false;
                                    }

                                    if (bolContinuar)
                                    {                                                                     
                                        //REVISAMOS SI LA LINEA AEREA PERMITE EMISION AUTOMATICA
                                        if (!csWSSabre.bolLineaAereaEmiteAutomaticamente(strLineaAerea))
                                        {
                                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Linea aerea no permite emision automatica [" + strLineaAerea + "]");
                                            bolContinuar = false;
                                        }
                                    }

                                    if (bolContinuar)
                                    {
                                        for (int i = 0; i < RS.TravelItinerary.ItineraryInfo.ReservationItems.Length; i++)
                                        {
                                            if (RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment != null)
                                            {
                                                if (RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment[0].Status.ToString() != "HK")
                                                {
                                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva tiene items distinto de HK");
                                                    bolContinuar = false;
                                                    break;
                                                }

                                                if (RS.TravelItinerary.ItineraryInfo.Ticketing[0].TicketTimeLimit.Substring(0, 2) == "T-")
                                                {
                                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva tiene items emitidos");
                                                    bolContinuar = false;
                                                    break;
                                                }
                                            }
                                        }


                                        if (bolContinuar)
                                        {


                                            //obtenemos la fecha de termino
                                            for (int i = 0; i < RS.TravelItinerary.ItineraryInfo.ReservationItems.Length; i++)
                                            {
                                                if (RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment != null)
                                                {
                                                    DateTime datFechaInicio;
                                                    DateTime datFechaTermino;
                                                    datFechaInicio = Convert.ToDateTime(RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment[0].DepartureDateTime.ToString());
                                                    datFechaTermino = Convert.ToDateTime(datFechaInicio.Year.ToString() + "-" + RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment[0].ArrivalDateTime.ToString());
                                                    ts = datFechaTermino - datFechaInicio;
                                                    if (ts.TotalHours < 0)
                                                    {
                                                        datFechaTermino = Convert.ToDateTime((datFechaInicio.Year + 1).ToString() + "-" + RS.TravelItinerary.ItineraryInfo.ReservationItems[i].FlightSegment[0].ArrivalDateTime.ToString());
                                                    }

                                                    dteFechaTermino = datFechaTermino;
                                                }
                                            }
                                        }
                                    }

                                }
                                else //Si no tiene items de reserva entonces no emite
                                {
                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva no tiene items de reserva");
                                    bolContinuar = false;
                                }
                            }
                        }

                        //REALIZAMOS VALIDACIONES PARA LAS "EXCEPCIONES"
                        //Excepción 1, si es GOL y tiene child, entonces no emite
                        //Amarillo: Se comenta debido a que GOL es participante de Sabre y ya no tiene restricciones
                        //if (bolContinuar)
                        //{
                        //    if (strLineaAerea == "G3")
                        //    {
                        //        for (int i = 0; i < RS.TravelItinerary.CustomerInfo.PersonName.Length; i++)
                        //        {
                        //            strTipoPax = RS.TravelItinerary.CustomerInfo.PersonName[i].PassengerType;
                        //            if (strTipoPax == "C01" || strTipoPax == "C02" || strTipoPax == "C03"
                        //                 || strTipoPax == "C04" || strTipoPax == "C05" || strTipoPax == "C06"
                        //                 || strTipoPax == "C07" || strTipoPax == "C08" || strTipoPax == "C09"
                        //                 || strTipoPax == "C10" || strTipoPax == "C11" || strTipoPax == "C12")
                        //            {
                        //                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva linea aérea GOL con Pax tipo child, excepción 1.");
                        //                bolContinuar = false;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}

                        //Excepción 2, si existe un infante y NO pertenece a una linea aérea que permite emitir infantes, 
                        //entonces no la emite

                        if (bolContinuar)
                        {
                            for (int i = 0; i < RS.TravelItinerary.CustomerInfo.PersonName.Length; i++)
                            {
                                strTipoPax = RS.TravelItinerary.CustomerInfo.PersonName[i].PassengerType;
                                if (strTipoPax == "INF")
                                {
                                    //Validamos si la linea aerea no permite emitir infantes
                                    //if (!csWSSabre.bolLineaAereaEmisionInf(strLineaAerea))
                                    //{
                                    //    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva tiene pasajero infante y linea aerea no permite emisión aut., excepción 2.");
                                    //    bolContinuar = false;
                                    //    break;
                                    //}
                                }
                            }

                        }

                        //Excepción 3, Emisión Remota

                        //Excepción 4, tarifas que no originan en SCL
                        //Amarillo: Se comenta debido a que GOL es participante de Sabre y ya no tiene restricciones
                        //if (bolContinuar)
                        //{
                        //    if (strOrigen != "SCL")
                        //    {
                        //        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Reserva tiene origen distinto de SCL, excepción 4.");
                        //        bolContinuar = false;
                        //    }
                        //}



                        if (bolContinuar)
                        {
                            //Revisamos si la ejecutiva ingreso KP, si es asi guardamos la comision
                            bolTieneKP = true;
                           
                            //Vamos a ver lo de los PQ's si hay dos pax adultos será PQ1, si además hay dos child de distinta edad, PQ1-3.
                            //si fuesen dos child con igual edad seria PQ1-2
                            ///DT 23-11-2016 Se agregan tipos de pasajeros PFA, CNN para Tarifas Privadas Copa
                            string strPQ;
                            int intCanAdl = 0, intCanInf = 0, intCanChd;
                            int intCanChd01 = 0, intCanChd02 = 0, intCanChd03 = 0, intCanChd04 = 0, intCanChd05 = 0, intCanChd06 = 0;
                            int intCanChd07 = 0, intCanChd08 = 0, intCanChd09 = 0, intCanChd10 = 0, intCanChd11 = 0;

                            for (int i = 0; i < RS.TravelItinerary.CustomerInfo.PersonName.Length; i++)
                            {
                                strTipoPax = RS.TravelItinerary.CustomerInfo.PersonName[i].PassengerType;
                                ///Adult
                                if (strTipoPax == "ADT" && intCanAdl == 0)
                                    intCanAdl++;
                                if (strTipoPax == "PFA" && intCanAdl == 0)
                                    intCanAdl++;
                                ///Child
                                if (strTipoPax == "C01" && intCanChd01 == 0)
                                    intCanChd01++;
                                if (strTipoPax == "C02" && intCanChd02 == 0)
                                    intCanChd02++;
                                if (strTipoPax == "C03" && intCanChd03 == 0)
                                    intCanChd03++;
                                if (strTipoPax == "C04" && intCanChd04 == 0)
                                    intCanChd04++;
                                if (strTipoPax == "C05" && intCanChd05 == 0)
                                    intCanChd05++;
                                if (strTipoPax == "C06" && intCanChd06 == 0)
                                    intCanChd06++;
                                if (strTipoPax == "C07" && intCanChd07 == 0)
                                    intCanChd07++;
                                if (strTipoPax == "C08" && intCanChd08 == 0)
                                    intCanChd08++;
                                if (strTipoPax == "C09" && intCanChd09 == 0)
                                    intCanChd09++;
                                if (strTipoPax == "C10" && intCanChd10 == 0)
                                    intCanChd10++;
                                if (strTipoPax == "C11" && intCanChd11 == 0)
                                    intCanChd11++;
                                if (strTipoPax == "CNN" && intCanChd11 == 0)
                                    intCanChd11++;
                                ///Infant
                                if (strTipoPax == "INF" && intCanInf == 0)
                                    intCanInf++;

                            }

                            intCanChd = intCanChd01 + intCanChd02 + intCanChd03 + intCanChd04 + intCanChd05 + intCanChd06;
                            intCanChd += intCanChd07 + intCanChd08 + intCanChd09 + intCanChd10 + intCanChd11;

                            if (intCanChd > 0)
                            {
                                ///Se modifica por solicitud realizada en el soporte 0068446
                                ///Fecha: 10/10/2012
                                ///Autor: MDS

                                //intCanChd = 1;
                            }

                            int intTotalTiposPax = intCanAdl + intCanChd + intCanInf;
                            if (intTotalTiposPax == 1)
                            {
                                strPQ = "PQ1";
                            }
                            else
                            {
                                strPQ = "PQ1-" + intTotalTiposPax.ToString();
                            }

                            //switch (intCanAdl + intCanChd + intCanInf)
                            //{
                            //    case 1:
                            //        strPQ = "PQ1";
                            //        break;
                            //    case 2:
                            //        strPQ = "PQ1-2";
                            //        break;
                            //    default:
                            //        strPQ = "PQ1";
                            //        break;
                            //}

                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Se emitira con PQ: " + strPQ);

                            //Si tiene KP entonces emitimos como esta
                            if (bolTieneKP)
                            {
                                stbComandoEmision.Append("W¥" + strPQ);
                            }
                          
                            //Loguea impresora ticket
                            strRespuesta = csSabre.ConsultarComando("W*CL");

                            //Print para impresion 
                            strRespuesta = csSabre.ConsultarComando("PTR/" + strTAEmision);

                            //Agregamos forma de pago CASH
                            stbComandoEmision.Append("¥FCA");

                            //Agregamos Código de Moneda TODO: Sacar letra M del config y agregarla al code behind
                            if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseCurrencyCode"]) == true)
                            {
                                stbComandoEmision.Append("¥" + System.Configuration.ConfigurationManager.AppSettings["CurrencyCode"]);
                            }

                            //EJECUTAMOS COMANDO DE EMISION!!!!!
                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "COMANDO EMISIÓN: " + stbComandoEmision);

                            //////////////////////////////////////////////////////////////////////////////////////////////////////
                            //////////////////////////////////////////////////////////////////////////////////////////////////////

                            strRespuesta = csSabre.ConsultarComando(stbComandoEmision.ToString());
                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Mensaje enviado por SABRE: " + strRespuesta);
                            responseTicketIssue = strRespuesta;



                            //Comentar
                            //strRespuesta = "100 - CITY DOES NOT MATCH EXACT ITINERARY IN ETR AIRLINE SYSTEM";

                            //////////////////////////////////////////////////////////////////////////////////////////////////////
                            //////////////////////////////////////////////////////////////////////////////////////////////////////


                            if (strRespuesta.IndexOf("CONTINUE TO TICKET?") > -1)
                            {
                                ///DToledo 20160512 Se corrige Mask Handling
                                strRespuesta = csSabre.ConsultarComando("CO<Y>");
                                //EJECUTAMOS Confirmación de Emisión!!!!!
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Confirmamos Emisión: <Y>");
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Mensaje enviado por SABRE: " + strRespuesta);
                                responseTicketIssue = strRespuesta;
                            }


                            if (csWSSabre.bolExisteMsjError(strReserva, strRespuesta))
                            {
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Error enviado por SABRE emitiendo: " + strRespuesta);
                                strRespuesta = csSabre.ConsultarComando("I");
                            }
                            else
                            {
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Grabando reserva, 6TKTROBOT");

                                strRespuesta = csSabre.ConsultarComando("6TKTROBOT");

                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Respuesta Grabando reserva: " + strRespuesta);

                                //Se agrega 5 intentos para realizar un ET y ver que esté emitido ok.
                                for (int i = 0; i < 5; i++)
                                {
                                    strRespuesta = csSabre.ConsultarComando("ET");


                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Respuesta ET(" + i.ToString() + "): " + strRespuesta);


                                    // if (strRespuesta.IndexOf("ETR MESSAGE PROCESSED") > -1 || strRespuesta.IndexOf("TTY REQ PEND") > -1 || strRespuesta.IndexOf("INVOICED") > -1)
                                    if (strRespuesta.IndexOf("OK ", 0, 4) > -1)
                                    {
                                        bolEmitidoCorrectamente = true;

                                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Respuesta ET OK");

                                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Recuperando PNR " + strReserva);

                                        RSRespuesta = csSabre.ConsultarPNR_New(strReserva);

                                        if (RSRespuesta.TravelItinerary != null)
                                        {

                                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "PNR Recuperado OK");
                                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Enviando EM, TripCase");

                                            strRespuesta = csSabre.ConsultarComando("6TKTROBOT");

                                            strRespuesta = csSabre.ConsultarComando("EM");

                                            if (strRespuesta.IndexOf("EMAIL REQUEST ACCEPTED") > -1)
                                            {
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + strRespuesta + ", Respuesta EM Ok, Se envía TripCase.");
                                                //MAN-1978 Emisión Automatica VOL - Error en OK de Emisión Automática
                                                NoMsgexisteError = false;
                                            }
                                            else if (strRespuesta.IndexOf("SIMULTANEOUS CHANGES") > -1)
                                            {
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + strRespuesta);
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Cambios simultáneos detectados, se reintenta envío de TripCase. ");
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Ignorando y recuperando ");

                                                strRespuesta = csSabre.ConsultarComando("I");

                                                RSRespuesta = csSabre.ConsultarPNR_New(strReserva);

                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + strRespuesta);
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Enviando EM. TripCase ");

                                                strRespuesta = csSabre.ConsultarComando("6TKTROBOT");
                                                strRespuesta = csSabre.ConsultarComando("EM");


                                                if (strRespuesta.IndexOf("EMAIL REQUEST ACCEPTED") > -1)
                                                {
                                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + strRespuesta + ", Respuesta EM Ok, Se envía TripCase.");
                                                    //MAN-1978 Emisión Automatica VOL - Error en OK de Emisión Automática
                                                    NoMsgexisteError = false;
                                                }
                                                else
                                                {
                                                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + strRespuesta + " Error respuesta EM, No se envía TripCase");
                                                    //MAN-1978 Emisión Automatica VOL - Error en OK de Emisión Automática
                                                    NoMsgexisteError = false;
                                                    strRespuesta = csSabre.ConsultarComando("I");
                                                }

                                            }
                                            else
                                            {
                                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + strRespuesta + " Error respuesta EM, No se envía TripCase");
                                                //MAN-1978 Emisión Automatica VOL - Error en OK de Emisión Automática
                                                NoMsgexisteError = false;
                                                strRespuesta = csSabre.ConsultarComando("I");
                                            }
                                        }
                                        else
                                        {
                                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, " " + RSRespuesta.ApplicationResults.Error.ToString() + ", No se pudo recuperar el PNR, No se envía TripCase");
                                            //MAN-1978 Emisión Automatica VOL - Error en OK de Emisión Automática
                                            NoMsgexisteError = false;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Error: " + ex.Message);
                    }
                    finally
                    {
                        //MAN-2340
                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Ingresando en revalidacion emison ticket en sabre.");
                        if (!NoMsgexisteError)
                        {
                            //MAN-2340	
                            //MAN-1978 Emisión Automatica VOL - Error en OK de Emisión Automática
                            //Validacion Emision OK
                            RSRespuesta = csSabre.ConsultarPNR_New(strReserva);

                            string strRes = csSabre.ConsultarComando("*T");
                            //MAN-2340
                            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Respuesta emision : " + strRes);
                            if (strRes.Contains(".TE "))
                            {
                                bolEmitidoCorrectamente = true;
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Revalidacion emision ok : " + bolEmitidoCorrectamente.ToString());
                            }
                            else
                            {
                                bolEmitidoCorrectamente = false;
                                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Revalidacion emision ok : " + bolEmitidoCorrectamente.ToString());
                            }
                        }


                        //Cerramos la sesion
                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Cerrando sesion");
                        csSabre.CerrarSesion();
                        Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Sesion cerrada");
                    }
                }
                else
                {
                    //Bitacora, error creando sesion
                    Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Error creando sesión");
                }
            }
            catch (Exception ex)
            {
                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Error: " + ex.Message);
            }
            //LVQ 20160404 se agrega detalle a la respuesta
            RespuestaDTO TheRespuesta = new RespuestaDTO();
            if (bolEmitidoCorrectamente)
            {
                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Se entrega mensaje de emision OK");
                TheRespuesta.Estado = "Emitido";
                TheRespuesta.Mensaje = "Emisión OK";
                TheRespuesta.Texto = responseTicketIssue;
            }
            else
            {
                Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "Se entrega mensaje de error de emisión");
                TheRespuesta.Estado = "Error";
                TheRespuesta.Mensaje = "Error interno";
                TheRespuesta.Texto = responseTicketIssue;
            }

            Log.grabaLog("wsSabre_bolEmiteReserva", strReserva, "//////////////////////////////  FIN EMISION  ///////////////////////////////////");

            //return bolEmitidoCorrectamente;
            return TheRespuesta;
            //LVQ.


        }


        public class ServiceAuthHeader : SoapHeader
        {
            public string Username;
            public string Password;
        }

        public ServiceAuthHeader CustomSoapHeader;



        public class ServiceAuthHeaderValidation
        {
            public static bool Validate(ServiceAuthHeader soapHeader)
            {
                if (soapHeader == null)
                {
                    throw new NullReferenceException("No soap header was specified.");
                }
                if (soapHeader.Username == null)
                {
                    throw new NullReferenceException("Username was not supplied for authentication in SoapHeader.");
                }
                if (soapHeader.Password == null)
                {
                    throw new NullReferenceException("Password was not supplied for authentication in SoapHeader.");
                }

                string UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
                string PassWord = System.Configuration.ConfigurationManager.AppSettings["PassWord"];
                if (soapHeader.Username != UserName || soapHeader.Password != PassWord)
                {
                    throw new Exception("Please pass the proper username and password for this service.");
                }
                return true;
            }
        }



        //LVQ 20160404 se agrega detalle a la respuesta objeto respuesta
        public class RespuestaDTO
        {
            public string Estado { get; set; }
            public string Mensaje { get; set; }
            public string Texto { get; set; }
        }


        //LVQ..
    }
}
