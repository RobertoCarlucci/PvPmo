namespace PvPmo.Util
{
    public static class ServiceHelper
    {
        public static T? GetService<T>() => Current.GetService<T>();
        public static IServiceProvider? Current =>
            Application.Current?.Handler?.MauiContext?.Services;
    }

}
