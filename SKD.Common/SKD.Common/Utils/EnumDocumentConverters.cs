using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Models;

namespace SKD.Common.Utils
{
    public class EnumListConverter<T> : DocumentConverter where T : Enum
    {
        public EnumListConverter(Type targetType) : base(targetType) { }

        public override bool ConvertFrom(DocumentObject value, out object? result)
        {
            result = value.List.Select(x => (T)Enum.Parse(typeof(T), x.String)).ToList();
            return true;
        }

        public override bool ConvertTo(object? value, out object? result)
        {
            if (value is List<T> list)
            {
                result = list.Select(x => x.ToString()).ToList();
                return true;
            }
            result = null;
            return false;
        }
    }

    public abstract class EnumKeyDictionaryConverter<TKey, TValue> : DocumentConverter where TKey : Enum
    {
        protected Func<DocumentObject, TValue>? valueMapper;
        public EnumKeyDictionaryConverter(Type targetType) : base(targetType) { }

        public override bool ConvertFrom(DocumentObject value, out object? result)
        {
            if (!(valueMapper is null))
            {
                result = value.Dictionary.ToDictionary(
                    kvp => (TKey)Enum.Parse(typeof(TKey), kvp.Key),
                    kvp => valueMapper.Invoke(kvp.Value));
                return true;
            }
            throw new InvalidOperationException("Must set value mapper to a non-null function");
        }

        public override bool ConvertTo(object? value, out object? result)
        {
            if (value is Dictionary<TKey, TValue> dict)
            {
                result = dict.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                return true;
            }
            result = null;
            return false;
        }
    }

    public class ProjectIMADictionaryConverter : EnumKeyDictionaryConverter<ImpactMeasurementArea, ImpactMeasurementAreaExplanation>
    {
        public ProjectIMADictionaryConverter(Type targetType) : base(targetType)
        {
            valueMapper = obj => new ImpactMeasurementAreaExplanation()
            {
                En = obj.Dictionary["En"].String,
                Es = obj.Dictionary["Es"].String
            };
        }
    }
}
