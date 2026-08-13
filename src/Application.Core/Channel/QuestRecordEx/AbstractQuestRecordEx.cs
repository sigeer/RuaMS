using Application.Core.Channel.Net.Packets;
using System.Globalization;
using System.Reflection;

namespace Application.Core.Channel.QuestRecordEx
{
    public abstract class AbstractQuestRecordEx
    {
        protected AbstractQuestRecordEx(short questId, string? rawContent)
        {
            QuestId = questId;

            Parse(rawContent);
        }

        [QuestRecordExIgnoreKey]
        public short QuestId { get; }

        string GetPropName(PropertyInfo propertyInfo)
        {
            var info = propertyInfo.GetCustomAttribute<QuestRecordExKeyAttribute>();
            if (info != null)
            {
                return info.Name;
            }
            return propertyInfo.Name;
        }

        protected virtual IEnumerable<string> GenerateData()
        {
            var activeProps = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<QuestRecordExIgnoreKeyAttribute>() == null);
            return activeProps.Select(x => $"{GetPropName(x)}={x.GetValue(this)}");
        }

        public override string ToString()
        {
            return string.Join(';', GenerateData());
        }

        protected virtual void Parse(string? rawContent)
        {
            var dic = KeyValueStringParser.Parse(rawContent);
            if (dic.Any())
            {
                var activeProps = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<QuestRecordExIgnoreKeyAttribute>() == null);
                foreach (var prop in activeProps)
                {
                    if (dic.TryGetValue(GetPropName(prop), out var value))
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        if (targetType == typeof(string))
                        {
                            prop.SetValue(this, value);
                        }
                        else if (!string.IsNullOrEmpty(value))
                        {
                            try
                            {
                                prop.SetValue(this, Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
                            }
                            catch
                            {
                                // 忽略无法转换的脏数据，保留默认值
                            }
                        }
                    }

                }
            }

        }

        public async Task Flush(Player chr)
        {
            var value = ToString();
            chr.AreaInfo[QuestId] = value;
            await chr.SendPacket(MessagePacket.QuestRecordEx(QuestId, value));
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class QuestRecordExKeyAttribute : Attribute
    {
        public QuestRecordExKeyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class QuestRecordExIgnoreKeyAttribute : Attribute
    {
    }
}
