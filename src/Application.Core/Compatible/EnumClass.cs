using System.Reflection;

namespace Application.Core.Compatible
{
    public class EnumClass
    {
        protected static List<TModel> GetValues<TModel>() where TModel : EnumClass
        {
            return typeof(TModel).GetFields().Where(x => x.IsStatic
                        && x.IsPublic
                        && x.FieldType == typeof(TModel)).Select(x => (TModel)x.GetValue(null)!).ToList();
        }

        public static TModel[] values<TModel>() where TModel : EnumClass
        {
            return typeof(TModel).GetFields().Where(x => x.IsStatic
                        && x.IsPublic
                        && x.FieldType == typeof(TModel)).Select(x => (TModel)x.GetValue(null)!).ToArray();
        }

        protected FieldInfo GetInfo()
        {
            return GetType().GetFields().Where(x => x.IsStatic
                && x.IsPublic
                && x.FieldType == GetType()).Where(x => x.GetValue(null) == this).FirstOrDefault()!;
        }

        public virtual int ordinal()
        {
            return GetType().GetFields().ToList().FindIndex(x => x.IsStatic
                && x.IsPublic
                && x.FieldType == GetType()
                && x.GetValue(null) == this);
        }

        public string name() => GetInfo().Name;
    }
}
