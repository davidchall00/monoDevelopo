using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LibProperties;
using System.IO;
using System.Text.RegularExpressions;


namespace MDDesigner {
    public partial class Form1 : Form {
        private int iNextNameSuffix = 0;

        private string strFormsAssemblyName = null;

        private PropertyForm frmPropertyForm = null;

        public PropertyForm PropForm {
            get { return frmPropertyForm; }
            set { frmPropertyForm = value; }
        }

        private List<Layout> lstControls = new List<Layout>();

        public Form1() {
            InitializeComponent();
            InitiateToolBox();
            strFormsAssemblyName = typeof(Button).Assembly.FullName;
        }

        private void tsmiExit_Click(object sender, EventArgs e) {
            this.Close();
        }

        //Allows user to move the ToolBox on the form and allows one item in the Toolbox, clicked by the user, to be highlighted
        private Point firstPoint = new Point();
        
        public void InitiateToolBox() {
            pnlToolBox.MouseDown += (ss, ee) => {
                if (ee.Button == System.Windows.Forms.MouseButtons.Left) { firstPoint = Control.MousePosition; }
            };

            pnlToolBox.MouseMove += (ss, ee) => {
                if (ee.Button == System.Windows.Forms.MouseButtons.Left) {
                    Point temporary = Control.MousePosition;
                    Point res = new Point(firstPoint.X - temporary.X, firstPoint.Y - temporary.Y);
                    pnlToolBox.Location = new Point(pnlToolBox.Location.X - res.X, pnlToolBox.Location.Y - res.Y);

                    firstPoint = temporary;
                }
            };

            foreach (Control cChild in this.pnlToolBox.Controls) {
                if (cChild is Label && cChild != lblCommonControls && cChild != lblContainers && cChild != lblMenusAndToolbars 
                    && cChild != lblData && cChild != lblComponents && cChild != lblPrinter && cChild != lblDialogs 
                    && cChild != lblWPFInter && cChild != lblPointer && cChild != lblDateTimePicker && cChild != lblListView
                    && cChild != lblNotifyIcon && cChild != lblToolTip && cChild != lblWebBrowser && cChild != lblWebBrowser2 
                    && cChild != lblContextMenuStrip && cChild != lblChart && cChild != lblBindingSource && cChild != lblDataGridView
                    && cChild != lblDataSet && cChild != lblBackgroundWorker && cChild != lblDirectoryEntry && cChild != lblDirectorySearcher
                    && cChild != lblErrorProvider && cChild != lblEventLog && cChild != lblFileSystemWatcher && cChild != lblHelpProvider
                    && cChild != lblImageList && cChild != lblMessageQueue && cChild != lblPerformanceCounter && cChild != lblProcess
                    && cChild != lblSerialPort && cChild != lblServiceController && cChild != lblTimer && cChild != lblPageSetupDialog
                    && cChild != lblPrintDialog && cChild != lblPrintDialog && cChild != lblPrintDocument && cChild != lblPrintPreviewControl
                    && cChild != lblPrintPreviewDialog && cChild != lblColorDialog && cChild != lblFolderBrowserDialog
                    && cChild != lblFontDialog && cChild != lblOpenFileDialog && cChild != lblSaveFileDialog && cChild != lblElementHost) {
                    Label lblThis = cChild as Label;
                    lblThis.Click += new EventHandler(lblPointer_Click);
                    lblThis.DoubleClick += new EventHandler(vPlaceNewControl);
                }
            }
        }

        private void vPlaceNewControl(object sender, EventArgs e)
        {
            Label lblThis = sender as Label;
            string strControlTypeName = lblThis.Text;
            Type tyControlType = Type.GetType("System.Windows.Forms." + strControlTypeName + ", " + strFormsAssemblyName);
            iNextNameSuffix++;
            string strControlName = strControlTypeName + iNextNameSuffix;
            Layout lNew = new Layout(tyControlType, strControlName, new Point(100, 100), this);
            // Add to the list of controls on the panel.
            pnlLayout.Controls.Add(lNew);
            lstControls.Add(lNew);
            // Move the focus to the new Layout object.
            lNew.Focus();
        }

        private void lblPointer_Click(object sender, EventArgs e) {
            foreach (Control cChild in this.pnlToolBox.Controls) {
                if (cChild is Label) {
                    Label lblOne = cChild as Label;
                    lblOne.BackColor = SystemColors.ControlLight;
                }
            }
            Label lblThis = sender as Label;
            lblThis.BackColor = Color.LightBlue;
        }

        ///***********************************************************************************
        // * Temporary testing code:                                                         *
        // * When we click in the panel, place a button there, inside a "Layout" object.     *
        // * *********************************************************************************/
        //private void vTemp_pnlLayout_Click(object sender, MouseEventArgs e) {
        //    // Create the Layout object.
        //    Layout lNew = new Layout(typeof(Button), "btnTest", e.Location, this);
        //    // Add to the list of controls on the panel.
        //    pnlLayout.Controls.Add(lNew);
        //    lstControls.Add(lNew);
        //    // Move the focus to the new Layout object.
        //    lNew.Focus();
        //}
        private void createNodeControl(Layout lControls, XmlTextWriter writer)
        {
            writer.WriteStartElement("Control");
            writer.WriteStartElement("Name");
            writer.WriteString(lControls.RC.Name);
            writer.WriteEndElement();
            writer.WriteStartElement("ControlType");
            writer.WriteString(lControls.RC.GetType().ToString());
            writer.WriteEndElement();
            writer.WriteStartElement("Properties");
            createNodeCProp(lControls.PropertyList, writer);
            writer.WriteEndElement();
            writer.WriteEndElement();

        }
        private void createNodeCProp(List<ControlProp> lstProps, XmlTextWriter writer)
        {
            foreach (ControlProp ctlProperty in lstProps)
            {

                if (ctlProperty.HasValueSpecified && ctlProperty.Value != null)
                {
                    writer.WriteStartElement("ControlProp");
                    writer.WriteStartElement("PropName");
                    writer.WriteString(ctlProperty.Name);
                    writer.WriteEndElement();
                    writer.WriteStartElement("PropValueType");
                    writer.WriteString(ctlProperty.ValueType.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("PropValue");
                    writer.WriteString(ctlProperty.Value.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
        }


        private void tsmiSave_Click_1(object sender, EventArgs e)
        {
            {
                //  XXXX     freezes if property form is open

                XmlTextWriter writer = new XmlTextWriter("NewXml.xml", System.Text.Encoding.UTF8);
                writer.WriteStartDocument(true);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                writer.WriteStartElement("Form");
                writer.WriteStartElement("Controls");

                foreach (Control cChild in this.lstControls)
                {
                    Layout lChild = cChild as Layout;
                    createNodeControl(lChild, writer);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
                MessageBox.Show("XML File created ! ");
            }

        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            //Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //dialogue to find xml file
            openFileDialog1.InitialDirectory = "C:\\Users\\BanditBan\\Documents\\Source\\Repos\\MDFormDesigner\\Source\\MDDesigner\\MDDesigner\\bin\\Debug";
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);
                using (StreamWriter sw = File.CreateText("NewCS.cs"))
                {
                    XmlNodeList elemList = doc.GetElementsByTagName("Control");
                    sw.WriteLine("using System;\r\nusing System.Collections.Generic;\r\nusing System.ComponentModel;\r\nusing System.Data;\r\nusing System.Drawing;\r\nusing System.Linq;\r\nusing System.Text;\r\nusing System.Threading.Tasks;\r\nusing System.Windows.Forms; ");
                    sw.WriteLine("partial class Form1 {");
                    sw.WriteLine("private void InitializeComponent(){");
                    foreach (XmlNode i in elemList)
                    {
                        string nName = i["Name"].InnerXml;
                        string ncType = i["ControlType"].InnerXml;

                        sw.WriteLine("this." + nName + " = new " + ncType + "();");
                        sw.WriteLine("this.Controls.Add(this." + nName + ");");
                        sw.WriteLine("this." + nName + ".Name = " + nName + ";");

                        XmlNode nProperties = i["Properties"];


                        XmlNodeList ControlPropList = nProperties.ChildNodes;
                        foreach (XmlNode j in ControlPropList)
                        {
                            string pName = j["PropName"].InnerXml;
                            string pvType = j["PropValueType"].InnerXml;
                            string pValue = "";

                            bool bHasInt = j["PropValue"].InnerXml.Any(char.IsDigit);
                            if (bHasInt)
                            {
                                pValue = Regex.Replace(j["PropValue"].InnerXml, @"[^0-9,]+", "");
                            }
                            else
                            {
                                pValue = j["PropValue"].InnerXml;
                            }
                            sw.WriteLine("this." + nName + "." + pName + " = new " + pvType + "(" + pValue + ");");
                        }
                    }




                    sw.WriteLine("}");
                    foreach (XmlNode i in elemList)
                    {
                        string nName = i["Name"].InnerXml;
                        string ncType = i["ControlType"].InnerXml;
                        sw.WriteLine("private " + ncType + " " + nName + ";");
                    }
                    sw.WriteLine("}");


                }

            }
        }
    }    
}
