using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CBUGen
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            var bancos = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(7, "BANCO DE GALICIA Y BUENOS AIRES S.A."),
                new KeyValuePair<int, string>(11, "BANCO DE LA NACION ARGENTINA"),
                new KeyValuePair<int, string>(14, "BANCO DE LA PROVINCIA DE BUENOS AIRES"),
                new KeyValuePair<int, string>(15, "INDUSTRIAL AND COMMERCIAL BANK OF CHINA"),
                new KeyValuePair<int, string>(16, "CITIBANK N.A."),
                new KeyValuePair<int, string>(17, "BBVA BANCO FRANCES S.A."),
                new KeyValuePair<int, string>(18, "THE BANK OF TOKYO-MITSUBISHI UFJ, LTD."),
                new KeyValuePair<int, string>(20, "BANCO DE LA PROVINCIA DE CORDOBA S.A."),
                new KeyValuePair<int, string>(27, "BANCO SUPERVIELLE S.A."),
                new KeyValuePair<int, string>(29, "BANCO DE LA CIUDAD DE BUENOS AIRES"),
                new KeyValuePair<int, string>(34, "BANCO PATAGONIA S.A."),
                new KeyValuePair<int, string>(44, "BANCO HIPOTECARIO S.A."),
                new KeyValuePair<int, string>(45, "BANCO DE SAN JUAN S.A."),
                new KeyValuePair<int, string>(60, "BANCO DEL TUCUMAN S.A."),
                new KeyValuePair<int, string>(65, "BANCO MUNICIPAL DE ROSARIO"),
                new KeyValuePair<int, string>(72, "BANCO SANTANDER RIO S.A."),
                new KeyValuePair<int, string>(83, "BANCO DEL CHUBUT S.A."),
                new KeyValuePair<int, string>(86, "BANCO DE SANTA CRUZ S.A."),
                new KeyValuePair<int, string>(93, "BANCO DE LA PAMPA SOCIEDAD DE ECONOMÍA M"),
                new KeyValuePair<int, string>(94, "BANCO DE CORRIENTES S.A."),
                new KeyValuePair<int, string>(97, "BANCO PROVINCIA DEL NEUQUÉN SOCIEDAD ANÓ"),
                new KeyValuePair<int, string>(147, "BANCO INTERFINANZAS S.A."),
                new KeyValuePair<int, string>(150, "HSBC BANK ARGENTINA S.A."),
                new KeyValuePair<int, string>(165, "JPMORGAN CHASE BANK, NATIONAL ASSOCIATIO"),
                new KeyValuePair<int, string>(191, "BANCO CREDICOOP COOPERATIVO LIMITADO"),
                new KeyValuePair<int, string>(198, "BANCO DE VALORES S.A."),
                new KeyValuePair<int, string>(247, "BANCO ROELA S.A."),
                new KeyValuePair<int, string>(254, "BANCO MARIVA S.A."),
                new KeyValuePair<int, string>(259, "BANCO ITAU ARGENTINA S.A."),
                new KeyValuePair<int, string>(262, "BANK OF AMERICA, NATIONAL ASSOCIATION"),
                new KeyValuePair<int, string>(266, "BNP PARIBAS"),
                new KeyValuePair<int, string>(268, "BANCO PROVINCIA DE TIERRA DEL FUEGO"),
                new KeyValuePair<int, string>(269, "BANCO DE LA REPUBLICA ORIENTAL DEL URUGU"),
                new KeyValuePair<int, string>(277, "BANCO SAENZ S.A."),
                new KeyValuePair<int, string>(281, "BANCO MERIDIAN S.A."),
                new KeyValuePair<int, string>(285, "BANCO MACRO S.A."),
                new KeyValuePair<int, string>(299, "BANCO COMAFI SOCIEDAD ANONIMA"),
                new KeyValuePair<int, string>(300, "BANCO DE INVERSION Y COMERCIO EXTERIOR S"),
                new KeyValuePair<int, string>(301, "BANCO PIANO S.A."),
                new KeyValuePair<int, string>(303, "BANCO FINANSUR S.A."),
                new KeyValuePair<int, string>(305, "BANCO JULIO SOCIEDAD ANONIMA"),
                new KeyValuePair<int, string>(309, "BANCO RIOJA SOCIEDAD ANONIMA UNIPERSONAL"),
                new KeyValuePair<int, string>(310, "BANCO DEL SOL S.A."),
                new KeyValuePair<int, string>(311, "NUEVO BANCO DEL CHACO S. A."),
                new KeyValuePair<int, string>(312, "BANCO VOII S.A."),
                new KeyValuePair<int, string>(315, "BANCO DE FORMOSA S.A."),
                new KeyValuePair<int, string>(319, "BANCO CMF S.A."),
                new KeyValuePair<int, string>(321, "BANCO DE SANTIAGO DEL ESTERO S.A."),
                new KeyValuePair<int, string>(322, "BANCO INDUSTRIAL S.A."),
                new KeyValuePair<int, string>(325, "DEUTSCHE BANK S.A."),
                new KeyValuePair<int, string>(330, "NUEVO BANCO DE SANTA FE SOCIEDAD ANONIMA"),
                new KeyValuePair<int, string>(331, "BANCO CETELEM ARGENTINA S.A."),
                new KeyValuePair<int, string>(332, "BANCO DE SERVICIOS FINANCIEROS S.A."),
                new KeyValuePair<int, string>(336, "BANCO BRADESCO ARGENTINA S.A."),
                new KeyValuePair<int, string>(338, "BANCO DE SERVICIOS Y TRANSACCIONES S.A."),
                new KeyValuePair<int, string>(339, "RCI BANQUE S.A."),
                new KeyValuePair<int, string>(340, "BACS BANCO DE CREDITO Y SECURITIZACION S"),
                new KeyValuePair<int, string>(341, "BANCO MASVENTAS S.A."),
                new KeyValuePair<int, string>(386, "NUEVO BANCO DE ENTRE RÍOS S.A."),
                new KeyValuePair<int, string>(389, "BANCO COLUMBIA S.A."),
                new KeyValuePair<int, string>(426, "BANCO BICA S.A."),
                new KeyValuePair<int, string>(431, "BANCO COINAG S.A."),
                new KeyValuePair<int, string>(432, "BANCO DE COMERCIO S.A."),
                new KeyValuePair<int, string>(44059, "FORD CREDIT COMPAÑIA FINANCIERA S.A."),
                new KeyValuePair<int, string>(44077, "COMPAÑIA FINANCIERA ARGENTINA S.A."),
                new KeyValuePair<int, string>(44088, "VOLKWAGEN FINANCIAL SERVICES CIA.FIN.S.A"),
                new KeyValuePair<int, string>(44090, "CORDIAL COMPAÑÍA FINANCIERA S.A."),
                new KeyValuePair<int, string>(44092, "FCA COMPAÑIA FINANCIERA S.A."),
                new KeyValuePair<int, string>(44093, "GPAT COMPAÑIA FINANCIERA S.A."),
                new KeyValuePair<int, string>(44094, "MERCEDES-BENZ COMPAÑÍA FINANCIERA ARGENT"),
                new KeyValuePair<int, string>(44095, "ROMBO COMPAÑÍA FINANCIERA S.A."),
                new KeyValuePair<int, string>(44096, "JOHN DEERE CREDIT COMPAÑÍA FINANCIERA S."),
                new KeyValuePair<int, string>(44098, "PSA FINANCE ARGENTINA COMPAÑÍA FINANCIER"),
                new KeyValuePair<int, string>(44099, "TOYOTA COMPAÑÍA FINANCIERA DE ARGENTINA"),
                new KeyValuePair<int, string>(44100, "FINANDINO COMPAÑIA FINANCIERA S.A."),
                new KeyValuePair<int, string>(45056, "MONTEMAR COMPAÑIA FINANCIERA S.A."),
                new KeyValuePair<int, string>(45072, "MULTIFINANZAS COMPAÑIA FINANCIERA S.A."),
                new KeyValuePair<int, string>(65203, "CAJA DE CREDITO 'CUENCA' COOPERATIVA LIM")
            };

            bancos.Sort((firstPair, nextPair) => firstPair.Value.CompareTo(nextPair.Value) );

            Bancos.DataSource = new BindingSource(bancos, null);
            Bancos.DisplayMember = "Value";
            Bancos.ValueMember = "Key";
            Bancos.Format += (s, le) =>
            {
                var item = (KeyValuePair<int, string>)le.ListItem;
                le.Value = "(" + item.Key.ToString("00000") + ") - " + item.Value;
            };
        }

        private void Generar_Click(object sender, EventArgs e)
        {
            var banco = (Bancos.SelectedValue as int?);

            if (!banco.HasValue)
                return;

            var ponderador = "97139713971397139713971397139713";
            var total = 0D;
            var digito = 0D;
            var pond = 0D;
            var cbu = string.Empty;
            var chunk = string.Empty;
            var rnd = new Random();

            chunk = banco.Value.ToString("000") + rnd.Next(1000, 9999);
            var bloque1 = "0" + chunk;

            for (int i = 0; i <= 7; i++)
            {
                digito = int.Parse(bloque1[i].ToString());
                pond = int.Parse(ponderador[i].ToString());
                total = total + (pond * digito) - ((Math.Floor(pond * digito / 10)) * 10);
            }

            var digito1 = 0;

            while (((Math.Floor((total + digito1) / 10)) * 10) != (total + digito1))
                digito1++;

            cbu = string.Concat(chunk, digito1);


            chunk = string.Concat(rnd.Next(10000, 99999), rnd.Next(10000, 99999), rnd.Next(100, 999));
            var bloque2 = "000" + chunk;
            total = 0D;

            for (int i = 0; i <= 15; i++)
            {
                digito = int.Parse(bloque2[i].ToString());
                pond = int.Parse(ponderador[i].ToString());
                total = total + (pond * digito) - ((Math.Floor(pond * digito / 10)) * 10);
            }

            var digito2 = 0;

            while (((Math.Floor((total + digito2) / 10)) * 10) != (total + digito2))
                digito2++;

            cbu += chunk + digito2;

            CBU.Text = cbu;
        }

        private void Copiar_Click(object sender, EventArgs e)
        {
            CBU.SelectAll();
            Clipboard.Clear();
            Clipboard.SetText(CBU.Text);

            MessageBox.Show("Copiado");
        }
    }
}
