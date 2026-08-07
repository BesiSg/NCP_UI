using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Utility.Lib.Ticket;

namespace Utility
{
    public static class Extension
    {
        public static string ToPascalCase(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var words = Regex.Split(value, @"[\s_\-]+")
                             .Where(x => !string.IsNullOrWhiteSpace(x));

            return string.Concat(
                words.Select(w =>
                    char.ToUpperInvariant(w[0]) +
                    w.Substring(1).ToLowerInvariant()));
        }
        public static string GetAttri<T>(this MemberInfo memberInfo)
        {
            var attrs = System.Attribute.GetCustomAttributes(memberInfo, typeof(TextAttribute));

            foreach (var attr in attrs)
            {
                if (attr is TextAttribute)
                {
                    return ((TextAttribute)attr).Attribute;
                }
            }
            return string.Empty;
        }

        public static string GetAttribute<T>(this T _, Expression<Func<T, object>> propertyAccessorExpression)
        {
            if (propertyAccessorExpression.Body is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    var attribute = propertyInfo.GetCustomAttribute<TextAttribute>();

                    if (attribute != null)
                    {
                        return attribute.Attribute;
                    }
                }
            }

            return default;
        }

        public static void CopyProperties<T>(this T destination, T source)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Source or/and Destination objects are null");

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    object value = property.GetValue(source, null);
                    property.SetValue(destination, value, null);
                }
            }
        }
        public static bool AreEqual<T>(this T obj1, T obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 == null || obj2 == null)
                return false;

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead)
                    continue;

                object value1 = property.GetValue(obj1);
                object value2 = property.GetValue(obj2);

                if (value1 == null && value2 == null)
                    continue;

                if (value1 == null || value2 == null)
                    return false;

                if (!value1.Equals(value2))
                    return false;
            }

            return true;
        }

        public static IEnumerable<string> GetAttribute<T>(this Object obj)
        {
            PropertyInfo[] props = typeof(Object).GetProperties();
            var Out = new List<string>();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (!(attr is TextAttribute)) continue;
                    if (attr is null) continue;
                    var authAttr = attr as TextAttribute;
                    Out.Add(authAttr.Attribute);
                }
            }
            return Out;
        }
        public static void AddRange<T>(this ObservableCollection<T> obj, IEnumerable<T> source)
        {
            source.ToList().ForEach(item => obj.Add(item));
        }

        public static string GetBeautifulSoup(this string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var cleanedtext = htmlDoc.DocumentNode.InnerText.Replace("&nbsp;", " ");
            cleanedtext = cleanedtext.Replace("&amp;amp;", "&amp;");
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(cleanedtext, re, "");
        }

        public static string ToPlainText(this AdfDoc doc)
        {
            if (doc?.Content == null || doc.Content.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var node in doc.Content)
                AppendNode(sb, node);

            return sb.ToString().Trim();
        }

        private static void AppendNode(StringBuilder sb, AdfNode node)
        {
            if (node == null) return;

            switch (node.Type)
            {
                case "paragraph":
                    if (node.Content != null)
                    {
                        foreach (var child in node.Content)
                            AppendNode(sb, child);
                    }
                    sb.AppendLine(); // end of paragraph
                    break;

                case "text":
                    if (!string.IsNullOrEmpty(node.Text))
                        sb.Append(node.Text);
                    break;

                case "hardBreak":
                    sb.AppendLine();
                    break;

                default:
                    // For any other node types, just walk children if present.
                    if (node.Content != null)
                    {
                        foreach (var child in node.Content)
                            AppendNode(sb, child);
                    }
                    break;
            }
        }
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attr = field?
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault();

            return attr?.Description ?? value.ToString();
        }

        public static TEnum ToEnumByAttribute<TEnum>(this string value)
            where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields(
                         BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<NameAttribute>();

                if (attribute != null &&
                    string.Equals(attribute.Name,
                                  value,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    return (TEnum)field.GetValue(null)!;
                }
            }

            throw new ArgumentException(
                $"No {typeof(TEnum).Name} found for '{value}'.");
        }

        public static bool TryToEnumByAttribute<TEnum>(
            this string value,
            out TEnum result)
            where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields(
                         BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<NameAttribute>();

                if (attribute != null &&
                    string.Equals(attribute.Name,
                                  value,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    result = (TEnum)field.GetValue(null)!;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }

    public static class ObjectCopy
    {
        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }
        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    ArrayExtensions.ForEach(clonedArray, (array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((Object)original);
        }
    }
    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }
    public static class ArrayExtensions
    {
        public static void ForEach(Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            this.maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                this.maxLengths[i] = array.GetLength(i) - 1;
            }
            this.Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < this.Position.Length; ++i)
            {
                if (this.Position[i] < this.maxLengths[i])
                {
                    this.Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        this.Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }


    public interface INameAttribute
    {
        string Name { get; }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class NameAttribute : Attribute, INameAttribute
    {
        public string Name { get; }

        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
