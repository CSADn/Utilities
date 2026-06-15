using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Helpers;

using httpConnection;
using ClassLibrarySDK.Packages.com.sysone.model;
using ClassLibrarySDK.Packages.com.sysone.model.policy;
using ClassLibrarySDK.Packages.com.sysone.service;
using ClassLibrarySDK.Packages.sdk.serializer;
using ClassLibrarySDK.Packages.com.sysone.model.party;
using ClassLibrarySDK.Packages.com.sysone.model.address;
using ClassLibrarySDK.Packages.com.sysone.model.beans;

using sysoneBus.Entities;
using sysoneBus.Enums;
using sysoneBus.Maestros;
using System.Net;

namespace sysoneBus
{
    public class Bus : GenericSingleton<Bus>
    {

        private static SDKConf conf;
        private static List<Productor> productores;

        #region Constructor..

        /// <summary>
        /// inicializador con  valores  del archivo  configuracion 
        /// </summary>
        public Bus():this(Config.GetResourceURL(), Config.GetApplicationID(), Config.GetChannelID(), Config.GetUser(), Config.GetPassword(), Config.GetAuthAgent(), Config.GetTimeOut())
        {
            
        }
        /// <summary>
        /// /Inicializador  del recurso con el sistema BUS
        /// </summary>
        /// <param name="ResourceURL">Url del recurso</param>
        /// <param name="User">Nombre de usuario  BUS</param>
        /// <param name="Password">Contraseña del usuario BUS</param>
        public Bus(string ResourceURL, string User, string Password):this(ResourceURL,"1","1",User,Password, "S1SDK", 10000)
        {

        }
        /// <summary>
        /// /Inicializador  del recurso con el sistema BUS
        /// </summary>
        /// <param name="ResourceURL">Url del recurso</param>
        /// <param name="User">Nombre de usuario  BUS</param>
        /// <param name="Password">Contraseña del usuario BUS</param>
        /// <param name="TimeOut">Tiempo de espera de conexion expresados en milisegundos</param>
        public Bus(string ResourceURL, string User, string Password,int TimeOut) : this(ResourceURL, "1", "1", User, Password, "S1SDK", TimeOut)
        {

        }
        /// <summary>
        /// Inicializador  del recurso con el sistema BUS
        /// </summary>
        /// <param name="ResourceURL">Url del recurso</param>
        /// <param name="ApplicationID">Id de Applicacion ej: "1".. </param>
        /// <param name="ChannelID">Id del Canal ej: "1"..</param>
        /// <param name="User">Nombre de usuario  BUS</param>
        /// <param name="Password">Contraseña del usuario BUS</param>
        /// <param name="AuthAgent">Agente autorizador ej: "S1SDK"..</param>
        /// <param name="TimeOut">Tiempo de espera de conexion expresados en milisegundos</param>
        public Bus(string ResourceURL,string ApplicationID,string ChannelID,string User, string Password,string AuthAgent,int TimeOut)
        {
            try
            {
                conf = new SDKConf(new BUSExcepciones());
                conf.ResourceURL = ResourceURL;
                conf.LogJsonRequest = false;
                conf.LogJsonResponde = false;
                conf.ApplicationID = ApplicationID;
                conf.ChannelID = ChannelID;
                conf.Password = Password;
                conf.AuthAgent = AuthAgent;
                conf.User = User;
                conf.TimeOut = TimeOut;

            }
            catch (Exception ex)
            {
                throw new BUSExcepciones(ex);
            }

        }

        #endregion

        #region Metodos Publicos

        /// <summary>
        /// Busca una lista de polizas consolidadas con tasas 
        /// </summary>
        /// <param name="linea">Sistema o Ramo del cual se realizara la busqueda</param>
        /// <param name="nroPoliza">Numero de poliza</param>
        /// <param name="CUIT">Numnero de CUIT </param>
        /// <param name="apellido">Apellido del usuario </param>
        /// <param name="razonSocial">Razon social del usuario</param>
        /// <returns>Devuelve una lista del tipo Poliza</returns>
        public List<Poliza> BuscarPolizasConsolidadas(Lineas linea,string nroPoliza, string CUIT, string razonSocial) 
        {


            var polizas = new List<Poliza>();
            var request = new PolicyServiceImpl(conf);
            PolicySearchBean search = new PolicySearchBean();

            try
            {
                search.FirstNameData = razonSocial;
                search.PolicyNumberData = nroPoliza;
                List<Policy> policyresponse = request.findByPolicySearchBean(search);

                if (policyresponse == null)
                    throw new BUSExcepciones("La búsqueda no obtuvo ningún resultado");

                polizas = policyresponse.Select(x => new Poliza()
                {
                    Asegurado = getAsegurados(x.HeaderData.InsuredData),
                    Cobertura = getCobertura(x.HeaderData.CoveragePlanData),
                    Descripcion = x.EndorsementTypeData.DescriptionData,
                    Endosos = getEndosos(x),
                    FechaEmision = (DateTime)x.IssuanceDateData,
                    FechaVigenciaDesde = (DateTime)x.HeaderData.FromDateData,
                    FechaVigenciaHasta = (DateTime)x.HeaderData.ToDateData,
                    id = (long)x.HeaderData.IdData,
                    Numero = x.IdData.PolicyNumberData,
                    Tomador = getTomador(x.HeaderData.HolderData),
                    Benificiario = getBenificiario(),
                    Estado = new EstadoPoliza() { Codigo = x.StatusData.ClientCodeData, id = (long)x.StatusData.IdData, Deescripcion = x.StatusData.DescriptionData },

            }).ToList();


            }
            catch (Exception ex)
            {
                throw new BUSExcepciones(ex);

            }
             //Este método debería devolver una lista de pólizas consolidadas(Con tasas si es posible).
            return polizas;
        }
        /// <summary>
        /// Busca una  poliza consolidada completa 
        /// </summary>
        /// <param name="linea">Sistema o Ramo del cual se realizara la busqueda</param>
        /// <param name="nroPoliza">Numero de poliza</param>
        /// <param name="nroEndoso">Numero de Endoso</param>
        /// <returns>Devuelve valor del tipo Poliza</returns>
        public Poliza BuscarPolizaConsolidada(Lineas linea, string nroPoliza,int nroEndoso)
        {
            var poliza = new Poliza();
            var request = new PolicyServiceImpl(conf);
            PolicyId search = new PolicyId();

            try
            {

             
                search.SectionIdData = linea.GetLinea();
                search.SubSectionIdData = 1000; //verificar para que sirve y de donde lo obtengo
                search.PolicyNumberData = nroPoliza;
                search.InsurerIdData = 1006; //asegurador "Meridional"
                search.EndorsementNumberData = nroEndoso;

                Policy policyresponse = request.getCompletePolicy(search);
                             
                                
                if (policyresponse == null)
                    throw new BUSExcepciones("La búsqueda no obtuvo ningún resultado");

                

                poliza.Asegurado = getAsegurados(policyresponse.HeaderData.InsuredData);
                poliza.Cobertura = getCobertura(policyresponse.HeaderData.CoveragePlanData);
                poliza.Descripcion = policyresponse.EndorsementTypeData.DescriptionData;
                poliza.Endosos = getEndosos(policyresponse);
                poliza.FechaEmision = (DateTime)policyresponse.IssuanceDateData;
                poliza.FechaVigenciaDesde = (DateTime)policyresponse.HeaderData.FromDateData;
                poliza.FechaVigenciaHasta = (DateTime)policyresponse.HeaderData.ToDateData;
                poliza.id = (long)policyresponse.HeaderData.IdData;
                poliza.Numero = policyresponse.IdData.PolicyNumberData;
                poliza.Tomador = getTomador(policyresponse.HeaderData.HolderData);
                poliza.Benificiario = getBenificiario();
                poliza.Estado = new EstadoPoliza() { Codigo = policyresponse.StatusData.ClientCodeData, id = (long)policyresponse.StatusData.IdData, Deescripcion = policyresponse.StatusData.DescriptionData };

               //Este método debería traer la póliza consolidada completa.

            }
            catch (Exception ex)
            {
                throw new BUSExcepciones(ex);
            }      


            return poliza;
        }        
        /// <summary>
        /// Busca los endosos de una  poliza especifica
        /// </summary>
        /// <param name="linea">Sistema o Ramo del cual se realizara la busqueda</param>
        /// <param name="poliza">Numero de poliza</param>
        /// <returns>Devuelve una lista de endosos</returns>
        public List<Endosos> BuarcarEndosos(Lineas linea,Poliza poliza)
        {
            //Búsqueda por: Línea + Nro.Póliza.
            //Este método debería devolver una lista con todos los números de endosos para esa póliza. (O encabezado de póliza + lista de números de endosos).

            return new List<Endosos>();            
        }
        /// <summary>
        /// Busca un endoso especifico
        /// </summary>
        /// <param name="linea">Sistema o Ramo del cual se realizara la busqueda</param>
        /// <param name="nroEndoso">Numero de Endoso</param>
        /// <returns></returns>
        public Endosos BuscarEndoso(Lineas linea, int nroEndoso)
        {
            return new Endosos();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lienas">Sistema o Ramo del cual se realizara la busqueda</param>
        /// <param name="poliza"></param>
        public void MovimientosPoliza(Lineas liena,Poliza poliza)
        {
            //Endosos por: Línea + Nro.Póliza.
            //Este método sería conveniente que nos permita enviar una lista de movimientos todos juntos en un solo request
            //, y que la API devuelva los errores de cada movimiento, si es que los hubo, de lo contrario un OK.
        }
        #endregion

        #region Metodos Privados

        private List<Asegurado> getAsegurados(ContractInsured insured)
        {
            var asegurados = new List<Asegurado>();

            var partyaddress = insured.PartyData.AddressesData;
            var partyphone = insured.PartyData.PhonesData;

            var direccion = partyaddress.Select(x =>  x.AddressStreetData + " " + x.AddressNumberData +" " + x.ApartmentData).ToList();
            var telefono = partyphone.Select(x =>  x.CountryCodeData.ToString() + x.AreaCodeData.ToString() + x.NumberData.ToString()).ToList();
             
            asegurados.Add(new Asegurado { Nombre = "Dummy", Apellido = "Dummy", Telefono = telefono, CP = new List<string> { "1050" }, Dirreccion = direccion , NumeroDocumento = "25430441", TipoPersona = TipoPersona.FISICA,Fecha = (DateTime)insured.PartyData.DateCreatedData, FechaNacimiento=DateTime.Parse("1976-07-17"), TipoDocumento=TipoDocumento.DNI });

            return asegurados;

        }

        private List<Cobertura> getCobertura(CoveragePlan coverageplan)
        {

            var coberturas = new List<Cobertura>();

            coberturas.Add(new Cobertura {Descripcion="Cobertura Dummy", id=1001, Numero=1028, Tipo=new TipoCobertura{ id=100, Descripcion="Incendio"}});

            return coberturas;


        }

        private List<Beneficiario> getBenificiario()
        {
            var beneficiarios = new List<Beneficiario>();

            beneficiarios.Add(new Beneficiario { Nombre = "Dummy", Apellido = "Dummy", Telefono = new List<string> { "1050" }, CP = new List<string> { "1050" }, Dirreccion = new List<string> { }, NumeroDocumento = "25430441", TipoPersona = TipoPersona.FISICA, Fecha = DateTime.Now, FechaNacimiento = DateTime.Parse("1976-07-17"), TipoDocumento = TipoDocumento.DNI });

            return beneficiarios;
        }

        private List<Endosos> getEndosos(Policy policy)
        {

            var endosos = new List<Endosos>();

            endosos.Add(new Endosos { Descripcion = policy.EndorsementTypeData.DescriptionData, Id = (long)policy.EndorsementTypeIdData, Numero=(int)policy.IdData.EndorsementNumberData, VigenciaDesde =(DateTime)policy.EndorsementFromData, VigenciaHasta=(DateTime)policy.EndorsementToData,Tipo=new TipoEndoso { id=int.Parse(policy.EndorsementTypeData.ClientCodeData), Descripcion=policy.EndorsementTypeData.DescriptionData} });

            return endosos;

        }

        private Tomador getTomador(PolicyHolder holder)
        {
            
            var partyaddress = holder.PartyData.AddressesData;
            var partyphone = holder.PartyData.PhonesData;

            var direccion = partyaddress.Select(x => x.AddressStreetData + " " + x.AddressNumberData + " " + x.ApartmentData).ToList();
            var telefono = partyphone.Select(x => x.CountryCodeData.ToString() + x.AreaCodeData.ToString() + x.NumberData.ToString()).ToList();

            var tomador = new Tomador() { Nombre = "Dummy", Apellido = "Dummy", Telefono = telefono, CP = new List<string> { "1050" }, Dirreccion = direccion, NumeroDocumento = "25430441", TipoPersona = TipoPersona.FISICA, Fecha = (DateTime)holder.PartyData.DateCreatedData, FechaNacimiento = DateTime.Parse("1976-07-17"), TipoDocumento = TipoDocumento.DNI };

            return tomador;
        }
        
        private List<Productor> getMestroProductores()
        {

            var productores = new List<Productor>();

            var productor = new IntermediaryServiceImpl(conf);

            var getProducatores =  productor.searchAll();

            //productores = getProducatores.Select(x => new Productor() { Codigo = x.CodeData, Nombre=x.NameData, Grupo= new Grupo() { Codigo =  } }).ToList();

            return null;


        }   




        #endregion

    }
}
