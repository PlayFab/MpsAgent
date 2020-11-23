// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Extensions
{
    using System;
    using System.Reflection;

    public static class EnumExtensions
    {
        public static bool IsObsolete(this Enum value)
        {
            Type enumType = value.GetType();
            string enumName = enumType.GetEnumName(value);
            FieldInfo fieldInfo = enumType.GetField(enumName);
            return Attribute.IsDefined(fieldInfo, typeof(ObsoleteAttribute));
        }
    }
}
