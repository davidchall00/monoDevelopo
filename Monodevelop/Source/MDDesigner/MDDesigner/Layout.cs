using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using LibProperties;

namespace MDDesigner {
    public class Layout : Control {
        private List<ControlProp> lstPropertyList;

        // Initialize a list of the properties needing to be changed when editing the Control.
        public List<ControlProp> PropertyList { get { return lstPropertyList; } }

        // Initialize the actual control. For now, initialize to null.
        private Control RealControl = null;

        // Boolean to keep track of the Control if it is selected or not.
        private bool Selected = false;

        private const int iResize = 5;

        private Form1 frmMain = null;

        public Control RC { get { return this.RealControl; } }

        // Constructor.
        public Layout(Type RealControlType, string Name, Point InitLocation, Form1 frmMn) {
            frmMain = frmMn;
            // Create and set up the real control.
            RealControl = Activator.CreateInstance(RealControlType) as Control;
            RealControl.Name = Name;
            RealControl.Text = Name;
            RealControl.Location = new Point(5, 5);

            // We are initially selected.
            this.Selected = true;
            this.Name = Name + "Layout";
            this.Location = new Point(InitLocation.X - 5, InitLocation.Y - 5);
            this.Size = new Size(RealControl.Width + 10, RealControl.Height + 10);
            // Add real control to our list of subcontrols.
            this.Controls.Add(RealControl);
            // Associate the "OnLeave" method with the "Leave" event.
            this.Leave += new EventHandler(this.OnLeave);
            // Associate the "OnFocus" method with the "Click" event.
            this.Enter += new EventHandler(this.OnFocus);
            // Automatically redraw whenever we are resized.
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            Control_Move();
            this.ResizeRedraw = true;
            this.Resize += new EventHandler(this.OnResize);
            // Set up our basic properties.
            lstPropertyList = new List<ControlProp>();
            PropertyInfo[] propInfos = RealControlType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propInfo in propInfos) {
                if (propInfo.CanWrite) {
                    ControlProp cpNewProperty = new ControlProp(propInfo.Name, propInfo.GetValue(RealControl), propInfo.PropertyType);
                    lstPropertyList.Add(cpNewProperty);
                }
            }
            ControlProp cpLocation = lstPropertyList.Find(cpProp => (cpProp.Name == "Location"));
            cpLocation.Value = this.Location + new Size(5, 5);
            cpLocation.HasValueSpecified = true;
            ControlProp cpSize = lstPropertyList.Find(cpProp => (cpProp.Name == "Size"));
            cpSize.HasValueSpecified = true;
            lstPropertyList.Sort((cpFirst, cpSecond) => (cpFirst.Name.CompareTo(cpSecond.Name)));
            if (frmMain.PropForm == null) {
                frmMain.PropForm = new PropertyForm(lstPropertyList, this, frmMain);
                frmMain.PropForm.Show();
            }
            else {
                frmMain.PropForm.vReload(lstPropertyList, this);
            }
        }

        private Point MouseDownLocation = new Point();

        public void Control_Move() {
            //   XXXX Setting to allow drop, set to true.
            this.AllowDrop = true;

            RealControl.MouseDown += (ss, ee) => {
                if (ee.Button == System.Windows.Forms.MouseButtons.Left) { MouseDownLocation = Control.MousePosition; }
            };

            RealControl.MouseMove += (ss, ee) => {
                if (ee.Button == System.Windows.Forms.MouseButtons.Left) {
                    Point temporary = Control.MousePosition;
                    Point res = new Point(MouseDownLocation.X - temporary.X, MouseDownLocation.Y - temporary.Y);
                    this.Location = new Point(this.Location.X - res.X, this.Location.Y - res.Y);

                    MouseDownLocation = temporary;
                }
                ControlProp cpLocation = lstPropertyList.Find(cpProp => (cpProp.Name == "Location"));
                cpLocation.Value = this.Location + new Size(5, 5);
                cpLocation.HasValueSpecified = true;
                if (frmMain.PropForm != null) {
                    frmMain.PropForm.UpdatePropValue("Location", new Point(this.Location.X + 5, this.Location.Y + 5));
                }
            };
        }

        protected override void OnPaint(PaintEventArgs e) {
            // Do the base class painting.
            base.OnPaint(e);
            Rectangle rectObject = new Rectangle(5, 5, this.Width - 11, this.Height - 11);
            Rectangle rectTopLeft = new Rectangle(0, 0, 5, 5);
            Rectangle rectLeftMiddle = new Rectangle(0, this.RealControl.Height / 2 + 2, 5, 5);
            Rectangle rectBottomLeft = new Rectangle(0, this.Height - 6, 5, 5);
            Rectangle rectBottomMiddle = new Rectangle(this.RealControl.Width / 2 + 2, this.Height - 6, 5, 5);
            Rectangle rectBottomRight = new Rectangle(this.Width - 6, this.Height - 6, 5, 5);
            Rectangle rectRightMiddle = new Rectangle(this.Width - 6, this.RealControl.Height / 2 + 2, 5, 5);
            Rectangle rectTopRight = new Rectangle(this.Width - 6, 0, 5, 5);
            Rectangle rectTopMiddle = new Rectangle(this.RealControl.Width / 2 + 2, 0, 5, 5);

            if (Selected) {
                // Draw a rectangle around the real control.
                using (Pen myPen = new Pen(Color.Black)) {
                    e.Graphics.DrawRectangle(myPen, rectObject);
                    e.Graphics.DrawRectangle(myPen, rectBottomRight);
                    e.Graphics.DrawRectangle(myPen, rectTopLeft);
                    e.Graphics.DrawRectangle(myPen, rectBottomLeft);
                    e.Graphics.DrawRectangle(myPen, rectTopRight);
                    e.Graphics.DrawRectangle(myPen, rectTopMiddle);
                    e.Graphics.DrawRectangle(myPen, rectLeftMiddle);
                    e.Graphics.DrawRectangle(myPen, rectBottomMiddle);
                    e.Graphics.DrawRectangle(myPen, rectRightMiddle);
                }
            }
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (m.Msg == 0x84) {
                Point pClient = this.PointToClient(new Point(m.LParam.ToInt32()));
                // Bottom Right. Done
                if (pClient.X >= this.ClientSize.Width - iResize && pClient.Y >= this.ClientSize.Height - iResize) {
                    m.Result = new IntPtr(17);
                }
                // Middle Right. Done
                else if (pClient.X >= this.ClientSize.Width - iResize && pClient.Y >= this.ClientSize.Height / 2 - 2 && pClient.Y <= this.ClientSize.Height / 2 + 2) {
                    m.Result = new IntPtr(11);
                }
                // Top Right. Done
                else if (pClient.X >= this.ClientSize.Width - iResize && pClient.Y <= iResize) {
                    m.Result = new IntPtr(14);
                }
                //Top Middle. 
                else if (pClient.X >= this.ClientSize.Width / 2 - 2 && pClient.X <= this.ClientSize.Width / 2 + 2 && pClient.Y <= iResize) {
                    m.Result = new IntPtr(12);
                }
                // Top Left. Done
                else if (pClient.X <= iResize && pClient.Y <= iResize) {
                    m.Result = new IntPtr(13);
                }
                // Middle left. Done
                else if (pClient.Y >= this.ClientSize.Height / 2 - 2 && pClient.Y <= this.ClientSize.Height / 2 + 2 && pClient.X <= iResize) {
                    m.Result = new IntPtr(10);
                }
                // Bottom Left. Done
                else if (pClient.X <= iResize && pClient.Y >= this.ClientSize.Height - iResize) {
                    m.Result = new IntPtr(16);
                }
                // Bottom Middle.
                else if (pClient.X >= this.ClientSize.Width / 2 - 2 && pClient.X <= this.ClientSize.Width / 2 + 2 && pClient.Y >= this.ClientSize.Height - iResize) {
                    m.Result = new IntPtr(15);
                }
            }
        }

        private Predicate<ControlProp> prdSize = (ControlProp cp) => { return cp.Name == "Size"; };

        protected void OnResize(object sender, EventArgs e) {
            RealControl.Width = this.Width - 10;
            RealControl.Height = this.Height - 10;

            if (frmMain.PropForm != null) {
                frmMain.PropForm.UpdatePropValue("Size", new Size(RealControl.Width, RealControl.Height));
                frmMain.PropForm.UpdatePropValue("Height", RealControl.Height);
                frmMain.PropForm.UpdatePropValue("Width", RealControl.Width);
            }
            else {
                PropagateChange("Size", new Size(RealControl.Width, RealControl.Height));
                PropagateChange("Height", RealControl.Height);
                PropagateChange("Width", RealControl.Width);
            }
            ControlProp cpSize = lstPropertyList.Find(prdSize);
            cpSize.HasValueSpecified = true;
        }

        public void PropagateChange(string strName, object objNewValue) {
            ControlProp cpChanged = lstPropertyList.Find(cpProp => (cpProp.Name == strName));
            cpChanged.Value = objNewValue;
            if (strName == "Size") {
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(Size));
                Size szNewSize = (Size)tc.ConvertFromString(objNewValue.ToString());
                this.RealControl.Size = szNewSize;
                Size = szNewSize + new Size(10, 10);
            }
            else if (strName == "Width") {
                TypeConverter tcW = TypeDescriptor.GetConverter(typeof(int));
                int iNewWidth = (int)tcW.ConvertFromString(objNewValue.ToString());
                this.RealControl.Width = iNewWidth;
                this.Width = iNewWidth + 10;
            }
            else if (strName == "Height") {
                TypeConverter tcH = TypeDescriptor.GetConverter(typeof(int));
                int iNewHeight = (int)tcH.ConvertFromString(objNewValue.ToString());
                this.RealControl.Height = iNewHeight;
                this.Height = iNewHeight + 10;
            }
            else if (strName == "Location") {
                TypeConverter tcL = TypeDescriptor.GetConverter(typeof(Point));
                Point ptNewLocation = (Point)tcL.ConvertFromString(objNewValue.ToString());
                this.Location = ptNewLocation + new Size(-5, -5);
            }
            else if (strName == "BackColor" || strName == "ForeColor") {
                if (objNewValue.ToString() == "Color[ControlDark]") {
                    this.RealControl.BackColor = SystemColors.ControlDark;
                }
                else if (objNewValue.ToString() == "Color[Control]") {
                    this.RealControl.BackColor = SystemColors.Control;
                }
                else if (objNewValue.ToString() == "Color[ControlLight]") {
                    this.RealControl.BackColor = SystemColors.ControlLight;
                }
                else if (objNewValue.ToString() == "Color[Highlight]") {
                    this.RealControl.BackColor = SystemColors.Highlight;
                }
            }
            else if (strName == "AccessibleRole") {
            }
            else if (strName == "AllowDrop") {
            }
            else if (strName == "Anchor") {
            }
            else if (strName == "AutoEllipsis") {
            }
            else if (strName == "AutoScrollOffset") {
            }
            else if (strName == "AutoSize") {
            }
            else if (strName == "AutoSizeMode") {
            }
            else if (strName == "BackgroundImage") {
            }
            else if (strName == "BackgroundImageLayout") {
            }
            else if (strName == "Capture") {
            }
            else if (strName == "CausesValidation") {
            }
            else if (strName == "ClientSize") {
            }
            else if (strName == "DialogResult") {
            }
            else if (strName == "Dock") {
                if (objNewValue.ToString() == "None") {
                    Dock = DockStyle.None;
                }
                else if (objNewValue.ToString() == "Top") {
                    Dock = DockStyle.Top;
                }
                else if (objNewValue.ToString() == "Left") {
                    Dock = DockStyle.Left;
                }
                else if (objNewValue.ToString() == "Fill") {
                    Dock = DockStyle.Fill;
                }
                else if (objNewValue.ToString() == "Right") {
                    Dock = DockStyle.Right;
                }
                else if (objNewValue.ToString() == "Bottom") {
                    Dock = DockStyle.Bottom;
                }
            }
            else if (strName == "Enabled") {
            }
            else if (strName == "FlatStyle") {
            }
            else if (strName == "ImageAlign") {
            }
            else if (strName == "IsAccessible") {
            }
            else if (strName == "ImageIndex") {
            }
            else if (strName == "ImeMode") {
            }
            else if (strName == "Left") {
            }
            else if (strName == "Margin") {
            }
            else if (strName == "MaximumSize") {
            }
            else if (strName == "MinimumSize") {
            }
            else if (strName == "Padding") {
            }
            else if (strName == "TabIndex") {
            }
            else if (strName == "TabStop") {
            }
            else if (strName == "TextAlign") {
                if (objNewValue.ToString() == "Left" || objNewValue.ToString() == "Right" || objNewValue.ToString() == "Right") {

                }
            }
            else if (strName == "TextImageRelation") {
            }
            else if (strName == "Top") {
            }
            else if (strName == "UseVisualStyleBackColor") {
            }
            else if (strName == "UseCompatibleTextRendering") {
            }
            else if (strName == "UseMnemonic") {
            }
            else if (strName == "UseWaitCursor") {
            }
            else if (strName == "Visible") {
            }
            else if (strName == "ReadOnly") {
            }
            else if (strName == "Multiline") {
            }
            else {
                Type tType = this.RealControl.GetType();
                PropertyInfo piPropInfo = tType.GetProperty(strName);
                piPropInfo.SetValue(this.RealControl, objNewValue);
            }
            }

        protected void OnLeave(object sender, EventArgs e) {
            Selected = false;
            this.Refresh();
        }

        protected void OnFocus(object sender, EventArgs e) {
            Selected = true;
            this.Refresh();
            if (frmMain.PropForm == null) {
                frmMain.PropForm = new PropertyForm(lstPropertyList, this, frmMain);
                frmMain.PropForm.Show();
            }
            else {
                frmMain.PropForm.vReload(lstPropertyList, this);
            }
        }
    }
}
