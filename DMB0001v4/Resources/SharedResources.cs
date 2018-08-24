using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Threading;

//[assembly: NeutralResourcesLanguage("en")]

namespace DMB0001v4.Resources
{

    public class SharedResources
    {
        public string Get(string key)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // get a list of resource names from the manifest
            string[] resNames = assembly.GetManifestResourceNames();
            
            // set UI culture
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            // create a resourcemanager to load satellite assembly
            ResourceManager resMan = new ResourceManager(resNames[0], Assembly.GetExecutingAssembly());//"SatelliteAssemblyClient.MyResources"

            // set label
            string str = resMan.GetString(key);

            return str;
        }
    }
}
