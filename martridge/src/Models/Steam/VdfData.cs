using System.Collections.Generic;
namespace Martridge.Models.Steam {


    public class VdfObject {
        public string Name { get; }
        public Dictionary<string, VdfData> Properties { get; } = new Dictionary<string, VdfData>();
        
        public Dictionary<string, VdfObject> Children { get; } = new Dictionary<string, VdfObject>();

        public VdfObject(string name) {
            this.Name = name;
        }
    }
    public class VdfData {
        public string Name { get; }
        public Constants.VdfDataType DataType { get; }
        public object? Value { get; }

        public VdfData(string name, Constants.VdfDataType dataType, object value) {
            this.Name = name;
            this.DataType = dataType;
            this.Value = value;
        }

        public override string ToString() {
            return "<" + this.Name + "=" + this.Value + ">";
        }
    }
}
