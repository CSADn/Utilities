using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Helpers
{
    enum TipoTarjetaCredito
    {
        Visa = 1,
        AmericanExpress = 2,
        Mastercard = 3,
        TarjetaNaranja = 4,
        DinersClub = 5
    }

    public static class Validadores
    {
        #region Validar Cuenta Standard Bank (ICBC)

        public static bool ValidarCuenta(this string NroCuenta, string TipoCuenta)
        {
            try
            {
                if (string.IsNullOrEmpty(NroCuenta) || NroCuenta.Length != 14)
                    return false;

                var codCuenta = "";
                switch (TipoCuenta)
                {
                    case "CA":
                    case "CD":
                        codCuenta = "01";
                        break;
                    case "CC":
                    case "DD":
                        codCuenta = "02";
                        break;  
                    default:
                        codCuenta = NroCuenta.Substring(4, 2);
                        break;
                }   

                var aux = 0D;
                var productoCta = int.Parse(codCuenta);
                if (productoCta >= 1 && productoCta <= 9)
                {
                    aux = double.Parse(string.Concat(NroCuenta.Substring(0, 4), NroCuenta.Substring(11, 1), NroCuenta.Substring(5, 1), NroCuenta.Substring(6, 5)));
                    
                }
                else if (productoCta >= 10 && productoCta <= 39 ||
                            productoCta >= 41 && productoCta <= 49 ||
                            productoCta >= 51 && productoCta <= 94)
                {
                    aux = double.Parse(string.Concat(NroCuenta.Substring(0, 4), NroCuenta.Substring(11, 1), NroCuenta.Substring(4, 1), NroCuenta.Substring(5, 1), NroCuenta.Substring(6, 5)));
                }

                var aa = (aux % 97);
                var digito = "";
                if (aa.ToString().Length == 1)
                    digito = string.Concat(aa.ToString(), "0");
                else
                    digito = string.Concat(aa.ToString().Substring(1, 1), aa.ToString().Substring(0, 1));

                if (digito == NroCuenta.Substring(12, 2))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Validar CBU

        public static bool ValidarCBU(this string cbu)
        {
            try
            {
                if (Regex.IsMatch(cbu, "/[0-9]{22}/"))
                    return false;

                var ponderador = "97139713971397139713971397139713";
                var nTotal = 0D;
                var nDigito = 0D;
                var nPond = 0D;

                //Banco
                var cBanco = int.Parse(cbu.Substring(0, 3));
                if (!ExisteBanco(cBanco))
                    return false;

                //Bloque 1
                var sBloque1 = "0" + cbu.Substring(0, 7);
                for (int i = 0; i <= 7; i++)
                {
                    nDigito = int.Parse(sBloque1[i].ToString());
                    nPond = int.Parse(ponderador[i].ToString());
                    nTotal = nTotal + (nPond * nDigito) - ((Math.Floor(nPond * nDigito / 10)) * 10);
                }

                var nDigito1 = 0;
                while (((Math.Floor((nTotal + nDigito1) / 10)) * 10) != (nTotal + nDigito1))
                {
                    nDigito1++;
                }

                if (!cbu.Substring(7, 1).Equals(nDigito1.ToString()))
                    return false;

                //Bloque 2
                var sBloque2 = "000" + cbu.Substring(8, 13);
                nTotal = 0D;
                for (int i = 0; i <= 15; i++)
                {
                    nDigito = int.Parse(sBloque2[i].ToString());
                    nPond = int.Parse(ponderador[i].ToString());
                    nTotal = nTotal + (nPond * nDigito) - ((Math.Floor(nPond * nDigito / 10)) * 10);
                }

                var nDigito2 = 0;
                while (((Math.Floor((nTotal + nDigito2) / 10)) * 10) != (nTotal + nDigito2))
                {
                    nDigito2++;
                }

                if (!cbu.Substring(21, 1).Equals(nDigito2.ToString()))
                    return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Validar Tarjeta de Credito

        public static bool ValidarNroTarjetaNaranja(this string nroTarjeta)
        {
            try
            {
                nroTarjeta = CleanTarjeta(nroTarjeta);

                if (nroTarjeta.Length != 16)
                    return false;

                var multiplicador = "432765432765432";
                var sumatoria = 0D;

                for (int iLoop = 0; iLoop < nroTarjeta.Length - 1; iLoop++)
                    sumatoria += int.Parse(nroTarjeta[iLoop].ToString()) * int.Parse(multiplicador[iLoop].ToString());

                var result = 11 - (sumatoria % 11);

                if (result > 9)
                    result = 0;

                return (result == int.Parse(nroTarjeta[nroTarjeta.Length - 1].ToString()));
            }
            catch
            {
                return false;
            }
        }
        
        public static bool ValidarNroTarjeta(this string nroTarjeta)
        {
            try
            {
                nroTarjeta = CleanTarjeta(nroTarjeta);

                if (!ValidateTarjetaLegth(nroTarjeta))
                    return false;
               
                var TCType = GetTCType(nroTarjeta);

                return ValidateTCType(nroTarjeta, TCType);
            }
            catch
            {
                return false;
            }
        }
        
        public static bool ValidarNroTarjeta(this string nroTarjeta, int tipoTarjeta)
        {
            try
            {
                nroTarjeta = CleanTarjeta(nroTarjeta);

                if (!ValidateTarjetaLegth(nroTarjeta)) 
                    return false;

                var TCType = GetTCType(nroTarjeta);

                if(TCType != tipoTarjeta)
                    return false;

                return ValidateTCType(nroTarjeta, TCType);
            }
            catch
            {
                return false;
            }
        }
        
        #endregion

        #region Validar CUIT

        public static bool ValidaCuit(this string cuit)
        {
            if (cuit == null)
                return false;

            cuit = cuit.Replace("-", string.Empty).Replace(" ", string.Empty);
            if (cuit.Length != 11)
                return false;
            else
            {
                int[] mult = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                char[] nums = cuit.ToCharArray();
                int total = 0;
                for (int i = 0; i < mult.Length; i++)
                {
                    total += int.Parse(nums[i].ToString()) * mult[i];
                }
                var resto = total % 11;

                int calculado = resto == 0 ? 0 : resto == 1 ? 9 : 11 - resto;
                int digito = int.Parse(cuit.Substring(10));
                return calculado == digito;
            }
        }

        #endregion

        #region Validar EMail

        public static bool IsValidEmail(this string strIn)
        {
            var invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper, RegexOptions.None);
            }
            catch (Exception)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Metodos Privados

        private static bool ValidateTarjetaLegth(string nroTarjeta)
        {
            return nroTarjeta.Length.Between(13, 16);
        }

        private static bool ValidateMod10(string tarjeta)
        {
            int suma = 0;
            bool flag = true;

            for (int i = tarjeta.Length - 1; i >= 0; i--)
            {
                if (!flag)
                {
                    int tmp = (tarjeta[i] - '0') << 1;
                    suma += tmp >= 10 ? tmp - 9 : tmp;
                }
                else
                {
                    suma += (tarjeta[i] - '0');
                }
                flag = !flag;
            }
            return suma % 10 == 0;
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                return null;
            }
            return match.Groups[1].Value + domainName;
        }

        private static bool ExisteBanco(int codBanco)
        {
            // https://docs.google.com/spreadsheets/d/1UrK5D64Ye4Agu-SL3jGP83x5D68HPrfckXKyZ3Rnltg/edit?usp=sharing
	        // Fuente: Banco Central Argentino

            var Bancos = new Dictionary<int, string>()
            {
                {5, "A.B.N. AMRO BANK N.V."},
                {7, "BANCO DE GALICIA Y BUENOS AIRES S.A."},
                {11, "BANCO DE LA NACIÓN ARGENTINA"},
                {14, "BANCO DE LA PROVINCIA DE BUENOS AIRES"},
                {15, "STANDARD BANK ARGENTINA S.A."},
                {16, "CITIBANK N.A."},
                {17, "BBVA BANCO FRANCÉS S.A."},
                {18, "THE BANK OF TOKYO - MITSUBISHI UFJ, LTD."},
                {20, "BANCO DE LA PROVINCIA DE CORDOBA S.A."},
                {27, "BANCO SUPERVIELLE S.A."},
                {29, "BANCO DE LA CIUDAD DE BUENOS AIRES"},
                {34, "BANCO PATAGONIA S.A."},
                {44, "BANCO HIPOTECARIO S.A."},
                {45, "BANCO DE SAN JUAN S.A."},
                {46, "BANCO DO BRASIL S.A."},
                {60, "BANCO DEL TUCUMAN S.A."},
                {65, "BANCO MUNICIPAL DE ROSARIO"},
                {72, "BANCO SANTANDER RIO S.A."},
                {79, "BANCO REGIONAL DE CUYO S.A."},
                {83, "BANCO DEL CHUBUT S.A."},
                {86, "BANCO DE SANTA CRUZ S.A."},
                {93, "BANCO DE LA PAMPA SOCIEDAD DE ECONOMÍA MIXTA"},
                {94, "BANCO DE CORRIENTES S.A."},
                {97, "BANCO PROVINCIA DEL NEUQUÉN S.A."},
                {147, "BANCO B. I. CREDITANSTALT S.A."},
                {150, "HSBC BANK ARGENTINA S.A."},
                {165, "J P MORGAN CHASE BANK, NATIONAL ASSOCIATION (SUCURSAL BUENOS AIRES)"},
                {191, "BANCO CREDICOOP COOPERATIVO LIMITADO"},
                {198, "BANCO DE VALORES S.A."},
                {247, "BANCO ROELA S.A."},
                {254, "BANCO MARIVA S.A."},
                {259, "BANCO ITAU BUEN AYRE S.A."},
                {262, "BANK OF AMERICA, NATIONAL ASSOCIATION"},
                {266, "BNP PARIBAS"},
                {268, "BANCO PROVINCIA DE TIERRA DEL FUEGO"},
                {269, "BANCO DE LA REPUBLICA ORIENTAL DEL URUGUAY"},
                {277, "BANCO SAENZ S.A."},
                {281, "BANCO MERIDIAN S.A."},
                {285, "BANCO MACRO S.A."},
                {293, "BANCO MERCURIO S.A."},
                {295, "AMERICAN EXPRESS BANK LTD. S.A."},
                {299, "BANCO COMAFI S.A."},
                {300, "BANCO DE INVERSION Y COMERCIO EXTERIOR S.A."},
                {301, "BANCO PIANO S.A."},
                {303, "BANCO FINANSUR S.A."},
                {305, "BANCO JULIO S.A."},
                {306, "BANCO PRIVADO DE INVERSIONES S.A."},
                {309, "NUEVO BANCO DE LA RIOJA S.A."},
                {310, "BANCO DEL SOL S.A."},
                {311, "NUEVO BANCO DEL CHACO S.A."},
                {312, "M.B.A. BANCO DE INVERSIONES S.A."},
                {315, "BANCO DE FORMOSA S.A."},
                {319, "BANCO CMF S.A."},
                {321, "BANCO DE SANTIAGO DEL ESTERO S.A."},
                {322, "NUEVO BANCO INDUSTRIAL DE AZUL S.A."},
                {325, "DEUTSCHE BANK S.A."},
                {330, "NUEVO BANCO DE SANTA FE S.A."},
                {331, "BANCO CETELEM ARGENTINA S.A."},
                {332, "BANCO DE SERVICIOS FINANCIEROS S.A."},
                {335, "BANCO COFIDIS S.A."},
                {336, "BANCO BRADESCO ARGENTINA S.A."},
                {338, "BANCO DE SERVICIOS Y TRANSACCIONES S.A."},
                {339, "RCI BANQUE"},
                {340, "BACS BANCO DE CREDITO Y SECURITIZACION S.A."},
                {341, "BANCO MASVENTAS S.A."},
                {386, "NUEVO BANCO DE ENTRE RIOS S.A."},
                {388, "NUEVO BANCO BISEL S.A."},
                {389, "BANCO COLUMBIA S.A."}
            };

            return Bancos.ContainsKey(codBanco);

        }
        
        private static string CleanTarjeta(string nroTarjeta)
        {
            return nroTarjeta.Replace("-", string.Empty).Replace(" ", string.Empty);

        }
        
        private static bool ValidateTCType(string nroTarjeta, int TCType)
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

        private static int GetTCType(string nroTarjeta)
        {
            var TCType = 0;
            if (int.Parse(nroTarjeta.Substring(0, 1)) == 4)
            {
                TCType = (int)TipoTarjetaCredito.Visa;
            }
            else if (int.Parse(nroTarjeta.Substring(0, 2)) >= 51 && int.Parse(nroTarjeta.Substring(0, 2)) <= 55)
            {
                TCType = (int)TipoTarjetaCredito.Mastercard;
            }
            else if (int.Parse(nroTarjeta.Substring(0, 2)) == 34 || int.Parse(nroTarjeta.Substring(0, 2)) == 37)
            {
                TCType = (int)TipoTarjetaCredito.AmericanExpress;
            }
            else if (int.Parse(nroTarjeta.Substring(0, 3)) >= 300 && int.Parse(nroTarjeta.Substring(0, 3)) <= 305 || int.Parse(nroTarjeta.Substring(0, 2)) == 36 || int.Parse(nroTarjeta.Substring(0, 2)) == 38)
            {
                TCType = (int)TipoTarjetaCredito.DinersClub;
            }
            return TCType;
        }

        #endregion
    }
}
