using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data
{
    public static class DbInitializer
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using (serviceProvider.CreateScope())
            {
                var context = serviceProvider.GetRequiredService<UserDbContext>();

                context.Database.Migrate();

                context.Database.ExecuteSqlRaw(Load("AuthApi.StoredProcedures.core.Drop.sql"));
                context.Database.ExecuteSqlRaw(Load("AuthApi.StoredProcedures.core.AuthApi_Login.sql"));
                context.Database.ExecuteSqlRaw(Load("AuthApi.StoredProcedures.core.User_By_UserName_S.sql"));
            }        
        }

        private static string Load(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using Stream? stream = assembly.GetManifestResourceStream(name);
            if (stream is not null)
            {
                using StreamReader reader = new StreamReader(stream);
                string result = reader.ReadToEnd();
                return result;
            }

            return "";
        }
    }
}
