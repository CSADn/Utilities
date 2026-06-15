using CreditCardValidation;
using Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Helpers
{
    public sealed class ValidadorTarjetas
    {
        #region Delegados

        private delegate bool ValidateTarjetaHandler(string nroTarjeta, TipoTarjetaCredito tipo, out string message);
        
        #endregion

        #region Atributos privados

        private NLog.Logger _logger = null;
        private Dictionary<TipoTarjetaCredito, ValidateTarjetaHandler> _dicValidacion = null;
        private Dictionary<TipoTarjetaCredito, string> _dicTarjetaRegex = null;
        private List<TipoTarjetaCredito> _vendors = null;
        
        #endregion

        #region Constructor

        public ValidadorTarjetas()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _dicValidacion = new Dictionary<TipoTarjetaCredito, ValidateTarjetaHandler>()
            {
                { TipoTarjetaCredito.Visa, ValidateVisa },
                { TipoTarjetaCredito.AmericanExpress, ValidateAmex },
                { TipoTarjetaCredito.Mastercard, ValidateMastercard },
                { TipoTarjetaCredito.TarjetaNaranja, ValidateNaranja },
                { TipoTarjetaCredito.DinersClub, ValidateDiners },
                { TipoTarjetaCredito.Cabal, ValidateCabal },
                { TipoTarjetaCredito.Carta, ValidateCarta },
                { TipoTarjetaCredito.Cencosud, ValidateCencosud },
                { TipoTarjetaCredito.Cliper, ValidateCliper },
                { TipoTarjetaCredito.Credencial, ValidateCredencial },
                { TipoTarjetaCredito.Discover, ValidateDiscover },
                { TipoTarjetaCredito.Elebar, ValidateElebar },
                { TipoTarjetaCredito.Enroute, ValidateEnroute },
                { TipoTarjetaCredito.FalaClassic, ValidateFalaClassic },
                { TipoTarjetaCredito.FalaMaster, ValidateFalaMaster },
                { TipoTarjetaCredito.Favacard, ValidateFavacard },
                { TipoTarjetaCredito.Italcred, ValidateItalcred },
                { TipoTarjetaCredito.Jcb, ValidateJcb },
                { TipoTarjetaCredito.Mas, ValidateMas },
                { TipoTarjetaCredito.NativaV, ValidateNativaVisa },
                { TipoTarjetaCredito.NativaM, ValidateNativaMaster},
                { TipoTarjetaCredito.Nevada, ValidateNevada },
                { TipoTarjetaCredito.Proven, ValidateProven}
            };

            _dicTarjetaRegex = new Dictionary<TipoTarjetaCredito, string>()
            {
                { TipoTarjetaCredito.Visa, "^4[0-9]{12}(?:[0-9]{3})?$" },
                { TipoTarjetaCredito.AmericanExpress, "^3[47][0-9]{13}$" },
                { TipoTarjetaCredito.Mastercard, "^((52213[57]|501105)[0-9]{10}|5[1-5][0-9]{14})$" },
                { TipoTarjetaCredito.TarjetaNaranja, "^((41176[3-5]|40291[78]|589562)[0-9]{10}|5275[0-9]{12})$" },
                { TipoTarjetaCredito.DinersClub, "^3(?:0[0-5]|[68][0-9])[0-9]{11}$" },
                { TipoTarjetaCredito.Cabal, "^(589657|627170|(604(2[0-9][1-9]|3[0-9]{2}|400)))[0-4][0-9]{9}$" },
                { TipoTarjetaCredito.Carta, "^504569[0-9]{10}$" },
                { TipoTarjetaCredito.Cencosud, "^(559(198|137)|557935)[0-9]{10}$" },
                { TipoTarjetaCredito.Cliper, "^589955[0-9]{12}$" },
                { TipoTarjetaCredito.Credencial, "^5070[0-9]{12}$" },
                { TipoTarjetaCredito.Discover, "^6(?:011|5[0-9]{2})[0-9]{12}$" },
                { TipoTarjetaCredito.Elebar, "^504[7]{3}[0-9]{10}$" },
                { TipoTarjetaCredito.Enroute, "^(2014|2149)[0-9]{11}$" },
                { TipoTarjetaCredito.FalaClassic, "^62718050[0-9]{8}$" },
                { TipoTarjetaCredito.FalaMaster, "^557039[0-9]{10}$" },
                { TipoTarjetaCredito.Favacard, "^504408[0-9]{12}$" },
                { TipoTarjetaCredito.Italcred, "^504338[0-9]{10}$" },
                { TipoTarjetaCredito.Jcb, "^(?:2131|1800|35\\d{3})\\d{11}$" },
                { TipoTarjetaCredito.Mas, "^603493[0-9]{10}$" },
                { TipoTarjetaCredito.NativaV, "^487017[0-9]{10}$" },
                { TipoTarjetaCredito.NativaM, "^546553[0-9]{10}$" },
                { TipoTarjetaCredito.Nevada, "^(504363|50431[0-2])[0-9]{10}$" },
                { TipoTarjetaCredito.Proven, "^16[0-9]{11}$"}
            };

            _vendors = new List<TipoTarjetaCredito>
            {
                TipoTarjetaCredito.AmericanExpress,
                TipoTarjetaCredito.Visa,
                TipoTarjetaCredito.Mastercard,
                TipoTarjetaCredito.Cabal,
                TipoTarjetaCredito.DinersClub
            };
        }
              
        #endregion

        #region Metodos de validacion de tarjetas

        private bool ValidateCliper(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateAlgorithmCliper(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;  
        }

        private bool ValidateProven(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            return valid;
        }

        private bool ValidateNevada(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod11(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateNativaVisa(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            List<string> firstNativaDigists = new List<string>
            {
                "487017"
            };

            valid &= firstNativaDigists.Any(a => nroTarjeta.Substring(0, 6) == a);

            if (!valid)
            {
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";
            }
            else
            { 
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (!ValidateMod10(nroTarjeta))
                {
                    valid &= false;
                    message = "Dígito de control no válido";
                }
            }

            return valid;
        }

        private bool ValidateNativaMaster(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            List<string> firstNativaDigists = new List<string>
            {
                "546553"
            };

            valid &= firstNativaDigists.Any(a => nroTarjeta.Substring(0, 6) == a);

            if (!valid)
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";
            else
            {
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (!ValidateMod10(nroTarjeta))
                {
                    valid &= false;
                    message = "Dígito de control no válido";
                }
            }

            return valid;
        }

        private bool ValidateMas(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateJcb(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateItalcred(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateFavacard(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateFalaClassic(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            //Falabella: 557039/62718050 
            //Del ultimo solo tomo 6 que es el bin code
            List<string> falaFirstDigists = new List<string>
            {
                "627180"
            };

            valid &= falaFirstDigists.Any(a => nroTarjeta.Substring(0, 6) == a);

            if (!valid)
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";
            else
            {
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (!ValidateMod10(nroTarjeta))
                {
                    valid &= false;
                    message = "Dígito de control no válido";
                }
            }

            return valid;
        }

        private bool ValidateFalaMaster(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            //Falabella: 557039/62718050 
            //Del ultimo solo tomo 6 que es el bin code
            List<string> falaFirstDigists = new List<string>
            {
                "557039"
            };

            valid &= falaFirstDigists.Any(a => nroTarjeta.Substring(0, 6) == a);

            if (!valid)
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";
            else
            {
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (!ValidateMod10(nroTarjeta))
                {
                    valid &= false;
                    message = "Dígito de control no válido";
                }
            }

            return valid;
        }

        private bool ValidateEnroute(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateElebar(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateDiscover(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateCredencial(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateCencosud(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            //559198;557935;559137
            List<string> firstCencoDigits = new List<string>
            {
                "559198",
                "557935",
                "559137"
            };

            valid &= firstCencoDigits.Any(a => nroTarjeta.Substring(0, 6) == a);

            if (!valid)
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";
            else
            {
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (!ValidateMod10(nroTarjeta))
                {
                    valid &= false;
                    message = "Dígito de control no válido";
                }
            }

            return valid;
        }

        private bool ValidateCarta(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            return valid;
        }

        private bool ValidateCabal(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            List<string> firstCabalDigits = new List<string>
            {
                "589657",
                "627170"
            };

            valid &= firstCabalDigits.Any(a => nroTarjeta.Substring(0, 6) == a) || Convert.ToInt32(nroTarjeta.Substring(0, 6)).Between(604201, 604400);

            if (valid)
            {
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (valid)
                {
                    if (nroTarjeta.Substring(0, 6) == "627170")
                    {
                        if (!ValidateMod11(nroTarjeta))
                        {
                            valid &= false;
                            message = "Dígito de control no válido";
                        }
                    }
                    else
                    {
                        if (!ValidateMod21(nroTarjeta))
                        {
                            valid &= false;
                            message = "Dígito de control no válido";
                        }
                    }
                }
            }
            else
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";

            return valid;
        }

        private bool ValidateDiners(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateNaranja(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            //BINS Naranja
            //411763; 411764; 411765; 402917; 402918; 5275; 589562
            List<string> firstNaranjaDigits = new List<string>
            {
                "411763",
                "411764",
                "411765",
                "402917",
                "402918",
                "5275",
                "589562"
            };

            valid &= firstNaranjaDigits.Any(a => nroTarjeta.Substring(0, 6).StartsWith(a));
            if(valid)
            {
                if (!ValidateRegex(nroTarjeta, tipo))
                {
                    valid &= false;
                    message = "Nro de tarjeta no válido";
                }

                if (valid)
                {
                    if (nroTarjeta.Substring(0, 6) == "589562")
                    {
                        if (!ValidateMod11(nroTarjeta))
                        {
                            valid &= false;
                            message = "Dígito de control no válido";
                        }
                    }
                    else
                    {
                        if (!ValidateMod21(nroTarjeta))
                        {
                            valid &= false;
                            message = "Dígito de control no válido";
                        }
                    }
                }
            }
            else
                message = "El BIN de la tarjeta no correspomde con el tipo elegido.";

            return valid;
        }

        private bool ValidateMastercard(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateVisa(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            if (!ValidateMod10(nroTarjeta))
            {
                valid &= false;
                message = "Dígito de control no válido";
            }

            return valid;
        }

        private bool ValidateAmex(string nroTarjeta, TipoTarjetaCredito tipo, out string message)
        {
            bool valid = true;
            message = string.Empty;

            if (!ValidateRegex(nroTarjeta, tipo))
            {
                valid &= false;
                message = "Nro de tarjeta no válido";
            }

            //if (!ValidateMod10(nroTarjeta))
            //{
            //    valid &= false;
            //    message = "Dígito de control no válido";
            //}

            return valid;
        }

        private bool ValidateRegex(string nroTarjeta, TipoTarjetaCredito tipo)
        {
            try
            {
                return Regex.IsMatch(nroTarjeta, _dicTarjetaRegex[tipo]);
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, $"Error en validación de expresión regular para tarjeta: {tipo.Description()}\nExcepción:\n");
                _logger.Log(NLog.LogLevel.Error, ex);
                _logger.Log(NLog.LogLevel.Error, $"RegEx: {_dicTarjetaRegex[tipo]}");
                _logger.Log(NLog.LogLevel.Error, $"Tarjeta: {nroTarjeta}");

                return false;
            }
        }

        #endregion

        #region Metodos privados

        private bool ValidateTarjetaLegth(string nroTarjeta)
        {
            return nroTarjeta.Length.Between(13, 16);
        }

        private bool ValidateMod10(string tarjeta)
        {
            int suma = 0;
            char[] arr = tarjeta.Substring(0, tarjeta.Length - 1).ToCharArray();
            Array.Reverse(arr);
            string reverse = new string(arr);

            for (int i = 0; i < reverse.Length; i++)
            {
                int value = Convert.ToInt32(char.GetNumericValue(tarjeta[i]));

                if (i % 2 == 0)
                {
                    int tmp = value * 2;
                    suma += tmp >= 10 ? tmp - 9 : tmp;
                }
                else
                {
                    suma += value;
                }
            }

            return (suma * 9) % 10 == char.GetNumericValue(tarjeta[tarjeta.Length - 1]); //suma % 10 == 0;
        }

        private bool ValidateMod11(string tarjeta)
        {
            int[] multiplicador = new int[] { 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

            var sumatoria = 0D;

            for (int iLoop = 0; iLoop < tarjeta.Length - 1; iLoop++)
                sumatoria += char.GetNumericValue(tarjeta[iLoop]) * multiplicador[iLoop];

            var result = 11 - (sumatoria % 11);
            
            return (result > 9 ? 0 : result) == char.GetNumericValue(tarjeta[tarjeta.Length - 1]);
        }

        private bool ValidateMod21(string tarjeta)
        {
            int suma = 0;
   
            for (int i = tarjeta.Length - 2; i >= 0; i--)
            {
                int value = Convert.ToInt32(char.GetNumericValue(tarjeta[i]));

                if (i % 2 == 0)
                {
                    int tmp =  value * 2;
                    suma += tmp >= 10 ? tmp - 9 : tmp;
                }
                else
                    suma += value;
            }
            
            int result = ((Convert.ToInt32((suma / 10)) + 1) * 10) - suma;

            return (result == 10 ? 0 : result) == char.GetNumericValue(tarjeta[tarjeta.Length - 1]);
        }

        private bool ValidateAlgorithmCliper(string tarjeta)
        {
            string cliper = tarjeta.Substring(6, 12);
            double suma = 0;

            int[] multiplier = new int[] { 7, 1, 4, 6, 6, 7, 1, 4, 6, 6, 7 };

            for (int i = 0; i < cliper.Length - 1; i++)
            {
                double value = char.GetNumericValue(cliper[i]) * multiplier[i];
                suma += value > 9 ? value % 10 : value;
            }

            var result = (suma > 9 ? suma % 10 : suma) - 10;

            return (result == -10 ? 0 : Math.Abs(result)) == char.GetNumericValue(cliper[cliper.Length - 1]);

        }

        private string CleanTarjeta(string nroTarjeta)
        {
            return nroTarjeta.Replace("-", string.Empty).Replace(" ", string.Empty);
        }

        private bool ValidateTCType(string nroTarjeta, int TCType)
        {
            switch (TCType)
            {
                case (int)TipoTarjetaCredito.Visa:
                    return ((nroTarjeta.Length == 13 || nroTarjeta.Length == 16) && ValidateMod10(nroTarjeta));
                case (int)TipoTarjetaCredito.AmericanExpress:
                    return (nroTarjeta.Length == 15 && ValidateMod10(nroTarjeta));
                case (int)TipoTarjetaCredito.DinersClub:
                    return (nroTarjeta.Length == 14 && ValidateMod10(nroTarjeta));
                case (int)TipoTarjetaCredito.Mastercard:
                    return (nroTarjeta.Length == 16 && ValidateMod10(nroTarjeta));
                default:
                    return false;
            }
        }

        private List<TipoTarjetaCredito> GetTCType(string nroTarjeta)
        {   
            List<KeyValuePair<TipoTarjetaCredito, string>> values = _dicTarjetaRegex.ToList();
            List<TipoTarjetaCredito> retList = new List<TipoTarjetaCredito>();

            foreach (var value in values)
            {
                if (Regex.IsMatch(nroTarjeta, value.Value))
                    retList.Add(value.Key);
            }

            return retList;
        }

        /// <summary>
        /// Validar bin_code con un servidor
        /// </summary>
        /// <param name="binCode"></param>
        /// <param name="insert"></param>
        /// <returns></returns>
        private ValidacionBinTarjeta ValidateTipoTarjetaToBinList(string binCode, bool insert = false)
        {
            string binListUrl = @"http://www.binlist.net/json/";

            string response = Utilities.MakeRequest(binListUrl + binCode);

            if (string.IsNullOrWhiteSpace(response) || response.ToLowerInvariant().Contains("El WebService no respondió correctamente".ToLowerInvariant()))
                return new ValidacionBinTarjeta
                {
                    TipoTarjeta = CardType.Desconocido,
                    Valid = true,
                    Mensaje = "La tarjeta ingresada no pudo ser identificada"
                };

            TarjetaJson tarjeta = JsonConvert.DeserializeObject<TarjetaJson>(response);

            if (insert)
            {
                //Guardar en la base si fue encontrado
                if (tarjeta != null)
                {
                    Bin_Codes bc = new Bin_Codes
                    {
                        BinCode = tarjeta.bin,
                        Brand = tarjeta.brand,
                        CardSubBrand = tarjeta.sub_brand,
                        CountryCode = tarjeta.country_code,
                        Country = tarjeta.country_name,
                        Bank = tarjeta.bank,
                        CardType = tarjeta.card_type,
                        CardCategory = tarjeta.card_category,

                    };
                    try
                    {
                        string pk = Crud.Bin_Codes.Instance.Add(bc);
                        //TODO:var si se lanza exception
                    }
                    catch (Exception)
                    {
                        throw new Exception("No pudo darse de alta el código bin de la tarjeta");
                    }
                }
            }

            bool valid = tarjeta != null && tarjeta.card_type != CardType.Debito.Description();

            return new ValidacionBinTarjeta
            {
                TipoTarjeta = tarjeta != null ? (tarjeta.card_type == CardType.Debito.Description() ? CardType.Debito : CardType.Credito) : CardType.Desconocido,
                Valid = valid,
                Mensaje = valid ? null : "La tarjeta ingresada solo puede ser de crédito"
            };

        }

        #endregion

        #region Metodos Publicos

        public ValidacionBinTarjeta ValidarBinCode(string binCode)
        {
            bool valOnline = "OnlineTCValidation".FromAppSettings(false);

            Bin_Codes bc = Crud.Bin_Codes.Instance.GetByCode(binCode);

            if (bc != null) //Valido el tipo
            {
                if (bc.CardType != CardType.Debito.Description())
                    return new ValidacionBinTarjeta
                    {
                        TipoTarjeta = CardType.Credito,
                        Valid = true,
                        Mensaje = null
                    };
                else
                {
                    if (valOnline)
                        return ValidateTipoTarjetaToBinList(binCode);
                    else
                    { 
                        return new ValidacionBinTarjeta
                        {
                            TipoTarjeta = bc.CardType == CardType.Debito.Description() ? CardType.Debito : CardType.Desconocido,
                            Valid = bc.CardType != CardType.Debito.Description(),
                            Mensaje = bc.CardType == CardType.Debito.Description() ? "La tarjeta ingresada solo puede ser de crédito" : null
                        };
                    }
                }
            }
            else
            {
                if (valOnline)
                    return ValidateTipoTarjetaToBinList(binCode, true);
                else
                    return new ValidacionBinTarjeta
                    {
                        TipoTarjeta = CardType.Desconocido,
                        Valid = true,
                        Mensaje = null
                    };
            }
        }

        public bool ValidarNroTarjeta(string nroTarjeta)
        {
            try
            {
                nroTarjeta = CleanTarjeta(nroTarjeta);

                var TCType = GetTCType(nroTarjeta);

                bool result = false;
                string message = string.Empty;

                foreach (var tipo in TCType)
                {
                    if (_dicValidacion[tipo].Invoke(nroTarjeta, tipo, out message))
                    {
                        result = true;
                        break;
                    }
                }

                if(!result)
                    _logger.Log(NLog.LogLevel.Error, message);

                return result;
            }
            catch(Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, ex, "Tarjeta no válida");
                return false;
            }
        }

        public ValidacionNumeroTarjeta ValidarNroTarjeta(string nroTarjeta, int tipoTarjeta)
        {
            ValidacionNumeroTarjeta validacion = new ValidacionNumeroTarjeta(tipoTarjeta);

            try
            {
                nroTarjeta = CleanTarjeta(nroTarjeta);

                var TCType = GetTCType(nroTarjeta);

                if (TCType.Count == 0)
                    validacion.Mensaje = "El número de tarjera no pudo ser identificado";
                else
                {
                    if (!TCType.Any(a => (TipoTarjetaCredito)tipoTarjeta == a) && TCType.Count >= 1)
                    {
                        validacion.Mensaje = "El número de tarjeta ingresado no coincide con la tarjeta seleccionada";
                        validacion.TipoTarjetaIdentificada = TCType.FirstOrDefault();
                        validacion.TipoTarjetaSeleccionada = (TipoTarjetaCredito)tipoTarjeta;
                    }

                    if (TCType.Any(a => (TipoTarjetaCredito)tipoTarjeta == a) && TCType.Count > 1)
                    {
                        if (_vendors.Any(a => a.GetHashCode() == tipoTarjeta))
                        {
                            validacion.Mensaje = "El número de tarjeta ingresado no coincide con la tarjeta seleccionada";
                            validacion.TipoTarjetaSeleccionada = (TipoTarjetaCredito)tipoTarjeta;
                            validacion.TipoTarjetaIdentificada = TCType.FirstOrDefault(f => f != (TipoTarjetaCredito)tipoTarjeta);
                            validacion.Valida = false;
                        }
                        else
                        {
                            string mensaje = "El número de tarjeta ingresado coincide con varios tipos de tarjetas";

                            _logger.Log(NLog.LogLevel.Warn, mensaje);
                            _logger.Log(NLog.LogLevel.Warn, $"BIN: {nroTarjeta.Substring(0, 6)}");
                            _logger.Log(NLog.LogLevel.Warn, $"Tarjeta Ingresada: {((TipoTarjetaCredito)tipoTarjeta).Description()}");
                            _logger.Log(NLog.LogLevel.Warn, "Tarjetas Reconocidas\n");
                            TCType.ForEach(f => _logger.Log(NLog.LogLevel.Warn, $"Tarjeta: {f.Description()}"));

                            validacion.Valida = _dicValidacion[(TipoTarjetaCredito)tipoTarjeta].Invoke(nroTarjeta, (TipoTarjetaCredito)tipoTarjeta, out mensaje);
                            validacion.TipoTarjetaSeleccionada = (TipoTarjetaCredito)tipoTarjeta;
                            validacion.TipoTarjetaIdentificada = (TipoTarjetaCredito)tipoTarjeta;
                            validacion.Mensaje = mensaje;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(validacion.Mensaje))
                {
                    string message;
                    foreach (var tipo in TCType)
                    {
                        if (_dicValidacion[tipo].Invoke(nroTarjeta, tipo, out message) && tipo == (TipoTarjetaCredito)tipoTarjeta)
                        {
                            validacion.TipoTarjetaIdentificada = tipo;
                            validacion.Valida = true;
                            break;
                        }
                        else
                        {
                            validacion.TipoTarjetaIdentificada = tipo;
                            validacion.Mensaje = message;
                        }
                    }
                }

                return validacion;
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "Error al validar la tarjeta");
                _logger.Log(NLog.LogLevel.Error, ex);

                validacion.Mensaje = "Error al validar la tarjeta";

                return validacion;
            }
        }

        public ValidacionVtoTarjeta ValidarVtoTarjeta(string vto)
        {
            ValidacionVtoTarjeta vvt = new ValidacionVtoTarjeta();
            
            vvt.Valid = false;

            if (string.IsNullOrWhiteSpace(vto))
            {
                vvt.Message = "La fecha de vencimiento está vacia.";
                return vvt;
            }

            if (vto.Length != 7)
            {
                vvt.Message = "La longitud de la fecha de vencimiento no es correcta.";
                return vvt;
            }

            if (vto.IndexOf('/') == -1)
            {
                vvt.Message = "El Formato de la fecha de vencimiento no es correcto. Use MM/yyyy";
                return vvt;
            }

            try
            {
                DateTime result = DateTime.MinValue;

                if (!DateTime.TryParseExact(vto, "MM/yyyy", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out result))
                {
                    vvt.Message = "Fecha de vencimiento no válida.";
                    return vvt;
                }

                if (result == DateTime.MinValue || result == DateTime.MaxValue)
                {
                    vvt.Message = "Fecha de vencimiento no válida.";
                    return vvt;
                }

                if (result.Date > DateTime.Today.AddYears(10))
                {
                    vvt.Message = "La Fecha de vencimiento excede del máximo permitido.";
                    return vvt;
                }

                if (result.Date.AddMonths(1).AddDays(-1) < DateTime.Today)
                {
                    vvt.Message = "La Fecha de vencimiento está expirada.";
                    return vvt;
                }

                vvt.Message = "Fecha Correcta";
                vvt.Valid = true;
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "Se ha producido un error en la validación de la fecha de vto. de la tarjeta.");
                _logger.Log(NLog.LogLevel.Error, ex);

                if (!string.IsNullOrWhiteSpace(vvt.Message))
                    _logger.Log(NLog.LogLevel.Error, vvt.Message);

                vvt.Message = vvt?.Message + ex.ToString();
                vvt.Valid = false;
            }

            return vvt;
        }
        
        #endregion
    }
}
