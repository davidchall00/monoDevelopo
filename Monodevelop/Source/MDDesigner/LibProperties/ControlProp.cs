using System;

namespace LibProperties {
    // A class for a single property of a control.
    public class ControlProp {
        //fields
        private string strName;
        private Type tyValueType;
        private bool bHasValueSpecified;
        private Object objValue;

        public string Name {
            get {
                return strName;
            }
        }

        public Type ValueType {
            get {
                return tyValueType;
            }
        }

        public bool HasValueSpecified {
            get {
                return bHasValueSpecified;
            }
            set {
                bHasValueSpecified = value;
            }
        }

        public Object Value {
            get {
                return objValue;
            }
            set {
                objValue = value;
            }
        }

        //constructor
        public ControlProp(string strNm, object objV, Type tyPropT) {
            strName = strNm;
            objValue = objV;
            bHasValueSpecified = false;
            tyValueType = tyPropT;
        }
    }
}
