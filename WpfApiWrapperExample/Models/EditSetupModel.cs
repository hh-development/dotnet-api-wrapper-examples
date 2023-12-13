using HHDev.Core.Enums;
using HHDev.Core.Helpers;
using HHDev.DataManagement.Api.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApiWrapperExample.Models
{
    [AddINotifyPropertyChangedInterface]
    public class EditSetupModel
    {
        private ApiSetupModel _apiSetup;
        private ApiCustomPropertyDefinitionModel _apiDefinition;

        public ApiAttachmentInfoModel SelectedAttachedFile { get; set; }

        public List<ApiAttachmentInfoModel> AttachedFiles => _apiSetup.AttachedFiles;

        public int Id { get; set; }

        public List<ISetupParameterModel> SetupParameters { get; } = new List<ISetupParameterModel>();

        public ISetupParameterModel NameParameter { get; private set; }

        public EditSetupModel(ApiSetupModel setup, ApiCustomPropertyDefinitionModel definition)
        {
            _apiSetup = setup;
            _apiDefinition = definition;
            SelectedAttachedFile = setup.AttachedFiles.FirstOrDefault();

            var name = (string)_apiSetup.Parameters["Name"];
            NameParameter = new TextSetupParameterModel("Name", (string)_apiSetup.Parameters["Name"]);
            SetupParameters.Add(NameParameter);

            foreach (var parameterDefinition in _apiDefinition.ParameterDefinitions)
            {
                var parameterName = (string)parameterDefinition.Parameters["Name"];
                var propertyType = (ePropertyType)Convert.ToInt32(parameterDefinition.Parameters["PropertyType"]);

                switch (propertyType)
                {
                    case ePropertyType.Double:
                        SetupParameters.Add(new DoubleSetupParameterModel(parameterName, (double?)_apiSetup.Parameters[parameterName]));
                        break;
                    case ePropertyType.Text:
                        SetupParameters.Add(new TextSetupParameterModel(parameterName, (string)_apiSetup.Parameters[parameterName]));
                        break;
                    case ePropertyType.DateTime:
                        SetupParameters.Add(new ReadOnlySetupParameterModel(parameterName, _apiSetup.Parameters[parameterName].ToStringInvariant(), "DateTime"));
                        break;
                    case ePropertyType.Boolean:
                        SetupParameters.Add(new BooleanSetupParameterModel(parameterName, (bool)_apiSetup.Parameters[parameterName]));
                        break;
                    case ePropertyType.Enum:
                    case ePropertyType.Integer:
                    case ePropertyType.Part:
                    case ePropertyType.Assembly:
                    default:
                        throw new NotImplementedException("These property types are not found in the ParameterDefinitions collection.");
                }
            }

            foreach (var partParameterDefinition in _apiDefinition.PartParameterDefinitions)
            {
                var parameterName = (string)partParameterDefinition.Parameters["Name"];
                var partItemParameterName = parameterName + "PartItem";

                var partValue = (ApiPartModel)_apiSetup.Parameters[parameterName];
                var partItemValue = (ApiPartItemModel)_apiSetup.Parameters[partItemParameterName];

                SetupParameters.Add(new ReadOnlySetupParameterModel(parameterName, partValue?.Name, "Part"));
                SetupParameters.Add(new ReadOnlySetupParameterModel(partItemParameterName, partItemValue?.Name, "Part Item"));

            }

            foreach (var assemblyParameterDefinition in _apiDefinition.AssemblyParameterDefinitions)
            {
                var parameterName = (string)assemblyParameterDefinition.Parameters["Name"];
                
                var assemblyIterationValue = (ApiAssemblyIterationModel)_apiSetup.Parameters[parameterName];

                SetupParameters.Add(new ReadOnlySetupParameterModel(parameterName, assemblyIterationValue?.Name, "Assembly Iteration"));
            }

            foreach (var mathParameterDefinition in _apiDefinition.MathParameterDefinitions)
            {
                var parameterName = (string)mathParameterDefinition.Parameters["Name"];

                var mathValue = _apiSetup.Parameters[parameterName].ToStringInvariant(); 

                SetupParameters.Add(new ReadOnlySetupParameterModel(parameterName, mathValue, "Math"));
            }

            SetupParameters = SetupParameters.OrderBy(x => x.Name).ToList();
        }
    }
}
