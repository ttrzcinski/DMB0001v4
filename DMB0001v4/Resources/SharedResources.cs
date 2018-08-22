using Microsoft.Extensions.Localization;
using System;
using System.Reflection;
using System.Resources;

using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Text;
using System.Threading;

//[assembly: NeutralResourcesLanguage("en")]

namespace DMB0001v4.Resources
{

    public class SharedResources
    {
        public string Get(string key)
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            var assembly = Assembly.GetExecutingAssembly();
            // get a list of resource names from the manifest
            string[] resNames = assembly.GetManifestResourceNames();
            /*//StringBuilder strbld = new StringBuilder();
            //foreach (var res in resNames)
            //{
            //    strbld.Append(res).AppendLine();
            //}

            //ResourceManager rm = new ResourceManager("DMB0001v4.Resources.Phrases.resources", Assembly.GetExecutingAssembly());
            //resNames[0] = DMB0001v4.Resources.Phrases.resources
            // Create a resource manager to retrieve resources.
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            ResourceManager rm = new ResourceManager(resNames[0], Assembly.GetEntryAssembly());
            //ResourceManager rm = new ResourceManager("DMB0001v4.Resource1", Assembly.GetExecutingAssembly());
            //ResourceManager rm = new ResourceManager("Phrases", Assembly.GetExecutingAssembly());

            // Retrieve the value of the string resource named "welcome".
            // The resource manager will retrieve the value of the  
            // localized resource using the caller's current culture setting.
            String str = rm.GetString(key);//rm.GetString("response_greet_hello", CultureInfo.CurrentCulture));//rm.GetString(key);//resNames[0];//

            return str;*/
            // set UI culture
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            // create a resourcemanager to load satellite assembly
            ResourceManager resMan = new ResourceManager(resNames[0], Assembly.GetExecutingAssembly());//"SatelliteAssemblyClient.MyResources"

            // set label
            string str = resMan.GetString("Hello");

            return str;
        }
    }
}
