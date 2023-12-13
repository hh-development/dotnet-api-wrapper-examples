using HHDev.Core.Helpers;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApiWrapperExample.Models
{   
    public class DoubleSetupParameterModel : TypedScalarSetupParameterModel<double?>
    {
        public override string Type => "Double";

        public DoubleSetupParameterModel(string name, double? value) : base(name, value)
        {
            
        }        
    }

    public class TextSetupParameterModel : TypedScalarSetupParameterModel<string>
    {
        public override string Type => "Text";

        public TextSetupParameterModel(string name, string value) : base(name, value)
        {

        }
    }

    public class BooleanSetupParameterModel : TypedScalarSetupParameterModel<bool>
    {
        public override string Type => "Boolean";

        public override string GetValueString()
        {
            return Value.ToString();
        }

        public BooleanSetupParameterModel(string name, bool value) : base(name, value)
        {

        }
    }

    public class ReadOnlySetupParameterModel : ISetupParameterModel
    {
        public ReadOnlySetupParameterModel(string name, string value, string type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public string Name { get; }
        public string Value { get; }

        public bool IsDirty => false;

        public string Type { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string GetValueString()
        {
            throw new NotImplementedException();
        }
    }

    [AddINotifyPropertyChangedInterface]
    public abstract class TypedScalarSetupParameterModel<T> : ISetupParameterModel
    {
        public TypedScalarSetupParameterModel(string name, T value)
        {
            Name = name;
            Value = value;
            IsDirty = false;
        }

        public string Name { get; }
        public bool IsDirty { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual string GetValueString()
        {
            return Value.ToStringInvariant();
        }

        public T Value { get; set; }

        public abstract string Type { get; }

        private void OnValueChanged()
        {
            IsDirty = true;
        }
    }


    public interface ISetupParameterModel : INotifyPropertyChanged
    {
        string Name { get; }
        bool IsDirty { get; }
        string Type { get; }
        string GetValueString();
    }
}
