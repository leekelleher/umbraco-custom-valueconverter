/* Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
#if NET472
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
#endif

namespace Umbraco.Community.CustomValueConverter
{
    // NOTE: A custom `DataValueEditor` to specifically support `ToEditor()` calls, as it is called without DataType configuration.
    // ref: https://github.com/umbraco/Umbraco-CMS/blob/release-9.0.0/src/Umbraco.Core/Models/Mapping/ContentPropertyBasicMapper.cs#L81
    internal sealed class CustomValueConverterDataValueEditor : DataValueEditor
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        public CustomValueConverterDataValueEditor(
            IDataTypeService dataTypeService,
            PropertyEditorCollection propertyEditors,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
#if NET472
            : base()
#else
            : base(localizedTextService, shortStringHelper, jsonSerializer)
#endif
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
        }

#if NET472
        public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
#else
        public override object ToEditor(IProperty property, string culture = null, string segment = null)
#endif
        {
            var dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
            if (dataType != null &&
                dataType.Configuration is Dictionary<string, object> config &&
                config.TryGetValueAs(CustomValueConverterConfigurationEditor.DataType, out GuidUdi udi) == true)
            {
                var dataType2 = _dataTypeService.GetDataType(udi.Guid);
                if (dataType2 != null && _propertyEditors.TryGet(dataType2.EditorAlias, out var dataEditor2) == true)
                {
                    var valueEditor2 = dataEditor2.GetValueEditor(dataType2.Configuration);

#if NET472
                    return valueEditor2.ToEditor(property, dataTypeService, segment);
#else
                    return valueEditor2.ToEditor(property, culture, segment);
#endif
                }
            }

#if NET472
            return base.ToEditor(property, dataTypeService, segment);
#else
            return base.ToEditor(property, culture, segment);
#endif
        }
    }
}
