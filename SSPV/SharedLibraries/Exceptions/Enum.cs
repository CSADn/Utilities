using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Exceptions
{
    public enum Code
    {
        #region Errores Genericos 0 - 99

        [Description("Error grave. Contacte al administrador.")]
        Generico = 0,
        [Description("No fue posible establecer una conexión con la base de datos.")]
        BaseDatosNotFound = 50,
        [Description("No fue posible establecer una conexión con la base de datos.")]
        GetValueNotFound = 51,

        #endregion

        #region Errores de Seguridad 100 - 199

        [Description("La sesión ha caducado.")]
        UserNotFoundInSession = 100,

        [Description("La aplicación aun no ha sido dada de alta.")]
        AplicationNotFound = 101,

        [Description("Productor inexistente o sin permisos.")]
        ProductorAccessDenied = 102,

        #endregion

        #region Errores de Impresion 600 - 699

        [Description("No existe el template.")]
        ImpresionTemplateNotFound = 600,
        [Description("Acceso denegado.")]
        ImpresionProductorNotValid = 601,
        [Description("No existe el template del Body para EMail de Cotización.")]
        EMailCotizacionTemplateNotFound = 602,
        [Description("No existe el la configuración del Servidor SMTP.")]
        SMTPHostEMailCotizacionNotFound = 603,
        [Description("EMail del Asegurado Invalido.")]
        EMailAseguradoInvalid = 604,
        [Description("Cotización del Asegurado Inexistente.")]
        CotizacionEMailNotFound = 605,
        [Description("No existe el la configuración del Servidor SMTP.")]
        SMTPHostEMailSolicitudNotFound = 606,
        [Description("No existe la Solicitud.")]
        EMailSolicitudNotFound = 607,
        [Description("Cotización del Asegurado Inexistente.")]
        FECotizacionNotFound = 608,
        [Description("No existe el template del Body para EMail Solicitud.")]
        EMailSolicitudTemplateNotFound = 609,

        #endregion

        #region Errores en Api 900 - 999

        [Description("Error Api. Contacte al administrador.")]
        ErrorGenericoApi = 900,

        [Description("Error en Token.")]
        TokenError = 901,

        [Description("Token Inexistente.")]
        TokenInexistente = 902,

        [Description("Token Invalido.")]
        TokenInvalido = 903,

        [Description("Token Fuera de Vigencia.")]
        TokenFueraVigencia = 904,

        [Description("Error al grabar la foto.")]
        ApiErrorGrabarFoto = 905,
        
        [Description("Error usuario inexistente.")]
        ApiErrorUsuario = 906,

        [Description("Error tipo de usuario no valido.")]
        ApiErrorTipoUsuario = 907,

        #endregion

        #region Errores de la Cotización 1000 - 1999

        [Description("Datos de la Cotización Inválidos.")]
        CotizacionDataError = 1000,
        [Description("Productor Vacío.")]
        ProductorEmpty = 1001,
        [Description("Productor Inexistente.")]
        ProductorNotFound = 1002,
        [Description("Cobertura de Ascensores Inexistente.")]
        CoberturaAscensoresNotFound = 1003,
        [Description("Suma Asegurada de Ascensores Inválida.")]
        SumaCoberturaAscensoresInvalid = 1004,
        [Description("Unidad de Negocio Vacía.")]
        UNegEmpty = 1005,
        [Description("Unidad de Negocio Inexistente o No Pertenece al Productor.")]
        UNegNotFound = 1006,
        [Description("Cobertura Inexistente.")]
        CoberturaNotFound = 1007,
        [Description("Comisión Inválida.")]
        ComisionInvalid = 1008,
        [Description("Actividad Inválida.")]
        ActividadNotFound = 1009,
        [Description("Actividad Vacía.")]
        ActividadEmpty = 1010,
        [Description("Cobertura Sin Parametrización.")]
        CoberturaParamNotFound = 1011,

        [Description("Tipo de Documento Inválido.")]
        TipoDocumentoNotFound = 1012,
        [Description("Condición de IVA Inválido.")]
        CondicionIVANotFound = 1013,
        [Description("Condición de IVA Inválido para el tipo de Documento indicado.")]
        CondicionIVATipoDocIndicadoNotFound = 1150,
        [Description("Condición de Ingresos Brutos Inválido.")]
        CondicionIIBBNotFound = 1014,
        [Description("Código de Provincia Inválido.")]
        ProvinciaNotFound = 1015,
        [Description("Código de Provincia Difiere a la de la Cotización.")]
        ProvinciaDifCotizacion = 1016,
        [Description("CUIT Inválido.")]
        CUITNotFound = 1017,
        [Description("Usuario Inexistente.")]
        UsuarioNotFound = 1018,
        [Description("Usuario inhabilitado por baja para cotizar.")]
        UsuarioInhabilitadoPorBajaCotizar = 5172,
        [Description("Tipo de Documento Difiere a la de la Cotización.")]
        TipoDocumentoDifCotizacion = 5169,

        [Description("Número de Documento Difiere a la de la Cotización.")]
        NroDocumentoDifCotizacion = 5170,

        [Description("Provincia Vacía.")]
        ProvinciaEmpty = 1019,
        [Description("Localidad Vacía.")]
        LocalidadEmpty = 1020,
        [Description("Localidad Inexistente.")]
        LocalidadNotFound = 1021,
        [Description("Código Postal Vacío.")]
        CodPosEmpty = 1022,
        [Description("Código Postal Inválido.")]
        CodPosNotFound = 1023,

        [Description("Accesorio del Vehículo Inválido.")]
        AccesorioNotFound = 1024,
        [Description("Accesorio Inválido para el Combustible del Vehículo.")]
        AccesorioInvalid = 1025,
        [Description("Accesorio del Vehículo Inválido para la Unidad de Negocio.")]
        AccesorioInvalidUNeg = 1026,
        [Description("Suma Asegurada de Accesorios Supera el Maximo.")]
        SumaAccesoriosErrorMax = 1027,
        [Description("Suma Asegurada de Accesorios Menor al Minimo.")]
        SumaAccesoriosErrorMin = 1028,
        [Description("Código Infoauto Vacío.")]
        InfoautoEmpty = 1029,
        [Description("Código Infoauto Inválido.")]
        InfoautoNotFound = 1030,
        [Description("Detalle del Modelo Infoauto Inválido.")]
        InfoautoItemNotFound = 1031,
        [Description("Año del Vehículo Vacío.")]
        AnnoEmpty = 1032,
        [Description("Año del Vehículo Inválido.")]
        AnnoNotFound = 1033,
        [Description("Uso del Vehículo Vacío.")]
        UsoVehiculoEmpty = 1034,
        [Description("Uso del Vehículo Inválido.")]
        UsoVehiculoNotFound = 1035,
        [Description("Rastreador del Vehículo Vacío.")]
        RastreadorEmpty = 1036,
        [Description("Ratreador del Vehículo Inválido.")]
        RasteadorNotFound = 1037,
        [Description("El Tipo de Rastreador Seleccionado No Coincide con la Suma Asegurada del Vehículo.")]
        RasteadorSAInvalid = 1038,
        [Description("Edad Inválida. Fuera del Rango de Límites.")]
        EdadInvalid = 1039,
        [Description("Código de Tarifa Inválido.")]
        TarifaNotFound = 1040,
        [Description("Sexo Vacío.")]
        SexoEmpty = 1041,
        [Description("Sexo Inválido.")]
        SexoNotFound = 1042,
        [Description("Estado Civil Vacío.")]
        EstadoCivilEmpty = 1043,
        [Description("Estado Civil Inválido.")]
        EstadoCivilNotFound = 1044,
        [Description("Ocupación Vacío.")]
        OcupacionEmpty = 1045,
        [Description("Ocupación Inválido.")]
        OcupacionNotFound = 1046,
        [Description("Clausula de Ajuste Vacía.")]
        ClausulaEmpty = 1047,
        [Description("Clausula de Ajuste Inválida.")]
        ClausulaNotFound = 1048,
        [Description("Medio de Pago Vacío.")]
        MedioPagoEmpty = 1049,
        [Description("Medio de Pago Inválido.")]
        MedioPagoNotFound = 1050,
        [Description("Medio de Pago Inválido para la Unidad de Negocio.")]
        MedioPagoUNegNotFound = 1051,
        [Description("Medio de Pago Alternativo Inválido.")]
        MedioPagoAlternativoInvalido = 1053,
        
        [Description("Periocidad Vacía.")]
        PeriodoEmpty = 1052,
        [Description("Tipo de Periocidad inválida.")]
        PeriodoNotFound = 1053,
        [Description("Cantidad de Cuotas Inválidas.")]
        CantidadCuotasInvalid = 1054,
        [Description("Fecha de Nacimiento Vacía.")]
        FechaNacimientoEmpty = 1055,
        [Description("Código de Organizador Inválido.")]
        OrganizadorInvalido = 1056,
        [Description("Error en la Generación de Impuestos.")]
        GenerarImpuestosError = 1057,
        [Description("La Suma Asegurada del Vehículo supera los Límites.")]
        SumaAseguradaLimiteError = 1058,
        [Description("Apellido/Razón Social Inexistente.")]
        CotizacionApellidoNotFound = 1059,
        [Description("Convenio Inexistente.")]
        ConvenioNotFound = 1060,
        [Description("Convenio del Productor dado de Baja.")]
        ConvenioDelete = 1061,
        [Description("Medio de Pago del Convenio asignado al Productor Inexistente.")]
        ConvenioMedioPagoNotFound = 1062,
        [Description("Supera la Cantidad de Cuotas asignadas al Convenio.")]
        ConvenioCuotasMax = 1063,
        [Description("No Existe Convenio para el Periodo de la Solicitud.")]
        ConvenioItemNotFound = 1064,
        [Description("Tarifario Vencido.")]
        TarifarioVencido = 1065,
        [Description("Error al Guardar los datos del Presupuesto")]
        PresupuestoSaveDataError = 1066,
        [Description("No hay parametrización de edad para la unidad de negocio")]
        EdadPeriodoUsoVehiculoNull = 1067,
        [Description("Configuración de Rastreador con Suma Asegurada Inexistente.")]
        ConfigRastreadorNotFound = 1068,
        [Description("Suma Asegurada del Vehículo es Inválida para el Rastreador Seleccionado.")]
        ConfigRastreadorSumaAseguradaInvalid = 1069,

        [Description("El código postal no está asociado a ninguna zona.")]
        CodPostZonaNotFound = 1070,

        [Description("Ubicación del Riesgo Inválida.")]
        UbicacionRiesgoNotFound = 1071,
        [Description("Ubicación del Riesgo Vacía.")]
        UbicacionRiesgoEmpty = 1072,
        [Description("Domicilio Vacío.")]
        DomicilioEmpty = 1073,
        [Description("Tipo de Vivienda Vacío.")]
        TipoViviendaEmpty = 1074,
        [Description("Tipo de Vivienda Inválido.")]
        TipoViviendaNotFound = 1075,
        [Description("Ocupación Vacío.")]
        TipoOcupacionEmpty = 1076,
        [Description("Ocupación Inválido.")]
        TipoOcupacionNotFound = 1077,
        [Description("Titular Vacío.")]
        TitularEmpty = 1078,
        [Description("Titular Inválido.")]
        TitularNotFound = 1079,
        [Description("Descuento Especial Inválido.")]
        DescuentoEspecialNotFound = 1080,

        [Description("Tipo de Plan Vacío.")]
        TipoPlanEmpty = 1081,
        [Description("Tipo de Plan Inválido.")]
        TipoPlanNotFound = 1082,
        [Description("Tipo de Movimiento Vacío.")]
        TipoMovimientoEmpty = 1083,
        [Description("Tipo de Movimiento Inválido.")]
        TipoMovimientoNotFound = 1084,
        [Description("Coberturas no existen en el Plan.")]
        CoberturasNotInPlan = 1085,
        [Description("La Suma Asegurada de la Cobertura es Inválida.")]
        SumaAseguradaCoberturaInvalid = 1086,
        [Description("Sponsor Vacío")]
        SponsorEmpty = 1087,
        [Description("Sponsor Inexistente")]
        SponsorNotFound = 1088,
        [Description("Captcha Obligatorio e Inexistente")]
        CaptchaNotFound = 1089,
        [Description("La Imagen Seleccionada no es Válida")]
        CaptchaNotValid = 1090,

        [Description("Directo Vacío")]
        DirectoEmpty = 1091,
        [Description("Directo Inexistente")]
        DirectoNotFound = 1092,
        [Description("Captcha Obligatorio e Inexistente")]
        DirectoCaptchaNotFound = 1093,
        [Description("La Imagen Seleccionada no es Válida")]
        DirectoCaptchaNotValid = 1094,

        [Description("No existe parametrización disponible para el producto y tipo de vivienda")]
        ProductoParametrosNotFound = 1095,

        [Description("La unidad de negocio asiganda al productor, no tiene planes/packs asignados")]
        UNegPlanesEmpty = 1096,
        [Description("El código de Subproducto no puede ser nulo")]
        SubproductoEmpty = 1097,
        [Description("El código de Subproducto para el productor, no existe")]
        SubproductoNotFound = 1098,

        [Description("Debe especificar importes para al menos tres coberturas")]
        CoberturasOutOfRange = 1099,

        [Description("No se especificó importe para ninguna cobertura")]
        CoberturasEmpty = 1100,

        [Description("Unidad de negocios inexistente para el productor en la cotización")]
        CotizacionProdUnegNotFound = 1101,

        [Description("Edad no ingresada")]
        EdadEmpty = 1102,

        [Description("Tipo Persona Invalida para la Condici&oacute;n de IVA seleccionada")]
        CondicionIVAMonitribInvalid = 1103,

        [Description("Ajuste de Tasas Inválido")]
        AjusteTasasInvalid = 1104,

        [Description("Acreedor Prendario/Leasing no seleccionado.")]
        CotizacionLeasingPrendarioNotFound = 1105,

        [Description("Ajuste de Tasas Inexistente")]
        AjusteTasasNotFound = 1106,

        [Description("Tipo de Vehículo Inválido")]
        TipoVehiculoInvalid = 1107,

        [Description("Configuración de Coeficientes para el Tipo de Vehículo Inexistente")]
        ConfigCoefNotFound = 1108,

        [Description("Configuración de Grupo de Coberturas para el Tipo de Vehículo Inexistente")]
        ConfigGrupoCoberturasNotFound = 1109,

        [Description("Acreedor Prendario inválido.")]
        CotizacionPrendarioInvalid = 1110,

        [Description("Empresa de Leasing inválido.")]
        CotizacionLeasingInvalid = 1111,

        [Description("Modalidad Inexistente.")]
        ModalidadNotFound = 1112,
        [Description("Opcional de Modalidad Inexistente.")]
        OpcionalNotFound = 1113,

        [Description("La jornada escolar no se permite con uso de moto.")]
        ModJornadaEscNotUsoMoto = 1152,

        [Description("Cantidad de Vidas Totales difiere con la de las Actividades.")]
        CantidadVidasError = 1114,
        [Description("Cantidad de Vidas supera el Máximo permitido.")]
        CantVidasMaxError = 1115,
        [Description("Porc. de Recargo Inválido.")]
        PorcRecargoInvalid = 1116,
        [Description("Categoria de Actividad no Parametrizada.")]
        CategoriaActividadNotFound = 1117,
        [Description("Capital Asegurado por Categoría Inexistente")]
        MinimoCategoriaNotFound = 1118,
        [Description("El Capital Asegurado no puede superar al de la categoría mas baja")]
        SumaAseguradaExcedeMaximoPorCategoria = 1119,
        [Description("Cobertura de Muerte por Accidente Inexistente.")]
        CoberturaMuerteAccNotFound = 1120,
        [Description("Cobertura de Muerte por Accidente No puede ser Menor a 0.")]
        CoberturaMuerteAccMenorCero = 1121,
        [Description("El Capital Asegurado de la Cobertura No puede ser Menor a 0.")]
        CoberturaMinorZero = 1122,
        [Description("Capital Asegurado supera el Máximo permitido.")]
        CapitalMaxError = 1123,
        [Description("El Capital Asegurado No puede Superar el % de la Suma Asegurada de Muerte por Accidente.")]
        CapitalAseguradoMayorInvalid = 1124,
        [Description("Deducible de Gastos de Asistencia Médica Farm. por Accidente Inexistente.")]
        DeducibleNotFound = 1125,
        [Description("Días de Carencia Inexistente.")]
        DiasCarenciaNotFound = 1126,
        [Description("Días de Cobertura Inexistente.")]
        DiasCoberturaNotFound = 1127,
        [Description("Altura a Cubrir debe ser Mayor a 0.")]
        AlturaCubrirInvalid = 1128,
        [Description("Días de Cobertura debe ser Mayor a 0.")]
        DiasCoberturaInvalid = 1129,
        [Description("Días de cobertura supera el Máximo permitido.")]
        DiasCoberturaMaxError = 1130,
        [Description("Altura a Cubrir supera el Máximo permitido.")]
        AlturaCubrirMaxError = 1131,
        [Description("Supera el Valor de Prima Máxima.")]
        PrimaMaximaInvalid = 1132,
        [Description("Parámetros de Configuración Inexistente.")]
        ParametrosNotFound = 1133,
        [Description("Sin totales para el presupuesto.")]
        PresupuestoTotalesNotFound = 1134,
        [Description("La Linea no es válida.")]
        LineaNotValid = 1135,
        [Description("El Productor no posee convenios asignados")]
        ProductorConvenioNotFound = 1136,
        [Description("Tipo de Carga  Vacío")]
        TipoCargaEmpty = 1137,
        [Description("Tipo de Carga Inexistente")]
        TipoCargaNotFound = 1138,
        [Description("Coeficiente de Medio de Pago para Unidad de Negocio Inexistente")]
        UNegMedioPagoNotFound = 1139,
        [Description("Plan Cerrado Vacío")]
        PlanCerradoEmpty = 1140,
        [Description("Plan Cerrado Inexistente")]
        PlanCerradoNotFound = 1141,
        [Description("Tipo Persona Inválida para la Actividad seleccionada")]
        TipoPersonaActividadInvalid = 1142,
        [Description("Cuit no válido para el tipo de persona seleccionado")]
        CuitTipoPersonaInvalid = 1143,
        [Description("Nro. DNI/CUIT Inexistente")]
        DNINotFound = 1144,
        [Description("Se especificó una cantidad de cuotas fuera de parametrización para recargos financieros")]
        RecFinancieroCuotas = 1145,
        [Description("Empresa de Leasing/Prendario inválido para el productor seleccionado.")]
        CotizacionLeasingPrendarioProductorInvalid = 1146,
        [Description("CUIT/CUIL no permitido")]
        CuitNoPermitido = 1147,
        [Description("Condición de Ingresos Brutos Inválido para el tipo de documento.")]
        CondicionIIBBInvTipoDoc = 1148,
        [Description("En el Apellido no corresponde valores numericos.")]
        CotizacionApellidoConsFinalNumericoNotCorresponde = 1149,
        
        [Description("Cobertura padre no ingresada en la cotización.")]
        CotizacionCoberturaPadreNotFound = 1151,

        #endregion

        #region Errores de la Solicitud 5000 - 5999

        [Description("Datos de la Solicitud Inexistente.")]
        DatosSolicitudNotFound = 5000,
        [Description("Nro. de Cotización Inválido.")]
        NroCotizacionNotFound = 5001,
        [Description("Cotización Inexistente.")]
        CotizacionNotFound = 5002,
        [Description("Cotización ya emitida.")]
        CotizacionEmitida = 5003,
        [Description("Nro. de póliza inválido")]
        NroPolizaInvalid = 5004,

        [Description("Sistema inexistente.")]
        SistemaNotFound = 5005,

        [Description("El Servicio de Emisión de Solicitudes no se Encuentra Disponible.")]
        ServicioEmisionNoDisponible = 5006,

        [Description("Cotización Vencida.")]
        CotizacionFechaVtoInvalida = 5007,
        [Description("Fecha de Vigencia Inválida.")]
        SolicitudFechaVigenciaInvalida = 5008,
        [Description("Cantidad de Cuotas Inválida.")]
        CantidadCuotasInvalida = 5009,

        [Description("Forma de Pago Inválido.")]
        FormaPagoInvalid = 5010,
        [Description("Forma de Pago difiere a la de la Cotización.")]
        FormaPagoDiferent = 5011,
        [Description("CBU Inválido.")]
        FormaPagoCBUInvalid = 5012,
        [Description("Nro. Tarjeta Inválido.")]
        FormaPagoNroTarjetaInvalid = 5013,
        [Description("Fecha de Vto. Inválida.")]
        FormaPagoVtoTarjetaInvalid = 5014,
        [Description("CUIT Inválido.")]
        FormaPagoCUITInvalid = 5015,
        [Description("Nro. de Cuenta ICBC Inválido.")]
        FormaPagoNroCuentaInvalid = 5016,
        [Description("Error a Guardar los datos de la Póliza.")]
        PolizaSaveDataError = 5017,
        [Description("Error a Guardar los datos de la Certificado.")]
        CertificadoSaveDataError = 5018,
        [Description("Error al Guardar los datos de la Constancia de Cobertura.")]
        ConstanciaSaveDataError = 5019,
        
        //Datos de Asegurados
        [Description("Apellido del Asegurado Vacío.")]
        AseguradoApellidoNotFound = 5030,
        [Description("Tipo de Documento del Asegurado Vacío.")]
        AseguradoTipoDocumentoNotFound = 5031,
        [Description("Tipo de Documento del Asegurado Inválida.")]
        AseguradoTipoDocumentoInvalid = 5032,
        [Description("Nro. de Documento del Asegurado Vacío.")]
        AseguradoDocumentoNotFound = 5033,
        [Description("Domicilio del Asegurado Vacío.")]
        AseguradoDireccionNotFound = 5034,
        [Description("Domicilio Nro. del Asegurado Vacío.")]
        AseguradoDireccionNroNotFound = 5035,
        [Description("Provincia del Asegurado Vacía.")]
        AseguradoProvinciaNotFound = 5036,
        [Description("La Provincia no esta Habilitada para Emitir.")]
        AseguradoProvinciaNotAvailable = 5037,
        [Description("Provincia del Asegurado Inválida.")]
        AseguradoProvinciaInvalid = 5038,
        [Description("Provincia del Asegurado difiere entre la cotizacion vs emision.")]
        AseguradoProvinciaDifiere = 5173,

        [Description("Localidad del Asegurado Vacía.")]
        AseguradoLocalidadNotFound = 5039,
        [Description("Localidad del Asegurado Inválida.")]
        AseguradoLocalidadInvalid = 5040,
        [Description("Tipo de IVA del Asegurado Vacío.")]
        AseguradoTipoIVANotFound = 5041,
        [Description("Tipo de IVA del Asegurado Inválido.")]
        AseguradoTipoIVAInvalid = 5042,
        [Description("Tipo de IVA del Asegurado Difiere a la de la Cotización.")]
        AseguradoTipoIVADifCotizacion = 5043,
        [Description("Tipo de I.Brutos del Asegurado Vacío.")]
        AseguradoTipoIBNotFound = 5044,
        [Description("Tipo de I.Brutos del Asegurado Inválido.")]
        AseguradoTipoIBInvalid = 5045,
        [Description("Tipo de I.Brutos del Asegurado Difiere a la de la Cotización.")]
        AseguradoTipoIBDifCotizacion = 5046,
        [Description("Tipo Persona Vacío.")]
        TipoPersonaNotFound = 5047,
        [Description("Tipo Persona Inválido.")]
        TipoPersonaInvalid = 5048,
        [Description("Tipo de Documento Inválido según el Tipo Persona.")]
        DocumentoTipoPersonaInvalid = 5049,
        [Description("Género Vacío.")]
        GeneroNotFound = 5050,
        [Description("Género Inválido.")]
        GeneroInvalid = 5051,
        [Description("Algún asegurado es mayor a 65 años.")]
        AseguradoMayor65Invalid = 5052,
        [Description("Solicitud Asegurado inexistente.")]
        AseguradoNotFound = 5053,
        [Description("Estado Civil Vacío.")]
        AseguradoEstadoCivilEmpty = 5054,
        [Description("Estado Civil Inválido.")]
        AseguradoEstadoCivilNotFound = 5055,
        [Description("Sexo del Asegurado Inválido.")]
        AseguradoSexoNotFound = 5056,
        [Description("Sexo del Asegurado difiere de la Cotización.")]
        AseguradoSexoDifCotizacion = 5057,
        [Description("Fecha de Nacimiento del Asegurado Inválido.")]
        AseguradoFechaNacNotFound = 5058,


        //Datos del Tomador
        [Description("Datos del Tomador para el Leasing Vacío.")]
        TomadorNotFound = 5059,
        [Description("Apellido/Razón Social del Tomador Vacío.")]
        TomadorApellidoNotFound = 5060,
        [Description("Tipo de Documento del Tomador Vacío.")]
        TomadorTipoDocumentoNotFound = 5061,
        [Description("Tipo de Documento del Tomador Inválido.")]
        TomadorTipoDocumentoInvalid = 5062,
        [Description("Nro. de Documento del Tomador Vacío.")]
        TomadorDocumentoNotFound = 5063,
        [Description("Estado Civil Vacío.")]
        TomadorEstadoCivilEmpty = 5064,
        [Description("Estado Civil Inválido.")]
        TomadorEstadoCivilNotFound = 5065,
        [Description("Domicilio del Tomador Vacío.")]
        TomadorDireccionNotFound = 5066,
        [Description("Domicilio Nro. del Tomador Vacío.")]
        TomadorDireccionNroNotFound = 5067,
        [Description("Provincia del Tomador Vacía.")]
        TomadorProvinciaNotFound = 5068,
        [Description("Provincia del Tomador Inválida.")]
        TomadorProvinciaInvalid = 5069,
        [Description("Localidad del Tomador Vacía.")]
        TomadorLocalidadNotFound = 5070,
        [Description("Localidad del Tomador Inválida.")]
        TomadorLocalidadInvalid = 5071,
        [Description("Género Vacío.")]
        TomadorSexoNotFound = 5072,
        [Description("Género Inválido.")]
        TomadorSexoInvalid = 5073,

        //Predario
        [Description("Datos de la Prenda Vacío.")]
        PrendaNotFound = 5074,
        [Description("Código de Acreedor Prendario Vacío.")]
        CodAcreedorPrendarioNotFound = 5075,
        [Description("Nombre de Acreedor Prendario Vacío.")]
        NombreAcreedPrendarioNotFound = 5076,
        [Description("Número de Prestamo Prendario Vacío.")]
        NroPrestamoPrendarioNotFound = 5077,
        [Description("Fecha Desde Prestamo Vacío.")]
        FechaDesdePrestamoNotFound = 5078,
        [Description("Fecha Hasta Prestamo Vacío.")]
        FechaHastaPrestamoNotFound = 5079,
        [Description("Cantidad de Cuotas Prestamo Vacío.")]
        CantCuotasPrestamoNotFound = 5080,
        [Description("Código de Sucursal Prestamo Vacío.")]
        CodSucursalPrestamoNotFound = 5081,
        [Description("Fecha Desde Prestamo debe ser menor o igual a la Fecha de Inicio de Vigencia.")]
        FechaDesdePrestamoInvalid = 5082,
        [Description("Cantidad de Cuotas Prestamo Inválida.")]
        CantCuotasPrestamoInvalid = 5083,
        [Description("Fecha Hasta Prestamo debe ser mayor o igual a la Fecha Fin de Vigencia.")]
        FechaHastaPrestamoInvalid = 5110,
        [Description("Fecha Hasta Prestamo debe ser mayor o igual a la Fecha Desde Prestamo.")]
        FechaPrestamoInvalid = 5111,

        //Vehiculo
        [Description("Solicitud Cobertura inexistente.")]
        SolicitudCoberturaNotFound = 5084,
        [Description("Solicitud Vehículo(s) inexistente(s).")]
        SolicitudVehiculosNotFound = 5085,
        [Description("Solicitud Modelo Vehículo inexistente.")]
        SolicitudVehiculoModeloNotFound = 5086,
        [Description("Solicitud Marca Vehículo inexistente.")]
        SolicitudVehiculoMarcaNotFound = 5087,
        [Description("El vehículo ya ha sido asegurado en el dia de la fecha.")]
        VehiculoYaAseguradoToDay = 5171,


        //Grabar Solicitud
        [Description("Error en la Generación de Impuestos.")]
        SolicitudGenerarImpuestoError = 5088,
        [Description("Error al Obtener la Cuotificación del Convenio.")]
        SolicitudCuotificacionError = 5089,
        [Description("Error en la Solicitud al Obtener el Productor.")]
        SolicitudProductorError = 5090,
        [Description("El Productor de la Solicitud es diferente al Productor de la Cotización.")]
        SolicitudCotizacionProductorError = 5091,

        //Planes
        [Description("Solicitud Plan Inexistente")]
        SolicitudPlanNotFound = 5092,
        [Description("Suma Asegurada Inválida")]
        SolicitudSumaAseguradaInvalid = 5093,
        [Description("No se Han Asignado Coberturas para la Solicitud")]
        SolicitudCoberturasNotFound = 5094,
        [Description("Coberturas para la Cotización fuera de Pautas")]
        CotizacionCoberturasOutPautas = 5095,

        //Clausulado
        [Description("Clausula Inexistente")]
        ClausuladoNotFound = 5096,
        [Description("Texto de Cláusula Inexistente")]
        TextoNotFound = 5097,

        //Campañas
        [Description("Campaña inexistente.")]
        CampañaNotFound = 5098,
        [Description("Producto inexistente.")]
        ProductoNotFound = 5099,

        //Generacion de NUPs
        [Description("Error en obtener el Código NUP.")]
        NUPNotFound = 5100,

        [Description("Productor no autorizado para emitir")]
        EmisionProductorNotValid = 5101,

        [Description("Debe Cotizar las Coberturas de Incendio de Edificio, Incendio Contenido y Robo Contenido Genera y La suma asegurada para Incendio de Contenido no puede superar la suma asegurada de Incendio de Edificio")]
        CoberturaIncendioEdificioNotValid = 5102,

        [Description("Error de parametrización. El WebService SOAP no Emite Certificados.")]
        EmisionParametrosError = 5103,

        //Leasing
        [Description("Datos de Leasing vacíos.")]
        LeasingNotFound = 5104,
        [Description("Datos de factura en Leasing vacíos.")]
        LeasingFacturaANombreDeNotFound = 5105,
        [Description("Datos de contrato Leasing vacíos.")]
        NroContratoLeasingNotFound = 5106,

        
        [Description("Sexo del Tomador difiere de la Cotización.")]
        TomadorSexoDifCotizacion = 5107,
        [Description("Asegurado en cotización Inexistente.")]
        CotizacionAseguradoNotFound = 5108,
        [Description("Tomador en cotización Inexistente.")]
        CotizacionTomadorNotFound = 5109,

        [Description("Tipo de IVA del Tomador Vacío.")]
        TomadorTipoIVANotFound = 5110,
        [Description("Tipo de IVA del Tomador Inválido.")]
        TomadorTipoIVAInvalid = 5111,
        [Description("Tipo de IVA del Tomador Difiere a la de la Cotización.")]
        TomadorTipoIVADifCotizacion = 5112,
        [Description("Tipo de I.Brutos del Tomador Vacío.")]
        TomadorTipoIBNotFound = 5113,
        [Description("Tipo de I.Brutos del Tomador Inválido.")]
        TomadorTipoIBInvalid = 5114,
        [Description("Tipo de I.Brutos del Tomador Difiere a la de la Cotización.")]
        TomadorTipoIBDifCotizacion = 5115,

        [Description("Difiere la cantidad de vidas Cotizadas con las personas a Asegurar.")]
        CotizacionAseguradosInvalid = 5116,
        [Description("Difiere la cantidad de vidas Cotizada por Categoria con las personas a Asegurar.")]
        CotizacionAseguradosCategoriaInvalid = 5117,
        [Description("Difiere las Categorías Cotizadas con las Categorías a Asegurar.")]
        CotizacionCategoriaInvalid = 5118,

        [Description("Actividad del Asegurado Vacío.")]
        AseguradoActividadNotFound = 5119,
        [Description("Actividad del Asegurado Inválida.")]
        AseguradoActividadInvalid = 5120,
        [Description("Tipo de Beneficiario del Asegurado Vacío.")]
        AseguradoTipoBeneficiarioNotFound = 5121,
        [Description("Tipo de Beneficiario del Asegurado Inválido.")]
        AseguradoTipoBeneficiarioInvalid = 5122,

        [Description("Apellido del Beneficiario Vacío.")]
        AseguradoApellidoBeneficiarioNotFound = 5123,
        [Description("Tipo de Documento del Beneficiario Vacío.")]
        AseguradoTipoDocumentoBeneficiarioNotFound = 5124,
        [Description("Tipo de Documento del Beneficiario Inválido.")]
        AseguradoTipoDocumentoBeneficiarioInvalid = 5125,
        [Description("Nro. de Documento del Beneficiario Vacío.")]
        AseguradoNroDocumentoBeneficiarioNotFound = 5126,
        [Description("Pocentaje de Beneficiarios Inválido.")]
        AseguradoPorcBeneficiariosInvalid = 5127,
        [Description("Tipo de Beneficiario Inválido.")]
        AseguradoTipoBeneficiariosInvalid = 5128,

        //Clausula de No Repeticion Empresas
        [Description("CUIT de la Empresa Vacío.")]
        ClausulaNoRepeticionCUINotFound = 5129,
        [Description("CUIT de la Empresa Inválido.")]
        ClausulaNoRepeticionCUIInvalid = 5130,
        [Description("Razón Social de la Empresa Vacía.")]
        ClausulaNoRepeticionNombreEmpresaNotFound = 5131,

        [Description("Constancia Inexistente.")]
        ConstanciaNotFound = 5132,
        [Description("Constancia Asegurados Inexistentes.")]
        ConstanciaAseguradosNotFound = 5133,
        [Description("Constancia Beneficiaros Inexistentes.")]
        ConstanciaBeneficiariosNotFound = 5134,
        [Description("Constancia Coberturas Inexistentes.")]
        ConstanciaCoberturasNotFound = 5135,

        //Planes Cerrados
        [Description("Plan Cerrado Inexistentes.")]
        PlanNotFound = 5136,
        [Description("No hay planes multilínea definidos.")]
        PlanMultilineaNotFound = 5137,

        //Solicitud SBS
        [Description("Solicitud Actividad Inexistente")]
        SolicitudActividadNotFound = 5138,
        [Description("Nro. de certificado inválido")]
        NroCertificadoInvalid = 5139,

        [Description("La Cobertura de Incendio de Edificio No existe para la Ubicación de Riesgo Seleccionada")]
        CoberturaIncendioEdificioUbicacionNotValid = 5140,

        [Description("No se ha ingresado suma asegurada de cobertura por Robo Contenido General y no se pueden ingresar coberturas de objetos especificos")]
        CoberturaRoboGeneralVsObjetosEspecificosInvalid = 5141,

        //PACKS
        [Description("No hay packs definidos.")]
        PackNotFound = 5142,
        [Description("Datos del Pack no definitidos.")]
        DatosPackNotFound = 5143,
        [Description("Fecha de Vigencia del Pack Inválida.")]
        DatosPackFechaVigenciaInvalida = 5144,
        [Description("ID del Pack Inválido.")]
        IdPackNotFound = 5145,
        [Description("Pack No Asociado al Productor Seleccionado.")]
        PackProductorInvalid = 5146,
        [Description("Configuración default de packs no definida (Tabla PACK_CONFIG).")]
        PackConfigInnvalid = 5168,

        //Planes Cerrados
        [Description("Id del Plan Cerrado Inexistente.")]
        IdPlanCerradoNotFound = 5147,
        [Description("Nro. de Pack Inexistente.")]
        NroPlanCerradoNotFound = 5148,
        [Description("Primas de Pack Inexistentes.")]
        PrimaCoberturasNotFound = 5149,
        [Description("Coberturas de Pack Inexistentes.")]
        CoberturasPackNotFound = 5150,

        [Description("Póliza Inexistente.")]
        PolizaNotFound = 5151,
        [Description("Solicitud Inexistente.")]
        SolicitudNotFound = 5152,
        [Description("Solicitud Asegurados Inexistentes.")]
        SolicitudAseguradosNotFound = 5153,
        [Description("Solicitud Beneficiaros Inexistentes.")]
        SolicitudBeneficiariosNotFound = 5154,
        [Description("Constancia de Solicitud Inexistentes.")]
        SolicitudConstanciaNotFound = 5155,
        [Description("Error al Guardar los datos de la Solicitud de Cobertura.")]
        SolicitudSaveDataError = 5156,

        [Description("Ramo Invalido.")]
        RamoNotFound = 5174,

        //Lista Amarilla
        [Description("La Emisión no es posible por estar la patente en lista amarilla")]
        PatenteEnListaAmarilla = 5157,
        [Description("La Emisión no es posible por estar el cuil en lista amarilla")]
        CuilEnListaAmarilla = 5158,
        [Description("La Emisión no es posible por estar el cuit en lista amarilla")]
        CuitEnListaAmarilla = 5159,
        [Description("La Emisión no es posible por estar el dni en lista amarilla")]
        DniEnListaAmarilla = 5160,
        [Description("La Emisión no es posible por estar el nro. de chasis en lista amarilla")]
        NroChasisEnListaAmarilla = 5161,
        [Description("La Emisión no es posible por estar el nro. de motor en lista amarilla")]
        NroMotorEnListaAmarilla = 5162,
        [Description("La Emisión no está permitida por el suscriptor")]
        ListaAmarillaNoEmite = 5163,
        [Description("La Emisión queda pendiente a la aprobación del suscriptor")]
        ListaAmarillaEmisionSuspendida = 5164,
        [Description("Tipo de alerta inexistente")]
        TipoAlertaNotFound = 5165,
        [Description("Solicitud en lista amarilla inexistente")]
        SolicitudListaAmarillaNotFound = 5166,

        [Description("La suma asegurada por Robo e Inc.de edificio y Robo  e Inc.en Rep Argentina no debe superar la suma Asegurada por Robo en Contenido")]
        CoberturaRoboGeneralVsObjetosEspecificosXRoboInc= 5167,

        [Description("Configuración Inexistente")]
        ConfigNotFound = 5168,

        #endregion


        #region Errores de la Impresion del Certificado 6000 - 6499

        [Description("Certificado Inexistente")]
        PrintCertificadoNotFound = 6000,
        [Description("Asegurado Inexistente")]
        PrintAseguradoNotFound = 6001,
        [Description("Cobertura Inexistente")]
        PrintCertificadoCoberturaNotFound = 6002,
        [Description("Cobertura Inexistente")]
        PrintCoberturaNotFound = 6003,
        [Description("Vehículo Inexistente")]
        PrintVehiculosNotFound = 6004,
        [Description("Modelo del Vehículo Inexistente")]
        PrintVehiculoModeloNotFound = 6005,
        [Description("Marca del Vehículo Inexistente")]
        PrintVehiculoMarcaNotFound = 6006,

        #endregion

        #region Errores En Archivos 5500 - 5100

        [Description("El archivo Rodamiento no puede superar 1Mb")]
        ErrorTamanoFileMaximo_NroRodamiento = 5500,
        [Description("El archivo Factura de compra no puede superar 1Mb")]
        ErrorTamanoFileMaximo_FacturaCompra = 5501,

        #endregion

        #region Errores de Solicitud de Inspecciones 6500 -  6999

        [Description("No existe el template del Body para EMail de Solicitud de Inspección en Centro.")]
        InspeccionTemplateEnCentroNotFound = 6500,
        [Description("No existe el template del Body para EMail de Solicitud de Inspección en Domicilio.")]
        InspeccionTemplateEnDomicilioNotFound = 6501,
        [Description("No existen resultados cargados para la inspección solicitada.")]
        InspeccionNotFound = 6502,
        [Description("No existe configuración de Inspección para la Clase de Vehículo Seleccionado.")]
        InspeccionClaseVehiculoNotFound = 6503,
        [Description("El Uso de vehículo PARTICULAR no esta permitido para la Clase de Vehículo Seleccionado.")]
        UsoClaseVehiculoInvalid = 6504,
        [Description("No existe el template del Body para EMail de Solicitud de Inspección IOL Mobile.")]
        InspeccionTemplateIOLMobileNotFound = 6505,

        #endregion

        #region Errores de Endosos 7001 -  7003

        [Description("Error al Guardar los datos del Endoso.")]
        EndosoSaveDataError = 7001,
        [Description("No se encuantra el numero de poliza.")]
        PolizaNotFoundEndosos = 7002,
        [Description("No se encuantra el numero de endoso para la poliza.")]
        EndosoNotFoundEndosos = 7003,
        [Description("No se encuantra el nombre  del tomador para la poliza.")]
        NombreyApellidoNotFoundEndosos = 7003,

        #endregion

        #region Lookups

        [Description("El Código de Productor no Existe")]
        ProductorLookupsNotFound = 7000,

        #endregion

        #region Historicos

        [Description("Historico Error en eliminacion Constraint")]
        Historico_ErrorDelConstraint = 9001,

        [Description("Historico Error al renombrar tabla historica to old")]
        Historico_ErrorRenombreHistorico = 9002,

        [Description("Historico Error al revertir renombrar tabla historica")]
        Historico_ErrorRevertirRenombre = 9003,

        [Description("Historico Error al revertir renombrar tabla historica")]
        Historico_CreacionTablaHistorico = 9004,

        [Description("Historico Error al crear Pk tabla historica")]
        Historico_ErrorTablaHistoricoCreacionPK = 9005,

        [Description("Historico Error al realizar migracion tabla historica")]
        Historico_ErrorMigracion = 9006,

        [Description("Historico Error migracion tabla historico old")]
        Historico_ErrorMigracionTablaHistoricoOld = 9007,

        [Description("Historico Error configuracion ParametrosHistocico")]
        Historico_ErrorConfiguracionTablaHistorico = 9008,
 

        #endregion

    }
}
