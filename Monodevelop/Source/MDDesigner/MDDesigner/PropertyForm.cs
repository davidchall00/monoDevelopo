using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibProperties;
using MDDesigner;
using System.Reflection;

namespace MDDesigner {
    public partial class PropertyForm : Form {

        private Form1 frmMain;
        private string strName = "";
        private EventHandlerList lstEvents;
        private Layout ltDisplayControl;

        public PropertyForm(List<ControlProp> lstPropertyList, Layout ltDC, Form1 frmM) {
            InitializeComponent();
            ltDisplayControl = ltDC;

            frmMain = frmM;
            this.TopMost = true;
            foreach (var propInfo in lstPropertyList) {
                if (propInfo.Name == "Name") {
                    this.Text = "Properties: " + propInfo.Value.ToString();
                }
            }
            loadDataTable(lstPropertyList);
        }

        private void loadDataTable(List<ControlProp> lstPropertyList) {
            DataSet ds = new DataSet();
            DataTable dt;
            DataRow dr;
            DataColumn pType;
            DataColumn pProperty;
            DataColumn pValue;

            dt = new DataTable();
            pType = new DataColumn("Property Type", typeof(Type));
            pProperty = new DataColumn("Property Name", Type.GetType("System.String"));
            pValue = new DataColumn("Value", Type.GetType("System.String"));

            dt.Columns.Add(pType);
            dt.Columns.Add(pProperty);
            dt.Columns.Add(pValue);
            foreach (var propInfo in lstPropertyList) {
                dr = dt.NewRow();
                dr["Property Type"] = propInfo.ValueType;
                dr["Property Name"] = propInfo.Name.ToString();
                if (propInfo.Value != null) {

                    dr["Value"] = propInfo.Value.ToString();
                }
                else {
                    propInfo.Value = null;
                }
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);
            dgvProperties.DataSource = ds.Tables[0];
            dgvProperties.Columns[0].Visible = false;
            dgvProperties.Columns[0].ReadOnly = true;
            dgvProperties.Columns[1].ReadOnly = true;
            dgvProperties.Columns[2].ReadOnly = false;
        }

        public void vReload(List<ControlProp> lstPropertyList, Layout ltDC) {
            ltDisplayControl = ltDC;
            this.TopMost = true;
            foreach (var propInfo in lstPropertyList) {
                if (propInfo.Name == "Name") {
                    this.Text = "Properties: " + propInfo.Value.ToString();
                }
            }
            DataTable dt = dgvProperties.DataSource as DataTable;
            dt.Clear();
            foreach (var propInfo in lstPropertyList) {
                DataRow dr = dt.NewRow();
                dr["Property Type"] = propInfo.ValueType;
                dr["Property Name"] = propInfo.Name.ToString();
                if (propInfo.Value != null) {

                    dr["Value"] = propInfo.Value.ToString();
                }
                else {
                    propInfo.Value = null;
                }
                dt.Rows.Add(dr);
            }
        }

        private void dgvProperties_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
            ComboBox combo = e.Control as ComboBox;
            if (combo != null) {
                combo.SelectedIndexChanged -= new EventHandler(ComboBox_SelectedIndexChanged);
                combo.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            }

            TextBox txtNew = e.Control as TextBox;
            if (txtNew != null) {
                txtNew.Leave -= new EventHandler(TextBox_Leave);
                txtNew.Leave += new EventHandler(TextBox_Leave);
            }
        }
        
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBox cb = (ComboBox)sender;
            string strNewItem = "";
            strNewItem = cb.Text;
            if (strNewItem != null) {
                object objNewValue = (object)strNewItem;
                MessageBox.Show("Value has been changed: " + strName + " = " + objNewValue.ToString());
                ltDisplayControl.PropagateChange(strName, objNewValue);
            }
            lstEvents = DetachEvents(cb);
        }
        private void TextBox_Leave(object sender, EventArgs e) {
            TextBox tb = (TextBox)sender;
            string strNI = "";
            strNI = tb.Text;
            if (strNI != null) {
                object objNewValue = (object)strNI;
                MessageBox.Show("Value has been changed: " + strName + " = " + objNewValue.ToString());
                ltDisplayControl.PropagateChange(strName, objNewValue);
            }
        }
        public EventHandlerList DetachEvents(Component obj) {
            object objNew = obj.GetType().GetConstructor(new Type[] { }).Invoke(new object[] { });
            PropertyInfo propEvents = obj.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);

            EventHandlerList eventHandlerList_obj = (EventHandlerList)propEvents.GetValue(obj, null);
            EventHandlerList eventHandlerList_objNew = (EventHandlerList)propEvents.GetValue(objNew, null);

            eventHandlerList_objNew.AddHandlers(eventHandlerList_obj);
            eventHandlerList_obj.Dispose();

            return eventHandlerList_objNew;
        }

        #region All cases for what dgvproperties.Value is equal to, and applying to a DataGridViewCell

        private void dgvProperties_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex > -1 && e.ColumnIndex != 1) {
                if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Boolean") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbChooseValue = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbChooseValue;
                    List<string> lstValueOptions = new List<string>();
                    lstValueOptions.Add("True");
                    lstValueOptions.Add("False");
                    cmbChooseValue.DataSource = lstValueOptions;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.AutoSizeMode") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbASM = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbASM;
                    List<string> lstASMOptions = new List<string>();
                    lstASMOptions.Add("GrowOnly");
                    lstASMOptions.Add("GrowAndShrink");
                    cmbASM.DataSource = lstASMOptions;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.DialogResult") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbDialog = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbDialog;
                    List<string> lstDialogOptions = new List<string>();
                    lstDialogOptions.Add("None");
                    lstDialogOptions.Add("OK");
                    lstDialogOptions.Add("Cancel");
                    lstDialogOptions.Add("Abort");
                    lstDialogOptions.Add("Retry");
                    lstDialogOptions.Add("Ignore");
                    lstDialogOptions.Add("Yes");
                    lstDialogOptions.Add("No");
                    cmbDialog.DataSource = lstDialogOptions;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.FlatStyle") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbFS = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbFS;
                    List<string> lstFS = new List<string>();
                    lstFS.Add("Flat");
                    lstFS.Add("Popup");
                    lstFS.Add("Standard");
                    lstFS.Add("System");
                    cmbFS.DataSource = lstFS;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Drawing.ContentAlignment") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbCA = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbCA;
                    List<string> lstCA = new List<string>();
                    lstCA.Add("TopLeft");
                    lstCA.Add("MiddleLeft");
                    lstCA.Add("BottomLeft");
                    lstCA.Add("TopCenter");
                    lstCA.Add("MiddleCenter");
                    lstCA.Add("BottomCenter");
                    lstCA.Add("TopRight");
                    lstCA.Add("MiddleRight");
                    lstCA.Add("BottomRight");
                    cmbCA.DataSource = lstCA;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.TextImageRelation") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbTIR = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbTIR;
                    List<string> lstTIR = new List<string>();
                    lstTIR.Add("Overlay");
                    lstTIR.Add("ImageAboveText");
                    lstTIR.Add("TextAboveImage");
                    lstTIR.Add("ImageBeforeText");
                    lstTIR.Add("TextBeforeImage");
                    cmbTIR.DataSource = lstTIR;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.ImageLayout") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbIL = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbIL;
                    List<string> lstIL = new List<string>();
                    lstIL.Add("None");
                    lstIL.Add("Tile");
                    lstIL.Add("Center");
                    lstIL.Add("Strech");
                    lstIL.Add("Zoom");
                    cmbIL.DataSource = lstIL;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.DockStyle") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbDS = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbDS;
                    List<string> lstDS = new List<string>();
                    lstDS.Add("None");
                    lstDS.Add("Top");
                    lstDS.Add("Left");
                    lstDS.Add("Fill");
                    lstDS.Add("Right");
                    lstDS.Add("Bottom");
                    cmbDS.DataSource = lstDS;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.RightToLeft") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbRTL = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbRTL;
                    List<string> lstRTL = new List<string>();
                    lstRTL.Add("No");
                    lstRTL.Add("Yes");
                    lstRTL.Add("Inherit");
                    cmbRTL.DataSource = lstRTL;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Drawing.Color") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbBC = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbBC;
                    List<string> lstBC = new List<string>();
                    lstBC.Add("Color[Control]");
                    lstBC.Add("Color[ControlLight]");
                    lstBC.Add("Color[ControlDark]");
                    lstBC.Add("Color[Highlight]");
                    cmbBC.DataSource = lstBC;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.ImeMode") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbIM = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbIM;
                    List<string> lstIM = new List<string>();
                    lstIM.Add("NoControl");
                    lstIM.Add("Inherit");
                    lstIM.Add("On");
                    lstIM.Add("Off");
                    lstIM.Add("Disable");
                    lstIM.Add("Alpha");
                    lstIM.Add("Close");
                    lstIM.Add("OnHalf");
                    lstIM.Add("Hiragana");
                    lstIM.Add("Katakana");
                    lstIM.Add("KatakanaHalf");
                    lstIM.Add("AlphaFull");
                    lstIM.Add("HangulFull");
                    lstIM.Add("Hangul");
                    cmbIM.DataSource = lstIM;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.AccessibleRole") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbAR = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbAR;
                    List<string> lstAR = new List<string>();
                    lstAR.Add("Default");
                    lstAR.Add("None");
                    lstAR.Add("TitleBar");
                    lstAR.Add("MenuBar");
                    lstAR.Add("ScrollBar");
                    lstAR.Add("Grip");
                    lstAR.Add("Sound");
                    lstAR.Add("Cursor");
                    lstAR.Add("Caret");
                    lstAR.Add("Character");
                    lstAR.Add("Alert");
                    lstAR.Add("Window");
                    lstAR.Add("Client");
                    lstAR.Add("MenuPopup");
                    lstAR.Add("MenuItem");
                    lstAR.Add("ToolTip");
                    lstAR.Add("Application");
                    lstAR.Add("HelpBalloon");
                    lstAR.Add("Document");
                    lstAR.Add("Pane");
                    lstAR.Add("Chart");
                    lstAR.Add("Dialog");
                    lstAR.Add("Border");
                    lstAR.Add("Grouping");
                    lstAR.Add("Separator");
                    lstAR.Add("Link");
                    lstAR.Add("ToolBar");
                    lstAR.Add("StatusBar");
                    lstAR.Add("Table");
                    lstAR.Add("ColumnHeader");
                    lstAR.Add("RowHeader");
                    lstAR.Add("Column");
                    lstAR.Add("Row");
                    lstAR.Add("Cell");
                    lstAR.Add("List");
                    lstAR.Add("ListItem");
                    lstAR.Add("Outline");
                    lstAR.Add("OutlineItem");
                    lstAR.Add("PageTab");
                    lstAR.Add("Indicator");
                    lstAR.Add("Graphic");
                    lstAR.Add("StaticText");
                    lstAR.Add("PropertyPage");
                    lstAR.Add("Text");
                    lstAR.Add("PushButton");
                    lstAR.Add("CheckButton");
                    lstAR.Add("RadioButton");
                    lstAR.Add("ComboBox");
                    lstAR.Add("DropList");
                    lstAR.Add("ProgressBar");
                    lstAR.Add("Dial");
                    lstAR.Add("HotkeyField");
                    lstAR.Add("Slider");
                    lstAR.Add("SpinButton");
                    lstAR.Add("Diagram");
                    lstAR.Add("Animation");
                    lstAR.Add("Equation");
                    lstAR.Add("ButtonDropDown");
                    lstAR.Add("ButtonMenu");
                    lstAR.Add("ButtonDropDownGrid");
                    lstAR.Add("WhiteSpace");
                    lstAR.Add("PageTabList");
                    lstAR.Add("Clock");
                    lstAR.Add("SplitButton");
                    lstAR.Add("IpAddress");
                    lstAR.Add("OutlineButton");
                    cmbAR.DataSource = lstAR;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.AnchorStyles") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbAS = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbAS;
                    List<string> lstAS = new List<string>();
                    lstAS.Add("None");
                    lstAS.Add("Right");
                    lstAS.Add("Top, Bottom, Right");
                    lstAS.Add("Bottom, Right");
                    lstAS.Add("Top, Left");
                    lstAS.Add("Top, Bottom, Left");
                    lstAS.Add("Left");
                    lstAS.Add("Bottom");
                    lstAS.Add("Top, Bottom");
                    lstAS.Add("Top");
                    cmbAS.DataSource = lstAS;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.HorizontalAlignment") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewComboBoxCell cmbHA = new DataGridViewComboBoxCell();
                    dgvProperties[e.ColumnIndex, e.RowIndex] = cmbHA;
                    List<string> lstHA = new List<string>();
                    lstHA.Add("Left");
                    lstHA.Add("Right");
                    lstHA.Add("Center");
                    cmbHA.DataSource = lstHA;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                }
                else if ((dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.String" ||
                    (dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Drawing.Point" ||
                    (dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Drawing.Size" ||
                    (dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Int32" ||
                    (dgvProperties[e.ColumnIndex - 2, e.RowIndex]).Value.ToString() == "System.Windows.Forms.Padding") {
                    strName = (dgvProperties[e.ColumnIndex - 1, e.RowIndex]).Value.ToString();
                    DataGridViewCell textBoxCell = dgvProperties.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    dgvProperties.CurrentCell = textBoxCell;
                    dgvProperties.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dgvProperties_EditingControlShowing);
                    dgvProperties.BeginEdit(true);
                }
            }
            #endregion
        }
        private void dgvProperties_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            //  Leave this here.  If it is removed, you will recieve an error.
        }

        private void PropertyForm_FormClosed(object sender, FormClosedEventArgs e) {
            frmMain.PropForm = null;
        }

        public void UpdatePropValue(string strPropName, object objPropNewValue) {
            foreach (DataGridViewRow dgvrOneRow in dgvProperties.Rows) {
                if (dgvrOneRow.Cells["Property Name"].Value.ToString() == strPropName) {
                    dgvrOneRow.Cells["Value"].Value = objPropNewValue;
                    break;
                }
            }
        }
    }
}
