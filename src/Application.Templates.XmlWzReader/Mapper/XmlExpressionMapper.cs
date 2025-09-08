using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Mapper
{
    [Obsolete]
    public class XmlExpressionMapper<T> : IXmlMapperBuilder, IXmlMapper where T : class
    {
        private interface ISetter
        {
            void SetSub(object obj, XElement value);
            void SetRoot(T obj, XElement value);
        }
        private class SetterInfo<TProperty> : ISetter
        {
            public Type PropertyType;
            private readonly Action<T, TProperty> _setter;
            XmlExpressionMapper<T> Source { get; }
            public SetterInfo(XmlExpressionMapper<T> source ,Action<T, TProperty> setter)
            {
                Source = source;
                PropertyType = typeof(TProperty);
                _setter = setter;
            }

            public void SetSub(object obj, XElement value)
            {
                Source.SubMapper[PropertyType].Map(obj, value);
            }

            public void SetRoot(T obj, XElement value)
            {
                if (PropertyType == typeof(Point))
                    _setter(obj, (TProperty)(object)value.GetVectorValue());
                else if (PropertyType == typeof(int))
                    _setter(obj, (TProperty)(object)value.GetIntValue());
                else if (PropertyType == typeof(float))
                    _setter(obj, (TProperty)(object)value.GetFloatValue());
                else if (PropertyType == typeof(double))
                    _setter(obj, (TProperty)(object)value.GetDoubleValue());
                else if (PropertyType == typeof(string))
                    _setter(obj, (TProperty)(object)value.GetStringValue());
                else if (PropertyType == typeof(int[]))
                {
                    List<int> list = [];
                    foreach (var arrItem in value.Elements())
                    {
                        if (int.TryParse(arrItem.GetName(), out var idx))
                        {
                            list.Add(arrItem.GetIntValue());
                        }
                    }
                    _setter(obj, (TProperty)(object)list.ToArray());
                }
                else if (PropertyType.IsArray)
                {
                    Type listType = typeof(List<>).MakeGenericType(PropertyType.GetElementType());
                    var list = (System.Collections.IList)Activator.CreateInstance(listType);
                    foreach (var arrItem in value.Elements())
                    {
                        if (int.TryParse(arrItem.GetName(), out var idx))
                        {
                            var model = Activator.CreateInstance(PropertyType.GetElementType());
                            SetSub(model, arrItem);
                            list.Add(model);
                        }
                    }
                    var array = Array.CreateInstance(PropertyType.GetElementType(), list.Count);
                    list.CopyTo(array, 0);
                    _setter(obj, (TProperty)(object)array);
                }
                else if (PropertyType.IsClass)
                {
                    var model = Activator.CreateInstance(PropertyType);
                    foreach (var propItem in value.Elements())
                    {
                        SetSub(model, propItem);
                    }
                    _setter(obj, (TProperty)model);
                }
            }
        }

        private readonly Dictionary<string, ISetter> _setters = new(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<Type, IXmlMapper> SubMapper = new Dictionary<Type, IXmlMapper>();
        public string? CurrentNode { get; set; }

        public XmlExpressionMapper<T> CreateMap<TProperty>(string name, Expression<Func<T, TProperty>> propertyExpr)
        {
            if (propertyExpr.Body is not MemberExpression memberExpr)
                throw new ArgumentException("Expression must be a property access", nameof(propertyExpr));

            var paramObj = Expression.Parameter(typeof(T), "obj");
            var paramValue = Expression.Parameter(typeof(TProperty), "value");

            var assign = Expression.Assign(
                Expression.Property(paramObj, memberExpr.Member.Name),
                paramValue
            );

            var lambda = Expression.Lambda<Action<T, TProperty>>(assign, paramObj, paramValue);
            _setters[name] = new SetterInfo<TProperty>(this, lambda.Compile());

            return this;
        }

        public void Build()
        {
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var attrs = prop.GetCustomAttributes<WZPathAttribute>();
                if (attrs.Count() == 0)
                    continue;

                foreach (var attr in attrs)
                {
                    var pathArr = attr.Path.Split('/');
                    if (pathArr.Length == 1)
                        break;

                    if (attr.Path.EndsWith("/"))
                    {
                        var mapperObj = Activator.CreateInstance(typeof(XmlExpressionMapper<>).MakeGenericType(prop.PropertyType))!;
                        ((IXmlMapperBuilder)mapperObj).Build();
                        var mapper = (IXmlMapper)mapperObj;
                        mapper.CurrentNode = attr.Path.Substring(0, attr.Path.Length - 1);
                        SubMapper[prop.PropertyType] = mapper;
                    }
                    else
                        CreateProperyMapper(prop, attr.Path);
                }
            }
        }

        void CreateProperyMapper(PropertyInfo prop, string properyMapPath)
        {
            // 构建表达式 x => x.Prop
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.Property(param, prop);
            var lambda = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(typeof(T), prop.PropertyType),
                body,
                param
            );

            var method = typeof(XmlExpressionMapper<T>)
                .GetMethod(nameof(CreateMap))!
                .MakeGenericMethod(prop.PropertyType);


            method.Invoke(this, new object[] { properyMapPath, lambda });
        }

        public void MapDocument(T obj, XElement doc)
        {
            //foreach (var propNode in doc.Elements())
            //{
            //    var propName = propNode.GetName()!;
            //    if (_setters.TryGetValue(propName, out var info))
            //    {
            //        info.SetRoot(obj, propNode);
            //    }
            //}

            foreach (var item in _setters)
            {
                var sepIndex = item.Key.IndexOf("/");
                if (sepIndex == -1)
                    item.Value.SetRoot(obj, doc);
                else
                {
                    item.Value.SetRoot(obj, doc.Descendants().FirstOrDefault(x => x.GetName() == item.Key));
                }
            }
        }

        public void Map(object obj, XElement objNode)
        {
            var fullPath = CurrentNode == null ? "" : $"{CurrentNode}/{objNode.GetName()}";
            if (_setters.TryGetValue(fullPath, out var info))
            {
                info.SetSub(obj, objNode);
            }
        }
    }
}
