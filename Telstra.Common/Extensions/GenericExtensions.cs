using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Telstra.Common
{
    public static class GenericExtensions
    {
        public static T IsNotNull<T>(this object @this, string varName = null)
        {
            if (@this is null) throw new ArgumentNullException(varName);
            return (T)@this;
        }

        public static TDestination Map<TSource, TDestination>(this TSource source, bool excludeNulls = true)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => excludeNulls && srcMember != null));
            });

            IMapper mapper = config.CreateMapper();
            return mapper.Map<TDestination>(source);
        }

        public static TDestination NativeMap<TSource, TDestination>(this TSource source)
        {
            var obj = (TDestination)Activator.CreateInstance(typeof(TDestination));
            if (obj != null)
            {
                source.GetType().GetProperties().Where(m => m.CanRead).ToList().ForEach(prop =>
                {
                    var prop2 = obj.GetType().GetProperty(prop.Name);
                    if (prop2 != null && prop2.CanWrite && prop2.PropertyType == prop.PropertyType)
                        prop2.SetValue(obj, prop.GetValue(source));
                });
            }
            return obj;
        }

    }
}
