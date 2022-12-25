using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenJsonNetContractResolver : DomainContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Select(p => base.CreateProperty(p, memberSerialization))
                .ToList();

            var writableProps = props.Where(p => p.Writable || p.HasMemberAttribute).ToList();

            foreach (var prop in writableProps.Where(prop => prop != null))
            {
                // This ensures that marten queries can ask for values of public domain props (e.g. Assets) that are readonly,
                // but there are their writable alternatives with lower case (assets).
                prop.PropertyName = prop.PropertyName![..1].ToUpper() + prop.PropertyName![1..];
            }
            return writableProps;
        }
    }
}