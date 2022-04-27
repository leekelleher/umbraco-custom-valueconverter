/* Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
#if NET472
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using UmbConstants = Umbraco.Core.Constants;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using UmbConstants = Umbraco.Cms.Core.Constants;
#endif

namespace Umbraco.Community.CustomValueConverter
{
    public sealed class CustomValueConverterConfigurationEditor : ConfigurationEditor
    {
        internal const string DataType = "dataType";
        internal const string ValueConverter = "valueConverter";
        internal const string ItemPickerViewPath = "~/App_Plugins/CustomValueConverter/item-picker.html";
        internal const string ItemPickerOverlayViewPath = "~/App_Plugins/CustomValueConverter/item-picker.overlay.html";
        internal const string NotesViewPath = "~/App_Plugins/CustomValueConverter/notes.html";

        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        public CustomValueConverterConfigurationEditor(
            IDataTypeService dataTypeService,
            PropertyEditorCollection propertyEditors,
            PropertyValueConverterCollection propertyValueConverters,
            IShortStringHelper shortStringHelper,
            IIOHelper ioHelper)
            : base()
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;

            Fields.Add(new ConfigurationField
            {
                Key = "notes",
                Name = "Notes",
                View = ioHelper.ResolveRelativeOrVirtualUrl(NotesViewPath),
                Config = new Dictionary<string, object>
                {
                    { "heading", "A note about changing the default value converter." },
                    { "message", @"<p>Some of Umbraco's built-in value converters are designed to work explicitly with their associated property editor and data type configuration.</p>
<p>This means they may not be hot-swappable. For example, the <code>MediaPicker</code> and <code>MultiNodeTreePicker</code> value converters will try to detect if the data type is configured for single or multiple use. So if you wanted to change the value converter for a textstring editor, then the target value converter would not be aware of the intended configuration, and potentially cause an error.</p>
<p>For primitive values, e.g. integer, decimal, string, boolean. These should work as expected.</p>
<p>If you are using you own custom value converter code, then you don't need to be concerned with this note, you will have full control over the value conversion.</p>" },
                },
                HideLabel = true,
            });

            Fields.Add(new ConfigurationField
            {
                Key = DataType,
                Name = "Data type",
                Description = "Select the data type to be wrapped with a value converter.",
                View = "treepicker",
                Config = new Dictionary<string, object>
                {
                    { "multiPicker", false },
                    { "entityType", nameof(UmbConstants.ObjectTypes.DataType) },
                    { "type", UmbConstants.Applications.Settings },
                    { "treeAlias", UmbConstants.Trees.DataTypes },
                    { "idType", "udi" },
                }
            });

            var items = propertyValueConverters.Select(x =>
            {
                var type = x.GetType();

                return new
                {
                    name = type.Name.SplitPascalCasing(shortStringHelper),
                    value = type.FullName,
                    description = type.FullName,
                };
            });

            Fields.Add(new ConfigurationField
            {
                Key = ValueConverter,
                Name = "Value converter",
                Description = "Select the value converter to use.",
                View = ioHelper.ResolveRelativeOrVirtualUrl(ItemPickerViewPath),
                Config = new Dictionary<string, object>
                {
                    { "defaultIcon", CustomValueConverterDataEditor.DataEditorIcon },
                    { "items", items },
                    { "overlayView", ioHelper.ResolveRelativeOrVirtualUrl(ItemPickerOverlayViewPath) },
                }
            });
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var config = base.ToValueEditor(configuration);

            if (config.TryGetValueAs(DataType, out GuidUdi udi) == true)
            {
                var dataType = _dataTypeService.GetDataType(udi.Guid);
                if (dataType != null && _propertyEditors.TryGet(dataType.EditorAlias, out var dataEditor) == true)
                {
                    return dataEditor.GetConfigurationEditor().ToValueEditor(dataType.Configuration);
                }
            }

            return base.ToValueEditor(configuration);
        }
    }
}
