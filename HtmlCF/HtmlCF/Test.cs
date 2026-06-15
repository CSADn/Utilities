using HtmlCF.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HtmlCF
{
    public partial class Test : Form
    {
        private string _input = @"c:\temp\HtmlCF\captura.npd.publicsex.txt";
        private string _output = @"C:\temp\HtmlCF\captura.npd.publicsex.flv.txt";
        private string _cookie = @"C:\temp\HtmlCF\cookies.npd.txt";

        public Test()
        {
            InitializeComponent();

            lbCurrent.Text = "0";
            lbTotal.Text = "0";
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            DoMagic();
        }

        private void DoMagic()
        {
            var sites = new List<Site>();
            var lines = File.ReadAllLines(_input);

            foreach (var line in lines)
            {
                if (line.Trim().Equals("link", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                #region New Site

                sites.Add(new Site
                {
                    Url = line.Trim(),
                    Mask = null,
                    From = 0,
                    To = 0,
                    SetCookie = true,
                    CookieValue = null,

                    /*
                     Chrome Extension: Cookies.txt
                     https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg
                    */
                    CookiePath = _cookie,

                    Elements = new List<Element>
                    {
                        new Element
                        {
                            Id = 1,
                            Tag = "div",
                            Type = ElementType.Normal,

                            Selector = new List<Selector>
                            {
                                new Selector
                                {
                                    Type = SelectorType.Id,
                                    Name = string.Empty,
                                    Value = "single-post"
                                }
                            },

                            Capture = new List<Objects.Capture>(),

                            Childs = new List<Element>
                            {
                                new Element
                                {
                                    Id = 2,
                                    Tag = "div",
                                    Type = ElementType.Normal,

                                    Selector = new List<Selector>
                                    {
                                        new Selector
                                        {
                                            Type = SelectorType.Class,
                                            Name = string.Empty,
                                            Value = "single-categoria"
                                        }
                                    },

                                    Capture = new List<Capture>
                                    {
                                        new Capture
                                        {
                                            Description = "category",
                                            Type = CaptureType.Function,
                                            Name = "InnerText",
                                            Value = "true"
                                        }
                                    }
                                },

                                new Element
                                {
                                    Id = 3,
                                    Tag = "div",
                                    Type = ElementType.Normal,

                                    Selector = new List<Selector>
                                    {
                                        new Selector
                                        {
                                            Type = SelectorType.Class,
                                            Name = string.Empty,
                                            Value = "single-fecha"
                                        }
                                    },

                                    Capture = new List<Capture>
                                    {
                                        new Capture
                                        {
                                            Description = "date",
                                            Type = CaptureType.Function,
                                            Name = "InnerText",
                                            Value = "true"
                                        }
                                    }
                                },

                                new Element
                                {
                                    Id = 4,
                                    Tag = "div",
                                    Type = ElementType.Normal,

                                    Selector = new List<Selector>
                                    {
                                        new Selector
                                        {
                                            Type = SelectorType.Id,
                                            Name = string.Empty,
                                            Value = "containingBlock"
                                        }
                                    },

                                    Capture = new List<Capture>(),

                                    Childs = new List<Element>
                                    {
                                        new Element
                                        {
                                            Id = 5,
                                            Tag = "object",
                                            Type = ElementType.Normal,

                                            Selector = new List<Selector>
                                            {
                                                new Selector
                                                {
                                                    Type = SelectorType.Id,
                                                    Name = string.Empty,
                                                    Value = "svdo_0"
                                                }
                                            },

                                            Capture = new List<Capture>(),

                                            Childs = new List<Element>
                                            {
                                                new Element
                                                {
                                                    Id = 6,
                                                    Tag = "param",
                                                    Type = ElementType.CaptureBreak,

                                                    Selector = new List<Selector>
                                                    {
                                                        new Selector
                                                        {
                                                            Type = SelectorType.Attribute,
                                                            Name = "name",
                                                            Value = "flashvars"
                                                        },
                                                        new Selector
                                                        {
                                                            Type = SelectorType.Function,
                                                            Name = "First",
                                                            Value = string.Empty
                                                        }
                                                    },

                                                    Capture = new List<Capture>
                                                    {
                                                        new Capture
                                                        {
                                                            Description = "flv",
                                                            Type = CaptureType.Attribute,
                                                            Name = "value",
                                                            Value = string.Empty
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

                #endregion
            }

            lbTotal.Text = sites.Count().ToString();

            var writer = new StreamWriter(_output, false, Encoding.GetEncoding("iso-8859-1"));

            var c = 0;
            var f = new Fetcher(sites);

            f.OnFetchingSite += (s, site) =>
            {
                lbCurrent.Text = c++.ToString();
                Application.DoEvents();
            };

            f.OnCapture += (s, e) =>
            {
                if (writer == null)
                    return;

                var r = e.Results;
                var ubound = r.Count - 1;

                r[ubound] = r[ubound].Replace(Environment.NewLine, string.Empty);

                var regex = new System.Text.RegularExpressions.Regex("file=(.*)&", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var match = regex.Match(r[ubound]);

                if (match != null & match.Groups.Count == 2)
                    r[ubound] = match
                        .Groups[1]
                        .Value
                        .Replace(" ", "%20");

                writer.WriteLine(string.Join(";", r));
                writer.Flush();
            };

            f.OnDone += (s) =>
            {
                writer.Close();
                lbCurrent.Text = c++.ToString();
                MessageBox.Show(".!.Done");
                btStart.Enabled = true;
                lbCurrent.Text = "0";
                lbTotal.Text = "0";
            };

            btStart.Enabled = false;
            f.Start();
        }
    }
}
