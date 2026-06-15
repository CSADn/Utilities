using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HtmlCF.Objects;
using HtmlCF.Extensions;
using HtmlCF.Controls;
using HtmlCF.Utilities;
using HtmlCF.Functions;
using System.IO;

namespace HtmlCF
{
    public partial class Main : Form
    {
        #region Members

        private Site _site;
        private BindingList<Element> _elements;
        private BindingList<Selector> _selectors;
        private BindingList<Capture> _captures;
        private BindingList<Element> _childs;
        private Element _selectedElement;
        private Selector _selectedSelector;
        private Capture _selectedCapture;
        private bool _stopUpdate;
        private string _path;
        private StreamWriter _writer;

        public Selector SelectedSelector { get { return _selectedSelector; } set { _selectedSelector = value; } }

        #endregion

        #region Constructor

        public Main()
        {
            InitializeComponent();

            _site = new Site
            {
                Elements = new List<Element>(),
                Mask = string.Empty
            };

            _elements = new BindingList<Element>(_site.Elements);
            _elements.ListChanged += _elements_ListChanged;

            _selectors = null;
            _captures = null;
            _childs = null;

            _selectedElement = new Element
            {
                Selector = new List<Selector>(),
                Capture = new List<Capture>(),
                Childs = new List<Element>()
            };

            _selectedSelector = null;
            _selectedCapture = null;

            _stopUpdate = false;

            PopulateControls();
        }

        private void PopulateControls()
        {
            cbElementType.DisplayMember = "Value";
            cbElementType.ValueMember = "Key";
            cbElementType.DataSource = new BindingSource(Dictionary.Of<ElementType>(), null);


            cbSelectorFunction.DisplayMember = "Value";
            cbSelectorFunction.ValueMember = "Key";
            cbSelectorFunction.DataSource = new BindingSource(Dictionary.Of<SelectorFunctions>(), null);

            cbSelectorType.DisplayMember = "Value";
            cbSelectorType.ValueMember = "Key";
            cbSelectorType.DataSource = new BindingSource(Dictionary.Of<SelectorType>(), null);


            cbCaptureFunction.DisplayMember = "Value";
            cbCaptureFunction.ValueMember = "Key";

            cbCaptureType.DisplayMember = "Value";
            cbCaptureType.ValueMember = "Key";
            cbCaptureType.DataSource = new BindingSource(Dictionary.Of<CaptureType>(), null);

            rbCookieValue.CheckedChanged += rbCookie_CheckedChanged;
            rbCookieFile.CheckedChanged += rbCookie_CheckedChanged;
        }

        #endregion

        #region Events

        private void tvDom_AfterSelect(object sender, TreeViewEventArgs e)
        {
            gbNode.Enabled = (tvDom.SelectedNode != null);
            gbSelector.Enabled = (tvDom.SelectedNode != null);
            chkCapture.Enabled = (tvDom.SelectedNode != null);

            if (tvDom.SelectedNode != null)
                EditNode(tvDom.SelectedNode.Name);
        }

        #endregion

        private void NewNode()
        {
            int eid = GetNewElementID();

            _elements.Add(new Element
            {
                Id = eid,
                Tag = "Nuevo elemento " + eid,
                Selector = new List<Selector>(),
                Capture = new List<Capture>(),
                Childs = new List<Element>()
            });

            tvDom.SelectedNode = tvDom.Nodes[eid.ToString()];
        }

        private void NewChildNode(Element parent)
        {
            int eid = GetNewElementID();

            _childs.Add(new Element
            {
                Id = eid,
                Tag = "Nuevo elemento " + eid,
                Selector = new List<Selector>(),
                Capture = new List<Capture>(),
                Childs = new List<Element>()
            });

            BuildTree(_childs, FindNodeByKey(_selectedElement.Id.ToString()));

            tvDom.SelectedNode = FindNodeByKey(eid.ToString());
        }

        private int GetNewElementID()
        {
            return GetNewElementID(_elements, 0) + 1;
        }

        private int GetNewElementID(IList<Element> elements, int baseId)
        {
            int id = baseId;

            foreach (var e in elements)
            {
                if (e.Id > id)
                    id = e.Id;

                if (e.Childs != null && e.Childs.Count > 0)
                {
                    var value = GetNewElementID(e.Childs, id);
                    if (value > id)
                        id = value;
                }
            }
            
            return id;
        }

        private void EditNode(string nodeName)
        {
            var element = FindElementId(_elements, Convert.ToInt32(nodeName));

            if (element == null)
                return;

            if (_selectedElement != null)
                _selectedElement.PropertyChanged -= _selectedElement_PropertyChanged;

            _selectedElement = element;
            _selectedElement.PropertyChanged += _selectedElement_PropertyChanged;

            _selectors = new BindingList<Selector>(_selectedElement.Selector);
            _captures = new BindingList<Capture>(_selectedElement.Capture);
            _childs = new BindingList<Element>(_selectedElement.Childs);

            cbElementTag.Text = _selectedElement.Tag;
            cbElementType.SelectedValue = _selectedElement.Type;

            lsSelectorRules.DataSource = _selectors;
            lsSelectorRules.SelectedIndex = (lsSelectorRules.Items.Count > 0 ? 0 : -1);

            ValidateSelectorPanel();
            ValidateSelectorType();

            lsCaptureRules.DataSource = _captures;
            lsCaptureRules.SelectedIndex = (lsCaptureRules.Items.Count > 0 ? 0 : -1);

            ValidateCapturePanel();
        }

        private void ValidateCapturePanel()
        {
            chkCapture.Checked = _selectedElement.HasCapture;
            pnCapture.Enabled = _selectedElement.HasCapture;
            if (!pnCapture.Enabled)
                _selectedCapture = null;
        }

        private void ValidateSelectorPanel()
        {
            pnSelector.Enabled = _selectedElement.HasSelector;
            if (!pnSelector.Enabled)
                _selectedSelector = null;
        }

        private void CleanInputs()
        {
            CleanInputs(true);
        }

        private void CleanInputs(bool enable)
        {
            gbNode.Enabled = enable;
            gbSelector.Enabled = enable;
            chkCapture.Enabled = enable;
            gbCapture.Enabled = false;
        }


        #region ContextMenuStrip

        private void tvDom_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var ht = tvDom.HitTest(e.Location);
                var f = (ht.Node == null);

                if (!f)
                    tvDom.SelectedNode = ht.Node;

                tsmTreeAdd.Visible = f;
                tsmTreeAddChild.Visible = !f;
                tsmTreeRemove.Visible = !f;
            }
        }

        private void lsSelectorRules_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var i = lsSelectorRules.IndexFromPoint(e.Location);

                if (i > -1)
                    lsSelectorRules.SelectedIndex = i;

                tsmSelectorAdd.Visible = (i == -1);
                tsmSelectorRemove.Visible = (i > -1);
            }
        }

        private void lsCaptureRules_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var i = lsCaptureRules.IndexFromPoint(e.Location);
                var f = (i == -1);

                if (!f)
                    lsCaptureRules.SelectedIndex = i;

                tsmCaptureAdd.Visible = f;
                tsmCaptureRemove.Visible = !f;
            }
        }


        private void tsmTreeAdd_Click(object sender, EventArgs e)
        {
            NewNode();
        }

        private void tsmSelectorAdd_Click(object sender, EventArgs e)
        {
            var s = new Selector(SelectorType.Attribute, string.Empty, string.Empty);
            _selectors.Add(s);

            ValidateSelectorPanel();
        }

        private void tsmCaptureAdd_Click(object sender, EventArgs e)
        {
            var c = new Capture(string.Empty, CaptureType.Attribute, string.Empty, string.Empty);
            _captures.Add(c);

            ValidateCapturePanel();
        }


        private void tsmTreeAddChild_Click(object sender, EventArgs e)
        {
            var parent = GetTreeSelectedItem();
            NewChildNode(parent);
        }

        private Element GetTreeSelectedItem()
        {
            if (_elements == null || _elements.Count == 0)
                return null;

            if (tvDom.Nodes == null || tvDom.Nodes.Count == 0)
                return null;

            if (tvDom.SelectedNode == null)
                return null;

            var e = FindElementId(_elements, Convert.ToInt32(tvDom.SelectedNode.Name));
            return e;
        }

        private Element FindElementId(IList<Element> elements, int id)
        {
            Element found = null;

            foreach (var e in elements)
            {
                if (e.Id == id)
                    found = e;
                else if (e.Childs != null && e.Childs.Count > 0)
                    found = FindElementId(e.Childs, id);

                if (found != null)
                    return found;
            }

            return null;
        }

        private TreeNode FindNodeByKey(string key)
        {
            return tvDom.Nodes.Find(key, true).FirstOrDefault();
        }

        #endregion


        #region TreeView Construction

        private void _elements_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                case ListChangedType.ItemDeleted:
                case ListChangedType.ItemMoved:
                    BuildTree();
                    break;
                case ListChangedType.ItemChanged:
                    UpdateTreeItem(e.NewIndex);
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                case ListChangedType.Reset:
                default:
                    throw new NotImplementedException();
            }
        }

        private void _selectedElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateTreeItem((Element)sender);
        }

        private void UpdateTreeItem(int itemIdx)
        {
            var e = _elements[itemIdx];
            UpdateTreeItem(e);
        }

        private void UpdateTreeItem(Element e)
        {
            var node = FindNodeByKey(e.Id.ToString());
            var image = GetNodeIcon(e);

            node.Text = e.Tag;
            node.ImageKey = image;
            node.SelectedImageKey = image;
        }

        private void BuildTree()
        {
            tvDom.Nodes.Clear();
            BuildTree(_elements, null);
        }

        private void BuildTree(IList<Element> elements, TreeNode parent)
        {
            var nodes = (parent != null ? parent.Nodes : tvDom.Nodes);

            nodes.Clear();

            foreach (var e in elements)
            {
                var image = GetNodeIcon(e);

                var newNode = nodes.Add(e.Id.ToString(), e.Tag, image, image);
                newNode.EnsureVisible();

                if (e.Childs != null && e.Childs.Count > 0)
                    BuildTree(e.Childs, newNode);
            }
        }

        private static string GetNodeIcon(Element e)
        {
            var image = (
                e.HasBreak
                    ? "tag-break"
                    : (
                        e.HasCapture
                            ? "tag-capture"
                            : "tag-normal"
                    )
            );

            return image;
        }

        #endregion


        #region Toolbar

        private void tsmLoad_Click(object sender, EventArgs e)
        {
            LoadProject();
        }

        private void tsmSave_Click(object sender, EventArgs e)
        {
            SaveProject();
        }


        private void tsbStart_Click(object sender, EventArgs e)
        {
            StartProcess();
        }


        private void LoadProject()
        {
            var p = new ParseProject(@"c:\temp\HtmlCF\projectx.hcf");

            _site = p.Site;

            tbSiteUrl.Text = _site.Url;
            tbSiteUrlMask.Text = _site.Mask;
            tbSiteMaskFrom.Text = _site.From.ToString();
            tbSiteMaskTo.Text = _site.To.ToString();
            chkCookie.Checked = _site.SetCookie;

            rbCookieValue.CheckedChanged -= rbCookie_CheckedChanged;
            rbCookieFile.CheckedChanged -= rbCookie_CheckedChanged;

            if (!string.IsNullOrEmpty(_site.CookieValue))
                rbCookieValue.Checked = true;
            else
                rbCookieFile.Checked = true;

            tbCookieValue.Text = _site.CookieValue;
            tbCookieFile.Text = _site.CookiePath;

            rbCookieValue.CheckedChanged += rbCookie_CheckedChanged;
            rbCookieFile.CheckedChanged += rbCookie_CheckedChanged;

            _elements.ListChanged -= _elements_ListChanged;
            _elements = new BindingList<Element>(_site.Elements);
            _elements.ListChanged += _elements_ListChanged;

            _selectors = null;
            _captures = null;
            _childs = null;

            BuildTree();

            if (tvDom.Nodes.Count > 0)
                tvDom.SelectedNode = tvDom.Nodes[0];
        }

        private void SaveProject()
        {
            ParseProject.SaveProject(@"c:\temp\HtmlCF\projectx.hcf", _site);
            MessageBox.Show("Done.!.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void StartProcess()
        {
            _path = @"C:\temp\HtmlCF\captura.txt";
            _writer = new StreamWriter(_path, false, Encoding.GetEncoding("iso-8859-1"));

            var f = new Fetcher(new List<Site> { _site });
            f.OnCapture += f_OnCapture;
            f.OnDone += f_OnDone;
            f.Start();

            _writer.Close();
        }

        private void f_OnCapture(object sender, FetcherCaptureArgs e)
        {
            if (_writer == null)
                return;

            var line = string.Join(";", e.Results.ToArray());
            line = line.Replace(Environment.NewLine, string.Empty);

            _writer.WriteLine(line);
            _writer.Flush();
        }

        private void f_OnDone(object sender)
        {
            MessageBox.Show(".!.Done");
        }

        #endregion


        #region Site

        private void tbSiteUrl_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _site == null)
                return;

            _site.Url = tbSiteUrl.Text.Trim();
        }

        private void tbSiteUrlMask_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _site == null)
                return;

            _site.Mask = tbSiteUrlMask.Text.Trim();
        }

        private void tbSiteMaskFrom_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _site == null)
                return;

            _site.From = Convert.ToInt32(tbSiteMaskFrom.Text.Trim());
        }

        private void tbSiteMaskTo_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _site == null)
                return;

            _site.To = Convert.ToInt32(tbSiteMaskTo.Text.Trim());
        }

        private void chkCookie_CheckedChanged(object sender, EventArgs e)
        {
            gbCookie.Enabled = chkCookie.Checked;

            if (_site != null)
                _site.SetCookie = chkCookie.Checked;
        }

        private void tbCookieValue_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _site == null)
                return;

            _site.CookieValue = tbCookieValue.Text.Trim();
        }

        #endregion

        #region Element

        private void cbElementTag_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _selectedElement == null)
                return;

            _selectedElement.Tag = cbElementTag.Text.Trim();
        }

        private void cbElementType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbElementType.SelectedValue == null)
                return;

            _selectedElement.Type = (ElementType)cbElementType.SelectedValue;
        }

        #endregion

        #region Selector

        private void lsSelectorRules_SelectedValueChanged(object sender, EventArgs e)
        {
            var value = lsSelectorRules.SelectedValue;
            if (value == null)
                return;

            EditSelector((Selector)value);
        }

        private void EditSelector(Selector value)
        {
            if (_selectedSelector == value)
                return;

            _selectedSelector = value;
            ValidateSelectorType();
        }

        private void ValidateSelectorType()
        {
            _stopUpdate = true;

            cbSelectorFunction.Hide();
            tbSelectorName.Clear();
            tbSelectorName.Enabled = false;
            tbSelectorValue.Clear();
            tbSelectorValue.Enabled = false;

            if (_selectedSelector == null)
            {
                _stopUpdate = false;
                return;
            }

            cbSelectorType.SelectedValue = _selectedSelector.Type;

            switch (_selectedSelector.Type)
            {
                case SelectorType.Id:
                case SelectorType.Class:
                    tbSelectorValue.Text = _selectedSelector.Value;
                    tbSelectorValue.Enabled = true;
                    break;

                case SelectorType.Property:
                case SelectorType.Attribute:
                    tbSelectorName.Text = _selectedSelector.Name;
                    tbSelectorName.Enabled = true;
                    tbSelectorValue.Text = _selectedSelector.Value;
                    tbSelectorValue.Enabled = true;
                    break;

                case SelectorType.Function:
                    var function = SelectorFunctions.Eq;

                    if (Enum.GetNames(typeof(SelectorFunctions)).Any(a => a == _selectedSelector.Name))
                        function = (SelectorFunctions)Enum.Parse(typeof(SelectorFunctions), _selectedSelector.Name);

                    cbSelectorFunction.SelectedIndex = -1;
                    cbSelectorFunction.SelectedValue = function;
                    cbSelectorFunction.Show();
                    break;
            }

            _stopUpdate = false;
        }


        private void cbSelectorType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || cbSelectorType.SelectedValue == null)
                return;

            if (_selectedSelector != null)
                _selectedSelector.Type = (SelectorType)cbSelectorType.SelectedValue;

            ValidateSelectorType();
        }

        private void tbSelectorName_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _selectedSelector == null)
                return;

            _selectedSelector.Name = tbSelectorName.Text.Trim();
        }

        private void tbSelectorValue_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _selectedSelector == null)
                return;

            _selectedSelector.Value = tbSelectorValue.Text.Trim();
        }

        private void cbSelectorFunction_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbSelectorFunction.SelectedValue == null)
                return;

            var function = (SelectorFunctions)cbSelectorFunction.SelectedValue;

            if (Function.HasParameter(function))
            {
                tbSelectorValue.Text = (_selectedSelector != null ? _selectedSelector.Value : "");
                tbSelectorValue.Enabled = true;
            }
            else
            {
                tbSelectorValue.Clear();
                tbSelectorValue.Enabled = false;
            }

            if (_selectedSelector != null)
                _selectedSelector.Name = function.ToString();
        }

        #endregion

        #region Capture

        private void lsCaptureRules_SelectedValueChanged(object sender, EventArgs e)
        {
            var value = lsCaptureRules.SelectedValue;
            if (value == null)
                return;

            EditCapture((Capture)value);
        }

        private void EditCapture(Capture value)
        {
            if (_selectedCapture == value)
                return;

            _selectedCapture = value;
            ValidateCaptureType();
        }

        private void ValidateCaptureType()
        {
            _stopUpdate = true;

            cbCaptureFunction.Hide();
            tbCaptureDescription.Clear();
            tbCaptureName.Clear();
            tbCaptureName.Enabled = false;
            tbCaptureValue.Clear();
            tbCaptureValue.Enabled = false;

            if (_selectedCapture == null)
            {
                _stopUpdate = false;
                return;
            }

            cbCaptureType.SelectedValue = _selectedCapture.Type;
            tbCaptureDescription.Text = _selectedCapture.Description;

            switch (_selectedCapture.Type)
            {
                case CaptureType.Property:
                case CaptureType.Attribute:
                    tbCaptureName.Text = _selectedCapture.Name;
                    tbCaptureName.Enabled = true;
                    tbCaptureValue.Text = _selectedCapture.Value;
                    tbCaptureValue.Enabled = true;
                    break;

                case CaptureType.Function:
                    var function = CaptureFunctions.EqText;

                    if (Enum.GetNames(typeof(CaptureFunctions)).Any(a => a == _selectedCapture.Name))
                        function = (CaptureFunctions)Enum.Parse(typeof(CaptureFunctions), _selectedCapture.Name);

                    cbCaptureFunction.DataSource = new BindingSource(Dictionary.Of<CaptureFunctions>(), null);
                    cbCaptureFunction.SelectedIndex = -1;
                    cbCaptureFunction.SelectedValue = function;
                    cbCaptureFunction.Show();
                    break;

                case CaptureType.Condition:
                    var condition = CaptureConditions.StopIfNotEmpty;

                    if (Enum.GetNames(typeof(CaptureConditions)).Any(a => a == _selectedCapture.Name))
                        condition = (CaptureConditions)Enum.Parse(typeof(CaptureConditions), _selectedCapture.Name);

                    cbCaptureFunction.DataSource = new BindingSource(Dictionary.Of<CaptureConditions>(), null);
                    cbCaptureFunction.SelectedIndex = -1;
                    cbCaptureFunction.SelectedValue = condition;
                    cbCaptureFunction.Show();
                    break;
            }

            _stopUpdate = false;
        }


        private void chkCapture_CheckedChanged(object sender, EventArgs e)
        {
            gbCapture.Enabled = chkCapture.Checked;
        }

        private void cbCaptureType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || cbCaptureType.SelectedValue == null)
                return;

            if (_selectedCapture != null)
                _selectedCapture.Type = (CaptureType)cbCaptureType.SelectedValue;

            ValidateCaptureType();
        }

        private void tbCaptureDescription_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _selectedCapture == null)
                return;

            _selectedCapture.Description = tbCaptureDescription.Text.Trim();
        }

        private void tbCaptureName_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _selectedCapture == null)
                return;

            _selectedCapture.Name = tbCaptureName.Text.Trim();
        }

        private void tbCaptureValue_TextChanged(object sender, EventArgs e)
        {
            if (_stopUpdate || _selectedCapture == null)
                return;

            _selectedCapture.Value = tbCaptureValue.Text.Trim();
        }

        private void cbCaptureFunction_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbCaptureFunction.SelectedValue == null)
                return;

            if (cbCaptureFunction.SelectedValue is CaptureFunctions)
            {
                var function = (CaptureFunctions)cbCaptureFunction.SelectedValue;

                if (Function.HasParameter(function))
                {
                    tbCaptureValue.Text = (_selectedCapture != null ? _selectedCapture.Value : "");
                    tbCaptureValue.Enabled = true;
                }
                else
                {
                    tbCaptureValue.Clear();
                    tbCaptureValue.Enabled = false;
                }

                if (_selectedCapture != null)
                    _selectedCapture.Name = function.ToString();
            }
            else if (cbCaptureFunction.SelectedValue is CaptureConditions)
            {
                var condition = (CaptureConditions)cbCaptureFunction.SelectedValue;

                if (Function.HasParameter(condition))
                {
                    tbCaptureValue.Text = (_selectedCapture != null ? _selectedCapture.Value : "");
                    tbCaptureValue.Enabled = true;
                }
                else
                {
                    tbCaptureValue.Clear();
                    tbCaptureValue.Enabled = false;
                }

                if (_selectedCapture != null)
                    _selectedCapture.Name = condition.ToString();
            }
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            htmlPanel1.Text += "<div>" + textBox1.Text + "</div>";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            htmlPanel1.Text = string.Empty;
        }

        private void rbCookie_CheckedChanged(object sender, EventArgs e)
        {
            _site.CookiePath = string.Empty;
            _site.CookieValue = string.Empty;

            lbCookieValue.Enabled = false;
            tbCookieValue.Enabled = false;
            lbCookieFile.Enabled = false;
            tbCookieFile.Enabled = false;
            btCookieBrowse.Enabled = false;
            tbCookieValue.Text = string.Empty;
            tbCookieFile.Text = string.Empty;

            if (rbCookieValue.Checked)
            {
                lbCookieValue.Enabled = true;
                tbCookieValue.Enabled = true;
            }
            else
            {
                lbCookieFile.Enabled = true;
                tbCookieFile.Enabled = true;
                btCookieBrowse.Enabled = true;
            }
        }

        private void btCookieBrowse_Click(object sender, EventArgs e)
        {
            //
            // https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg?hl=es-419
            //

            var ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.DefaultExt = "*.txt";
            ofd.Filter = "Cookies|*.txt";
            ofd.Title = "Buscar archivo de cookies...";

            ofd.ShowDialog();

            tbCookieFile.Text = ofd.FileName;
            _site.CookiePath = ofd.FileName;
        }
    }
}
